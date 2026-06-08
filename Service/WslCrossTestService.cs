using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Malcha.Service
{
    // WSL DonkeyCar 환경에서 cross_test.py 배치 inference 실행
    internal sealed class WslCrossTestService
    {
        private static readonly WslCrossTestService _instance = new();
        public static WslCrossTestService Instance => _instance;

        private WslCrossTestService() { }

        public async Task<CrossTestBatchResult> RunBatchAsync(
            string modelName,
            IReadOnlyList<(int Index, string ImagePath)> frames,
            string? modelType = null,
            IProgress<(int Percent, string Message)>? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (frames.Count == 0)
                throw new InvalidOperationException("교차 테스트할 프레임이 없습니다.");

            var wsl = WslTrainingService.Instance;
            if (!wsl.IsConfigured)
                throw new InvalidOperationException("mycar 폴더가 설정되지 않았습니다.");

            string logicalName = WslTrainingService.NormalizeBaseName(modelName);
            if (!wsl.ModelWeightsExist(logicalName))
                throw new FileNotFoundException($"모델 파일 없음: {wsl.GetModelWeightsUncPath(logicalName)}");

            var modelFile = WslTrainingService.ToFinalFileName(logicalName);

            var workDir = Path.Combine(Path.GetTempPath(), "MalchaCrossTest", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workDir);

            try
            {
                return await RunBatchCoreAsync(
                    wsl, logicalName, modelFile, frames, modelType, progress, cancellationToken, workDir);
            }
            catch (OperationCanceledException)
            {
                try { Directory.Delete(workDir, true); } catch { }
                throw;
            }
        }

        private async Task<CrossTestBatchResult> RunBatchCoreAsync(
            WslTrainingService wsl,
            string modelName,
            string modelFile,
            IReadOnlyList<(int Index, string ImagePath)> frames,
            string? modelType,
            IProgress<(int Percent, string Message)>? progress,
            CancellationToken cancellationToken,
            string workDir)
        {
            var manifestPath = Path.Combine(workDir, "manifest.json");
            var outputPath = Path.Combine(workDir, "predictions.json");

            var manifest = new
            {
                model = modelName,
                modelType = modelType ?? string.Empty,
                frames = frames.Select(f => new { index = f.Index, image = f.ImagePath }).ToArray()
            };
            await File.WriteAllTextAsync(manifestPath, JsonSerializer.Serialize(manifest), cancellationToken)
                .ConfigureAwait(false);

            var scriptPath = ResolveScriptPath();
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"cross_test.py 없음: {scriptPath}");

            progress?.Report((5, $"모델 {modelFile} · {frames.Count:N0} 프레임 inference…"));

            var manifestWsl = WslPathHelper.WindowsToWslPath(manifestPath);
            var outputWsl = WslPathHelper.WindowsToWslPath(outputPath);
            var scriptWsl = WslPathHelper.WindowsToWslPath(scriptPath);
            var modelRel = $"models/{modelFile}";

            // 학습(train.py)과 동일 — bash -ic 으로 conda 초기화
            var typeArg = string.IsNullOrWhiteSpace(modelType) ? "" : $" --model-type '{modelType.Trim()}'";
            var bashCmd =
                $"conda activate e2e_env && cd {wsl.CarDirectoryLinux} && " +
                $"python3 -u '{scriptWsl}' --model '{modelRel}' --manifest '{manifestWsl}' --output '{outputWsl}'{typeArg}";

            var log = new StringBuilder();
            bool ok = await Task.Run(
                () =>
                {
                    Process? wslProcess = null;
                    try
                    {
                        return RunWslProcess(wsl.Distro, bashCmd, log, progress, frames.Count, cancellationToken, out wslProcess);
                    }
                    finally
                    {
                        TerminateWslProcess(wslProcess);
                    }
                },
                cancellationToken).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(outputPath))
            {
                var tail = log.Length > 800 ? log.ToString()[^800..] : log.ToString();
                throw new InvalidOperationException(
                    ok ? "예측 결과 파일이 생성되지 않았습니다." : $"WSL inference 실패.\n{tail}");
            }

            progress?.Report((95, "결과 읽는 중…"));
            var json = await File.ReadAllTextAsync(outputPath, cancellationToken).ConfigureAwait(false);
            var parsed = JsonSerializer.Deserialize<CrossTestJsonRoot>(json, JsonOptions())
                ?? throw new InvalidOperationException("예측 JSON 파싱 실패");

            try { Directory.Delete(workDir, true); } catch { }

            progress?.Report((100, "완료"));
            return new CrossTestBatchResult
            {
                ModelName = modelName,
                Predictions = parsed.Predictions ?? new List<CrossTestJsonItem>(),
                Errors = parsed.Errors ?? new List<CrossTestJsonError>(),
                LogTail = log.ToString()
            };
        }

        private static string ResolveScriptPath()
        {
            var baseDir = AppContext.BaseDirectory;
            var candidate = Path.Combine(baseDir, "Scripts", "cross_test.py");
            if (File.Exists(candidate)) return candidate;

            // 개발 시 프로젝트 루트
            var dev = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Scripts", "cross_test.py"));
            if (File.Exists(dev)) return dev;
            return candidate;
        }

        private static bool RunWslProcess(
            string distro,
            string bashCommand,
            StringBuilder log,
            IProgress<(int Percent, string Message)>? progress,
            int frameCount,
            CancellationToken cancellationToken,
            out Process? process)
        {
            process = null;
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = $"-d {distro} -- bash -ic \"{bashCommand}\"",
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
                    if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(e.Data)) return;
                    log.AppendLine(e.Data);
                    progress?.Report((40, e.Data.Length > 60 ? e.Data[..60] + "…" : e.Data));
                };
                process.ErrorDataReceived += (_, e) =>
                {
                    if (cancellationToken.IsCancellationRequested || string.IsNullOrWhiteSpace(e.Data)) return;
                    log.AppendLine(e.Data);
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                int waited = 0;
                while (!process.WaitForExit(500))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    waited += 500;
                    int pct = Math.Min(90, 10 + waited / Math.Max(1, frameCount));
                    progress?.Report((pct, $"inference 중… ({frameCount:N0} 프레임)"));
                }

                return process.ExitCode == 0;
            }
            catch (OperationCanceledException)
            {
                throw;
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

        private static JsonSerializerOptions JsonOptions() => new() { PropertyNameCaseInsensitive = true };

        internal sealed class CrossTestJsonRoot
        {
            public string? Model { get; set; }
            public List<CrossTestJsonItem>? Predictions { get; set; }
            public List<CrossTestJsonError>? Errors { get; set; }
        }

        internal sealed class CrossTestJsonItem
        {
            public int Index { get; set; }
            public double Angle { get; set; }
            public double Throttle { get; set; }
        }

        internal sealed class CrossTestJsonError
        {
            public int Index { get; set; }
            public string? Error { get; set; }
        }
    }

    internal sealed class CrossTestBatchResult
    {
        public string ModelName { get; init; } = "";
        public List<WslCrossTestService.CrossTestJsonItem> Predictions { get; init; } = new();
        public List<WslCrossTestService.CrossTestJsonError> Errors { get; init; } = new();
        public string LogTail { get; init; } = "";
    }
}
