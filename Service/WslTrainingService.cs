using Malcha.Repository;
using System.Diagnostics;
using System.Text;

namespace Malcha.Service
{
    // [Service] WSL에서 DonkeyCar train.py 실행 (mycar 경로는 사용자 지정·저장)
    internal class WslTrainingService
    {
        public const string DefaultDistro = "Ubuntu-22.04";
        public const string DefaultTubDirName = "data";

        private static readonly WslTrainingService _instance = new();
        public static WslTrainingService Instance => _instance;

        private string _distro = DefaultDistro;
        private string _carDirectoryLinux = string.Empty;

        private WslTrainingService()
        {
            TryLoadSavedPath();
        }

        // mycar 경로가 설정·저장되었는지
        public bool IsConfigured { get; private set; }

        public string Distro => _distro;
        public string CarDirectoryLinux => _carDirectoryLinux;

        // data tub UNC 경로
        public string TubUncPath
        {
            get
            {
                if (!IsConfigured)
                    throw new InvalidOperationException("mycar 폴더가 설정되지 않았습니다.");
                return GetUncPath($"{_carDirectoryLinux}/{DefaultTubDirName}");
            }
        }

        // database.json UNC 경로
        public string DatabaseUncPath
        {
            get
            {
                if (!IsConfigured)
                    throw new InvalidOperationException("mycar 폴더가 설정되지 않았습니다.");
                return GetUncPath($"{_carDirectoryLinux}/models/database.json");
            }
        }

        // Linux 경로 → Windows UNC
        public string GetUncPath(string linuxPath)
        {
            var relative = linuxPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine($@"\\wsl.localhost\{_distro}", relative);
        }

        // 폴더 선택 대화상자에서 고른 mycar 경로 적용·저장
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
                CarDirectoryLinux = _carDirectoryLinux
            });
        }

        // 저장된 설정 불러오기 (train.py 있으면 유효)
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

        // 미설정 시 폴더 선택 후 저장 (취소 시 false)
        public bool TryConfigure(Func<string?> promptFolder)
        {
            if (IsConfigured) return true;
            var selected = promptFolder();
            if (string.IsNullOrEmpty(selected)) return false;
            ConfigureFromFolder(selected);
            return true;
        }

        // WSL conda 환경에서 train.py 실행
        public Task<bool> TrainAsync(string modelFileName, IProgress<string>? output = null, CancellationToken cancellationToken = default)
            => TrainAsync(DefaultTubDirName, modelFileName, output, cancellationToken);

        public async Task<bool> TrainAsync(
            string tubDirName,
            string modelFileName,
            IProgress<string>? output = null,
            CancellationToken cancellationToken = default)
        {
            if (!IsConfigured)
                throw new InvalidOperationException("mycar 폴더가 설정되지 않았습니다.");

            return await Task.Run(() => RunTrainProcess(tubDirName, modelFileName, output, cancellationToken), cancellationToken);
        }

        // models/{name}.h5 가중치 파일 삭제
        public void DeleteModelFile(string modelName)
        {
            if (!IsConfigured) return;
            var fileName = modelName.EndsWith(".h5", StringComparison.OrdinalIgnoreCase)
                ? modelName : $"{modelName}.h5";
            var path = GetUncPath($"{_carDirectoryLinux}/models/{fileName}");
            if (File.Exists(path)) File.Delete(path);
        }

        private bool RunTrainProcess(
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
                if (process == null) return false;

                process.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        output?.Report(e.Data);
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(e.Data))
                        output?.Report(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.WaitForExit(200))
                    cancellationToken.ThrowIfCancellationRequested();

                return process.ExitCode == 0;
            }
            catch (OperationCanceledException)
            {
                try { process?.Kill(entireProcessTree: true); } catch { }
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WSL 오류: {ex.Message}");
                output?.Report($"[오류] {ex.Message}");
                return false;
            }
        }
    }
}
