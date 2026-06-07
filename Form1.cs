using Malcha.Controller;
using Malcha.Data;
using Malcha.Service;
using Malcha.UI;
using Malcha.View;
using Microsoft.VisualBasic;
using static Malcha.UI.TimelineSelectionBinder;

namespace Malcha
{
    // [View / MainForm] UI 표시·입력만, 흐름은 Controller에 위임
    public partial class Form1 : Form
    {
        private CancellationTokenSource? _playCts;
        private readonly CatalogSession _session = new();
        private readonly FrameRangeSelection _selection = new();
        private readonly FrameRangeSelection _deletedSelection = new();
        private CatalogDisplayController _display = null!;
        private readonly CatalogService _catalog = CatalogService.Instance;
        private CrossTestController? _crossTestController;
        private SaveFileController _saveController;

        private float _playbackSpeed = 1.0f;
        private Label? _speedFlashOverlay;
        private System.Windows.Forms.Timer? _speedFeedbackTimer;
        private System.Windows.Forms.Timer? _speedOverlayTimer;

        public Form1()
        {
            InitializeComponent();
            _display = new CatalogDisplayController(
                chtDataGraph, picVideoScreen, trbTimeline,
                lblAngleValue, lblThrottleValue, lblModeValue, lblRecordCount);

            InitializeCatalogController();
            _saveController = new SaveFileController(_session, _catalogController);
            _crossTestController = new CrossTestController(_session, _selection);
            InitializeTrainingPanel();

            new TimelineSelectionBinder(trbTimeline, lstDataList, _selection, RefreshSelectionUi).Attach();
            new DeletedListSelectionBinder(lstDeleted, _deletedSelection, RefreshDeletedSelectionUi).Attach();
            new FrameListDragDropBinder(
                lstDataList, lstDeleted, groupBox2, _selection, _deletedSelection,
                payload => _catalogController!.HandleDropToDeleted(payload),
                payload => _catalogController!.HandleDropToActive(payload)).Attach();
            SetupContextMenus();
            SetupEventHandlers();
            SetupToolTips();
            SetupPlaybackSpeedUi();

            trbTimeline.Enabled = false;
            picVideoScreen.SizeMode = PictureBoxSizeMode.StretchImage;
            HideModelLabels();
        }

        // 리스트·타임라인 우클릭 메뉴 설정
        private void SetupContextMenus()
        {
            var listMenu = new ContextMenuStrip();
            listMenu.Items.Add("선택 항목 삭제", null, async (_, _) =>
                await _catalogController!.HandleDeleteListItemsAsync(GetActiveListDeleteIndices()));
            lstDataList.ContextMenuStrip = listMenu;
            lstDataList.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                    _ = _catalogController!.HandleDeleteListItemsAsync(GetActiveListDeleteIndices());
            };

            var trackMenu = new ContextMenuStrip();
            trackMenu.Items.Add("선택 구간 삭제 목록으로 이동", null, async (_, _) =>
                await _catalogController!.HandleDeleteSelectionAsync());
            trbTimeline.ContextMenuStrip = trackMenu;

            var deletedMenu = new ContextMenuStrip();
            deletedMenu.Items.Add("선택 항목 복구", null, (_, _) =>
                _catalogController!.RestoreFromDeletedList(_deletedSelection.ToIndexList()));
            lstDeleted.ContextMenuStrip = deletedMenu;
        }

        // 버튼·키·Paint 이벤트 핸들러 연결
        private void SetupEventHandlers()
        {
            //btnSelectData.Click += async (_, _) => await _catalogController!.HandleSelectDataAsync();
            btnRefresh.Click += async (_, _) => await _catalogController!.HandleRefreshAsync();
            btnRecover.Click += async (_, _) => await _catalogController!.HandleRecoverAsync();
            btnApplyFilter.Click += async (_, _) => await _catalogController!.HandleApplyFilterAsync();
            btnChangeCleanData.Click += async (_, _) => await _catalogController!.HandleSyncTrainingDataAsync();
            btnDeleteSelection.Click += async (_, _) => await _catalogController!.HandleDeleteSelectionAsync();
            btnSetStartPoint.Click += (_, _) => SetRangeStart();
            btnSetEndPoint.Click += (_, _) => SetRangeEnd();
            lstDataList.SelectedIndexChanged += (_, _) => OnListSelectionChanged();
            btnNextFrame.Click += (_, _) => StepFrame(1);
            btnPrevFrame.Click += (_, _) => StepFrame(-1);
            btnFastForward.Click += (_, _) => StepFrame(5);
            btnRewind.Click += (_, _) => StepFrame(-5);
            btnPlayPause.Click += BtnPlayPause_Click;
            trbTimeline.Scroll += (_, _) => OnTimelineScroll();
            btnHelper.Click += (_, _) => HelpDialog.ShowFor(this);
            btnCrossTest.Click += async (_, _) =>
                await _crossTestController!.RunAsync((ITrainingView)this, (ICatalogView)this, RefreshFrameOverlay);
            picVideoScreen.Paint += PicVideoScreen_Paint;
            KeyPreview = true;
            KeyDown += Form1_KeyDown;
        }

