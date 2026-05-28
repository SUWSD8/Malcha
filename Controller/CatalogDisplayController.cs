using Malcha.Model;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms.DataVisualization.Charting;

namespace Malcha.Controller
{
    // [Controller] 프레임 표시 통합 — 이미지 캐시·차트·재생 UI·오버레이
    internal sealed class CatalogDisplayController
    {
        private readonly Chart _chart;
        private readonly PictureBox _pictureBox;
        private readonly TrackBar _timeline;
        private readonly Label _angleLabel;
        private readonly Label _throttleLabel;
        private readonly Label _modeLabel;
        private readonly Label _recordCountLabel;

        private readonly Dictionary<int, Image> _cache = new();
        private readonly LinkedList<int> _lru = new();
        private readonly object _cacheLock = new();
        private int _cacheMax = 100;
        private int _lastHighlightIndex = -1;

        // UI 컨트롤 참조를 받아 초기화
        public CatalogDisplayController(
            Chart chart, PictureBox pictureBox, TrackBar timeline,
            Label angleLabel, Label throttleLabel, Label modeLabel, Label recordCountLabel)
        {
            _chart = chart;
            _pictureBox = pictureBox;
            _timeline = timeline;
            _angleLabel = angleLabel;
            _throttleLabel = throttleLabel;
            _modeLabel = modeLabel;
            _recordCountLabel = recordCountLabel;
        }

        // 지정 프레임을 화면·차트·라벨·타임라인에 표시
        public int ShowFrame(int index, IReadOnlyList<Frame> frames,
            IReadOnlyList<string> imagePaths, Control invokeTarget)
        {
            if (frames.Count == 0) return 0;
            index = Math.Clamp(index, 0, frames.Count - 1);

            if (invokeTarget.InvokeRequired)
            {
                invokeTarget.BeginInvoke(() => ShowFrame(index, frames, imagePaths, invokeTarget));
                return index;
            }

            var f = frames[index];
            _angleLabel.Text = f.Angle.ToString("+#0.000;-#0.000;0.000");
            _throttleLabel.Text = f.Throttle.ToString("+#0.000;-#0.000;0.000");
            _modeLabel.Text = f.Mode ?? string.Empty;
            _recordCountLabel.Text = $"{index}/{Math.Max(0, frames.Count - 1)}";

            _timeline.Minimum = 0;
            _timeline.Maximum = Math.Max(0, frames.Count - 1);
            _timeline.Enabled = frames.Count > 0;
            if (_timeline.Value != index) _timeline.Value = index;

            HighlightChart(index, frames);
            LoadFrameImage(index, f, frames, imagePaths);
            return index;
        }

        // 화면·라벨·타임라인 초기화
        public void ClearDisplay()
        {
            _timeline.Enabled = false;
            _timeline.Value = 0;
            _pictureBox.Image?.Dispose();
            _pictureBox.Image = null;
            _angleLabel.Text = _throttleLabel.Text = _modeLabel.Text = string.Empty;
            _recordCountLabel.Text = "0";
            _pictureBox.Invalidate();
        }

        // 차트 하이라이트 인덱스 초기화
        public void ResetChartHighlight() => _lastHighlightIndex = -1;

        // 전체 프레임 데이터로 angle/throttle 차트 갱신
        public void RefreshChart(IReadOnlyList<Frame> frames)
        {
            try
            {
                var sAngle = _chart.Series["user/angle"];
                var sThrottle = _chart.Series["user/throttle"];
                sAngle.Points.Clear();
                sThrottle.Points.Clear();
                for (int i = 0; i < frames.Count; i++)
                {
                    sAngle.Points.AddXY(i, frames[i].Angle);
                    sThrottle.Points.AddXY(i, frames[i].Throttle);
                }
                if (_chart.ChartAreas.Count > 0)
                {
                    var area = _chart.ChartAreas[0];
                    area.AxisX.Minimum = 0;
                    area.AxisX.Maximum = Math.Max(0, frames.Count - 1);
                    area.RecalculateAxesScale();
                }
                _lastHighlightIndex = -1;
            }
            catch (Exception ex) { Debug.WriteLine($"Chart error: {ex.Message}"); }
        }

