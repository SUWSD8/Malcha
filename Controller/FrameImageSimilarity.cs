using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Malcha
{
    // CNN tub 정제용 — 축소 그레이스케일 MAD 기반 화면 유사도 (0~1, 1=동일)
    internal static class FrameImageSimilarity
    {
        private const int CompareWidth = 96;
        private const int CompareHeight = 72;

        public static double Compare(
            string? pathA,
            string? pathB,
            Dictionary<string, byte[]?> cache)
        {
            var a = LoadGrayThumb(pathA, cache);
            var b = LoadGrayThumb(pathB, cache);
            if (a == null || b == null || a.Length != b.Length)
                return 0;

            long sum = 0;
            for (int i = 0; i < a.Length; i++)
                sum += Math.Abs(a[i] - b[i]);

            double mad = sum / (double)(a.Length * 255);
            return Math.Clamp(1.0 - mad, 0, 1);
        }

        private static byte[]? LoadGrayThumb(string? path, Dictionary<string, byte[]?> cache)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (cache.TryGetValue(path, out var cached))
                return cached;

            byte[]? result = null;
            try
            {
                if (!File.Exists(path))
                {
                    cache[path] = null;
                    return null;
                }

                using var src = new Bitmap(path);
                using var resized = new Bitmap(CompareWidth, CompareHeight, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(resized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                    g.DrawImage(src, 0, 0, CompareWidth, CompareHeight);
                }

                var rect = new Rectangle(0, 0, CompareWidth, CompareHeight);
                var data = resized.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                try
                {
                    int stride = data.Stride;
                    int bytes = stride * CompareHeight;
                    var buffer = new byte[bytes];
                    Marshal.Copy(data.Scan0, buffer, 0, bytes);

                    result = new byte[CompareWidth * CompareHeight];
                    int idx = 0;
                    for (int y = 0; y < CompareHeight; y++)
                    {
                        int row = y * stride;
                        for (int x = 0; x < CompareWidth; x++)
                        {
                            int p = row + x * 3;
                            result[idx++] = (byte)(buffer[p + 2] * 0.299 + buffer[p + 1] * 0.587 + buffer[p] * 0.114);
                        }
                    }
                }
                finally
                {
                    resized.UnlockBits(data);
                }
            }
            catch
            {
                result = null;
            }

            cache[path] = result;
            return result;
        }
    }
}
