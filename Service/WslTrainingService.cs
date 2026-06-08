using Malcha.Repository;
using System.Diagnostics;
using System.Text;

namespace Malcha.Service
{
    internal sealed class TrainRunResult
    {
        public bool WasCancelled { get; init; }
        public int ExitCode { get; init; }
        public bool Succeeded => !WasCancelled && ExitCode == 0;
    }

    internal enum StoppedWeightsOutcome
    {
        None,
        StagingReady,
        FinalOnDisk
    }

    // [Service] WSL에서 DonkeyCar train.py 실행 (mycar 경로는 사용자 지정·저장)
    internal class WslTrainingService
    {
        public const string DefaultDistro = "Ubuntu-22.04";
        public const string DefaultTubDirName = "data";
        public const string StagingSuffix = ".malcha_staging";
        public const string BackupSuffix = ".malcha_backup";

        private static readonly WslTrainingService _instance = new();
        public static WslTrainingService Instance => _instance;

        private string _distro = DefaultDistro;
        private string _carDirectoryLinux = string.Empty;

        private WslTrainingService()
        {
            TryLoadSavedPath();
        }

        public bool IsConfigured { get; private set; }

        public string Distro => _distro;
        public string CarDirectoryLinux => _carDirectoryLinux;

        public string TubUncPath
        {
            get
            {
                if (!IsConfigured)
                    throw new InvalidOperationException("mycar 폴더가 설정되지 않았습니다.");
                return GetUncPath($"{_carDirectoryLinux}/{DefaultTubDirName}");
            }
        }

        public string DatabaseUncPath
        {
            get
            {
                if (!IsConfigured)
                    throw new InvalidOperationException("mycar 폴더가 설정되지 않았습니다.");
                return GetUncPath($"{_carDirectoryLinux}/models/database.json");
            }
        }

        public string GetUncPath(string linuxPath)
        {
            var relative = linuxPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine($@"\\wsl.localhost\{_distro}", relative);
        }

        public static string NormalizeBaseName(string modelName)
        {
            var baseName = modelName.Trim();
            if (baseName.EndsWith(".h5", StringComparison.OrdinalIgnoreCase))
                baseName = Path.GetFileNameWithoutExtension(baseName);

            if (baseName.EndsWith(StagingSuffix, StringComparison.OrdinalIgnoreCase))
                baseName = baseName[..^StagingSuffix.Length];

            if (baseName.EndsWith(BackupSuffix, StringComparison.OrdinalIgnoreCase))
                baseName = baseName[..^BackupSuffix.Length];

            return baseName;
        }

        public static string ToFinalFileName(string modelName) =>
            $"{NormalizeBaseName(modelName)}.h5";

        public static string ToStagingFileName(string modelName) =>
            $"{NormalizeBaseName(modelName)}{StagingSuffix}.h5";

        public static string ToBackupFileName(string modelName) =>
            $"{NormalizeBaseName(modelName)}{BackupSuffix}.h5";

        public string GetModelWeightsUncPath(string modelName) =>
            GetUncPath($"{_carDirectoryLinux}/models/{ToFinalFileName(modelName)}");

        public string GetStagingWeightsUncPath(string modelName) =>
            GetUncPath($"{_carDirectoryLinux}/models/{ToStagingFileName(modelName)}");

        public string GetBackupWeightsUncPath(string modelName) =>
            GetUncPath($"{_carDirectoryLinux}/models/{ToBackupFileName(modelName)}");

        public bool ModelWeightsExist(string modelName) =>
            IsConfigured && File.Exists(GetModelWeightsUncPath(modelName));

        public bool StagingWeightsExist(string modelName) =>
            IsConfigured && File.Exists(GetStagingWeightsUncPath(modelName));

        public bool BackupWeightsExist(string modelName) =>
            IsConfigured && File.Exists(GetBackupWeightsUncPath(modelName));

