using Malcha.Controller;
using Malcha.Service;
using Malcha.UI;
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

        public Form1()
        {
            InitializeComponent();

            _display = new CatalogDisplayController(
                chtDataGraph, picVideoScreen, trbTimeline,
                lblAngleValue, lblThrottleValue, lblModeValue, lblRecordCount);

            InitializeCatalogController();
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

            trbTimeline.Enabled = false;
            picVideoScreen.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        // 리스트·타임라인 우클릭 메뉴 설정
        private void SetupContextMenus()
        {
            var listMenu = new ContextMenuStrip();
            listMenu.Items.Add("선택 항목 삭제", null, async (_, _) =>
                await _catalogController!.HandleDeleteListItemsAsync(lstDataList.SelectedIndices.Cast<int>().ToList()));
            lstDataList.ContextMenuStrip = listMenu;
            lstDataList.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                    _ = _catalogController!.HandleDeleteListItemsAsync(lstDataList.SelectedIndices.Cast<int>().ToList());
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
            btnSelectData.Click += async (_, _) => await _catalogController!.HandleSelectDataAsync();
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
            tips.SetToolTip(btnHelper, "F1 도움말");
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

        // 단축키 처리 (F1 도움말, [, ], Esc, Ctrl+Z)
        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1) { HelpDialog.ShowFor(this); e.Handled = true; return; }
            if (_session.CurrentFrames.Count == 0) return;
            switch (e.KeyCode)
            {
                case Keys.OemOpenBrackets: SetRangeStart(); e.Handled = true; break;
                case Keys.OemCloseBrackets: SetRangeEnd(); e.Handled = true; break;
                case Keys.Escape:
                    if (_selection.HasSelection) { _selection.Clear(); RefreshSelectionUi(); }
                    else if (_deletedSelection.HasSelection) { _deletedSelection.Clear(); RefreshDeletedSelectionUi(); }
                    e.Handled = true; break;
                case Keys.Z when e.Control:
                    _catalogController!.TryRecoverFromUndo(); e.Handled = true; break;
            }
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
        private void ShowFrame(int index) =>
            _session.CurrentIndex = _display.ShowFrame(index, _session.CurrentFrames, _session.FrameImagePaths, this);

        // 재생 루프 중지
        private void StopPlayback()
        {
            try { _playCts?.Cancel(); } catch { }
            _playCts = null;
            btnPlayPause.Text = "재생 / 정지";
        }

        // 세션·UI 전체 초기화
        private void ResetAllUi()
        {
            StopPlayback();
            _session.Reset();
            _selection.Clear();
            _deletedSelection.Clear();
            _display.ClearCache();
            lstDataList.Items.Clear();
            lstDeleted.Items.Clear();
            _display.ClearDisplay();
            _display.RefreshChart(_session.CurrentFrames);
            txtFilePath.Text = string.Empty;
            toolStripStatusLabel1.Text = "초기화됨";
        }

        // 재생 관련 UI만 초기화 (카탈로그 닫힘)
        private void ClearPlayback()
        {
            _session.CurrentFrames = new();
            _session.CurrentCatalogPath = string.Empty;
            _session.CurrentIndex = 0;
            _session.ClearDeleted();
            _selection.Clear();
            _deletedSelection.Clear();
            lstDataList.Items.Clear();
            lstDeleted.Items.Clear();
            _display.ClearDisplay();
            _display.RefreshChart(_session.CurrentFrames);
            _display.ClearCache();
        }

        // PictureBox 위 조향·쓰로틀 오버레이 그리기
        private void PicVideoScreen_Paint(object? sender, PaintEventArgs e)
        {
            if (_session.CurrentFrames.Count == 0) return;
            if (_session.CurrentIndex < 0 || _session.CurrentIndex >= _session.CurrentFrames.Count) return;
            CatalogDisplayController.DrawOverlay(e.Graphics, _session.CurrentFrames[_session.CurrentIndex], picVideoScreen.ClientSize);
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
            lstDataList.SelectedIndex = _session.CurrentIndex;
        }

        // 재생/정지 토글 (100ms 간격 자동 재생)
        private async void BtnPlayPause_Click(object? sender, EventArgs e)
        {
            if (_session.CurrentFrames.Count == 0) return;
            if (_playCts != null) { StopPlayback(); return; }

            _playCts = new CancellationTokenSource();
            btnPlayPause.Text = "정지";
            if (_session.CurrentIndex >= _session.CurrentFrames.Count - 1) _session.CurrentIndex = -1;

            try
            {
                while (!_playCts.Token.IsCancellationRequested)
                {
                    int next = _session.CurrentIndex + 1;
                    if (next >= _session.CurrentFrames.Count) break;
                    ShowFrame(next);
                    lstDataList.SelectedIndex = _session.CurrentIndex;
                    await Task.Delay(100, _playCts.Token);
                }
            }
            catch (TaskCanceledException) { }
            finally { StopPlayback(); }
        }

        private async void btnSaveCatalog_Click(object sender, EventArgs e)
        {
            // 방금 만든 통합 덤프 저장 메서드 하나만 심플하게 호출하면 끝!
            await _catalogController.HandleSaveSessionAsync();
        }
    }
}
