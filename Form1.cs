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
using System.Text.Json;

namespace Malcha
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _playCts;

        private ContextMenuStrip _listContextMenu;
        private ContextMenuStrip _trackContextMenu;

        private readonly CatalogSession _session = new();
        private CatalogManager _catalogManager;
        private ImageController _imageController;
        private ChartController _chartController;
        private PlaybackController _playbackController;
        private CatalogEditorService _editorService;
        private SelectionManager _selectionManager = new();
        private bool _timelineRangeDrag;

        public static extern void DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int pvAttribute, int cbAttribute);
        private int GetTimelineIndexFromMouse(TrackBar tb, int mouseX)
        {
            int trackWidth = Math.Max(1, tb.ClientSize.Width - 8);
            float ratio = Math.Max(0f, Math.Min(1f, (float)mouseX / trackWidth));
            int value = (int)Math.Round(ratio * (tb.Maximum - tb.Minimum)) + tb.Minimum;
            return Math.Max(tb.Minimum, Math.Min(tb.Maximum, value));
        }

        private void RefreshSelectionUi()
        {
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
            UpdateRangeStatusText();
        }

        private void UpdateRangeStatusText()
        {
            if (!_selectionManager.HasSelection) return;
            var (s, e) = _selectionManager.GetRange();
            toolStripStatusLabel1.Text =
                $"구간 {s}~{e} ({_selectionManager.FrameCount:N0}프레임) · Esc 해제";
        }

        private void ClearRangeSelection()
        {
            if (!_selectionManager.HasSelection) return;
            _selectionManager.Clear();
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
            if (_session.CurrentFrames.Count > 0)
                toolStripStatusLabel1.Text = $"{_session.CurrentFrames.Count:N0} 프레임";
        }

        private void LstDataList_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                if (e.Index < 0) return;
                var lb = (ListBox)sender;
                var g = e.Graphics;
                var itemRect = e.Bounds;
                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

                // background
                using (var bg = new SolidBrush(selected ? SystemColors.Highlight : lb.BackColor))
                {
                    g.FillRectangle(bg, itemRect);
                }

                // highlight start/end range
                    var r = _selectionManager.GetRange();
                    if (r.s >= 0 && r.e >= 0)
                    {
                        if (e.Index >= r.s && e.Index <= r.e)
                        {
                            using (var h = new SolidBrush(Color.FromArgb(60, Color.Orange)))
                            {
                                g.FillRectangle(h, itemRect);
                            }
                        }
                    }

                // text
                string text = lb.Items[e.Index].ToString();
                using (var txtBrush = new SolidBrush(selected ? SystemColors.HighlightText : lb.ForeColor))
                {
                    g.DrawString(text, lb.Font, txtBrush, itemRect.Left + 2, itemRect.Top + 2);
                }
            }
            catch { }
        }

        private void TrbTimeline_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                var tb = (TrackBar)sender;
                var g = e.Graphics;
                int w = tb.ClientSize.Width;
                int h = tb.ClientSize.Height;

                var r = _selectionManager.GetRange();
                if (r.s >= 0)
                {
                    int min = tb.Minimum;
                    int max = tb.Maximum;
                    float startRatio = (float)(r.s - min) / Math.Max(1, max - min);
                    float endRatio = (r.e >= 0) ? (float)(r.e - min) / Math.Max(1, max - min) : startRatio;
                    float sx = startRatio * w;
                    float ex = endRatio * w;
                    if (ex < sx) { var t = sx; sx = ex; ex = t; }
                    var rect = new RectangleF(sx, 0, Math.Max(4, ex - sx), h);
                    using (var brush = new SolidBrush(Color.FromArgb(80, Color.Orange)))
                    {
                        g.FillRectangle(brush, rect);
                    }
                }
            }
            catch { }
        }

        private async void BtnRecover_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_session.CurrentCatalogPath))
            {
                MessageBox.Show(this, "먼저 카탈로그 파일을 열어 주세요.", "복구",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var workingPath = CatalogPaths.ResolveWorkingCatalogPath(_session.CurrentCatalogPath);
            var backupPath = CatalogPaths.FindLatestBackupPath(workingPath);

            if (string.IsNullOrEmpty(backupPath) || !File.Exists(backupPath))
            {
                if (TryRecoverFromUndoStack())
                    return;

                MessageBox.Show(this,
                    "병합할 백업 파일을 찾지 못했습니다.\n" +
                    $"· {Path.Combine(Path.GetDirectoryName(workingPath) ?? "", CatalogPaths.BackupsFolderName)} 폴더\n" +
                    $"· {workingPath}.bak",
                    "복구", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<Frame> refinedFrames;
            try
            {
                var currentFull = Path.GetFullPath(_session.CurrentCatalogPath);
                var workingFull = Path.GetFullPath(workingPath);
                if (string.Equals(currentFull, workingFull, StringComparison.OrdinalIgnoreCase))
                    refinedFrames = _session.CurrentFrames.ToList();
                else if (File.Exists(workingPath))
                    refinedFrames = await _catalogManager.LoadCatalogFileAsync(workingPath);
                else
                    refinedFrames = _session.CurrentFrames.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"정제 파일을 읽지 못했습니다.\n{ex.Message}", "복구",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int backupCount;
            try
            {
                backupCount = (await _catalogManager.LoadCatalogFileAsync(backupPath))?.Count ?? 0;
            }
            catch
            {
                backupCount = -1;
            }

            var confirm = MessageBox.Show(this,
                "백업 원본과 현재 정제 파일을 병합합니다.\n\n" +
                $"백업: {Path.GetFileName(backupPath)} ({backupCount:N0} 프레임)\n" +
                $"정제: {Path.GetFileName(workingPath)} ({refinedFrames.Count:N0} 프레임)\n\n" +
                "· 백업 타임라인을 기준으로 빠진 프레임을 되살립니다\n" +
                "· 같은 시각·이미지는 정제본 값을 유지합니다\n\n" +
                "계속할까요?",
                "복구", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            PushUndoSnapshot();

            ProgressDialog progress = null;
            SetUiBusy(true);
            try
            {
                progress = new ProgressDialog("백업 병합");
                progress.ShowFor(this);
                progress.Refresh();

                List<Frame> backupFrames;
                try
                {
                    progress.Report(10, "백업 파일 읽는 중…");
                    backupFrames = await _catalogManager.LoadCatalogFileAsync(backupPath);
                }
                catch (Exception ex)
                {
                    CloseProgressDialog(ref progress);
                    ShowAppMessage($"백업을 읽지 못했습니다.\n{ex.Message}", "복구",
                        icon: MessageBoxIcon.Error);
                    return;
                }

                progress.Report(40, "프레임 병합 중…");
                var mergeResult = await Task.Run(() =>
                    CatalogMerger.Merge(backupFrames, refinedFrames), progress.Token);

                progress.Report(85, "작업 파일 저장 중…");
                try { CatalogPaths.CreateTimestampedBackup(workingPath); } catch { }

                _session.CurrentCatalogPath = workingPath;
                _session.CurrentFrames = mergeResult.Frames;
                _session.Catalogs[workingPath] = _session.CurrentFrames;

                await DataManager.Instance.SaveFramesAsync(workingPath, _session.CurrentFrames);

                ClearImageCache();
                _selectionManager.Clear();
                _chartController.ResetHighlight();
                _session.FrameImagePaths = _catalogManager.ResolveFrameImagePaths(workingPath, _session.CurrentFrames);
                RefreshFrameListUi();
                if (_session.CurrentFrames.Count > 0)
                    ShowFrame(Math.Min(_session.CurrentIndex, _session.CurrentFrames.Count - 1));

                toolStripStatusLabel1.Text =
                    $"복구 병합: {mergeResult.Frames.Count:N0} 프레임 (복원 {mergeResult.Frames.Count - refinedFrames.Count:N0})";

                CloseProgressDialog(ref progress);
                ShowAppMessage(
                    $"병합이 완료되었습니다.\n\n" +
                    $"백업 원본: {backupFrames.Count:N0} 프레임\n" +
                    $"정제 파일: {refinedFrames.Count:N0} 프레임\n" +
                    $"결과: {mergeResult.Frames.Count:N0} 프레임\n" +
                    $"정제본 우선 적용: {mergeResult.RefinedOverrides:N0}\n" +
                    $"정제에만 있던 추가: {mergeResult.FromRefinedOnly:N0}\n\n" +
                    $"저장: {workingPath}",
                    "복구");
            }
            catch (OperationCanceledException)
            {
                CloseProgressDialog(ref progress);
                ShowAppMessage("병합이 취소되었습니다.", "복구");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Recover merge error: {ex.Message}");
                CloseProgressDialog(ref progress);
                ShowAppMessage($"복구 중 오류가 발생했습니다.\n{ex.Message}", "복구",
                    icon: MessageBoxIcon.Error);
            }
            finally
            {
                CloseProgressDialog(ref progress);
                SetUiBusy(false);
                EnsureFormVisible();
            }
        }

        private bool TryRecoverFromUndoStack()
        {
            if (!_session.TryPopUndo(out var snap)) return false;
            try
            {
                _session.RestoreUndo(snap);
                ClearImageCache();
                _catalogManager.PopulateListBoxWithFrames(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
                UpdateCatalogPathDisplay();
                if (_session.CurrentFrames.Count > 0)
                    ShowFrame(_session.CurrentIndex);

                toolStripStatusLabel1.Text = "편집 직전 상태로 되돌렸습니다 (Undo).";
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Undo recover error: {ex.Message}");
                return false;
            }
        }

        public Form1()
        {
            InitializeComponent();

            // Catalog manager for separating catalog logic
            _catalogManager = new CatalogManager();
            _imageController = new ImageController();
            _chartController = new ChartController(chtDataGraph);
            _editorService = new CatalogEditorService(_catalogManager);
            _playbackController = new PlaybackController(
                _imageController,
                _chartController,
                picVideoScreen,
                trbTimeline,
                lblAngleValue,
                lblThrottleValue,
                lblModeValue,
                lblRecordCount);

            // list context menu for deletion
            _listContextMenu = new ContextMenuStrip();
            _listContextMenu.Items.Add("선택 항목 삭제", null, (s, e) => DeleteSelectedListItems());
            lstDataList.ContextMenuStrip = _listContextMenu;
            lstDataList.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) DeleteSelectedListItems(); };
            // owner-draw to show range highlights
            lstDataList.DrawMode = DrawMode.OwnerDrawFixed;
            lstDataList.DrawItem += LstDataList_DrawItem;

            // TrackBar: Ctrl+클릭/드래그=시작, Ctrl+Shift+클릭=끝, Shift+클릭=구간 해제
            trbTimeline.MouseDown += TrbTimeline_MouseDown;
            trbTimeline.MouseMove += TrbTimeline_MouseMove;
            trbTimeline.MouseUp += TrbTimeline_MouseUp;
            // list: Ctrl=시작, Ctrl+Shift=끝
            lstDataList.MouseDown += LstDataList_MouseDown;
            // track context menu for deleting marked range
            _trackContextMenu = new ContextMenuStrip();
            _trackContextMenu.Items.Add("선택된 구간 삭제", null, async (s, e) =>
            {
                var r = _selectionManager.GetRange();
                if (r.s < 0) return;
                int eidx = r.e >= 0 ? r.e : r.s;
                if (!ConfirmDelete(r.s, eidx)) return;
                var savePath = _session.CurrentCatalogPath;
                int removed = DeleteRange(r.s, eidx, persistAfter: false);
                if (removed > 0 && !string.IsNullOrEmpty(savePath))
                    await PersistCatalogAfterEditAsync(removed, savePath);
            });
            trbTimeline.ContextMenuStrip = _trackContextMenu;
            trbTimeline.Paint += TrbTimeline_Paint;

            KeyPreview = true;
            KeyDown += Form1_KeyDown;

            var rangeTips = new ToolTip { AutoPopDelay = 8000, InitialDelay = 400 };
            rangeTips.SetToolTip(btnSetStartPoint, "현재 프레임을 구간 시작으로 설정 ([ 키)");
            rangeTips.SetToolTip(btnSetEndPoint, "현재 프레임을 구간 끝으로 설정 (] 키)");
            rangeTips.SetToolTip(btnDeleteSelection, "주황색으로 표시된 구간 삭제");
            rangeTips.SetToolTip(trbTimeline,
                "Ctrl+드래그: 구간 선택\nCtrl+클릭: 시작점\nCtrl+Shift+클릭: 끝점\nShift+클릭: 구간 해제");
            rangeTips.SetToolTip(lstDataList, "Ctrl+클릭: 시작 · Ctrl+Shift+클릭: 끝");
            rangeTips.SetToolTip(btnHelper, "F1 · 사용 안내");

            // 이벤트 연결
            btnSelectData.Click += BtnSelectData_Click;
            btnRefresh.Click += BtnRefresh_Click;
            btnRecover.Click += BtnRecover_Click;
            btnSetStartPoint.Click += BtnSetStartPoint_Click;
            btnSetEndPoint.Click += BtnSetEndPoint_Click;
            btnDeleteSelection.Click += BtnDeleteSelection_Click;
            btnApplyFilter.Click += BtnApplyFilter_Click;
            lstDataList.SelectedIndexChanged += LstDataList_SelectedIndexChanged;
            btnNextFrame.Click += BtnNextFrame_Click;
            btnPrevFrame.Click += BtnPrevFrame_Click;
            btnPlayPause.Click += BtnPlayPause_Click;
            trbTimeline.Scroll += TrbTimeline_Scroll;
            btnHelper.Click += (_, _) => HelpDialog.ShowFor(this);

            // PictureBox에 별도의 오버레이를 가볍게 그리기 위해 Paint 이벤트 연결
            picVideoScreen.Paint += PicVideoScreen_Paint;

            trbTimeline.Enabled = false;

            // PictureBox가 이미지 크기와 달라도 채워지도록 설정
            // (StretchImage: PictureBox 전체를 채우도록 이미지를 늘리거나 줄입니다)
            picVideoScreen.SizeMode = PictureBoxSizeMode.StretchImage;

        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                HelpDialog.ShowFor(this);
                e.Handled = true;
                return;
            }

            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;

            if (e.KeyCode == Keys.Space)
            {
                BtnPlayPause_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Left)
            {
                BtnPrevFrame_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Right)
            {
                BtnNextFrame_Click(sender, e);
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.Escape)
            {
                ClearRangeSelection();
                e.Handled = true;
                return;
            }

            if (e.KeyCode == Keys.OemOpenBrackets)
            {
                _selectionManager.SetStart(_session.CurrentIndex);
                RefreshSelectionUi();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.OemCloseBrackets)
            {
                _selectionManager.SetEnd(_session.CurrentIndex);
                RefreshSelectionUi();
                e.Handled = true;
            }
        }

        private void LstDataList_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            if (!ctrl) return;

            int idx = lstDataList.IndexFromPoint(e.Location);
            if (idx < 0) return;

            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
            if (shift)
                _selectionManager.SetEnd(idx);
            else
                _selectionManager.SetStart(idx);

            RefreshSelectionUi();
        }

        private void TrbTimeline_MouseDown(object sender, MouseEventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            if (e.Button != MouseButtons.Left) return;

            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;

            if (shift && !ctrl)
            {
                ClearRangeSelection();
                return;
            }

            if (!ctrl) return;

            var tb = (TrackBar)sender;
            int value = GetTimelineIndexFromMouse(tb, e.X);

            if (shift)
            {
                _selectionManager.SetEnd(value);
                RefreshSelectionUi();
                return;
            }

            _selectionManager.SetStart(value);
            _timelineRangeDrag = true;
            tb.Capture = true;
            RefreshSelectionUi();
        }

        private void TrbTimeline_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_timelineRangeDrag || e.Button != MouseButtons.Left) return;
            if ((ModifierKeys & Keys.Control) != Keys.Control) return;

            var tb = (TrackBar)sender;
            _selectionManager.SetEnd(GetTimelineIndexFromMouse(tb, e.X));
            RefreshSelectionUi();
        }

        private void TrbTimeline_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_timelineRangeDrag) return;

            _timelineRangeDrag = false;
            var tb = (TrackBar)sender;
            tb.Capture = false;
            _selectionManager.SetEnd(GetTimelineIndexFromMouse(tb, e.X));
            RefreshSelectionUi();
        }

        private void BtnSetStartPoint_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            _selectionManager.SetStart(_session.CurrentIndex);
            RefreshSelectionUi();
        }

        private void BtnSetEndPoint_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            _selectionManager.SetEnd(_session.CurrentIndex);
            RefreshSelectionUi();
        }

        private async void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0)
            {
                MessageBox.Show(this, "정제할 카탈로그 데이터가 없습니다.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(this,
                $"현재 {_session.CurrentFrames.Count:N0}개 프레임을 자동 정제합니다.\n\n" +
                "· 연속된 동일 조향/쓰로틀 값(중복) 제거\n" +
                "· 급격히 튀었다가 되돌아오는 비정상 값 제거\n" +
                "· 허용 범위를 벗어난 값 제거\n\n" +
                "계속하시겠습니까?",
                "필터 적용",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            if (CatalogPaths.IsBackupCatalog(_session.CurrentCatalogPath) || CatalogPaths.IsUnderBackupsFolder(_session.CurrentCatalogPath))
            {
                var backupWarn = MessageBox.Show(this,
                    "현재 열린 파일은 백업입니다.\n정제 결과를 저장하면 이 백업 파일이 덮어씌워집니다.\n작업용 .catalog 파일을 여는 것을 권장합니다.\n\n그래도 계속할까요?",
                    "필터 적용", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (backupWarn != DialogResult.Yes)
                    return;
            }

            PushUndoSnapshot();

            try { _playCts?.Cancel(); } catch { }

            ProgressDialog progress = null;
            SetUiBusy(true);

            try
            {
                var framesCopy = _session.CurrentFrames.ToList();
                progress = new ProgressDialog("데이터 정제");
                progress.ShowFor(this);
                progress.Refresh();

                var uiProgress = new Progress<FrameRefinementFilter.ProgressReport>(r =>
                    progress.Report(r.Percent, r.Message));

                FrameRefinementFilter.Result refineResult;
                try
                {
                    refineResult = await Task.Run(() =>
                        FrameRefinementFilter.Refine(framesCopy, null, uiProgress, progress.Token));
                }
                catch (OperationCanceledException)
                {
                    CloseProgressDialog(ref progress);
                    ShowAppMessage("정제가 취소되었습니다.", "필터 적용");
                    return;
                }

                if (refineResult.Frames.Count == 0)
                {
                    CloseProgressDialog(ref progress);
                    ShowAppMessage("정제 후 남은 프레임이 없습니다. 기준을 완화하거나 원본을 복구해 주세요.",
                        "필터 적용", icon: MessageBoxIcon.Warning);
                    return;
                }

                progress.Report(95, "화면 갱신 중…");

                _session.CurrentFrames = refineResult.Frames;
                if (!string.IsNullOrEmpty(_session.CurrentCatalogPath))
                    _session.Catalogs[_session.CurrentCatalogPath] = _session.CurrentFrames;

                _session.FrameImagePaths = _catalogManager.ResolveFrameImagePaths(_session.CurrentCatalogPath, _session.CurrentFrames);
                ClearImageCache();
                _selectionManager.Clear();
                _chartController.ResetHighlight();
                RefreshFrameListUi();

                string backupPath = string.Empty;
                if (!string.IsNullOrEmpty(_session.CurrentCatalogPath) && File.Exists(_session.CurrentCatalogPath))
                {
                    progress.Report(98, "카탈로그 저장 중…");
                    try { backupPath = CatalogPaths.CreateTimestampedBackup(_session.CurrentCatalogPath); } catch { }
                    await DataManager.Instance.SaveFramesAsync(_session.CurrentCatalogPath, _session.CurrentFrames);
                }

                UpdateCatalogPathDisplay();

                toolStripStatusLabel1.Text =
                    $"정제 완료: {refineResult.OriginalCount:N0} → {refineResult.Frames.Count:N0} 프레임";

                var backupNote = string.IsNullOrEmpty(backupPath)
                    ? string.Empty
                    : $"\n백업: {backupPath}";

                CloseProgressDialog(ref progress);
                ShowAppMessage(
                    $"정제가 완료되었습니다.\n\n" +
                    $"원본: {refineResult.OriginalCount:N0} 프레임\n" +
                    $"결과: {refineResult.Frames.Count:N0} 프레임\n" +
                    $"제거: {refineResult.RemovedTotal:N0} (중복 {refineResult.RemovedDuplicate:N0}, " +
                    $"스파이크 {refineResult.RemovedSpike:N0}, 범위초과 {refineResult.RemovedOutOfRange:N0})" +
                    backupNote,
                    "필터 적용");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Filter apply error: {ex.Message}");
                CloseProgressDialog(ref progress);
                ShowAppMessage($"정제 중 오류가 발생했습니다.\n{ex.Message}", "필터 적용",
                    icon: MessageBoxIcon.Error);
            }
            finally
            {
                CloseProgressDialog(ref progress);
                SetUiBusy(false);
                EnsureFormVisible();
            }
        }

        private void CloseProgressDialog(ref ProgressDialog progress)
        {
            if (progress == null) return;
            try { progress.Close(); } catch { }
            try { progress.Dispose(); } catch { }
            progress = null;
        }

        private void SetUiBusy(bool busy)
        {
            UseWaitCursor = busy;
            btnApplyFilter.Enabled = !busy;
            btnRecover.Enabled = !busy;
            btnDeleteSelection.Enabled = !busy;
            btnSelectData.Enabled = !busy;
            btnPlayPause.Enabled = !busy;
            btnRefresh.Enabled = !busy;
        }

        private void EnsureFormVisible()
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Activate();
            BringToFront();
        }

        private DialogResult ShowAppMessage(string text, string caption,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            EnsureFormVisible();
            return MessageBox.Show(this, text, caption, buttons, icon);
        }

        private void PushUndoSnapshot() => _session.PushUndo();

        private void RefreshChartFromFrames() =>
            _chartController.RefreshFromFrames(_session.CurrentFrames);

        private void ShowFrame(int index)
        {
            _session.CurrentIndex = _playbackController.ShowFrame(
                index,
                _session.CurrentFrames,
                _session.FrameImagePaths,
                this);
        }

        private void ClearImageCache() => _imageController?.Clear();

        private void RefreshFrameListUi()
        {
            _catalogManager.PopulateListBoxWithFrames(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
            RefreshChartFromFrames();
            UpdateCatalogPathDisplay();

            if (_session.CurrentFrames.Count > 0)
            {
                _session.CurrentIndex = Math.Max(0, Math.Min(_session.CurrentIndex, _session.CurrentFrames.Count - 1));
                lstDataList.SelectedIndex = _session.CurrentIndex;
                ShowFrame(_session.CurrentIndex);
            }
            else
            {
                ClearPlayback();
            }

            lstDataList.Invalidate();
            trbTimeline.Invalidate();
        }

        private async void BtnDeleteSelection_Click(object sender, EventArgs e)
        {
            var r = _selectionManager.GetRange();
            if (r.s < 0)
            {
                ShowAppMessage(
                    "삭제할 구간이 없습니다.\n\n" +
                    "타임라인에서 Ctrl+드래그하거나 [ ] 키로 구간을 설정해 주세요.",
                    "선택구간 삭제", icon: MessageBoxIcon.Information);
                return;
            }

            int s = r.s;
            int eidx = r.e >= 0 ? r.e : r.s;
            if (!ConfirmDelete(s, eidx)) return;

            var savePath = _session.CurrentCatalogPath;
            int removed = DeleteRange(s, eidx, persistAfter: false);
            if (removed > 0 && !string.IsNullOrEmpty(savePath))
                await PersistCatalogAfterEditAsync(removed, savePath);

            // persist deleted range info to a JSON file in Data folder
            try
            {
                var deletedMeta = new { Start = s, End = eidx, Time = DateTime.UtcNow };
                var dir = Path.Combine(Environment.CurrentDirectory, "Data", "DeletedRanges");
                Directory.CreateDirectory(dir);
                var file = Path.Combine(dir, $"deleted_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                var txt = JsonSerializer.Serialize(deletedMeta);
                File.WriteAllText(file, txt);
            }
            catch { }
        }

        private async void DeleteSelectedListItems()
        {
            if (lstDataList.SelectedIndices.Count == 0) return;

            var idxs = lstDataList.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            if (!ConfirmDeleteIndices(idxs)) return;

            var savePath = _session.CurrentCatalogPath;
            int removed = 0;
            foreach (var idx in idxs)
                removed += DeleteRange(idx, idx, persistAfter: false);

            if (removed > 0 && !string.IsNullOrEmpty(savePath))
                await PersistCatalogAfterEditAsync(removed, savePath);
        }

        private bool ConfirmDelete(int start, int end)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0)
                return false;

            start = Math.Max(0, Math.Min(start, _session.CurrentFrames.Count - 1));
            end = Math.Max(0, Math.Min(end, _session.CurrentFrames.Count - 1));
            if (end < start) (start, end) = (end, start);

            int count = end - start + 1;
            return ConfirmDeleteCore(count, start, end, null);
        }

        private bool ConfirmDeleteIndices(IReadOnlyList<int> indices)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0)
                return false;
            if (indices.Count == 0) return false;

            if (indices.Count == 1)
                return ConfirmDelete(indices[0], indices[0]);

            var sorted = indices.OrderBy(i => i).ToList();
            int min = sorted[0];
            int max = sorted[^1];
            bool isContiguous = sorted.Count == max - min + 1;
            if (isContiguous)
                return ConfirmDelete(min, max);

            return ConfirmDeleteCore(sorted.Count, min, max,
                $"선택한 {sorted.Count:N0}개 프레임 (비연속)");
        }

        private bool ConfirmDeleteCore(int count, int start, int end, string? headline)
        {
            int remaining = Math.Max(0, _session.CurrentFrames.Count - count);
            var fileName = string.IsNullOrEmpty(_session.CurrentCatalogPath)
                ? "(열린 파일 없음)"
                : Path.GetFileName(_session.CurrentCatalogPath);

            string target = headline ?? (count == 1
                ? $"프레임 #{start}"
                : $"구간 {start}~{end} ({count:N0}프레임)");

            var message =
                $"{target}을(를) 삭제합니다.\n\n" +
                $"파일: {fileName}\n" +
                $"현재 {_session.CurrentFrames.Count:N0}프레임 → 삭제 후 {remaining:N0}프레임\n\n" +
                "삭제 후 파일에 저장되며, backups/ 폴더에 백업이 생성됩니다.\n" +
                "복구 버튼으로 직전 상태를 되돌릴 수 있습니다.";

            return ShowAppMessage(message, "프레임 삭제",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        /// <returns>삭제된 프레임 수 (실패 시 0)</returns>
        private int DeleteRange(int start, int end, bool persistAfter = true)
        {
            var savePath = _session.CurrentCatalogPath;
            var result = _session.DeleteRange(start, end);
            if (result == null) return 0;

            try { _playCts?.Cancel(); } catch { }

            _imageController?.DeleteRangeIndices(result.Start, result.Count);
            _selectionManager.OnFramesRemoved(result.Start, result.Count);
            _catalogManager.PopulateListBoxWithFrames(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
            _chartController.RemoveRange(result.Start, result.Count);

            if (_session.CurrentFrames.Count == 0)
                ClearPlayback();
            else
                ShowFrame(_session.CurrentIndex);

            trbTimeline.Invalidate();
            lstDataList.Invalidate();
            if (!_selectionManager.HasSelection && _session.CurrentFrames.Count > 0)
                toolStripStatusLabel1.Text = $"{_session.CurrentFrames.Count:N0} 프레임";

            EnsureFormVisible();

            if (persistAfter && !string.IsNullOrEmpty(savePath))
                _ = PersistCatalogAfterEditAsync(result.Count, savePath);

            return result.Count;
        }

        private async Task PersistCatalogAfterEditAsync(int removedCount, string? catalogPath = null)
        {
            catalogPath ??= _session.CurrentCatalogPath;
            if (string.IsNullOrEmpty(catalogPath))
            {
                if (_session.CurrentFrames.Count == 0)
                    toolStripStatusLabel1.Text = "삭제됨 (저장할 파일 없음)";
                return;
            }

            SetUiBusy(true);
            try
            {
                toolStripStatusLabel1.Text = "삭제 내용 저장 중…";

                string backupPath = string.Empty;
                try { backupPath = CatalogPaths.CreateTimestampedBackup(catalogPath); } catch { }

                var ok = await _editorService.SaveCatalogAsync(catalogPath, _session.CurrentFrames);
                if (!string.IsNullOrEmpty(_session.CurrentCatalogPath))
                    UpdateCatalogPathDisplay();

                if (ok)
                {
                    toolStripStatusLabel1.Text =
                        $"삭제 저장: {_session.CurrentFrames.Count:N0} 프레임 ({removedCount:N0}개 제거)";
                }
                else
                {
                    toolStripStatusLabel1.Text = "저장 실패";
                    ShowAppMessage(
                        "삭제는 적용됐지만 카탈로그 파일을 저장하지 못했습니다.\n" +
                        "새로고침하면 디스크 내용으로 되돌아갈 수 있습니다.",
                        "저장", icon: MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PersistCatalogAfterEdit error: {ex.Message}");
                toolStripStatusLabel1.Text = "저장 실패";
                ShowAppMessage($"저장 중 오류가 발생했습니다.\n{ex.Message}", "저장",
                    icon: MessageBoxIcon.Error);
            }
            finally
            {
                SetUiBusy(false);
            }
        }

        private void PicVideoScreen_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (_session.CurrentFrames.Count == 0) return;
                if (_session.CurrentIndex < 0 || _session.CurrentIndex >= _session.CurrentFrames.Count) return;
                FrameOverlayRenderer.Draw(e.Graphics, _session.CurrentFrames[_session.CurrentIndex], picVideoScreen.ClientSize);
            }
            catch { }
        }

        // 데이터 선택: 작업용·백업 카탈로그 파일을 직접 선택
        private async void BtnSelectData_Click(object sender, EventArgs e)
        {
            var initialDir = Path.Combine(Environment.CurrentDirectory, "Data", "TestData");
            if (!string.IsNullOrEmpty(_session.CurrentCatalogPath))
            {
                var parent = Path.GetDirectoryName(_session.CurrentCatalogPath);
                if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                    initialDir = parent;
            }

            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "카탈로그 파일 선택";
                dlg.InitialDirectory = initialDir;
                dlg.Filter =
                    "작업용 카탈로그 (*.catalog)|*.catalog|" +
                    "백업 (*.catalog.bak)|*.catalog.bak|" +
                    "backups 폴더 내 카탈로그 (*.catalog)|*.catalog|" +
                    "모든 파일 (*.*)|*.*";
                dlg.FilterIndex = 1;

                if (dlg.ShowDialog(this) != DialogResult.OK)
                    return;

                var path = dlg.FileName;
                if (!CatalogPaths.IsWorkingCatalog(path) && !CatalogPaths.IsBackupCatalog(path))
                {
                    MessageBox.Show(this,
                        "`.catalog` 또는 `.catalog.bak` 파일만 열 수 있습니다.",
                        "데이터 선택", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await LoadAndShowCatalogFileAsync(path);
            }
        }

        private void StopPlayback()
        {
            try { _playCts?.Cancel(); } catch { }
            _playCts = null;
            btnPlayPause.Text = "재생 / 정지";
        }

        private void ResetAllUi()
        {
            StopPlayback();
            _session.Reset();
            _selectionManager.Clear();
            ClearImageCache();
            lstDataList.Items.Clear();
            _playbackController.ClearDisplay();
            _chartController.RefreshFromFrames(_session.CurrentFrames);
            txtFilePath.Text = string.Empty;
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
            picVideoScreen.Invalidate();
            toolStripStatusLabel1.Text = "초기화됨";
        }

        // 새로고침: 열린 카탈로그를 디스크에서 다시 불러옴 (없으면 전체 초기화)
        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            StopPlayback();

            var path = _session.CurrentCatalogPath;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                _session.ClearUndo();
                await LoadAndShowCatalogFileAsync(path);
                var label = CatalogPaths.GetDisplayLabel(path);
                toolStripStatusLabel1.Text =
                    $"{label} {Path.GetFileName(path)} — {_session.CurrentFrames.Count:N0} 프레임 새로고침 완료";
                return;
            }

            ResetAllUi();
        }

        private void UpdateCatalogPathDisplay()
        {
            if (string.IsNullOrEmpty(_session.CurrentCatalogPath))
            {
                txtFilePath.Text = string.Empty;
                return;
            }

            var label = CatalogPaths.GetDisplayLabel(_session.CurrentCatalogPath);
            var name = Path.GetFileName(_session.CurrentCatalogPath);
            var count = _session.CurrentFrames?.Count ?? 0;
            txtFilePath.Text = $"{label} {name}  ({count:N0} 프레임)  —  {_session.CurrentCatalogPath}";
        }

        // 지정한 카탈로그 파일 하나를 로드해 표시
        private async Task LoadAndShowCatalogFileAsync(string catalogFilePath)
        {
            btnSelectData.Enabled = false;
            toolStripStatusLabel1.Text = "카탈로그 불러오는 중…";
            try
            {
                try { _playCts?.Cancel(); } catch { }

                _session.ClearUndo();
                ClearImageCache();
                _selectionManager.Clear();
                _chartController.ResetHighlight();

                var frames = await _catalogManager.LoadCatalogFileAsync(catalogFilePath);
                if (frames == null || frames.Count == 0)
                {
                    ClearPlayback();
                    toolStripStatusLabel1.Text = "카탈로그를 불러오지 못했습니다.";
                    MessageBox.Show(this, "카탈로그가 비어 있거나 읽을 수 없습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _session.CurrentCatalogPath = catalogFilePath;
                _session.Catalogs.Clear();
                _session.Catalogs[catalogFilePath] = frames;
                _session.CurrentFrames = frames;
                _session.FrameImagePaths = _catalogManager.ResolveFrameImagePaths(
                    _session.CurrentCatalogPath, _session.CurrentFrames);

                _catalogManager.PopulateListBoxWithFrames(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
                RefreshChartFromFrames();
                UpdateCatalogPathDisplay();

                // 리스트 이벤트/동일 인덱스(0)에도 첫 프레임을 반드시 표시
                _session.CurrentIndex = -1;
                ShowFrame(0);
                if (lstDataList.SelectedIndex != 0)
                    lstDataList.SelectedIndex = 0;

                lstDataList.Invalidate();
                trbTimeline.Invalidate();
                picVideoScreen.Invalidate();

                var label = CatalogPaths.GetDisplayLabel(catalogFilePath);
                toolStripStatusLabel1.Text =
                    $"{label} {Path.GetFileName(catalogFilePath)} — {frames.Count:N0} 프레임 로드 완료";

                // 첫 화면 표시 후 나머지 프레임 프리로드
                await _catalogManager.PreloadImagesAsync(_session.FrameImagePaths, _session.CurrentFrames, 5, (path, idx) =>
                {
                    try
                    {
                        var target = picVideoScreen.ClientSize;
                        if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                        var composed = _imageController.LoadAndCompose(path, _session.CurrentFrames[idx], target);
                        return composed;
                    }
                    catch { return null; }
                }, _imageController.AddToCache);
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
            // TrackBar/코드에 의해 SelectedIndex가 동기화될 때 중복 ShowFrame 호출을 막습니다.
            if (idx == _session.CurrentIndex) return;
            ShowFrame(idx);
        }

        // 타임라인 이동 시 프레임 표시
        private void TrbTimeline_Scroll(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            ShowFrame(trbTimeline.Value);
            if (lstDataList.SelectedIndex != _session.CurrentIndex)
                lstDataList.SelectedIndex = _session.CurrentIndex;
        }

        private void BtnNextFrame_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            ShowFrame(Math.Min(_session.CurrentFrames.Count - 1, _session.CurrentIndex + 1));
            lstDataList.SelectedIndex = _session.CurrentIndex;
        }

        private void BtnPrevFrame_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            ShowFrame(Math.Max(0, _session.CurrentIndex - 1));
            lstDataList.SelectedIndex = _session.CurrentIndex;
        }

        // 재생/정지 버튼
        private async void BtnPlayPause_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;

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
            if (_session.CurrentFrames != null && _session.CurrentFrames.Count > 0 && _session.CurrentIndex >= _session.CurrentFrames.Count - 1)
            {
                _session.CurrentIndex = -1;
            }

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int next = _session.CurrentIndex + 1;
                    if (next >= _session.CurrentFrames.Count) break;
                    ShowFrame(next);
                    lstDataList.SelectedIndex = _session.CurrentIndex;
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

        private void ClearPlayback()
        {
            _session.CurrentFrames = new List<Frame>();
            _session.CurrentCatalogPath = string.Empty;
            _session.CurrentIndex = 0;
            _selectionManager.Clear();
            lstDataList.Items.Clear();
            _playbackController.ClearDisplay();
            _chartController.RefreshFromFrames(_session.CurrentFrames);
            ClearImageCache();
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
        }

        private void btnTrainModel_Click(object sender, EventArgs e)
        {
            Form2 trainForm = new Form2();
            trainForm.StartPosition = FormStartPosition.Manual;
            trainForm.Location = this.Location;
            trainForm.Size = this.Size;

            // 핵심: Form2가 닫힐 때 프로그램 전체를 끄는 게 아니라, 숨겨져 있던 Form1을 다시 보여줍니다.
            trainForm.FormClosed += (s, args) => {
                if (Application.OpenForms["Form1"] is Form1 mainForm)
                {
                    mainForm.Location = trainForm.Location; // Form2의 위치를 다시 Form1에 동기화
                    mainForm.Show();
                }
            };

            trainForm.Show();
            this.Hide();
        }
    }
}
