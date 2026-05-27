using Malcha.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace Malcha
{
    // Manages loading, scaling, composing HUD and an image cache with LRU behavior.
    internal class ImageController
    {
        private Dictionary<int, Image> _imageCache = new Dictionary<int, Image>();
        private LinkedList<int> _lruList = new LinkedList<int>();
        private readonly object _cacheLock = new object();
        private int _cacheMaxSize = 100;

        internal void SetCacheMaxSize(int size) => _cacheMaxSize = Math.Max(1, size);

        internal bool TryGet(int index, out Image image)
        {
            lock (_cacheLock)
            {
                if (_imageCache.TryGetValue(index, out image))
                {
                    // update LRU
                    _lruList.Remove(index);
                    _lruList.AddFirst(index);
                    return true;
                }
                image = null;
                return false;
            }
        }

        internal void AddToCache(int index, Image img)
        {
            if (img == null) return;
            lock (_cacheLock)
            {
                if (_imageCache.ContainsKey(index)) return;
                _imageCache[index] = img;
                _lruList.Remove(index);
                _lruList.AddFirst(index);

                while (_imageCache.Count > _cacheMaxSize)
                {
                    RemoveOldest();
                }
            }
        }

        private void RemoveOldest()
        {
            if (_lruList.Count == 0) return;
            var last = _lruList.Last.Value;
            _lruList.RemoveLast();
            if (_imageCache.TryGetValue(last, out var img))
            {
                try { img.Dispose(); } catch { }
                _imageCache.Remove(last);
            }
        }

        internal void Clear()
        {
            lock (_cacheLock)
            {
                foreach (var kv in _imageCache)
                {
                    try { kv.Value.Dispose(); } catch { }
                }
                _imageCache.Clear();
                _lruList.Clear();
            }
        }

        // Remove a contiguous range of cached indices and shift remaining keys down
        internal void DeleteRangeIndices(int start, int count)
        {
            if (count <= 0) return;
            lock (_cacheLock)
            {
                int end = start + count - 1;
                // dispose and remove in range
                for (int i = start; i <= end; i++)
                {
                    if (_imageCache.TryGetValue(i, out var img))
                    {
                        try { img.Dispose(); } catch { }
                        _imageCache.Remove(i);
                    }
                }

                // shift keys greater than end
                var remaining = new Dictionary<int, Image>();
                foreach (var kv in _imageCache)
                {
                    int newIdx = kv.Key;
                    if (kv.Key > end) newIdx = kv.Key - count;
                    remaining[newIdx] = kv.Value;
                }
                _imageCache = remaining;

                // rebuild LRU list
                _lruList.Clear();
                foreach (var k in _imageCache.Keys.OrderByDescending(k => k)) _lruList.AddFirst(k);
            }
        }

        internal bool IsImageCached(Image img)
        {
            if (img == null) return false;
            lock (_cacheLock)
            {
                return _imageCache.Values.Contains(img);
            }
        }

        // Load file safely (minimize file lock)
        private Image LoadImageFromFile(string path)
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var img = Image.FromStream(fs);
                var bmp = new Bitmap(img);
                img.Dispose();
                return bmp;
            }
        }

        private Image CreateScaledBitmap(Image src, Size target)
        {
            if (src == null) return null;
            int w = Math.Max(1, target.Width);
            int h = Math.Max(1, target.Height);
            var bmp = new Bitmap(w, h);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, new Rectangle(0, 0, w, h));
            }
            return bmp;
        }

        // Compose HUD on top of a base image and return a new Image
        internal Image ComposeFrameImage(Image baseImg, Frame f, Size targetSize)
        {
            Bitmap bmp = new Bitmap(Math.Max(1, targetSize.Width), Math.Max(1, targetSize.Height));
            try
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);
                    var dest = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    g.DrawImage(baseImg, dest);
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    int arrowLen = Math.Max(24, Math.Min(bmp.Width, bmp.Height) / 6);
                    int arrowWidth = Math.Max(8, arrowLen / 4);
                    int headHeight = Math.Max(8, arrowLen / 4);

                    float centerX = bmp.Width / 2f;
                    float centerY = bmp.Height - (headHeight * 0.15f);

                    float maxDeg = 45f;
                    float angleDeg = (float)f.Angle * maxDeg;

                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(angleDeg);

                    using (var brush = new SolidBrush(Color.FromArgb(220, 255, 215, 64)))
                    using (var pen = new Pen(Color.FromArgb(255, 255, 215, 64), Math.Max(2, arrowWidth / 6)))
                    {
                        g.DrawLine(pen, 0f, 0f, 0f, -arrowLen + headHeight);
                        PointF[] tri = new PointF[] {
                            new PointF(0f, -arrowLen),
                            new PointF(arrowWidth/2f, -arrowLen + headHeight),
                            new PointF(-arrowWidth/2f, -arrowLen + headHeight)
                        };
                        g.FillPolygon(brush, tri);
                        g.DrawPolygon(pen, tri);
                    }

                    g.ResetTransform();

                    // throttle bar
                    try
                    {
                        float barWidth = Math.Max(8, bmp.Width / 80f);
                        float barHeight = Math.Max(40, bmp.Height / 4f);
                        float margin = 10f;
                        float barX = bmp.Width - margin - barWidth;
                        float barY = bmp.Height - margin - barHeight;

                        float t = Math.Max(-1f, Math.Min(1f, (float)f.Throttle));
                        float tNorm = (t + 1f) / 2f;
                        float fillH = barHeight * tNorm;
                        RectangleF bgRect = new RectangleF(barX, barY, barWidth, barHeight);
                        RectangleF fillRect = new RectangleF(barX, barY + (barHeight - fillH), barWidth, fillH);

                        using (var bgBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                        using (var borderPen = new Pen(Color.FromArgb(220, 200, 200, 200), 1f))
                        using (var fillBrush = new SolidBrush(Color.FromArgb(220, 60, 180, 75)))
                        {
                            g.FillRectangle(bgBrush, bgRect);
                            g.DrawRectangle(borderPen, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);
                            g.FillRectangle(fillBrush, fillRect);
                        }

                        string txt = f.Throttle.ToString("+#0.000;-#0.000;0.000");
                        using (var sf = new StringFormat())
                        using (var font = new Font("Segoe UI", Math.Max(8f, bmp.Width / 60f), FontStyle.Bold))
                        using (var txtBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                        {
                            sf.Alignment = StringAlignment.Far;
                            sf.LineAlignment = StringAlignment.Far;
                            var txtPt = new PointF(barX - 6f, bmp.Height - margin);
                            g.DrawString(txt, font, txtBrush, txtPt, sf);
                        }
                    }
                    catch { }
                }

                return bmp;
            }
            catch
            {
                try { bmp.Dispose(); } catch { }
                throw;
            }
        }

        // Load file, scale and compose; returns new Image (caller responsible for disposal if not cached)
        internal Image LoadAndCompose(string path, Frame f, Size target)
        {
            var raw = LoadImageFromFile(path);
            try
            {
                var scaled = CreateScaledBitmap(raw, target);
                try
                {
                    var composed = ComposeFrameImage(scaled, f, target);
                    return composed;
                }
                catch
                {
                    return scaled;
                }
            }
            finally { try { raw.Dispose(); } catch { } }
        }
    }
}