        // 학습 시작 — 기존 .h5 백업 + staging 초기화. true = 재학습(기존 .h5 있음)
        public bool BeginTrainingSession(string modelName)
        {
            modelName = NormalizeBaseName(modelName);
            var finalPath = GetModelWeightsUncPath(modelName);
            bool hadFinal = File.Exists(finalPath);

            DiscardStagingWeights(modelName);
            if (hadFinal)
                File.Copy(finalPath, GetBackupWeightsUncPath(modelName), overwrite: true);

            return hadFinal;
        }

        // 강제 종료 후 WSL→Windows 파일 반영 대기 (최대 ~20초)
        public async Task<StoppedWeightsOutcome> WaitForStoppedWeightsAsync(
            string modelName,
            bool hadFinalAtStart)
        {
            modelName = NormalizeBaseName(modelName);
            for (int i = 0; i < 40; i++)
            {
                if (StagingWeightsExist(modelName))
                    return StoppedWeightsOutcome.StagingReady;
                if (!hadFinalAtStart && ModelWeightsExist(modelName))
                    return StoppedWeightsOutcome.FinalOnDisk;
                await Task.Delay(500).ConfigureAwait(false);
            }

            if (StagingWeightsExist(modelName))
                return StoppedWeightsOutcome.StagingReady;
            if (!hadFinalAtStart && ModelWeightsExist(modelName))
                return StoppedWeightsOutcome.FinalOnDisk;
            return StoppedWeightsOutcome.None;
        }

        // 강제 종료 시 staging → final 승격 (또는 final 이미 있으면 그대로)
        public bool TryCommitStoppedWeights(string modelName, StoppedWeightsOutcome outcome)
        {
            modelName = NormalizeBaseName(modelName);
            switch (outcome)
            {
                case StoppedWeightsOutcome.StagingReady:
                    PromoteStagingWeights(modelName);
                    return ModelWeightsExist(modelName);
                case StoppedWeightsOutcome.FinalOnDisk:
                    DiscardStagingWeights(modelName);
                    return ModelWeightsExist(modelName);
                default:
                    return false;
            }
        }

        public void PromoteStagingWeights(string modelName)
        {
            var staging = GetStagingWeightsUncPath(modelName);
            var final = GetModelWeightsUncPath(modelName);
            if (!File.Exists(staging))
                throw new FileNotFoundException($"staging 가중치 없음: {staging}");

            File.Copy(staging, final, overwrite: true);
            DiscardStagingWeights(modelName);
        }

        public void DiscardStagingWeights(string modelName)
        {
            var staging = GetStagingWeightsUncPath(modelName);
            if (File.Exists(staging))
            {
                try { File.Delete(staging); } catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }
        }

        // 이미 덮어씌워진 경우 — 학습 시작 시 만들어 둔 .malcha_backup.h5 로 복구
        public bool TryRestoreFinalFromBackup(string modelName)
        {
            var backup = GetBackupWeightsUncPath(modelName);
            var final = GetModelWeightsUncPath(modelName);
            if (!File.Exists(backup)) return false;
            File.Copy(backup, final, overwrite: true);
            return true;
        }

        public void ConfigureFromFolder(string selectedFolderPath)
        {
            var (distro, linuxPath, uncPath) = WslPathHelper.ParseMycarFolder(selectedFolderPath);
            WslPathHelper.ValidateMycarFolder(uncPath);

            _distro = distro;
            _carDirectoryLinux = linuxPath;
            IsConfigured = true;

            TrainingSettingsRepository.Instance.Save(new TrainingSettingsRepository.TrainingSettings
            {
                Distro = _distro,
                CarDirectoryLinux = _carDirectoryLinux,
                LastSyncedFingerprint = WslDataSyncService.Instance.LastSyncedFingerprint,
                LastSyncedFrameCount = WslDataSyncService.Instance.LastSyncedFrameCount
            });
        }