        // 삭제된 구간만큼 차트 포인트 제거
        public void RemoveChartRange(int start, int count)
        {
            try
            {
                var sAngle = _chart.Series["user/angle"];
                var sThrottle = _chart.Series["user/throttle"];
                for (int i = 0; i < count; i++)
                {
                    if (sAngle.Points.Count > start) sAngle.Points.RemoveAt(start);
                    if (sThrottle.Points.Count > start) sThrottle.Points.RemoveAt(start);
                }
            }
            catch { }
        }

        // 현재 프레임 위치를 차트에 마커로 표시
        private void HighlightChart(int index, IReadOnlyList<Frame> frames)
        {
            try
            {
                var sAngle = _chart.Series["user/angle"];
                var sThrottle = _chart.Series["user/throttle"];
                if (sAngle.Points.Count != frames.Count)
                    RefreshChart(frames);

                if (_lastHighlightIndex >= 0 && _lastHighlightIndex < sAngle.Points.Count)
                {
                    sAngle.Points[_lastHighlightIndex].MarkerStyle = MarkerStyle.None;
                    sThrottle.Points[_lastHighlightIndex].MarkerStyle = MarkerStyle.None;
                }
                if (index >= 0 && index < sAngle.Points.Count)
                {
                    sAngle.Points[index].MarkerStyle = MarkerStyle.Circle;
                    sAngle.Points[index].MarkerSize = 8;
                    sAngle.Points[index].MarkerColor = Color.Yellow;
                    sAngle.Points[index].MarkerBorderColor = Color.Black;
                    sThrottle.Points[index].MarkerStyle = MarkerStyle.Circle;
                    sThrottle.Points[index].MarkerSize = 8;
                    sThrottle.Points[index].MarkerColor = Color.Orange;
                    sThrottle.Points[index].MarkerBorderColor = Color.Black;
                    _lastHighlightIndex = index;
                }
            }
            catch { }
        }

        // 이미지 캐시 전체 비우기
        public void ClearCache()
        {
            lock (_cacheLock)
            {
                foreach (var img in _cache.Values) try { img.Dispose(); } catch { }
                _cache.Clear();
                _lru.Clear();
            }
        }

        // 프레임 삭제 후 캐시 인덱스 재정렬
        public void DeleteCacheRange(int start, int count)
        {
            if (count <= 0) return;
            lock (_cacheLock)
            {
                int end = start + count - 1;
                for (int i = start; i <= end; i++)
                    if (_cache.Remove(i, out var img)) try { img.Dispose(); } catch { }

                var shifted = new Dictionary<int, Image>();
                foreach (var kv in _cache)
                    shifted[kv.Key > end ? kv.Key - count : kv.Key] = kv.Value;
                _cache.Clear();
                foreach (var kv in shifted) _cache[kv.Key] = kv.Value;
                _lru.Clear();
                foreach (var k in _cache.Keys.OrderByDescending(k => k)) _lru.AddFirst(k);
            }
        }

        // 앞쪽 N개 프레임 이미지를 백그라운드에서 미리 로드
        public async Task PreloadAsync(IReadOnlyList<string> paths, IReadOnlyList<Frame> frames, int count)
        {
            int n = Math.Min(count, paths.Count);
            var tasks = new List<Task>();
            for (int i = 0; i < n; i++)
            {
                if (string.IsNullOrEmpty(paths[i]) || !File.Exists(paths[i])) continue;
                int idx = i;
                tasks.Add(Task.Run(() =>
                {
                    try
                    {
                        var target = _pictureBox.ClientSize;
                        if (target.Width <= 0) target = new Size(320, 240);
                        var img = LoadAndCompose(paths[idx], target);
                        if (img != null) AddToCache(idx, img);
                    }
                    catch { }
                }));
            }
            try { await Task.WhenAll(tasks); } catch { }
        }

