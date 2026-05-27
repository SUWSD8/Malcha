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
            Enabled = false;
            try
            {
                progress = new ProgressDialog("백업 병합");
                progress.Show(this);
                progress.Refresh();

                List<Frame> backupFrames;
                try
                {
                    progress.Report(10, "백업 파일 읽는 중…");
                    backupFrames = await _catalogManager.LoadCatalogFileAsync(backupPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"백업을 읽지 못했습니다.\n{ex.Message}", "복구",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                MessageBox.Show(this,
                    $"병합이 완료되었습니다.\n\n" +
                    $"백업 원본: {backupFrames.Count:N0} 프레임\n" +
                    $"정제 파일: {refinedFrames.Count:N0} 프레임\n" +
                    $"결과: {mergeResult.Frames.Count:N0} 프레임\n" +
                    $"정제본 우선 적용: {mergeResult.RefinedOverrides:N0}\n" +
                    $"정제에만 있던 추가: {mergeResult.FromRefinedOnly:N0}\n\n" +
                    $"저장: {workingPath}",
                    "복구", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(this, "병합이 취소되었습니다.", "복구",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Recover merge error: {ex.Message}");
                MessageBox.Show(this, $"복구 중 오류가 발생했습니다.\n{ex.Message}", "복구",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progress?.Close();
                progress?.Dispose();
                Enabled = true;
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
            _listContextMenu.Items.Add("Delete Selected", null, (s, e) => DeleteSelectedListItems());
            lstDataList.ContextMenuStrip = _listContextMenu;
            lstDataList.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) DeleteSelectedListItems(); };
            // owner-draw to show range highlights
            lstDataList.DrawMode = DrawMode.OwnerDrawFixed;
            lstDataList.DrawItem += LstDataList_DrawItem;

            // TrackBar: Ctrl+클릭=구간 표시, Shift+클릭=구간 해제 (일반 드래그는 기본 동작)
            trbTimeline.MouseDown += TrbTimeline_MouseDown;
            // track context menu for deleting marked range
            _trackContextMenu = new ContextMenuStrip();
            _trackContextMenu.Items.Add("Delete Marked Range", null, (s, e) =>
            {
                var r = _selectionManager.GetRange();
                if (r.s >= 0 && r.e >= 0) DeleteRange(r.s, r.e);
                else if (r.s >= 0) DeleteRange(r.s, r.s);
            });
            trbTimeline.ContextMenuStrip = _trackContextMenu;
            trbTimeline.Paint += TrbTimeline_Paint;

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

            // PictureBox에 별도의 오버레이를 가볍게 그리기 위해 Paint 이벤트 연결
            picVideoScreen.Paint += PicVideoScreen_Paint;

            trbTimeline.Enabled = false;

            // PictureBox가 이미지 크기와 달라도 채워지도록 설정
            // (StretchImage: PictureBox 전체를 채우도록 이미지를 늘리거나 줄입니다)
            picVideoScreen.SizeMode = PictureBoxSizeMode.StretchImage;

        }

        private void TrbTimeline_MouseDown(object sender, MouseEventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;

            bool ctrl = (ModifierKeys & Keys.Control) == Keys.Control;
            bool shift = (ModifierKeys & Keys.Shift) == Keys.Shift;
            if (!ctrl && !shift)
                return;

            var tb = (TrackBar)sender;
            int trackWidth = Math.Max(1, tb.ClientSize.Width - 8);
            float ratio = Math.Max(0f, Math.Min(1f, (float)e.X / trackWidth));
            int value = (int)Math.Round(ratio * (tb.Maximum - tb.Minimum)) + tb.Minimum;
            value = Math.Max(tb.Minimum, Math.Min(tb.Maximum, value));

            if (ctrl)
            {
                if (_selectionManager.Start < 0)
                    _selectionManager.SetStart(value);
                else
                    _selectionManager.SetEnd(value);
            }
            else if (shift)
            {
                _selectionManager.Clear();
            }

            trbTimeline.Invalidate();
            lstDataList.Invalidate();
        }

        private void BtnSetStartPoint_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            _selectionManager.SetStart(_session.CurrentIndex);
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
        }

        private void BtnSetEndPoint_Click(object sender, EventArgs e)
        {
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0) return;
            _selectionManager.SetEnd(_session.CurrentIndex);
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
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
            Enabled = false;

            try
            {
                var framesCopy = _session.CurrentFrames.ToList();
                progress = new ProgressDialog("데이터 정제");
                progress.Show(this);
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
                    MessageBox.Show(this, "정제가 취소되었습니다.", "필터 적용",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                if (refineResult.Frames.Count == 0)
                {
                    MessageBox.Show(this, "정제 후 남은 프레임이 없습니다. 기준을 완화하거나 원본을 복구해 주세요.",
                        "필터 적용", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                MessageBox.Show(this,
                    $"정제가 완료되었습니다.\n\n" +
                    $"원본: {refineResult.OriginalCount:N0} 프레임\n" +
                    $"결과: {refineResult.Frames.Count:N0} 프레임\n" +
                    $"제거: {refineResult.RemovedTotal:N0} (중복 {refineResult.RemovedDuplicate:N0}, " +
                    $"스파이크 {refineResult.RemovedSpike:N0}, 범위초과 {refineResult.RemovedOutOfRange:N0})" +
                    backupNote,
                    "필터 적용",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Filter apply error: {ex.Message}");
                MessageBox.Show(this, $"정제 중 오류가 발생했습니다.\n{ex.Message}", "필터 적용",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progress?.Close();
                progress?.Dispose();
                Enabled = true;
            }
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

        private void BtnDeleteSelection_Click(object sender, EventArgs e)
        {
            var r = _selectionManager.GetRange();
            if (r.s < 0) return;
            int s = r.s;
            int eidx = r.e >= 0 ? r.e : r.s;
            DeleteRange(s, eidx);

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

        private void DeleteSelectedListItems()
        {
            if (lstDataList.SelectedIndices.Count == 0) return;

            // collect indices to remove (descending order so removal indices remain valid)
            var idxs = lstDataList.SelectedIndices.Cast<int>().OrderByDescending(i => i).ToList();
            foreach (var idx in idxs)
            {
                DeleteRange(idx, idx);
            }
        }

        private void DeleteRange(int start, int end)
        {
            var result = _session.DeleteRange(start, end);
            if (result == null) return;

            try { _playCts?.Cancel(); } catch { }

            _imageController?.DeleteRangeIndices(result.Start, result.Count);
            _catalogManager.PopulateListBoxWithFrames(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);
            _chartController.RemoveRange(result.Start, result.Count);

            if (_session.CurrentFrames.Count == 0)
                ClearPlayback();
            else
                ShowFrame(_session.CurrentIndex);
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
            _session.Catalogs.Clear();
            _session.CurrentFrames.Clear();
            _session.FrameImagePaths.Clear();

            // UI 초기화
            lstDataList.Items.Clear();
            ClearPlayback();
            txtFilePath.Text = string.Empty;

            await Task.CompletedTask;
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
            try
            {
                ClearImageCache();

                var frames = await _catalogManager.LoadCatalogFileAsync(catalogFilePath);
                if (frames == null || frames.Count == 0)
                {
                    ClearPlayback();
                    MessageBox.Show(this, "카탈로그가 비어 있거나 읽을 수 없습니다.", "알림",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _session.CurrentCatalogPath = catalogFilePath;
                _session.Catalogs.Clear();
                _session.Catalogs[catalogFilePath] = frames;
                _session.CurrentFrames = frames;

                _session.FrameImagePaths = _catalogManager.ResolveFrameImagePaths(_session.CurrentCatalogPath, _session.CurrentFrames);
                _catalogManager.PopulateListBoxWithFrames(lstDataList, _session.CurrentFrames, _session.FrameImagePaths);

                if (lstDataList.Items.Count > 0)
                    lstDataList.SelectedIndex = 0;
                else
                    ClearPlayback();

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

                RefreshChartFromFrames();
                UpdateCatalogPathDisplay();

                if (CatalogPaths.IsBackupCatalog(catalogFilePath) || CatalogPaths.IsUnderBackupsFolder(catalogFilePath))
                {
                    toolStripStatusLabel1.Text = "백업 카탈로그를 열었습니다. 정제 저장 시 이 파일이 덮어씌워집니다.";
                }
                else
                {
                    toolStripStatusLabel1.Text = "작업용 카탈로그를 열었습니다.";
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
            lstDataList.Items.Clear();
            _playbackController.ClearDisplay();
            _chartController.ResetHighlight();
            ClearImageCache();
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
