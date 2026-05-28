using Malcha.Data;
using Malcha.Model;
using Malcha.Service;
using Malcha.UI;
using Malcha.View;
using System.Text.Json;
using static Malcha.UI.TimelineSelectionBinder;

namespace Malcha.Controller
{
    // [Controller] 카탈로그 편집 흐름 — View ↔ Service 중재
    internal class CatalogEditorController
    {
        private readonly ICatalogView _view;
        private readonly CatalogSession _session;
        private readonly CatalogService _catalog = CatalogService.Instance;
        private readonly WslDataSyncService _dataSync = WslDataSyncService.Instance;
        private readonly WslTrainingService _wslTraining = WslTrainingService.Instance;
        private readonly FrameRangeSelection _selection;

        public CatalogEditorController(ICatalogView view, CatalogSession session, FrameRangeSelection selection)
        {
            _view = view;
            _session = session;
            _selection = selection;
        }

        // 파일 선택 대화상자로 카탈로그 열기
        public async Task HandleSelectDataAsync()
        {
            var initialDir = Path.Combine(Environment.CurrentDirectory, "Data", "TestData");
            if (!string.IsNullOrEmpty(_session.CurrentCatalogPath))
            {
                var parent = Path.GetDirectoryName(_session.CurrentCatalogPath);
                if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent)) initialDir = parent;
            }

