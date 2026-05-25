using System;
using System.Collections.Generic;
using System.Drawing;
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
        private CancellationTokenSource _playCts;
        private Image _previewImage;

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

            trbTimeline.Enabled = false;

            // PictureBox가 이미지 크기와 달라도 채워지도록 설정
            // (StretchImage: PictureBox 전체를 채우도록 이미지를 늘리거나 줄입니다)
            try
            {
                picVideoScreen.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch { }
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

        // 새로고침 버튼 클릭: 현재 경로 또는 기본 경로 재로드
        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            var path = txtFilePath.Text;
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(Environment.CurrentDirectory, "Data\\TestData");
                txtFilePath.Text = path;
            }

            await LoadAndShowCatalogAsync(path);
        }

        // 디렉터리에서 카탈로그 파일들을 로드하고 첫 카탈로그의 프레임 목록을 리스트에 표시
        private async Task LoadAndShowCatalogAsync(string directory)
        {
            btnSelectData.Enabled = false;
            try
            {
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

            // 이미지 로드: 카탈로그 파일과 동일한 폴더를 기준으로 상대경로 해석
            try
            {
                // 이미지 경로는 미리 계산된 _frameImagePaths에서 사용
                string resolved = null;
                if (_frameImagePaths != null && _frameImagePaths.Count > _currentIndex)
                {
                    resolved = _frameImagePaths[_currentIndex];
                }

                if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
                {
                    _previewImage?.Dispose();
                    _previewImage = null;

                    using (var fs = File.OpenRead(resolved))
                    {
                        using var tmp = Image.FromStream(fs);
                        _previewImage = new Bitmap(tmp);
                    }

                    picVideoScreen.Image?.Dispose();
                    picVideoScreen.Image = _previewImage;
                }
                else
                {
                    picVideoScreen.Image?.Dispose();
                    picVideoScreen.Image = null;
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