        // PictureBox Paint — 조향(주황)·쓰로틀 HUD. modelAngle 은 교차 테스트 예측용(노란 화살표, 추후 연결)
        public static void DrawOverlay(Graphics g, Frame frame, Size clientSize, double? modelAngle = null)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int w = clientSize.Width, h = clientSize.Height;

            DrawSteeringArrow(g, w, h, frame.Angle, model: false);
            if (modelAngle.HasValue)
                DrawSteeringArrow(g, w, h, modelAngle.Value, model: true);
            DrawThrottleHud(g, w, h, frame.Throttle);
        }

        // 조향 화살표 — model=false: 기록값(주황·큼), model=true: 모델 예측(노랑·작음)
        private static void DrawSteeringArrow(Graphics g, int w, int h, double angle, bool model)
        {
            const float scale = 0.8f;
            int baseLen = model
                ? Math.Max(24, Math.Min(w, h) / 6)
                : Math.Max(64, Math.Min(w, h) / 3);
            int arrowLen = Math.Max(model ? 20 : 48, (int)(baseLen * scale));

            int headHeight = model
                ? Math.Max(5, (int)(arrowLen / 6f))
                : Math.Max(8, (int)(arrowLen / 5f));
            float headHalfW = model
                ? Math.Max(4f, headHeight * 0.55f)
                : Math.Max(7f, headHeight * 0.65f);
            float shaftW = model
                ? Math.Max(2f, arrowLen / 16f)
                : Math.Max(4f, arrowLen / 11f);

            float cx = w / 2f, cy = h - headHeight * 0.15f;
            float angleDeg = (float)angle * 45f;
            float shaftTop = -arrowLen + headHeight;

            var color = model
                ? Color.FromArgb(220, 255, 215, 64)
                : Color.FromArgb(230, 255, 140, 0);

            g.TranslateTransform(cx, cy);
            g.RotateTransform(angleDeg);
            using (var brush = new SolidBrush(color))
            using (var pen = new Pen(color, shaftW) { StartCap = LineCap.Round, EndCap = LineCap.Flat })
            {
                g.DrawLine(pen, 0f, 0f, 0f, shaftTop);
                g.FillPolygon(brush, new[]
                {
                    new PointF(0f, -arrowLen),
                    new PointF(headHalfW, shaftTop),
                    new PointF(-headHalfW, shaftTop)
                });
            }
            g.ResetTransform();
        }

