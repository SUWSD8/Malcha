using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Malcha.Data;
using Malcha.Model;

namespace Malcha
{
    public partial class Form1 : Form
    {
        // 현재 로드된 카탈로그(파일 경로 -> 프레임 리스트)
        private Dictionary<string, List<Frame>> _catalogs = new Dictionary<string, List<Frame>>();
        // 현재 선택된 카탈로그의 프레임 리스트
        private List<Frame> _currentFrames = new List<Frame>();
        private string _currentCatalogPath = string.Empty;
        // 각 프레임에 대응하는 이미지 파일(해결된 절대 경로)을 캐시합니다. 없는 경우 null
        private List<string> _frameImagePaths = new List<string>();
        private int _currentIndex = 0;
        // 이미지 캐시: 인덱스 -> Image
        private Dictionary<int, Image> _imageCache = new Dictionary<int, Image>();
        // LRU 관리를 위한 연결 리스트(앞쪽이 최근 사용)
        private LinkedList<int> _lruList = new LinkedList<int>();
        private readonly object _cacheLock = new object();
        private int _cacheMaxSize = 100; // 필요시 조절
        private CancellationTokenSource _playCts;
        private Image _previewImage;
        // 차트에서 마지막으로 강조한 포인트 인덱스
        private int _lastChartIndex = -1;

        public Form1()
        {
            InitializeComponent();

            // 이벤트 연결
            btnSelectData.Click += BtnSelectData_Click;
            btnRefresh.Click += BtnRefresh_Click;
            lstDataList.SelectedIndexChanged += LstDataList_SelectedIndexChanged;
            btnNextFrame.Click += BtnNextFrame_Click;
            btnPrevFrame.Click += BtnPrevFrame_Click;
            btnPlayPause.Click += BtnPlayPause_Click;
            trbTimeline.Scroll += TrbTimeline_Scroll;

            // PictureBox에 별도의 오버레이를 가볍게 그리기 위해 Paint 이벤트 연결
            picVideoScreen.Paint += PicVideoScreen_Paint;

            trbTimeline.Enabled = false;

            // PictureBox가 이미지 크기와 달라도 채워지도록 설정
            // (StretchImage: PictureBox 전체를 채우도록 이미지를 늘리거나 줄입니다)
            picVideoScreen.SizeMode = PictureBoxSizeMode.StretchImage;

        }

        // PictureBox의 Paint 이벤트에서 오버레이(화살표)를 그립니다. 이미지 그리기는 PictureBox.Image가 담당합니다.
        private void PicVideoScreen_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (_currentFrames == null || _currentFrames.Count == 0) return;
                if (_currentIndex < 0 || _currentIndex >= _currentFrames.Count) return;

                var f = _currentFrames[_currentIndex];
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                int w = picVideoScreen.ClientSize.Width;
                int h = picVideoScreen.ClientSize.Height;
                // 화살표 크기: 화면에 더 잘 보이도록 충분히 크게 설정
                int arrowLen = Math.Max(64, Math.Min(w, h) / 3);
                int arrowWidth = Math.Max(16, arrowLen / 3);
                int headHeight = Math.Max(16, arrowLen / 3);
                float centerX = w / 2f;
                // Anchor the arrow near the bottom edge so it appears attached to the frame boundary
                float centerY = h - (headHeight * 0.15f);
                float maxDeg = 45f;
                float angleDeg = (float)f.Angle * maxDeg;

                // Make the arrow appear attached to the bottom border (slightly inset so it remains visible)
                centerY = h - (headHeight * 0.15f);

                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angleDeg);

