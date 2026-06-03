using System.Text.RegularExpressions;

namespace Malcha.Service
{
    // Windows UNC ↔ WSL Linux 경로 변환·mycar 폴더 검증
    internal static class WslPathHelper
    {
        private static readonly Regex WslUncPattern = new(
            @"^\\\\wsl(?:\.localhost|\$)\\([^\\]+)\\(.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 사용자가 고른 폴더 → (WSL 배포판, Linux 경로, UNC 경로)
        public static (string Distro, string LinuxPath, string UncPath) ParseMycarFolder(string selectedPath)
        {
            if (string.IsNullOrWhiteSpace(selectedPath))
                throw new ArgumentException("폴더 경로가 비어 있습니다.");

            var full = Path.GetFullPath(selectedPath.TrimEnd('\\', '/'));
            var match = WslUncPattern.Match(full);
            if (!match.Success)
            {
                throw new InvalidOperationException(
                    "WSL mycar 폴더를 선택하세요.\n" +
                    "예: \\\\wsl.localhost\\Ubuntu-22.04\\home\\사용자\\mycar");
            }

            string distro = match.Groups[1].Value;
            string linux = "/" + match.Groups[2].Value.Replace('\\', '/');
            return (distro, linux, full);
        }

        // train.py 존재 여부 확인
        public static void ValidateMycarFolder(string uncPath)
        {
            if (!Directory.Exists(uncPath))
                throw new DirectoryNotFoundException($"폴더를 찾을 수 없습니다.\n{uncPath}");

            if (!File.Exists(Path.Combine(uncPath, "train.py")))
                throw new FileNotFoundException($"train.py가 없습니다.\nmycar(파이썬 학습) 폴더인지 확인하세요.\n{uncPath}");
        }

        // Linux 경로 → \\wsl.localhost\{distro}\...
        public static string ToUncPath(string distro, string linuxPath)
        {
            var relative = linuxPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine($@"\\wsl.localhost\{distro}", relative);
        }

        // Windows 절대 경로 → WSL /mnt/... 경로
        public static string WindowsToWslPath(string windowsPath)
        {
            var full = Path.GetFullPath(windowsPath);
            if (full.Length >= 2 && full[1] == ':')
            {
                var drive = char.ToLowerInvariant(full[0]);
                var rest = full[2..].Replace('\\', '/');
                return $"/mnt/{drive}{rest}";
            }
            return full.Replace('\\', '/');
        }
    }
}