        // 쓰로틀 — 오른쪽 하단 세로 막대 + 바로 왼쪽에 수치
        private static void DrawThrottleHud(Graphics g, int w, int h, double throttle)
        {
            float margin = Math.Max(8f, Math.Min(w, h) / 50f);
            float barW = Math.Max(8f, w / 80f);
            float barH = Math.Max(40f, h / 4f);
            float barX = w - margin - barW;
            float barY = h - margin - barH;
            float fillH = barH * ((Math.Clamp((float)throttle, -1f, 1f) + 1f) / 2f);

            using (var bg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            using (var fill = new SolidBrush(Color.FromArgb(220, 60, 180, 75)))
            {
                g.FillRectangle(bg, barX, barY, barW, barH);
                if (fillH > 0.5f)
                    g.FillRectangle(fill, barX, barY + barH - fillH, barW, fillH);
            }

            string text = throttle.ToString("+#0.00;-#0.00;0.00");
            float fontSize = Math.Max(9f, Math.Min(w, h) / 30f);
            using (var font = new Font("Segoe UI", fontSize, FontStyle.Bold))
            {
                var textSize = g.MeasureString(text, font);
                float textX = barX - textSize.Width - 5f;
                float textY = barY + barH - textSize.Height;

                // 밝은 배경에서도 읽히도록 짧은 그림자 + 밝은 글자
                using (var shadow = new SolidBrush(Color.FromArgb(200, 0, 0, 0)))
                    g.DrawString(text, font, shadow, textX + 1f, textY + 1f);
                using (var textBrush = new SolidBrush(Color.FromArgb(255, 245, 242, 238)))
                    g.DrawString(text, font, textBrush, textX, textY);
            }
        }

        // 캐시 또는 디스크에서 프레임 이미지 로드 후 PictureBox에 표시
        private void LoadFrameImage(int index, Frame frame, IReadOnlyList<Frame> frames, IReadOnlyList<string> paths)
        {
            try
            {
                string? path = index < paths.Count ? paths[index] : null;
                Image? img = null;
                if (!string.IsNullOrEmpty(path) && File.Exists(path) && !TryGetCache(index, out img))
                {
                    var target = _pictureBox.ClientSize;
                    if (target.Width <= 0) target = new Size(320, 240);
                    img = LoadAndCompose(path, target);
                    if (img != null) AddToCache(index, img);
                }
                var old = _pictureBox.Image;
                _pictureBox.Image = img;
                if (old != null && !IsCached(old)) try { old.Dispose(); } catch { }
                _pictureBox.Invalidate();
                PreloadNext(index + 1, frames, paths);
            }
            catch (Exception ex) { Debug.WriteLine($"Frame image error: {ex.Message}"); }
        }

        // 다음 프레임 이미지를 백그라운드에서 선행 로드
        private void PreloadNext(int next, IReadOnlyList<Frame> frames, IReadOnlyList<string> paths)
        {
            if (next >= paths.Count || TryGetCache(next, out _)) return;
            var path = paths[next];
            if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;
            Task.Run(() =>
            {
                try
                {
                    var target = _pictureBox.ClientSize;
                    if (target.Width <= 0) target = new Size(320, 240);
                    var img = LoadAndCompose(path, target);
                    if (img != null) AddToCache(next, img);
                }
                catch { }
            });
        }

        // 파일에서 이미지를 읽어 PictureBox 크기로 스케일 (HUD는 Paint에서 그림)
        private static Image LoadAndCompose(string path, Size target)
        {
            using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var raw = Image.FromStream(fs);
            return CreateScaledBitmap(raw, target);
        }

        // 원본 이미지를 PictureBox 크기에 맞게 스케일
        private static Bitmap CreateScaledBitmap(Image src, Size target)
        {
            var bmp = new Bitmap(Math.Max(1, target.Width), Math.Max(1, target.Height));
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, new Rectangle(0, 0, bmp.Width, bmp.Height));
            return bmp;
        }

        // LRU 캐시에서 이미지 조회 (히트 시 순서 갱신)
        private bool TryGetCache(int index, out Image? image)
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(index, out image))
                { _lru.Remove(index); _lru.AddFirst(index); return true; }
                image = null; return false;
            }
        }

        // 캐시에 이미지 추가 (용량 초과 시 LRU 제거)
        private void AddToCache(int index, Image img)
        {
            lock (_cacheLock)
            {
                if (_cache.ContainsKey(index)) return;
                _cache[index] = img;
                _lru.Remove(index);
                _lru.AddFirst(index);
                while (_cache.Count > _cacheMax) RemoveOldest();
            }
        }

        // LRU 목록에서 가장 오래된 항목 제거
        private void RemoveOldest()
        {
            if (_lru.Count == 0) return;
            int last = _lru.Last!.Value;
            _lru.RemoveLast();
            if (_cache.Remove(last, out var img)) try { img.Dispose(); } catch { }
        }

        // 해당 Image 객체가 캐시에 있는지 확인
        private bool IsCached(Image img)
        {
            lock (_cacheLock) return _cache.Values.Contains(img);
        }
    }
}