                // 그림자(아래쪽으로 약간 오프셋된 검정 반투명)를 먼저 그림
                using (var shadowBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                using (var shadowPen = new Pen(Color.FromArgb(160, 0, 0, 0), Math.Max(4, arrowWidth / 4)))
                {
                    g.TranslateTransform(2f, 2f);
                    g.DrawLine(shadowPen, 0f, 0f, 0f, -arrowLen + headHeight);
                    PointF[] triShadow = new PointF[] {
                        new PointF(0f, -arrowLen),
                        new PointF(arrowWidth/2f, -arrowLen + headHeight),
                        new PointF(-arrowWidth/2f, -arrowLen + headHeight)
                    };
                    g.FillPolygon(shadowBrush, triShadow);
                    g.DrawPolygon(shadowPen, triShadow);
                    g.ResetTransform();
                    // 다시 원점으로 옮겨서 실제 화살표를 그릴 준비
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(angleDeg);
                }

                // 메인 화살표 (테두리 제거하여 더 깔끔하게 표시)
                using (var brush = new SolidBrush(Color.FromArgb(230, 255, 140, 0))) // 밝은 오렌지
                {
                    // 샤프트 (채워진 선으로 표시, 테두리 없이)
                    using (var shaftBrush = new SolidBrush(Color.FromArgb(230, 255, 140, 0)))
                    {
                        g.DrawLine(new Pen(shaftBrush), 0f, 0f, 0f, -arrowLen + headHeight);
                    }
                    PointF[] tri = new PointF[] {
                        new PointF(0f, -arrowLen),
                        new PointF(arrowWidth/2f, -arrowLen + headHeight),
                        new PointF(-arrowWidth/2f, -arrowLen + headHeight)
                    };
                    g.FillPolygon(brush, tri);
                }

                // After drawing arrow, draw throttle overlay on the image (right-bottom)
                try
                {
                    // Compute throttle bar geometry relative to PictureBox client size
                    float barWidth = Math.Max(8, w / 80f);
                    float barHeight = Math.Max(40, h / 4f);
                    float margin = 10f;
                    float barX = w - margin - barWidth;
                    float barY = h - margin - barHeight;

                    float t = Math.Max(-1f, Math.Min(1f, (float)f.Throttle));
                    float tNorm = (t + 1f) / 2f;
                    float fillH = barHeight * tNorm;
                    RectangleF bgRect = new RectangleF(barX, barY, barWidth, barHeight);
                    RectangleF fillRect = new RectangleF(barX, barY + (barHeight - fillH), barWidth, fillH);

                    using (var bgBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                    using (var borderPen = new Pen(Color.FromArgb(220, 200, 200, 200), 1f))
                    using (var fillBrush = new SolidBrush(Color.FromArgb(220, 60, 180, 75)))
                    using (var txtBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                    using (var font = new Font("Segoe UI", Math.Max(8f, w / 60f), FontStyle.Bold))
                    using (var sf = new StringFormat())
                    {
                        g.ResetTransform(); // ensure coordinates are in control space
                        g.FillRectangle(bgBrush, bgRect);
                        g.DrawRectangle(borderPen, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);
                        g.FillRectangle(fillBrush, fillRect);

                        sf.Alignment = StringAlignment.Far;
                        sf.LineAlignment = StringAlignment.Far;
                        var txtPt = new PointF(barX - 6f, h - margin);
                        string txt = f.Throttle.ToString("+#0.000;-#0.000;0.000");
                        g.DrawString(txt, font, txtBrush, txtPt, sf);
                    }
                }
                catch { }

                g.ResetTransform();
            }
            catch { }
        }

        // 안전하게 파일에서 이미지 로드 (FileStream 사용으로 파일 잠금 최소화)
        private Image LoadImageFromFile(string path)
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var img = Image.FromStream(fs);
                // 반환 전에 복사하여 원 스트림 닫아도 사용 가능하도록 함
                var bmp = new Bitmap(img);
                img.Dispose();
                return bmp;
            }
        }

        // 이미지를 PictureBox 크기에 맞게 스케일한 비트맵을 생성합니다.
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

        // 데이터 선택 버튼 클릭: 폴더 선택 후 카탈로그 로드
        private async void BtnSelectData_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "카탈로그(.catalog) 파일이 있는 폴더를 선택하세요.";
                dlg.SelectedPath = Path.Combine(Environment.CurrentDirectory, "Data\\TestData");
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtFilePath.Text = dlg.SelectedPath;
                    await LoadAndShowCatalogAsync(dlg.SelectedPath);
                }
            }
        }

        // 새로고침 버튼 클릭: 모든 상태 초기화 (완전 리셋)
        // 설명: 사용자가 새로고침 버튼을 누르면 현재 재생 중지, 캐시 해제, 목록 및 UI 초기화를 수행합니다.
        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            // 재생 중이면 중지
            try
            {
                _playCts?.Cancel();
            }
            catch { }

            // 캐시 및 상태 초기화
            ClearImageCache();
            _catalogs.Clear();
            _currentFrames.Clear();
            _frameImagePaths.Clear();

            // UI 초기화
            lstDataList.Items.Clear();
            ClearPlayback();
            txtFilePath.Text = string.Empty;

            await Task.CompletedTask;
        }

        // 디렉터리에서 카탈로그 파일들을 로드하고 첫 카탈로그의 프레임 목록을 리스트에 표시
        private async Task LoadAndShowCatalogAsync(string directory)
        {
            btnSelectData.Enabled = false;
            try
            {
                // 새로운 폴더 로드 전 기존 캐시 정리
                ClearImageCache();

                _catalogs = await DataManager.Instance.LoadCatalogsAsync(directory);

                // 간단한 UX: 여러 카탈로그가 있으면 첫 번째 것을 기본으로 사용
                if (_catalogs.Count == 0)
                {
                    ClearPlayback();
                    MessageBox.Show(this, "카탈로그 파일을 찾지 못했습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var firstKey = _catalogs.Keys.First();
                _currentCatalogPath = firstKey;
                _currentFrames = _catalogs[firstKey] ?? new List<Frame>();

                // 카탈로그 디렉터리를 기준으로 이미지 파일 경로를 미리 해석하여 캐시합니다.
                var baseDir = Path.GetDirectoryName(_currentCatalogPath) ?? string.Empty;
                _frameImagePaths = new List<string>(_currentFrames.Count);
                for (int i = 0; i < _currentFrames.Count; i++)
                {
                    var f = _currentFrames[i];
                    string img = f.ImagePath ?? string.Empty;
                    string resolved = null;

                    // 후보들: 절대경로, 카탈로그 폴더 + 이미지명, 카탈로그 폴더의 하위 폴더들(images, cam)
                    if (!string.IsNullOrEmpty(img))
                    {
                        if (Path.IsPathRooted(img) && File.Exists(img))
                        {
                            resolved = img;
                        }
                        else
                        {
                            var candidate = Path.Combine(baseDir, img);
                            if (File.Exists(candidate)) resolved = candidate;
                            else
                            {
                                // 몇 가지 일반적인 하위 폴더를 시도
                                var candidates = new[] { "images", "cam", "imgs", "image" };
                                foreach (var sub in candidates)
                                {
                                    var c2 = Path.Combine(baseDir, sub, img);
                                    if (File.Exists(c2))
                                    {
                                        resolved = c2;
                                        break;
                                    }
                                }
                                // 마지막으로 파일명 일치 검색 (대소문자 무시)
                                if (resolved == null)
                                {
                                    try
                                    {
                                        var files = Directory.GetFiles(baseDir);
                                        var nameOnly = Path.GetFileName(img);
                                        var found = files.FirstOrDefault(p => string.Equals(Path.GetFileName(p), nameOnly, StringComparison.OrdinalIgnoreCase));
                                        if (found != null) resolved = found;
                                    }
                                    catch { }
                                }
                            }
                        }
                    }

                    _frameImagePaths.Add(resolved);
                }

                // lstDataList를 프레임 이름으로 채움 (예: frame_0, frame_1 ...)
                lstDataList.Items.Clear();
                for (int i = 0; i < _currentFrames.Count; i++)
                {
                    // 프레임 이름은 단순하게 frame_{인덱스} 형태로 표시합니다.
                    // 이미지가 있으면 '(img)' 표시
                    var hasImg = (_frameImagePaths.Count > i && !string.IsNullOrEmpty(_frameImagePaths[i]));
                    lstDataList.Items.Add(hasImg ? $"frame_{i} (img)" : $"frame_{i} (no image)");
                }

                if (lstDataList.Items.Count > 0)
                {
                    lstDataList.SelectedIndex = 0;
                }
                else
                {
                    ClearPlayback();
                }

                // 초기 프리로드: 처음 몇 프레임을 미리 로드하여 첫 전환 끊김을 줄임
                int initialPreload = Math.Min(5, _frameImagePaths.Count);
                for (int i = 0; i < initialPreload; i++)
                {
                    var p = _frameImagePaths[i];
                    if (!string.IsNullOrEmpty(p) && File.Exists(p))
                    {
                        // 백그라운드에서 읽어서 캐시에 추가
                        int idx = i;
                        Task.Run(() =>
                        {
                            try
                            {
                                var im = LoadImageFromFile(p);
                                // 스케일해서 캐시 (PictureBox 크기에 맞춤)
                                var target = picVideoScreen.ClientSize;
                                if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                                var scaled = CreateScaledBitmap(im, target);
                                im.Dispose();
                                // Compose HUD (arrow + throttle) on top of scaled image
                                Image composed = null;
                                try
                                {
                                    var frame = _currentFrames[idx];
                                    composed = ComposeFrameImage(scaled, frame, target);
                                }
                                catch
                                {
                                    // fallback to scaled if compose fails
                                    composed = scaled;
                                    scaled = null;
                                }

                                if (scaled != null)
                                {
                                    try { scaled.Dispose(); } catch { }
                                }

                                AddToCache(idx, composed);
                            }
                            catch { }
                        });
                    }
                }

                // 차트에 angle / throttle 데이터 채우기
                try
                {
                    chtDataGraph.Series["user/angle"].Points.Clear();
                    chtDataGraph.Series["user/throttle"].Points.Clear();

                    for (int i = 0; i < _currentFrames.Count; i++)
                    {
                        var f = _currentFrames[i];
                        // X는 인덱스로 사용
                        chtDataGraph.Series["user/angle"].Points.AddXY(i, f.Angle);
                        chtDataGraph.Series["user/throttle"].Points.AddXY(i, f.Throttle);
                    }

                    var area = chtDataGraph.ChartAreas.Count > 0 ? chtDataGraph.ChartAreas[0] : null;
                    if (area != null)
                    {
                        area.AxisX.Minimum = 0;
                        area.AxisX.Maximum = Math.Max(0, _currentFrames.Count - 1);
                        area.RecalculateAxesScale();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Chart update error: {ex.Message}");
                }
            }
            finally
            {
                btnSelectData.Enabled = true;
            }
        }

        // 리스트에서 프레임 선택 시 해당 프레임을 화면에 표시
        private void LstDataList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstDataList.SelectedIndex < 0) return;
            int idx = lstDataList.SelectedIndex;
            ShowFrame(idx);
        }

        // 타임라인 이동 시 프레임 표시
        private void TrbTimeline_Scroll(object sender, EventArgs e)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            ShowFrame(trbTimeline.Value);
        }

        private void BtnNextFrame_Click(object sender, EventArgs e)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            ShowFrame(Math.Min(_currentFrames.Count - 1, _currentIndex + 1));
            lstDataList.SelectedIndex = _currentIndex;
        }

        private void BtnPrevFrame_Click(object sender, EventArgs e)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            ShowFrame(Math.Max(0, _currentIndex - 1));
            lstDataList.SelectedIndex = _currentIndex;
        }

        // 재생/정지 버튼
        private async void BtnPlayPause_Click(object sender, EventArgs e)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;

            if (_playCts != null)
            {
                _playCts.Cancel();
                _playCts = null;
                btnPlayPause.Text = "재생 / 정지";
                return;
            }

            _playCts = new CancellationTokenSource();
            btnPlayPause.Text = "정지";
            var token = _playCts.Token;

            // If currently at the last frame, reset index so playback restarts from the beginning
            if (_currentFrames != null && _currentFrames.Count > 0 && _currentIndex >= _currentFrames.Count - 1)
            {
                _currentIndex = -1;
            }

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int next = _currentIndex + 1;
                    if (next >= _currentFrames.Count) break;
                    ShowFrame(next);
                    lstDataList.SelectedIndex = _currentIndex;
                    await Task.Delay(100, token);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                _playCts = null;
                btnPlayPause.Text = "재생 / 정지";
            }
        }

        // 실제로 프레임을 UI에 반영하는 메서드
        private void ShowFrame(int index)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            index = Math.Max(0, Math.Min(index, _currentFrames.Count - 1));
            _currentIndex = index;

            var f = _currentFrames[_currentIndex];

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ShowFrame(_currentIndex)));
                return;
            }

            // 라벨 업데이트
            lblAngleValue.Text = f.Angle.ToString("+#0.000;-#0.000;0.000");
            lblThrottleValue.Text = f.Throttle.ToString("+#0.000;-#0.000;0.000");
            lblModeValue.Text = f.Mode ?? string.Empty;
            lblRecordCount.Text = ($"{_currentIndex}/{Math.Max(0, _currentFrames.Count - 1)}");

            // 트랙바 범위 업데이트
            trbTimeline.Minimum = 0;
            trbTimeline.Maximum = Math.Max(0, _currentFrames.Count - 1);
            trbTimeline.Enabled = _currentFrames.Count > 0;
            trbTimeline.Value = _currentIndex;

            // 차트에서 현재 인덱스 포인트 강조
            try
            {
                var seriesAngle = chtDataGraph.Series["user/angle"];
                var seriesThrottle = chtDataGraph.Series["user/throttle"];

                if (_lastChartIndex >= 0 && _lastChartIndex < seriesAngle.Points.Count)
                {
                    seriesAngle.Points[_lastChartIndex].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None;
                    seriesAngle.Points[_lastChartIndex].Color = System.Drawing.Color.White;
                    seriesThrottle.Points[_lastChartIndex].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.None;
                    seriesThrottle.Points[_lastChartIndex].Color = System.Drawing.Color.Red;
                }

                if (_currentIndex >= 0 && _currentIndex < seriesAngle.Points.Count)
                {
                    seriesAngle.Points[_currentIndex].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                    seriesAngle.Points[_currentIndex].MarkerSize = 8;
                    seriesAngle.Points[_currentIndex].Color = System.Drawing.Color.Yellow;
                    seriesThrottle.Points[_currentIndex].MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
                    seriesThrottle.Points[_currentIndex].MarkerSize = 8;
                    seriesThrottle.Points[_currentIndex].Color = System.Drawing.Color.Orange;
                    _lastChartIndex = _currentIndex;
                }
            }
            catch { }

            // 이미지 로드: 캐시 우선, 없으면 로드 후 캐시에 저장
            try
            {
                string resolved = null;
                if (_frameImagePaths != null && _frameImagePaths.Count > _currentIndex)
                {
                    resolved = _frameImagePaths[_currentIndex];
                }

                Image img = null;
                    if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
                    {
                        lock (_cacheLock)
                        {
                            if (_imageCache.TryGetValue(_currentIndex, out var cached))
                            {
                                img = cached;
                                // LRU 갱신
                                UpdateLru(_currentIndex);
                            }
                        }

                        if (img == null)
                        {
                            // 로드(동기) - 먼저 FileStream으로 읽고 PictureBox 크기에 맞게 스케일,
                            // HUD(화살표+쓰로틀)를 합성한 뒤 캐시에 넣음
                            var raw = LoadImageFromFile(resolved);
                            try
                            {
                                var target = picVideoScreen.ClientSize;
                                if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                                var scaled = CreateScaledBitmap(raw, target);
                                try
                                {
                                    // Compose HUD using frame data
                                    var composed = ComposeFrameImage(scaled, f, target);
                                    img = composed;
                                    AddToCache(_currentIndex, img);
                                }
                                catch
                                {
                                    // If compose fails, fall back to scaled image
                                    img = scaled;
                                    AddToCache(_currentIndex, img);
                                }
                                finally
                                {
                                    // scaled disposed if it was consumed by ComposeFrameImage; otherwise dispose here when not cached
                                    // (AddToCache stores the image we passed, so only dispose if scaled wasn't cached)
                                }
                            }
                            finally
                            {
                                try { raw.Dispose(); } catch { }
                            }
                        }
                    }

                    // 기본 이미지는 캐시에 보관된 스케일된 이미지(또는 방금 생성한 이미지)를 사용
                    var old = picVideoScreen.Image;
                    picVideoScreen.Image = img;
                    // 이전 이미지가 캐시에 없는 임시 이미지면 Dispose
                    if (old != null)
                    {
                        bool isCached = false;
                        lock (_cacheLock)
                        {
                            isCached = _imageCache.Values.Contains(old);
                        }
                        if (!isCached)
                        {
                            try { old.Dispose(); } catch { }
                        }
                    }

                    // 오버레이(화살표)는 Paint 이벤트에서 그리므로 강제 리프레시
                    picVideoScreen.Invalidate();

                // 프리로드: 다음 프레임 미리 로드 (비동기)
                int next = _currentIndex + 1;
                if (next < _frameImagePaths.Count)
                {
                    var nextPath = _frameImagePaths[next];
                    if (!string.IsNullOrEmpty(nextPath) && File.Exists(nextPath))
                    {
                        lock (_cacheLock)
                        {
                            if (!_imageCache.ContainsKey(next))
                            {
                                Task.Run(() =>
                                {
                                    try
                                    {
                                        var im = LoadImageFromFile(nextPath);
                                        try
                                        {
                                            var target = picVideoScreen.ClientSize;
                                            if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                                            var scaledNext = CreateScaledBitmap(im, target);
                                            try
                                            {
                                                var nextFrame = _currentFrames[next];
                                                Image composedNext = null;
                                                try
                                                {
                                                    composedNext = ComposeFrameImage(scaledNext, nextFrame, target);
                                                }
                                                catch
                                                {
                                                    composedNext = scaledNext;
                                                    scaledNext = null;
                                                }

                                                AddToCache(next, composedNext);
                                            }
                                            finally
                                            {
                                                try { if (scaledNext != null) scaledNext.Dispose(); } catch { }
                                            }
                                        }
                                        finally
                                        {
                                            try { im.Dispose(); } catch { }
                                        }
                                    }
                                    catch { }
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ShowFrame image error: {ex.Message}");
            }
        }

        // 상태 초기화
        private void ClearPlayback()
        {
            _currentFrames = new List<Frame>();
            _currentCatalogPath = string.Empty;
            _currentIndex = 0;
            trbTimeline.Enabled = false;
            picVideoScreen.Image?.Dispose();
            picVideoScreen.Image = null;
            lblAngleValue.Text = string.Empty;
            lblThrottleValue.Text = string.Empty;
            lblModeValue.Text = string.Empty;
            lblRecordCount.Text = "0";
            lstDataList.Items.Clear();
            ClearImageCache();
        }

        // 캐시 관리 유틸리티
        private void AddToCache(int index, Image img)
        {
            lock (_cacheLock)
            {
                if (_imageCache.ContainsKey(index)) return;
                _imageCache[index] = img;
                _lruList.Remove(index);
                _lruList.AddFirst(index);

                // 크기 제한 적용
                while (_imageCache.Count > _cacheMaxSize)
                {
                    RemoveOldest();
                }
            }
        }

        private void UpdateLru(int index)
        {
            lock (_cacheLock)
            {
                _lruList.Remove(index);
                _lruList.AddFirst(index);
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

        private void ClearImageCache()
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

        // 이미지 위에 HUD 오버레이(각도, 쓰로틀)를 그려서 반환합니다.
        private Image ComposeFrameImage(Image baseImg, Frame f, Size targetSize)
        {
            // 타겟 크기의 비트맵을 만들고 baseImg를 Fit하여 그립니다.
            Bitmap bmp = new Bitmap(Math.Max(1, targetSize.Width), Math.Max(1, targetSize.Height));
            try
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Black);

                    // 이미지 비율에 맞춰 채움
                    var dest = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    g.DrawImage(baseImg, dest);

                    // 하단 중앙에 차량의 조향을 나타내는 화살표(앵글)를 그림
                    // UI 라벨에 숫자형 정보가 있기 때문에 프레임 위에는 시각적 화살표만 보여줍니다.
                    g.SmoothingMode = SmoothingMode.AntiAlias;

                    // 화살표 크기 설정(화면 크기에 비례)
                    int arrowLen = Math.Max(24, Math.Min(bmp.Width, bmp.Height) / 6); // 전체 길이
                    int arrowWidth = Math.Max(8, arrowLen / 4);
                    int headHeight = Math.Max(8, arrowLen / 4);

                    // 중심 위치: 하단 중앙에 가깝게 고정하여 화살표가 경계에 붙어 보이도록 함
                    float centerX = bmp.Width / 2f;
                    float centerY = bmp.Height - (headHeight * 0.15f);

                    // 각도 매핑: Frame.Angle이 대개 -1..1 범위라 가정하여 도 단위로 변환
                    float maxDeg = 45f; // 최대 회전 각도
                    float angleDeg = (float)f.Angle * maxDeg;

                    // 그리기: 좌표계를 화살표 중심으로 이동하고 회전시킨 뒤 화살표를 그림
                    g.TranslateTransform(centerX, centerY);
                    g.RotateTransform(angleDeg);

                    using (var brush = new SolidBrush(Color.FromArgb(220, 255, 215, 64)))
                    using (var pen = new Pen(Color.FromArgb(255, 255, 215, 64), Math.Max(2, arrowWidth / 6)))
                    {
                        // 샤프트(중심에서 위로)
                        g.DrawLine(pen, 0f, 0f, 0f, -arrowLen + headHeight);

                        // 화살촉(삼각형)
                        PointF[] tri = new PointF[] {
                            new PointF(0f, -arrowLen),
                            new PointF(arrowWidth/2f, -arrowLen + headHeight),
                            new PointF(-arrowWidth/2f, -arrowLen + headHeight)
                        };
                        g.FillPolygon(brush, tri);
                        g.DrawPolygon(pen, tri);
                    }

                    // 변환 상태 원복
                    g.ResetTransform();

                    // 쓰로틀 시각화: 우측 하단에 수직 바와 숫자를 그림
                    try
                    {
                        float barWidth = Math.Max(8, bmp.Width / 80f);
                        float barHeight = Math.Max(40, bmp.Height / 4f);
                        float margin = 10f;
                        float barX = bmp.Width - margin - barWidth;
                        float barY = bmp.Height - margin - barHeight;

                        // 쓰로틀 값은 -1..1 범위를 가질 수 있으므로 0..1로 매핑 (음수는 아래로 표시)
                        float t = Math.Max(-1f, Math.Min(1f, (float)f.Throttle));
                        float tNorm = (t + 1f) / 2f; // 0..1

                        // 바의 채워진 높이 계산 (아래에서 위로 채움)
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

                        // 숫자 라벨
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

        private void btnTrainModel_Click(object sender, EventArgs e)
        {
            // 1. 새로 만든 모델 학습 폼(FormTrain)의 인스턴스를 생성합니다.
            Form2 trainForm = new Form2(); // 좌측 타입을 Form2로 정확하게 맞춰줍니다!

            // 2. 새 창이 메인 창(Form1)의 정중앙에 위치하도록 띄우는 설정입니다. (UI가 깔끔해짐)
            trainForm.StartPosition = FormStartPosition.CenterParent;

            // 3. 핵심! ShowDialog()를 사용하여 '모달' 방식으로 창을 열어줍니다.
            // (this)를 넣어주면 메인 폼이 새 폼의 부모(Owner)가 되어 관계가 명확해집니다.
            trainForm.ShowDialog(this);
        }
    }
}