            using var dlg = new OpenFileDialog
            {
                Title = "카탈로그 파일 선택",
                InitialDirectory = initialDir,
                Filter = "작업용 (*.catalog)|*.catalog|백업 (*.catalog.bak)|*.catalog.bak|모든 파일 (*.*)|*.*"
            };
            if (dlg.ShowDialog(_view.Owner) != DialogResult.OK) return;
            if (!CatalogPaths.IsWorkingCatalog(dlg.FileName) && !CatalogPaths.IsBackupCatalog(dlg.FileName))
            {
                _view.ShowMessage("`.catalog` 또는 `.catalog.bak`만 열 수 있습니다.", "데이터 선택", icon: MessageBoxIcon.Warning);
                return;
            }
            await LoadCatalogFileAsync(dlg.FileName);
        }

        // 현재 카탈로그 다시 로드 (없으면 UI 전체 초기화)
        public async Task HandleRefreshAsync()
        {
            _view.RequestStopPlayback();
            if (!string.IsNullOrEmpty(_session.CurrentCatalogPath) && File.Exists(_session.CurrentCatalogPath))
            {
                _session.ClearUndo();
                await LoadCatalogFileAsync(_session.CurrentCatalogPath);
                return;
            }
            _view.RequestResetAllUi();
        }

        // 카탈로그 파일 로드 후 세션·View 갱신
        public async Task LoadCatalogFileAsync(string catalogFilePath)
        {
            _view.SetCatalogBusy(true);
            _view.SetStatusText("카탈로그 불러오는 중…");
            try
            {
                _view.RequestStopPlayback();
                _session.ClearUndo();
                _view.RequestClearImageCache();
                _selection.Clear();

                var frames = await _catalog.LoadCatalogFileAsync(catalogFilePath);
                if (frames == null || frames.Count == 0)
                {
                    _view.RequestClearPlayback();
                    _view.ShowMessage("카탈로그가 비어 있거나 읽을 수 없습니다.", "알림");
                    return;
                }

                _session.CurrentCatalogPath = catalogFilePath;
                _session.Catalogs.Clear();
                _session.Catalogs[catalogFilePath] = frames;
                _session.CurrentFrames = frames;
                _session.FrameImagePaths = _catalog.ResolveFrameImagePaths(catalogFilePath, frames);
                _session.CurrentIndex = -1;
                await _view.CompleteCatalogLoadAsync();
                _view.UpdateCatalogPathFromSession();
            }
            finally { _view.SetCatalogBusy(false); }
        }

        // 정제된 카탈로그·이미지를 WSL data(tub)로 연동
        public async Task HandleSyncTrainingDataAsync()
        {
            if (_session.CurrentFrames.Count == 0)
            {
                _view.ShowMessage("연동할 카탈로그를 먼저 열어 주세요.", "정제 데이터 연동", icon: MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!_wslTraining.TryConfigure(() => MycarFolderDialog.Show(_view.Owner, null)))
                    return;
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "mycar 경로", icon: MessageBoxIcon.Error);
                return;
            }

            _view.SetCatalogBusy(true);
            _view.SetStatusText("WSL data 연동 중…");
            ProgressDialog? progress = null;
            try
            {
                progress = _view.ShowProgress("정제 데이터 연동");
                var uiProgress = new Progress<(int percent, string message)>(r =>
                    progress.Report(r.percent, r.message));

                var result = await _dataSync.SyncAsync(
                    _session.CurrentFrames,
                    _session.FrameImagePaths,
                    _session.CurrentCatalogPath,
                    uiProgress,
                    progress.Token);

                _view.SetStatusText($"연동 완료 — {result.FrameCount:N0} 프레임, {result.ImageCount:N0} 이미지");
                _view.ShowMessage(
                    $"WSL data 연동 완료\n\n프레임: {result.FrameCount:N0}\n이미지: {result.ImageCount:N0}\n경로: {result.TargetPath}",
                    "정제 데이터 연동");
            }
            catch (OperationCanceledException)
            {
                _view.ShowMessage("연동이 취소되었습니다.", "정제 데이터 연동");
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"연동 오류: {ex.Message}", "정제 데이터 연동", icon: MessageBoxIcon.Error);
            }
            finally
            {
                _view.CloseProgress(progress);
                _view.SetCatalogBusy(false);
                _view.EnsureVisible();
            }
        }

        // FrameRefinementFilter로 현재 프레임 정제 후 저장
        public async Task HandleApplyFilterAsync()
        {
            if (_session.CurrentFrames.Count == 0) { _view.ShowMessage("정제할 데이터 없음", "알림"); return; }
            if (_view.ShowMessage($"현재 {_session.CurrentFrames.Count:N0}개 프레임 정제?\n계속?",
                    "필터 적용", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            _session.PushUndo();
            _view.RequestStopPlayback();
            _view.SetCatalogBusy(true);
            ProgressDialog? progress = null;
            try
            {
                progress = _view.ShowProgress("데이터 정제");
                var uiProgress = new Progress<FrameRefinementFilter.ProgressReport>(r => progress.Report(r.Percent, r.Message));
                FrameRefinementFilter.Result refineResult;
                try
                {
                    refineResult = await _catalog.RefineAsync(_session.CurrentFrames.ToList(), uiProgress, progress.Token);
                }
                catch (OperationCanceledException) { _view.ShowMessage("정제 취소", "필터 적용"); return; }

                if (refineResult.Frames.Count == 0)
                {
                    _view.ShowMessage("정제 후 프레임 없음", "필터 적용", icon: MessageBoxIcon.Warning);
                    return;
                }

                _session.CurrentFrames = refineResult.Frames;
                if (!string.IsNullOrEmpty(_session.CurrentCatalogPath))
                    _session.Catalogs[_session.CurrentCatalogPath] = _session.CurrentFrames;
                _session.FrameImagePaths = _catalog.ResolveFrameImagePaths(_session.CurrentCatalogPath, _session.CurrentFrames);
                _view.RequestClearImageCache();
                _selection.Clear();
                _view.ResetChartHighlight();
                _view.RequestRefreshFrameList();

                if (!string.IsNullOrEmpty(_session.CurrentCatalogPath) && File.Exists(_session.CurrentCatalogPath))
                {
                    try { CatalogPaths.CreateTimestampedBackup(_session.CurrentCatalogPath); } catch { }
                    await _catalog.SaveCatalogAsync(_session.CurrentCatalogPath, _session.CurrentFrames);
                }
                _view.ShowMessage($"정제: {refineResult.OriginalCount:N0} → {refineResult.Frames.Count:N0}", "필터 적용");
            }
            catch (Exception ex) { _view.ShowMessage($"정제 오류: {ex.Message}", "필터 적용", icon: MessageBoxIcon.Error); }
            finally { _view.CloseProgress(progress); _view.SetCatalogBusy(false); _view.EnsureVisible(); }
        }

        // 백업 파일과 정제본 병합 후 저장
        public async Task HandleRecoverAsync()
        {
            if (string.IsNullOrEmpty(_session.CurrentCatalogPath)) { _view.ShowMessage("카탈로그를 먼저 열어 주세요.", "복구"); return; }

            var workingPath = CatalogPaths.ResolveWorkingCatalogPath(_session.CurrentCatalogPath);
            var backupPath = CatalogPaths.FindLatestBackupPath(workingPath);
            if (string.IsNullOrEmpty(backupPath) || !File.Exists(backupPath))
            {
                if (TryRecoverFromUndo()) return;
                _view.ShowMessage("백업 없음", "복구"); return;
            }

            List<Frame> refinedFrames;
            try { refinedFrames = await _catalog.LoadRefinedFramesAsync(_session.CurrentCatalogPath, _session.CurrentFrames); }
            catch (Exception ex) { _view.ShowMessage(ex.Message, "복구", icon: MessageBoxIcon.Error); return; }

            if (_view.ShowMessage($"백업 병합: {Path.GetFileName(backupPath)}\n계속?", "복구",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            _session.PushUndo();
            _view.SetCatalogBusy(true);
            ProgressDialog? progress = null;
            try
            {
                progress = _view.ShowProgress("백업 병합");
                var (backupFrames, mergeResult) = await _catalog.MergeWithBackupAsync(workingPath, refinedFrames, progress.Token);
                try { CatalogPaths.CreateTimestampedBackup(workingPath); } catch { }
                _session.CurrentCatalogPath = workingPath;
                _session.CurrentFrames = mergeResult.Frames;
                _session.Catalogs[workingPath] = _session.CurrentFrames;
                await _catalog.SaveCatalogAsync(workingPath, _session.CurrentFrames);
                _view.RequestClearImageCache();
                _selection.Clear();
                _view.ResetChartHighlight();
                _session.FrameImagePaths = _catalog.ResolveFrameImagePaths(workingPath, _session.CurrentFrames);
                _view.RequestRefreshFrameList();
                if (_session.CurrentFrames.Count > 0)
                    _view.RequestShowFrame(Math.Min(_session.CurrentIndex, _session.CurrentFrames.Count - 1));
                _view.ShowMessage($"병합: {mergeResult.Frames.Count:N0} 프레임", "복구");
            }
            catch (OperationCanceledException) { _view.ShowMessage("병합 취소", "복구"); }
            catch (Exception ex) { _view.ShowMessage($"복구 오류: {ex.Message}", "복구", icon: MessageBoxIcon.Error); }
            finally { _view.CloseProgress(progress); _view.SetCatalogBusy(false); _view.EnsureVisible(); }
        }

        // Undo 스택에서 이전 상태 복원
        public bool TryRecoverFromUndo()
        {
            if (!_session.TryPopUndo(out var snap)) return false;
            try
            {
                _session.RestoreUndo(snap);
                _view.RequestClearImageCache();
                _view.RequestRefreshFrameList();
                _view.SetStatusText("Undo 복원");
                return true;
            }
            catch { return false; }
        }

        // 타임라인 선택 구간 삭제
        public async Task HandleDeleteSelectionAsync()
        {
            var r = _selection.GetRange();
            if (r.s < 0) { _view.ShowMessage("삭제할 구간 없음", "선택구간 삭제"); return; }
            int eidx = r.e >= 0 ? r.e : r.s;
            if (!ConfirmDelete(r.s, eidx)) return;
            var savePath = _session.CurrentCatalogPath;
            int removed = DeleteRange(r.s, eidx, false);
            if (removed > 0 && !string.IsNullOrEmpty(savePath)) await PersistAfterEditAsync(removed, savePath);
            WriteDeletedAudit(r.s, eidx);
        }

        // 리스트에서 선택한 개별 항목 삭제
        public async Task HandleDeleteListItemsAsync(IReadOnlyList<int> indices)
        {
            if (indices.Count == 0) return;
            var idxs = indices.OrderByDescending(i => i).ToList();
            if (!ConfirmDeleteIndices(idxs)) return;
            var savePath = _session.CurrentCatalogPath;
            int removed = idxs.Sum(idx => DeleteRange(idx, idx, false));
            if (removed > 0 && !string.IsNullOrEmpty(savePath)) await PersistAfterEditAsync(removed, savePath);
        }

        // start~end 프레임 범위 삭제 (persistAfter=false면 저장 생략)
        public int DeleteRange(int start, int end, bool persistAfter = true)
        {
            var savePath = _session.CurrentCatalogPath;
            var result = _session.DeleteRange(start, end);
            if (result == null) return 0;
            _view.RequestStopPlayback();
            _view.OnFramesRemoved(result.Start, result.Count);
            _selection.OnFramesRemoved(result.Start, result.Count);
            _view.RequestRefreshFrameList();
            if (persistAfter && !string.IsNullOrEmpty(savePath))
                _ = PersistAfterEditAsync(result.Count, savePath);
            return result.Count;
        }

        // 편집 후 백업 생성 및 카탈로그 저장
        public async Task PersistAfterEditAsync(int removedCount, string? catalogPath = null)
        {
            catalogPath ??= _session.CurrentCatalogPath;
            if (string.IsNullOrEmpty(catalogPath)) return;
            _view.SetCatalogBusy(true);
            try
            {
                try { CatalogPaths.CreateTimestampedBackup(catalogPath); } catch { }
                var ok = await _catalog.SaveCatalogAsync(catalogPath, _session.CurrentFrames);
                _view.UpdateCatalogPathFromSession();
                if (!ok) _view.ShowMessage("저장 실패", "저장", icon: MessageBoxIcon.Error);
            }
            catch (Exception ex) { _view.ShowMessage($"저장 오류: {ex.Message}", "저장", icon: MessageBoxIcon.Error); }
            finally { _view.SetCatalogBusy(false); }
        }

        // 삭제 구간 정보를 JSON으로 감사 로그 저장
        private static void WriteDeletedAudit(int start, int end)
        {
            try
            {
                var dir = Path.Combine(Environment.CurrentDirectory, "Data", "DeletedRanges");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, $"deleted_{DateTime.Now:yyyyMMdd_HHmmss}.json"),
                    JsonSerializer.Serialize(new { Start = start, End = end, Time = DateTime.UtcNow }));
            }
            catch { }
        }

        // 연속 구간 삭제 확인 대화상자
        private bool ConfirmDelete(int start, int end)
        {
            if (_session.CurrentFrames.Count == 0) return false;
            start = Math.Clamp(start, 0, _session.CurrentFrames.Count - 1);
            end = Math.Clamp(end, 0, _session.CurrentFrames.Count - 1);
            if (end < start) (start, end) = (end, start);
            return ConfirmCore(end - start + 1, start, end, null);
        }

        // 다중 선택 항목 삭제 확인 (연속이면 구간, 아니면 개수 표시)
        private bool ConfirmDeleteIndices(IReadOnlyList<int> indices)
        {
            if (_session.CurrentFrames.Count == 0 || indices.Count == 0) return false;
            if (indices.Count == 1) return ConfirmDelete(indices[0], indices[0]);
            var sorted = indices.OrderBy(i => i).ToList();
            return sorted.Count == sorted[^1] - sorted[0] + 1
                ? ConfirmDelete(sorted[0], sorted[^1])
                : ConfirmCore(sorted.Count, sorted[0], sorted[^1], $"선택 {sorted.Count}개");
        }

        // 삭제 확인 공통 로직
        private bool ConfirmCore(int count, int start, int end, string? headline)
        {
            string target = headline ?? (count == 1 ? $"#{start}" : $"{start}~{end} ({count}개)");
            return _view.ShowMessage($"{target} 삭제?\n현재 {_session.CurrentFrames.Count:N0} 프레임",
                "프레임 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }
    }
}
