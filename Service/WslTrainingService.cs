using System.Diagnostics;

namespace Malcha.Service
{
    // [Service] WSL에서 DonkeyCar train.py 실행 (UI 의존성 없음)
    internal class WslTrainingService
    {
        public const string DefaultDistro = "Ubuntu-22.04";
        public const string DefaultCarDirectory = "/home/heejun/mycar";
        public const string DefaultDatabasePath =
            @"\\wsl.localhost\Ubuntu-22.04\home\heejun\mycar\models\database.json";

        private static readonly WslTrainingService _instance = new();
        public static WslTrainingService Instance => _instance;
        private WslTrainingService() { }

        // WSL conda 환경에서 train.py 실행, 성공 여부 반환
        public async Task<bool> TrainAsync(string tubDirName, string modelFileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "wsl.exe",
                        Arguments = $"-d {DefaultDistro} -- bash -ic \"conda activate e2e_env && cd {DefaultCarDirectory} && python3 train.py --tub {tubDirName} --model models/{modelFileName}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    using var process = Process.Start(startInfo);
                    if (process == null) return false;
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"WSL 오류: {ex.Message}");
                    return false;
                }
            });
        }
    }
}
