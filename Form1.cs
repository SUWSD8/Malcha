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
        // Range marks for deletion using trackbar
        private int _rangeMarkStart = -1;
        private int _rangeMarkEnd = -1;

        // Context menu for list deletion
        private ContextMenuStrip _listContextMenu;
        private ContextMenuStrip _trackContextMenu;
        private CatalogManager _catalogManager;
        private ImageController _imageController;
        // undo stack for recovery
        private Stack<UndoSnapshot> _undoStack = new Stack<UndoSnapshot>();
        private SelectionManager _selectionManager = new SelectionManager();

        private class UndoSnapshot
        {
            public List<Frame> Frames { get; set; }
            public List<string> ImagePaths { get; set; }
            public int CurrentIndex { get; set; }
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
            if (string.IsNullOrEmpty(_currentCatalogPath))
            {
                MessageBox.Show(this, "먼저 카탈로그 파일을 열어 주세요.", "복구",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var workingPath = CatalogPaths.ResolveWorkingCatalogPath(_currentCatalogPath);
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
                var currentFull = Path.GetFullPath(_currentCatalogPath);
                var workingFull = Path.GetFullPath(workingPath);
                if (string.Equals(currentFull, workingFull, StringComparison.OrdinalIgnoreCase))
                    refinedFrames = _currentFrames.ToList();
                else if (File.Exists(workingPath))
                    refinedFrames = await _catalogManager.LoadCatalogFileAsync(workingPath);
                else
                    refinedFrames = _currentFrames.ToList();
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

                _currentCatalogPath = workingPath;
                _currentFrames = mergeResult.Frames;
                _catalogs[workingPath] = _currentFrames;

                await DataManager.Instance.SaveFramesAsync(workingPath, _currentFrames);

                ClearImageCache();
                _selectionManager.Clear();
                _lastChartIndex = -1;
                _frameImagePaths = _catalogManager.ResolveFrameImagePaths(workingPath, _currentFrames);
                RefreshFrameListUi();
                if (_currentFrames.Count > 0)
                    ShowFrame(Math.Min(_currentIndex, _currentFrames.Count - 1));

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
            if (_undoStack.Count == 0) return false;
            var snap = _undoStack.Pop();
            try
            {
                _currentFrames = snap.Frames ?? new List<Frame>();
                _frameImagePaths = snap.ImagePaths ?? new List<string>();
                _currentIndex = Math.Max(0, Math.Min(_currentFrames.Count - 1, snap.CurrentIndex));

                ClearImageCache();
                _catalogManager.PopulateListBoxWithFrames(lstDataList, _currentFrames, _frameImagePaths);
                UpdateCatalogPathDisplay();
                if (_currentFrames.Count > 0)
                    ShowFrame(_currentIndex);

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
            // Image controller handles image load/compose/cache
            _imageController = new ImageController();

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
            if (_currentFrames == null || _currentFrames.Count == 0) return;

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
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            _selectionManager.SetStart(_currentIndex);
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
        }

        private void BtnSetEndPoint_Click(object sender, EventArgs e)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            _selectionManager.SetEnd(_currentIndex);
            trbTimeline.Invalidate();
            lstDataList.Invalidate();
        }

        private async void BtnApplyFilter_Click(object sender, EventArgs e)
        {
            if (_currentFrames == null || _currentFrames.Count == 0)
            {
                MessageBox.Show(this, "정제할 카탈로그 데이터가 없습니다.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(this,
                $"현재 {_currentFrames.Count:N0}개 프레임을 자동 정제합니다.\n\n" +
                "· 연속된 동일 조향/쓰로틀 값(중복) 제거\n" +
                "· 급격히 튀었다가 되돌아오는 비정상 값 제거\n" +
                "· 허용 범위를 벗어난 값 제거\n\n" +
                "계속하시겠습니까?",
                "필터 적용",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
                return;

            if (CatalogPaths.IsBackupCatalog(_currentCatalogPath) || CatalogPaths.IsUnderBackupsFolder(_currentCatalogPath))
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
                var framesCopy = _currentFrames.ToList();
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

                _currentFrames = refineResult.Frames;
                if (!string.IsNullOrEmpty(_currentCatalogPath))
                    _catalogs[_currentCatalogPath] = _currentFrames;

                _frameImagePaths = _catalogManager.ResolveFrameImagePaths(_currentCatalogPath, _currentFrames);
                ClearImageCache();
                _selectionManager.Clear();
                _lastChartIndex = -1;
                RefreshFrameListUi();

                string backupPath = string.Empty;
                if (!string.IsNullOrEmpty(_currentCatalogPath) && File.Exists(_currentCatalogPath))
                {
                    progress.Report(98, "카탈로그 저장 중…");
                    try { backupPath = CatalogPaths.CreateTimestampedBackup(_currentCatalogPath); } catch { }
                    await DataManager.Instance.SaveFramesAsync(_currentCatalogPath, _currentFrames);
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

        private void PushUndoSnapshot()
        {
            try
            {
                _undoStack.Push(new UndoSnapshot
                {
                    Frames = _currentFrames.Select(f => f).ToList(),
                    ImagePaths = _frameImagePaths != null ? new List<string>(_frameImagePaths) : new List<string>(),
                    CurrentIndex = _currentIndex
                });
            }
            catch { }
        }

        private void RefreshFrameListUi()
        {
            _catalogManager.PopulateListBoxWithFrames(lstDataList, _currentFrames, _frameImagePaths);
            RefreshChartFromFrames();
            UpdateCatalogPathDisplay();

            if (_currentFrames.Count > 0)
            {
                _currentIndex = Math.Max(0, Math.Min(_currentIndex, _currentFrames.Count - 1));
                lstDataList.SelectedIndex = _currentIndex;
                ShowFrame(_currentIndex);
            }
            else
            {
                ClearPlayback();
            }

            lstDataList.Invalidate();
            trbTimeline.Invalidate();
        }

        private void RefreshChartFromFrames()
        {
            try
            {
                chtDataGraph.Series["user/angle"].Points.Clear();
                chtDataGraph.Series["user/throttle"].Points.Clear();

                for (int i = 0; i < _currentFrames.Count; i++)
                {
                    var f = _currentFrames[i];
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

        private void BtnDeleteSelection_Click(object sender, EventArgs e)
        {
            var r = _selectionManager.GetRange();
            if (r.s < 0) return;
            int s = r.s;
            int eidx = r.e >= 0 ? r.e : r.s;
            PushUndoSnapshot();
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

        // Delete a range of frames inclusive [start, end]
        private void DeleteRange(int start, int end)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            // save undo snapshot before modifying
            try
            {
                var snap = new UndoSnapshot
                {
                    Frames = _currentFrames.Select(f => f).ToList(),
                    ImagePaths = _frameImagePaths != null ? new List<string>(_frameImagePaths) : new List<string>(),
                    CurrentIndex = _currentIndex
                };
                _undoStack.Push(snap);
            }
            catch { }
            start = Math.Max(0, Math.Min(start, _currentFrames.Count - 1));
            end = Math.Max(0, Math.Min(end, _currentFrames.Count - 1));
            if (end < start) { var t = start; start = end; end = t; }

            int count = end - start + 1;
            // Stop playback
            try { _playCts?.Cancel(); } catch { }

            // Update image cache via controller (dispose and shift indices)
            _imageController?.DeleteRangeIndices(start, count);

            // Remove frames and paths
            _currentFrames.RemoveRange(start, count);
            if (_frameImagePaths != null && _frameImagePaths.Count >= start + count)
            {
                _frameImagePaths.RemoveRange(start, count);
            }

            // Update listbox
            _catalogManager.PopulateListBoxWithFrames(lstDataList, _currentFrames, _frameImagePaths);

            // Update chart series
            try
            {
                var seriesAngle = chtDataGraph.Series["user/angle"];
                var seriesThrottle = chtDataGraph.Series["user/throttle"];
                // remove points in range and shift others
                for (int i = 0; i < count; i++)
                {
                    if (seriesAngle.Points.Count > start) seriesAngle.Points.RemoveAt(start);
                    if (seriesThrottle.Points.Count > start) seriesThrottle.Points.RemoveAt(start);
                }
            }
            catch { }

            // fix current index
            _currentIndex = Math.Max(0, Math.Min(_currentFrames.Count - 1, start));
            if (_currentFrames.Count == 0) ClearPlayback();
            else ShowFrame(_currentIndex);
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

        // 데이터 선택: 작업용·백업 카탈로그 파일을 직접 선택
        private async void BtnSelectData_Click(object sender, EventArgs e)
        {
            var initialDir = Path.Combine(Environment.CurrentDirectory, "Data", "TestData");
            if (!string.IsNullOrEmpty(_currentCatalogPath))
            {
                var parent = Path.GetDirectoryName(_currentCatalogPath);
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
            _catalogs.Clear();
            _currentFrames.Clear();
            _frameImagePaths.Clear();

            // UI 초기화
            lstDataList.Items.Clear();
            ClearPlayback();
            txtFilePath.Text = string.Empty;

            await Task.CompletedTask;
        }

        private void UpdateCatalogPathDisplay()
        {
            if (string.IsNullOrEmpty(_currentCatalogPath))
            {
                txtFilePath.Text = string.Empty;
                return;
            }

            var label = CatalogPaths.GetDisplayLabel(_currentCatalogPath);
            var name = Path.GetFileName(_currentCatalogPath);
            var count = _currentFrames?.Count ?? 0;
            txtFilePath.Text = $"{label} {name}  ({count:N0} 프레임)  —  {_currentCatalogPath}";
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

                _currentCatalogPath = catalogFilePath;
                _catalogs = new Dictionary<string, List<Frame>> { [catalogFilePath] = frames };
                _currentFrames = frames;

                _frameImagePaths = _catalogManager.ResolveFrameImagePaths(_currentCatalogPath, _currentFrames);
                _catalogManager.PopulateListBoxWithFrames(lstDataList, _currentFrames, _frameImagePaths);

                if (lstDataList.Items.Count > 0)
                    lstDataList.SelectedIndex = 0;
                else
                    ClearPlayback();

                await _catalogManager.PreloadImagesAsync(_frameImagePaths, _currentFrames, 5, (path, idx) =>
                {
                    try
                    {
                        var target = picVideoScreen.ClientSize;
                        if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                        var composed = _imageController.LoadAndCompose(path, _currentFrames[idx], target);
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
            ShowFrame(idx);
        }

        // 타임라인 이동 시 프레임 표시
        private void TrbTimeline_Scroll(object sender, EventArgs e)
        {
            if (_currentFrames == null || _currentFrames.Count == 0) return;
            ShowFrame(trbTimeline.Value);
            if (lstDataList.SelectedIndex != _currentIndex)
                lstDataList.SelectedIndex = _currentIndex;
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
                        if (_imageController.TryGet(_currentIndex, out var cachedImg))
                        {
                            img = cachedImg;
                        }

                        if (img == null)
                        {
                            var target = picVideoScreen.ClientSize;
                            if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                            try
                            {
                                var composed = _imageController.LoadAndCompose(resolved, f, target);
                                img = composed;
                                _imageController.AddToCache(_currentIndex, img);
                            }
                            catch { }
                        }
                    }

                    // 기본 이미지는 캐시에 보관된 스케일된 이미지(또는 방금 생성한 이미지)를 사용
                    var old = picVideoScreen.Image;
                    picVideoScreen.Image = img;
                    // 이전 이미지가 캐시에 없는 임시 이미지면 Dispose
                    if (old != null)
                    {
                        bool isCached = _imageController != null && _imageController.IsImageCached(old);
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
                                        var target = picVideoScreen.ClientSize;
                                        if (target.Width <= 0 || target.Height <= 0) target = new Size(320, 240);
                                        var composedNext = _imageController.LoadAndCompose(nextPath, _currentFrames[next], target);
                                        if (composedNext != null) _imageController.AddToCache(next, composedNext);
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

        // 캐시 관리: delegate to ImageController
        private void ClearImageCache()
        {
            _imageController?.Clear();
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
