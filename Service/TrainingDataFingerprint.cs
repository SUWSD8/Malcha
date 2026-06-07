using Malcha.Model;
using System.Security.Cryptography;
using System.Text;

namespace Malcha.Service
{
    // 화면 카탈로그와 WSL tub 연동 내용이 같은지 비교용 지문
    internal static class TrainingDataFingerprint
    {
        public static string Compute(IReadOnlyList<Frame> frames, IReadOnlyList<string> imagePaths)
        {
            if (frames.Count == 0) return string.Empty;

            var sb = new StringBuilder(256);
            sb.Append(frames.Count).Append('|');

            AppendSample(sb, frames, 0);
            AppendSample(sb, frames, frames.Count / 2);
            AppendSample(sb, frames, frames.Count - 1);

            int step = Math.Max(1, frames.Count / 32);
            for (int i = 0; i < frames.Count; i += step)
            {
                var f = frames[i];
                sb.Append(f.TimestampMs).Append(':')
                    .Append(f.Angle.ToString("R")).Append(':')
                    .Append(f.Throttle.ToString("R")).Append(';');
            }

            int missing = ImageCoverage.CountMissing(frames, imagePaths);
            sb.Append("|missing=").Append(missing);

            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(hash);
        }

        private static void AppendSample(StringBuilder sb, IReadOnlyList<Frame> frames, int index)
        {
            if (index < 0 || index >= frames.Count) return;
            var f = frames[index];
            sb.Append(index).Append('@')
                .Append(f.TimestampMs).Append(':')
                .Append(f.Angle.ToString("R")).Append(':')
                .Append(f.Throttle.ToString("R")).Append('|');
        }
    }

    internal static class ImageCoverage
    {
        public static int CountMissing(IReadOnlyList<Frame> frames, IReadOnlyList<string> imagePaths)
        {
            int missing = 0;
            for (int i = 0; i < frames.Count; i++)
            {
                string? path = i < imagePaths.Count ? imagePaths[i] : null;
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    missing++;
            }
            return missing;
        }

        public static (int Total, int WithImage, int Missing) Summarize(
            IReadOnlyList<Frame> frames,
            IReadOnlyList<string> imagePaths)
        {
            int total = frames.Count;
            int missing = CountMissing(frames, imagePaths);
            return (total, total - missing, missing);
        }

        public static string FormatSummary(IReadOnlyList<Frame> frames, IReadOnlyList<string> imagePaths)
        {
            var (total, withImage, missing) = Summarize(frames, imagePaths);
            if (total == 0) return "이미지 0/0";
            if (missing == 0) return $"이미지 {withImage:N0}/{total:N0} (전체)";
            double pct = 100.0 * withImage / total;
            return $"이미지 {withImage:N0}/{total:N0} ({pct:F1}%) · 누락 {missing:N0}";
        }
    }
}
