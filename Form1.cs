using Malcha.Controller;
using Malcha.Service;
using Malcha.UI;
using System.Diagnostics;
using static Malcha.UI.TimelineSelectionBinder;

namespace Malcha
{
    // [View / MainForm] UI 표시·입력만, 흐름은 Controller에 위임
    public partial class Form1 : Form
    {
        private CancellationTokenSource? _playCts;
        private Process? _trainingProcess = null;
        private readonly CatalogSession _session = new();
        private readonly FrameRangeSelection _selection = new();
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
            trackMenu.Items.Add("선택된 구간 삭제", null, async (_, _) =>
                await _catalogController!.HandleDeleteSelectionAsync());
            trbTimeline.ContextMenuStrip = trackMenu;
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
            tips.SetToolTip(btnSetStartPoint, "구간 시작 ([)");
            tips.SetToolTip(btnSetEndPoint, "구간 끝 (])");
            tips.SetToolTip(btnDeleteSelection, "선택 구간 삭제");
            tips.SetToolTip(trbTimeline, "Ctrl+드래그: 구간 · Shift: 해제");
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
            _display.ClearCache();
            lstDataList.Items.Clear();
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
            _selection.Clear();
            lstDataList.Items.Clear();
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

        private void btnshutdown_Click(object sender, EventArgs e)
        {
            // [상태 1] 현재 학습이 진행 중일 때 -> 버튼을 누르면 "학습 임의 중단" 기능으로 동작 ⭐
            if (_trainingProcess != null && !_trainingProcess.HasExited)
            {
                DialogResult result = MessageBox.Show(
                    "3,000장 등 현재 진척도에서 학습을 임의 중단하고\n지금까지 학습된 AI 모델을 저장할까요?",
                    "학습 임의 중단 요청",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        string flagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stop.txt");

                        // 파이썬이 읽을 수 있도록 'stop.txt' 플래그 파일 생성
                        File.WriteAllText(flagPath, "STOP_SIGNAL");

                        if (lstLog != null) lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] 사용자 요청: 파이썬에 학습 중단 신호(stop.txt)를 보냈습니다.");
                        toolStripStatusLabel1.Text = "중단 신호 송신 완료. 파이썬이 모델을 저장한 후 안전하게 종료됩니다.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"중단 신호 생성 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return; // 중단 로직을 실행했으므로 메서드 종료
            }

            // [상태 2] 현재 진행 중인 학습이 없을 때 -> 버튼을 누르면 "학습 시작" 기능으로 동작 ⭐
            // 혹시 남아있을지 모르는 기존 중단 신호 파일(stop.txt) 제거
            string oldFlagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stop.txt");
            if (File.Exists(oldFlagPath))
            {
                File.Delete(oldFlagPath);
            }

            try
            {
                if (lstLog != null) lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] AI 학습 프로세스를 시작합니다...");

                // 버튼 텍스트를 '학습 강제 종료'로 변경하여 사용자가 누를 수 있게 유도
                btnshutdown.Text = "학습 강제 종료";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python.exe",
                    Arguments = "train.py", // 실행할 파이썬 파일명
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory
                };

                _trainingProcess = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                // 파이썬 실시간 로그 출력 설정
                _trainingProcess.OutputDataReceived += (s, ev) => {
                    if (ev.Data != null && lstLog != null)
                        this.Invoke(new Action(() => {
                            lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {ev.Data}");
                            lstLog.SelectedIndex = lstLog.Items.Count - 1;
                        }));
                };

                _trainingProcess.ErrorDataReceived += (s, ev) => {
                    if (ev.Data != null && lstLog != null)
                        this.Invoke(new Action(() => {
                            lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] [에러] {ev.Data}");
                            lstLog.SelectedIndex = lstLog.Items.Count - 1;
                        }));
                };

                // 프로세스 종료 시 이벤트 (스스로 끝났거나 사용자가 중단시켰을 때 원래대로 복구)
                _trainingProcess.Exited += (s, ev) => {
                    this.Invoke(new Action(() => {
                        if (lstLog != null) lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] AI 학습 프로세스가 완전히 종료되었습니다.");
                        toolStripStatusLabel1.Text = "동키카 준비 완료 (Donkey Ready)";

                        // 학습이 끝났으므로 버튼 텍스트를 다시 원래대로 복구
                        btnshutdown.Text = "학습 강제 종료";

                        if (_trainingProcess != null) { _trainingProcess.Dispose(); _trainingProcess = null; }
                    }));
                };

                _trainingProcess.Start();
                _trainingProcess.BeginOutputReadLine();
                _trainingProcess.BeginErrorReadLine();

                toolStripStatusLabel1.Text = "AI 학습 진행 중... 원할 때 다시 버튼을 누르면 중단됩니다.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파이썬 학습 파일 실행 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnshutdown.Text = "학습 강제 종료";
            }
        }

        // 2. [학습 강제 종료] 버튼 클릭 이벤트 (사용자 임의 조작) ⭐
        private void btnTrainForceStop_Click(object sender, EventArgs e)
        {
            if (_trainingProcess == null || _trainingProcess.HasExited)
            {
                MessageBox.Show("현재 진행 중인 학습 프로세스가 없습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show(
                "3,000장 등 현재 진척도에서 학습을 임의 중단하고\n지금까지 학습된 AI 모델을 저장할까요?",
                "학습 임의 중단 요청", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    string flagPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stop.txt");
                    
                    // 파이썬이 읽을 수 있도록 'stop.txt' 플래그 파일 생성
                    File.WriteAllText(flagPath, "STOP_SIGNAL");
                    
                    if (lstLog != null) lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] 사용자 요청: 파이썬에 학습 중단 신호(stop.txt)를 보냈습니다.");
                    toolStripStatusLabel1.Text = "중단 신호 송신 완료. 파이썬이 모델을 저장한 후 안전하게 종료됩니다.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"중단 신호 생성 실패: {ex.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