        // 단축키·버튼 툴팁 설정
        private void SetupToolTips()
        {
            var tips = new ToolTip { AutoPopDelay = 8000, InitialDelay = 400 };
            tips.SetToolTip(btnPrevFrame, "이전 프레임 (1)");
            tips.SetToolTip(btnNextFrame, "다음 프레임 (1)");
            tips.SetToolTip(btnRewind, "5프레임 뒤로");
            tips.SetToolTip(btnFastForward, "5프레임 앞으로");
            tips.SetToolTip(btnSetStartPoint, "구간 시작 ([)");
            tips.SetToolTip(btnSetEndPoint, "구간 끝 (])");
            tips.SetToolTip(btnDeleteSelection, "선택 구간을 삭제 목록으로 이동");
            tips.SetToolTip(trbTimeline, "Ctrl+드래그: 구간 · Shift: 해제 · 리스트 드래그: 구간");
            tips.SetToolTip(lstDataList, "드래그: 구간 선택 · 삭제 목록으로 끌면 이동");
            tips.SetToolTip(lstDeleted, "드래그: 구간 선택 · 위쪽으로 끌면 복구");
            tips.SetToolTip(btnChangeCleanData, "정제된 카탈로그를 WSL data로 보냅니다 (학습 전 필수)");
            tips.SetToolTip(btnRefresh, "WSL data 연동·초기화 · 카탈로그 다시 읽기");
            tips.SetToolTip(btnCrossTest, "선택 모델로 카탈로그 inference · 주황=기록 · 노랑=예측");
            tips.SetToolTip(btnHelper, "F1 도움말");
            tips.SetToolTip(btnPlayPause, "재생 / 정지 (Space) · 배속 프리셋: 0~5·NumPad · 미세조절: ↑↓");
        }

        private void SetupPlaybackSpeedUi()
        {
            _speedFlashOverlay = new Label
            {
                AutoSize = true,
                BackColor = Color.FromArgb(210, 24, 24, 24),
                ForeColor = Color.FromArgb(255, 210, 90),
                Font = new Font("맑은 고딕", 32F, FontStyle.Bold),
                Padding = new Padding(16, 8, 16, 8),
                Visible = false
            };
            picVideoScreen.Controls.Add(_speedFlashOverlay);
            picVideoScreen.Resize += (_, _) => CenterSpeedFlashOverlay();
        }

        // 프레임 리스트 삭제 대상 — 주황 구간(_selection) 우선, 없으면 ListBox 다중 선택
        private List<int> GetActiveListDeleteIndices()
        {
            if (_selection.HasSelection)
                return _selection.ToIndexList();

            return lstDataList.SelectedIndices.Cast<int>().ToList();
        }

        // 구간 선택 UI·상태바 갱신
        private void RefreshSelectionUi()
        {
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
            if (!_selection.HasSelection) return;
            var (s, e) = _selection.GetRange();
            toolStripStatusLabel1.Text = $"구간 {s}~{e} ({_selection.FrameCount:N0}) · Esc 해제";
        }

        private void RefreshDeletedSelectionUi()
        {
            lstDeleted.Invalidate();
            if (!_deletedSelection.HasSelection) return;
            var (s, e) = _deletedSelection.GetRange();
            toolStripStatusLabel1.Text = $"삭제 목록 구간 {s}~{e} ({_deletedSelection.FrameCount:N0}) · Esc 해제";
        }