        private void TryLoadSavedPath()
        {
            var saved = TrainingSettingsRepository.Instance.Load();
            if (saved == null || string.IsNullOrWhiteSpace(saved.CarDirectoryLinux)) return;

            try
            {
                var unc = WslPathHelper.ToUncPath(saved.Distro, saved.CarDirectoryLinux);
                WslPathHelper.ValidateMycarFolder(unc);
                _distro = saved.Distro;
                _carDirectoryLinux = saved.CarDirectoryLinux;
                IsConfigured = true;
            }
            catch { IsConfigured = false; }
        }

        public bool TryConfigure(Func<string?> promptFolder)
        {
            if (IsConfigured) return true;
            var selected = promptFolder();
            if (string.IsNullOrEmpty(selected)) return false;
            ConfigureFromFolder(selected);
            return true;
        }

        public Task<TrainRunResult> TrainAsync(
            string finalModelFileName,
            IProgress<string>? output = null,
            CancellationToken cancellationToken = default)
            => TrainAsync(DefaultTubDirName, finalModelFileName, output, cancellationToken);

        public async Task<TrainRunResult> TrainAsync(
            string tubDirName,
            string finalModelFileName,
            IProgress<string>? output = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("mycar 폴더가 설정되지 않았습니다.");

            string logicalName = NormalizeBaseName(finalModelFileName);
            string stagingFile = ToStagingFileName(logicalName);

            return await Task.Run(
                () => RunTrainProcess(tubDirName, stagingFile, output, cancellationToken),
                CancellationToken.None).ConfigureAwait(false);
        }

        public void DeleteModelFile(string modelName)
        {
            if (!IsConfigured) return;
            modelName = NormalizeBaseName(modelName);
            foreach (var path in new[]
            {
                GetModelWeightsUncPath(modelName),
                GetStagingWeightsUncPath(modelName),
                GetBackupWeightsUncPath(modelName)
            })
            {
                if (File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }
            }
        }

        private void ForceStopProcess(Process? process)
        {
            if (process == null) return;
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch { }

            try
            {
                if (!process.HasExited)
                    process.WaitForExit(8000);
            }
            catch { }

            TerminateWslProcess(process);
        }

        private TrainRunResult RunTrainProcess(
            string tubDirName,
            string modelFileName,
            IProgress<string>? output,
            CancellationToken cancellationToken)
        {
            Process? process = null;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"-d {_distro} -- bash -ic \"conda activate e2e_env && cd {_carDirectoryLinux} && python3 -u train.py --tub {tubDirName} --model models/{modelFileName}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                };

                process = Process.Start(startInfo);
                if (process == null)
                    return new TrainRunResult { ExitCode = -1 };

                process.OutputDataReceived += (_, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;
                    output?.Report(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrWhiteSpace(e.Data)) return;
                    output?.Report(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.WaitForExit(200))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        ForceStopProcess(process);
                        return new TrainRunResult
                        {
                            WasCancelled = true,
                            ExitCode = process.HasExited ? process.ExitCode : -1
                        };
                    }
                }

                process.WaitForExit();
                int exitCode = process.ExitCode;
                if (exitCode != 0)
                    output?.Report($"[오류] WSL 종료 코드 {exitCode} — 로그에 conda/train.py 오류가 있는지 확인하세요.");
                return new TrainRunResult { ExitCode = exitCode };
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    ForceStopProcess(process);
                    return new TrainRunResult
                    {
                        WasCancelled = true,
                        ExitCode = process?.HasExited == true ? process.ExitCode : -1
                    };
                }

                Debug.WriteLine($"WSL 오류: {ex.Message}");
                output?.Report($"[오류] {ex.Message}");
                return new TrainRunResult { ExitCode = -1 };
            }
            finally
            {
                if (!cancellationToken.IsCancellationRequested)
                    TerminateWslProcess(process);
            }
        }

        private static void TerminateWslProcess(Process? process)
        {
            if (process == null) return;
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch { }
            try
            {
                process.CancelOutputRead();
                process.CancelErrorRead();
            }
            catch { }
            try
            {
                if (!process.HasExited)
                    process.WaitForExit(1500);
            }
            catch { }
            try { process.Dispose(); } catch { }
        }
    }
}