        // 단축키 처리 (F1, Space, 0~5 프리셋, ↑↓ 미세조절, [, ], Esc, Ctrl+Z)
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1) { HelpDialog.ShowFor(this); e.Handled = true; return; }

            if (_session.CurrentFrames.Count > 0 && !IsTypingInTextField())
            {
                float preset = PlaybackSettings.PresetSpeedFromDigitKey(e.KeyCode);
                if (preset > 0f)
                {
                    SetPlaybackSpeed(preset);
                    e.Handled = true;
                    return;
                }
            }

            if (_session.CurrentFrames.Count == 0) return;
            switch (e.KeyCode)
            {
                case Keys.Space:
                    BtnPlayPause_Click(null, EventArgs.Empty);
                    e.Handled = true;
                    break;
                case Keys.OemOpenBrackets: SetRangeStart(); e.Handled = true; break;
                case Keys.OemCloseBrackets: SetRangeEnd(); e.Handled = true; break;
                case Keys.Escape:
                    if (_selection.HasSelection) { _selection.Clear(); RefreshSelectionUi(); }
                    else if (_deletedSelection.HasSelection) { _deletedSelection.Clear(); RefreshDeletedSelectionUi(); }
                    else RestoreIdleStatusBar();
                    e.Handled = true; break;
                case Keys.Z when e.Control:
                    _catalogController!.TryRecoverFromUndo(); e.Handled = true; break;

                case Keys.Right when e.Control: StepFrame(5); e.Handled = true; break;
                case Keys.Left when e.Control: StepFrame(-5); e.Handled = true; break;

                case Keys.Right: StepFrame(1); e.Handled = true; break;
                case Keys.Left: StepFrame(-1); e.Handled = true; break;



                case Keys.Q: SetRangeStart(); e.Handled = true; break;
                case Keys.W: SetRangeEnd(); e.Handled = true; break;
                case Keys.Delete:
                    if (IsTypingInTextField()) break;
                    _ = _catalogController!.HandleDeleteSelectionAsync();
                    e.Handled = true; break;
                case Keys.Up:
                    if (!IsTypingInTextField() && _playbackSpeed < PlaybackSettings.MaxSpeed)
                        SetPlaybackSpeed(Math.Min(PlaybackSettings.MaxSpeed, _playbackSpeed + PlaybackSettings.ArrowSpeedStep));
                    e.Handled = true;
                    break;
                case Keys.Down:
                    if (!IsTypingInTextField() && _playbackSpeed > PlaybackSettings.MinSpeed)
                        SetPlaybackSpeed(Math.Max(PlaybackSettings.MinSpeed, _playbackSpeed - PlaybackSettings.ArrowSpeedStep));
                    e.Handled = true;
                    break;

            }
        }

        private bool IsTypingInTextField()
        {
            Control? c = ActiveControl;
            if (c is TextBoxBase) return true;
            if (c is DataGridView dgv && dgv.IsCurrentCellInEditMode) return true;
            return false;
        }

        private void SetPlaybackSpeed(float speed)
        {
            _playbackSpeed = speed;
            RefreshPlaybackSpeedIndicators();
            ShowSpeedChangeFeedback();

            if (_playCts != null)
                UpdatePlaybackStatusBar();
        }

        private void RefreshPlaybackSpeedIndicators()
        {
            bool show = _session.CurrentFrames.Count > 0;
            string label = PlaybackSettings.FormatSpeedLabel(_playbackSpeed);
            string playing = _playCts != null ? "■" : "▶";

            toolStripStatusLabelPlaybackSpeed.Visible = show;
            toolStripStatusLabelPlaybackSpeed.Text = $"{playing} {label}";
            UpdatePlayPauseButtonText();
        }

        private void ShowSpeedChangeFeedback()
        {
            string label = PlaybackSettings.FormatSpeedLabel(_playbackSpeed);

            _speedFeedbackTimer ??= new System.Windows.Forms.Timer { Interval = 2500 };
            _speedFeedbackTimer.Stop();
            _speedFeedbackTimer.Tick -= OnSpeedFeedbackTimerTick;
            _speedFeedbackTimer.Tick += OnSpeedFeedbackTimerTick;
            toolStripStatusLabel1.Text = $"▶ 재생 배속 → {label}";
            toolStripStatusLabel1.ForeColor = Color.FromArgb(255, 210, 90);
            _speedFeedbackTimer.Start();

            if (_speedFlashOverlay != null)
            {
                _speedFlashOverlay.Text = label;
                _speedFlashOverlay.Visible = true;
                CenterSpeedFlashOverlay();
                _speedFlashOverlay.BringToFront();
            }

            _speedOverlayTimer ??= new System.Windows.Forms.Timer { Interval = 900 };
            _speedOverlayTimer.Stop();
            _speedOverlayTimer.Tick -= OnSpeedOverlayTimerTick;
            _speedOverlayTimer.Tick += OnSpeedOverlayTimerTick;
            _speedOverlayTimer.Start();
        }

        private void CenterSpeedFlashOverlay()
        {
            if (_speedFlashOverlay == null) return;
            _speedFlashOverlay.PerformLayout();
            _speedFlashOverlay.Location = new Point(
                Math.Max(0, (picVideoScreen.ClientSize.Width - _speedFlashOverlay.Width) / 2),
                Math.Max(0, (picVideoScreen.ClientSize.Height - _speedFlashOverlay.Height) / 2));
        }

        private void OnSpeedOverlayTimerTick(object? sender, EventArgs e)
        {
            _speedOverlayTimer?.Stop();
            if (_speedFlashOverlay != null)
                _speedFlashOverlay.Visible = false;
        }

        private void OnSpeedFeedbackTimerTick(object? sender, EventArgs e)
        {
            _speedFeedbackTimer?.Stop();
            toolStripStatusLabel1.ForeColor = SystemColors.ButtonHighlight;
            RestoreIdleStatusBar();
        }

        private void RestoreIdleStatusBar()
        {
            if (_playCts != null) { UpdatePlaybackStatusBar(); return; }
            if (_deletedSelection.HasSelection) { RefreshDeletedSelectionUi(); return; }
            if (_selection.HasSelection) { RefreshSelectionUi(); return; }
            if (_session.CurrentFrames.Count > 0)
            {
                int i = Math.Max(0, _session.CurrentIndex) + 1;
                toolStripStatusLabel1.Text = $"프레임 {i:N0} / {_session.CurrentFrames.Count:N0}";
            }
        }

        private void UpdatePlayPauseButtonText()
        {
            string speed = PlaybackSettings.FormatSpeedLabel(_playbackSpeed);
            btnPlayPause.Text = _playCts != null
                ? $"정지 · {speed}"
                : $"재생 · {speed}";
        }

        private void UpdatePlaybackStatusBar()
        {
            if (_session.CurrentFrames.Count == 0) return;
            int i = Math.Max(0, _session.CurrentIndex) + 1;
            toolStripStatusLabel1.Text =
                $"재생 {PlaybackSettings.FormatSpeedLabel(_playbackSpeed)} · 프레임 {i:N0} / {_session.CurrentFrames.Count:N0}";
        }

        private void HidePlaybackSpeedIndicators()
        {
            toolStripStatusLabelPlaybackSpeed.Visible = false;
            if (_speedFlashOverlay != null)
                _speedFlashOverlay.Visible = false;
            btnPlayPause.Text = "재생 / 정지";
        }

        // 현재 프레임을 구간 시작으로 설정
        private void SetRangeStart()
        {
            if (_session.CurrentFrames.Count == 0) return;
            _selection.SetStart(_session.CurrentIndex);
            RefreshSelectionUi();
        }

        // 현재 프레임을 구간 끝으로 설정
        private void SetRangeEnd()
        {
            if (_session.CurrentFrames.Count == 0) return;
            _selection.SetEnd(_session.CurrentIndex);
            RefreshSelectionUi();
        }

        // 세션 프레임으로 차트 갱신
        private void RefreshChartFromFrames() => _display.RefreshChart(_session.CurrentFrames);

        // 지정 인덱스 프레임 표시
        private void ShowFrame(int index)
        {
            if (_session.CurrentFrames.Count == 0) return;
            index = Math.Clamp(index, 0, _session.CurrentFrames.Count - 1);
            _session.CurrentIndex = index;
            _display.ShowFrame(index, _session.CurrentFrames, _session.FrameImagePaths, this);
            RefreshModelLabels();
            picVideoScreen.Invalidate();
            if (_playCts == null && _speedFeedbackTimer is not { Enabled: true })
                RestoreIdleStatusBar();
        }

        // 교차 테스트 HUD·모델 라벨 갱신
        private void RefreshFrameOverlay()
        {
            RefreshModelLabels();
            picVideoScreen.Invalidate();
        }

        // model/angle 라벨 — 교차 테스트 후에만 표시
        private void RefreshModelLabels()
        {
            bool show = _session.HasCrossTest;
            lblModelAngle.Visible = show;
            lblModelValue.Visible = show;
            if (!show)
            {
                lblModelValue.Text = string.Empty;
                return;
            }

            var pred = _session.GetCrossTest(_session.CurrentIndex);
            lblModelValue.Text = pred != null
                ? pred.Angle.ToString("+#0.000;-#0.000;0.000")
                : "—";
        }

        private void HideModelLabels()
        {
            lblModelAngle.Visible = false;
            lblModelValue.Visible = false;
            lblModelValue.Text = string.Empty;
        }

        // 재생 루프 중지
        private void StopPlayback()
        {
            try { _playCts?.Cancel(); } catch { }
            _playCts = null;
            RefreshPlaybackSpeedIndicators();
            RestoreIdleStatusBar();
        }

        // 세션·UI 전체 초기화
        private void ResetAllUi()
        {
            StopPlayback();
            _session.Reset();
            _selection.Clear();
            _deletedSelection.Clear();
            _session.ClearCrossTest();
            HideModelLabels();
            _display.ClearCache();
            lstDataList.Items.Clear();
            lstDeleted.Items.Clear();
            _display.ClearDisplay();
            HideModelLabels();
            _display.RefreshChart(_session.CurrentFrames);
            txtFilePath.Text = string.Empty;
            toolStripStatusLabel1.Text = "초기화됨";
            HidePlaybackSpeedIndicators();
        }

        // 재생 관련 UI만 초기화 (카탈로그 닫힘)
        private void ClearPlayback()
        {
            _session.CurrentFrames = new();
            _session.CurrentCatalogPath = string.Empty;
            _session.CurrentIndex = 0;
            _session.ClearDeleted();
            _session.ClearCrossTest();
            _selection.Clear();
            _deletedSelection.Clear();
            lstDataList.Items.Clear();
            lstDeleted.Items.Clear();
            _display.ClearDisplay();
            HideModelLabels();
            _display.RefreshChart(_session.CurrentFrames);
            _display.ClearCache();
            HidePlaybackSpeedIndicators();
        }

        // PictureBox 위 조향·쓰로틀 오버레이 그리기
        private void PicVideoScreen_Paint(object? sender, PaintEventArgs e)
        {
            if (_session.CurrentFrames.Count == 0) return;
            if (_session.CurrentIndex < 0 || _session.CurrentIndex >= _session.CurrentFrames.Count) return;
            var pred = _session.GetCrossTest(_session.CurrentIndex);
            CatalogDisplayController.DrawOverlay(
                e.Graphics,
                _session.CurrentFrames[_session.CurrentIndex],
                picVideoScreen.ClientSize,
                pred?.Angle);
        }

        // 리스트 선택 변경 → 해당 프레임 표시
        private void OnListSelectionChanged()
        {
            if (lstDataList.SelectedIndex < 0 || lstDataList.SelectedIndex == _session.CurrentIndex) return;
            ShowFrame(lstDataList.SelectedIndex);
        }

        // 타임라인 스크롤 → 해당 프레임 표시
        private void OnTimelineScroll()
        {
            if (_session.CurrentFrames.Count == 0) return;
            ShowFrame(trbTimeline.Value);
            if (lstDataList.SelectedIndex != _session.CurrentIndex)
                lstDataList.SelectedIndex = _session.CurrentIndex;
        }

        // 이전/다음 프레임 이동
        private void StepFrame(int delta)
        {
            if (_session.CurrentFrames.Count == 0) return;
            ShowFrame(Math.Clamp(_session.CurrentIndex + delta, 0, _session.CurrentFrames.Count - 1));
            lstDataList.ClearSelected();
            lstDataList.SelectedIndex = _session.CurrentIndex;
        }

        // 재생/정지 토글 (기본 100ms 간격, 0~5 키로 배속 조절)
        private async void BtnPlayPause_Click(object? sender, EventArgs e)
        {
            if (_session.CurrentFrames.Count == 0) return;
            if (_playCts != null) { StopPlayback(); return; }

            _speedFeedbackTimer?.Stop();
            _playCts = new CancellationTokenSource();
            RefreshPlaybackSpeedIndicators();
            if (_session.CurrentIndex >= _session.CurrentFrames.Count - 1) _session.CurrentIndex = -1;

            try
            {
                while (!_playCts.Token.IsCancellationRequested)
                {
                    int next = _session.CurrentIndex + 1;
                    if (next >= _session.CurrentFrames.Count) break;
                    ShowFrame(next);
                    lstDataList.ClearSelected();
                    lstDataList.SelectedIndex = _session.CurrentIndex;
                    UpdatePlaybackStatusBar();
                    await Task.Delay(PlaybackSettings.DelayMs(_playbackSpeed), _playCts.Token);
                }
            }
            catch (TaskCanceledException) { }
            finally { StopPlayback(); }
        }

        private void btnUndo_Click(object? sender, EventArgs e)
        {
            if (_catalogController == null) return;
            if (_catalogController.TryRecoverFromUndo()) return;
            MessageBox.Show(this, "되돌릴 이전 상태가 없습니다.", "되돌리기",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnSaveCatalog_Click(object? sender, EventArgs e)
        {
            if (_catalogController == null) return;
            await _catalogController.HandleSaveSessionAsync();
            RefreshListBox();
        }
        private void RefreshListBox()
        {
            lstSave.Items.Clear(); // 기존 목록 지우기

            try
            {
                var files = _saveController.GetSaveFilesForUI();
                foreach (var file in files)
                {
                    // ListBox는 텍스트 하나만 들어가므로 이름만 추가
                    lstSave.Items.Add(file.Name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private async void lstSave_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 빈 공간을 더블클릭한 것이 아닌지 확인
            int index = lstSave.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                string selectedFileName = lstSave.Items[index].ToString();
                try
                {
                    bool isSuccess = await _saveController.RequestLoadSaveFile(selectedFileName);
                    if(!isSuccess) MessageBox.Show("현재 작업 중인 내용이 저장되지 않았습니다. 먼저 저장하시겠습니까?", "로드 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    MessageBox.Show($"'{selectedFileName}' 로드 완료!");
                    // 프레임 업데이트는 세션이 변경될 때 CatalogEditorController에서 ICatalogView.CompleteCatalogLoadAsync()를 호출하여 처리합니다.
                    _catalogController.RefreshFrameList(); // 목록 새로고침 요청 (세션 변경 감지 후에도 처리되지만, 확실히 하기 위해 추가)
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "로드 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void lstSave_MouseDown(object sender, MouseEventArgs e)
        {
            // 우클릭을 했을 때, 마우스 위치에 있는 아이템을 강제로 선택 상태로 만듦
            if (e.Button == MouseButtons.Right)
            {
                int index = lstSave.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    lstSave.SelectedIndex = index;
                    // 디자이너에서 ContextMenuStrip을 ListBox에 연결해두면 이 위치에서 메뉴가 뜹니다.
                    cmsSaveFile.Show(lstSave,e.Location);
                }
                else
                {
                    lstSave.ClearSelected(); // 빈 공간 우클릭 시 선택 해제
                }
            }
        }
        private void MenuDelete_Click(object sender, EventArgs e)
        {
            if (lstSave.SelectedItem == null) return;

            string selectedFileName = lstSave.SelectedItem.ToString();

            var result = MessageBox.Show($"'{selectedFileName}' 파일을 삭제하시겠습니까?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _saveController.RequestDelete(selectedFileName);
                    RefreshListBox(); // 삭제 후 목록 새로고침
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "삭제 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void MenuRename_Click(object sender, EventArgs e)
        {
            if (lstSave.SelectedItem == null) return;

            string oldName = lstSave.SelectedItem.ToString();

            // InputBox를 띄워 새 이름을 입력받음 (Microsoft.VisualBasic 참조 필요)
            // 만약 이게 번거롭다면 Form을 하나 새로 만들어서 (TextBox + 확인 버튼) 띄워도 됩니다.
            string newName = Interaction.InputBox("새로운 파일 이름을 입력하세요:", "이름 변경", oldName);

            if (!string.IsNullOrWhiteSpace(newName) && oldName != newName)
            {
                try
                {
                    _saveController.RequestRename(oldName, newName);
                    RefreshListBox(); // 이름 변경 후 목록 새로고침
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "이름 변경 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private async void btnSelectData_Click(object sender, EventArgs e)
        {
            await _catalogController!.HandleSelectDataAsync();
            RefreshListBox();
        }
    }
}
