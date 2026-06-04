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

            using var dlg = new FolderBrowserDialog
            {
                Description = "카탈로그 폴더 선택 (.catalog 또는 .catalog.bak 포함된 폴더)",
                UseDescriptionForTitle = true,
                SelectedPath = initialDir
            };

            if (dlg.ShowDialog(_view.Owner) == DialogResult.OK) 
            {
                string selectedFolder = dlg.SelectedPath;

                string lastSavedFileName = "merged_final.catalog";
                string lastSavedFilePath = Path.Combine(selectedFolder, lastSavedFileName);

                if (File.Exists(lastSavedFilePath))
                {
                    // 사용자에게 이전에 작업한 내역을 이어서 할지 묻습니다 (UX 향상)
                    var result = MessageBox.Show(
                        $"이전에 작업한 저장 파일({lastSavedFileName})이 발견되었습니다.\n해당 파일부터 이어서 작업하시겠습니까?\n\n(아니오를 누르면 원본 파일들을 처음부터 새로 병합합니다.)",
                        "이전 작업 불러오기",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // 사용자가 '예'를 누르면, 마지막 작업 파일 1개만 로드하도록 넘깁니다.
                        // (배열에 lastSavedFilePath 하나만 담아서 전달)
                        await LoadMultipleCatalogsAsync(new[] { lastSavedFilePath }, selectedFolder);
                        return; // 로드 완료 후 메서드 종료
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        return; // 폴더 선택 자체를 취소함
                    }
                    // '아니오'를 누른 경우, 아래의 원본 탐색 및 새로 병합 로직으로 계속 진행됩니다.
                }

                // 1. 하위 폴더(backups 등) 무시하고 최상단에서만 검색
                string[] topLevelCatalogs = Directory.GetFiles(selectedFolder, "*.catalog", SearchOption.TopDirectoryOnly);

                // 2. 혹시 같이 있는 이미 병합된 파일(merged)은 제외시킴
                string[] targetCatalogs = topLevelCatalogs
                    .Where(f => !Path.GetFileName(f).Contains("merged", StringComparison.OrdinalIgnoreCase))
                    .ToArray();


                if (targetCatalogs.Length == 0)
                {
                    _view.ShowMessage("선택한 폴더에 처리할 유효한 원본 카탈로그(.catalog)가 없습니다.\n(백업이나 이미 병합된 파일은 무시됩니다.)",
                        "데이터 선택", icon: MessageBoxIcon.Warning);
                    return;
                }

                // 2단계에서 Service를 통해 파일들을 병합(Merge) 로드할 브릿지 메서드를 호출합니다.
                // (기존의 await LoadCatalogFileAsync(단일_경로)는 삭제합니다.)
                await LoadMultipleCatalogsAsync(targetCatalogs, selectedFolder);
            }
        }

        // 새로고침 — WSL data 연동·초기화 / 카탈로그 재로드 / 닫기
        public async Task HandleRefreshAsync()
        {
            _view.RequestStopPlayback();

            bool hasCatalog = !string.IsNullOrEmpty(_session.CurrentCatalogPath) && _session.CurrentFrames.Count > 0;
            bool wslConfigured = _wslTraining.IsConfigured;
            var workingPath = hasCatalog
                ? CatalogPaths.ResolveWorkingCatalogPath(_session.CurrentCatalogPath)
                : string.Empty;

            if (!hasCatalog && !wslConfigured)
            {
                if (_view.ShowMessage("열린 데이터가 없습니다.\n화면을 초기화할까요?",
                        "새로고침", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    _view.RequestResetAllUi();
                return;
            }

            var choice = RefreshCatalogDialog.Show(_view.Owner, new RefreshCatalogDialog.Info
            {
                CatalogSummary = hasCatalog
                    ? $"화면: {Path.GetFileName(_session.CurrentCatalogPath)}  ·  {_session.CurrentFrames.Count:N0} 프레임"
                    : "화면: 열린 카탈로그 없음",
                WslDataSummary = _dataSync.DescribeSyncedData(),
                HasOpenCatalog = hasCatalog,
                WslConfigured = wslConfigured
            });
            if (choice == null) return;

            switch (choice.Value)
            {
                case RefreshChoice.ResyncWsl:
                    await HandleSyncTrainingDataAsync();
                    break;
                case RefreshChoice.ClearWsl:
                    await ClearWslDataAsync();
                    break;
                case RefreshChoice.ReloadDisk:
                    await ReloadFromDiskAsync(workingPath);
                    break;
                case RefreshChoice.CloseAll:
                    _session.ClearUndo();
                    _view.RequestResetAllUi();
                    _view.SetStatusText("데이터가 닫혔습니다");
                    break;
            }
        }

        // WSL mycar/data tub 내용 삭제
        private async Task ClearWslDataAsync()
        {
            try
            {
                if (!_wslTraining.TryConfigure(() => MycarFolderDialog.Show(_view.Owner, null)))
                    return;
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "WSL data 초기화", icon: MessageBoxIcon.Error);
                return;
            }

            var summary = _dataSync.DescribeSyncedData();
            if (_view.ShowMessage(
                    $"WSL mycar/data 의 학습용 파일을 삭제합니다.\n\n{summary}\n\n계속할까요?",
                    "WSL data 초기화", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            _view.SetCatalogBusy(true);
            _view.SetStatusText("WSL data 초기화 중…");
            try
            {
                await _dataSync.ClearSyncedDataAsync();
                _view.SetStatusText("WSL data 초기화 완료");
                _view.ShowMessage(
                    $"WSL data를 비웠습니다.\n\n학습 전 「정제 데이터 연동」 또는\n새로고침 → WSL data 다시 연동을 실행하세요.",
                    "WSL data 초기화");
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"초기화 오류: {ex.Message}", "WSL data 초기화", icon: MessageBoxIcon.Error);
            }
            finally
            {
                _view.SetCatalogBusy(false);
                _view.EnsureVisible();
            }
        }

        // 디스크의 작업용 .catalog 를 그대로 다시 읽기
        private async Task ReloadFromDiskAsync(string workingPath)
        {
            if (!File.Exists(workingPath))
            {
                _view.ShowMessage("카탈로그 파일을 찾을 수 없습니다.", "새로고침", icon: MessageBoxIcon.Warning);
                return;
            }

            _session.ClearUndo();
            await LoadCatalogFileAsync(workingPath);
            _view.SetStatusText($"다시 읽음 — {_session.CurrentFrames.Count:N0} 프레임");
            _view.ShowMessage(
                $"디스크에서 다시 불러왔습니다.\n\n프레임: {_session.CurrentFrames.Count:N0}\n\nWSL data와 다를 수 있습니다. 「새로고침 → WSL data 다시 연동」을 실행하세요.",
                "새로고침");
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
                _session.ClearDeleted();
                _session.ClearCrossTest();
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
        // 다중 파일 병합 로드 후 세션 및 UI 갱신
        public async Task LoadMultipleCatalogsAsync(string[] validCatalogFiles, string selectedFolder)
        {
            _view.SetCatalogBusy(true);
            _view.SetStatusText($"카탈로그 {validCatalogFiles.Length}개 병합 렌더링 중...");
            try
            {
                _view.RequestStopPlayback();
                _session.ClearUndo();
                _session.ClearDeleted();
                _session.ClearCrossTest();
                _view.RequestClearImageCache();
                _selection.Clear();

                //  병합 메서드 호출
                var mergedFrames = await _catalog.LoadAndConcatCatalogsAsync(validCatalogFiles);

                if (mergedFrames == null || mergedFrames.Count == 0)
                {
                    _view.RequestClearPlayback();
                    _view.ShowMessage("선택한 파일들에 유효한 데이터가 없습니다.", "알림");
                    return;
                }

                // 🌟 2. Session 업데이트
                // 가상의 마스터 파일 이름 "merged_final.catalog" 경로를 세션에 물림
                string virtualMasterPath = Path.Combine(selectedFolder, "merged_final.catalog");
                _session.CurrentCatalogPath = virtualMasterPath;

                _session.Catalogs.Clear();
                _session.Catalogs[virtualMasterPath] = mergedFrames;
                _session.CurrentFrames = mergedFrames;

                // 🌟 3. 이미지 절대 경로 해석 세팅 
                // -> 첫 번째 파일의 위치(즉, selectedFolder)를 기준 디렉토리로 잡고 이미지를 뒤지게 함
                _session.FrameImagePaths = _catalog.ResolveFrameImagePaths(virtualMasterPath, mergedFrames);

                _session.CurrentIndex = -1;
                await _view.CompleteCatalogLoadAsync();
                _view.UpdateCatalogPathFromSession();
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"병합 중 에러가 발생했습니다: {ex.Message}", "에러", icon : MessageBoxIcon.Error);
            }
            finally { _view.SetCatalogBusy(false); }
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
                    //try { CatalogPaths.CreateTimestampedBackup(_session.CurrentCatalogPath); } catch { }
                    //await _catalog.SaveCatalogAsync(_session.CurrentCatalogPath, _session.CurrentFrames);
                }
                _view.ShowMessage($"정제: {refineResult.OriginalCount:N0} → {refineResult.Frames.Count:N0}", "필터 적용");
            }
            catch (Exception ex) { _view.ShowMessage($"정제 오류: {ex.Message}", "필터 적용", icon: MessageBoxIcon.Error); }
            finally { _view.CloseProgress(progress); _view.SetCatalogBusy(false); _view.EnsureVisible(); }
        }

        // 백업 시점으로 복구 (복구 전 사용자 확인)
        public async Task HandleRecoverAsync()
        {
            if (string.IsNullOrEmpty(_session.CurrentCatalogPath))
            {
                _view.ShowMessage("카탈로그를 먼저 열어 주세요.", "복구");
                return;
            }

            var workingPath = CatalogPaths.ResolveWorkingCatalogPath(_session.CurrentCatalogPath);
            var backupPaths = CatalogPaths.GetAllBackupPaths(workingPath);

            if (backupPaths.Count == 0)
            {
                if (!_session.HasUndo)
                {
                    _view.ShowMessage("복구할 백업이 없습니다.\n(backups/ 폴더 또는 Undo 기록 없음)", "복구");
                    return;
                }

                if (_view.ShowMessage(
                        $"백업 파일이 없습니다.\n\n직전 편집 상태(Undo)로 되돌릴까요?\n현재 {_session.CurrentFrames.Count:N0} 프레임",
                        "복구", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                if (TryRecoverFromUndo())
                    _view.ShowMessage("직전 편집 상태로 복원했습니다.", "복구");
                else
                    _view.ShowMessage("Undo 복원에 실패했습니다.", "복구", icon: MessageBoxIcon.Warning);
                return;
            }

            if (_view.ShowMessage(
                    $"현재 {_session.CurrentFrames.Count:N0} 프레임\n\nbackups/ 에 저장된 시점 중 하나를 골라 복구합니다.\n복구 전 현재 작업본은 자동 백업됩니다.\n\n계속할까요?",
                    "복구", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            string? selectedBackupPath = null;

            foreach (var path in backupPaths)
            {
                var fileInfo = new FileInfo(path);
                string timeStr = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm");
                string backupName = Path.GetFileName(path);

                var result = _view.ShowMessage(
                    $"백업: {backupName}\n시간: {timeStr}\n\n이 시점으로 복구할까요?\n(아니오 → 더 오래된 백업 보기 · 취소 → 중단)",
                    "복구 시점 선택",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    selectedBackupPath = path;
                    break;
                }
                if (result == DialogResult.Cancel)
                    return;
            }

            if (string.IsNullOrEmpty(selectedBackupPath))
            {
                _view.ShowMessage("선택된 백업이 없어 복구를 취소했습니다.", "복구");
                return;
            }

            List<Frame> backupFrames;
            try
            {
                backupFrames = await _catalog.LoadCatalogFileAsync(selectedBackupPath)
                    ?? new List<Frame>();
            }
            catch (Exception ex)
            {
                _view.ShowMessage(ex.Message, "복구", icon: MessageBoxIcon.Error);
                return;
            }

            if (_view.ShowMessage(
                    $"아래 내용으로 복구합니다.\n\n백업: {Path.GetFileName(selectedBackupPath)}\n백업 프레임: {backupFrames.Count:N0}\n현재 프레임: {_session.CurrentFrames.Count:N0}\n\n현재 작업본은 backups/에 백업된 뒤 교체됩니다.\n복구할까요?",
                    "복구 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            _session.PushUndo();
            _view.SetCatalogBusy(true);
            ProgressDialog? progress = null;
            try
            {
                progress = _view.ShowProgress("복구 중");

                try { CatalogPaths.CreateTimestampedBackup(workingPath); } catch { }

                _session.CurrentCatalogPath = workingPath;
                _session.CurrentFrames = backupFrames;
                _session.Catalogs[workingPath] = _session.CurrentFrames;
                await _catalog.SaveCatalogAsync(workingPath, _session.CurrentFrames);
                _view.RequestClearImageCache();
                _selection.Clear();
                _view.ResetChartHighlight();
                _session.FrameImagePaths = _catalog.ResolveFrameImagePaths(workingPath, _session.CurrentFrames);
                _view.RequestRefreshFrameList();
                if (_session.CurrentFrames.Count > 0)
                    _view.RequestShowFrame(Math.Min(_session.CurrentIndex, _session.CurrentFrames.Count - 1));

                _view.SetStatusText($"복구 완료 — {_session.CurrentFrames.Count:N0} 프레임");
                _view.ShowMessage(
                    $"복구했습니다.\n\n프레임: {_session.CurrentFrames.Count:N0}\n\nWSL data와 다를 수 있습니다. 필요하면 「정제 데이터 연동」을 실행하세요.",
                    "복구");
            }
            catch (OperationCanceledException)
            {
                _view.ShowMessage("복구가 취소되었습니다.", "복구");
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"복구 오류: {ex.Message}", "복구", icon: MessageBoxIcon.Error);
            }
            finally
            {
                _view.CloseProgress(progress);
                _view.SetCatalogBusy(false);
                _view.EnsureVisible();
            }
        }

        // 병합 및 정제된 현재 세션을 단일 파일로 디스크에 추출(Dump)합니다.
        public async Task HandleSaveSessionAsync()
        {
            // 1. 메모리에 데이터가 있는지 체크
            if (_session.CurrentFrames == null || _session.CurrentFrames.Count == 0)
            {
                _view.ShowMessage("저장할 프레임 데이터가 없습니다.", "저장");
                return;
            }

            // (이때 _session.CurrentCatalogPath 안에는 우리가 2단계에서 세팅한 
            // "C:\데이터디렉토리\merged_final.catalog" 경로가 들어 있습니다.)
            string targetSavePath = _session.CurrentCatalogPath;

            _view.SetStatusText("통합 데이터셋 저장 중...");
            _view.SetCatalogBusy(true);

            try
            {
                // 2. 통합될 위치에 이미 같은 이름의 이전 통합본이 있다면, 기존 파일만 가볍게 백업
                // (10개 원본 카탈로그의 백업을 만드는 게 아닙니다!)
                if (File.Exists(targetSavePath))
                {
                    // 이거 하나면 ' merged_final_2026xxxx.catalog ' 식으로 예쁘게 빠집니다.
                    CatalogPaths.CreateTimestampedBackup(targetSavePath);
                }

                // 3. 순수하게 메모리의 프레임을 JSON 덤프로 저장
                // 원본 조각들은 그대로 보존되며, targetSavePath(merged_final) 하나만 생성됩니다.
                bool isSuccess = await _catalog.SaveCatalogAsync(targetSavePath, _session.CurrentFrames);

                if (isSuccess)
                {
                    _view.ShowMessage($"전체 병합 및 정제 결과가 저장되었습니다.\n\n저장 위치:\n{targetSavePath}",
                                      "저장 성공", icon : MessageBoxIcon.Information);

                    _session.ClearUndo(); // 저장 직후 실행 취소 스택 비우기
                }
                else
                {
                    _view.ShowMessage("파일 쓰기에 실패했습니다.", "저장 에러", icon : MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _view.ShowMessage($"저장 중 오류 발생: {ex.Message}", "저장 에러", icon : MessageBoxIcon.Error);
            }
            finally
            {
                _view.SetStatusText("대기 중");
                _view.SetCatalogBusy(false);
            }
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
            //if (removed > 0 && !string.IsNullOrEmpty(savePath)) await PersistAfterEditAsync(removed, savePath);
            WriteDeletedAudit(r.s, eidx);
        }

        // 리스트에서 선택한 개별 항목 삭제
        public async Task HandleDeleteListItemsAsync(IReadOnlyList<int> indices)
        {
            if (indices.Count == 0) return;
            var idxs = indices.Distinct().OrderBy(i => i).ToList();
            if (!ConfirmDeleteIndices(idxs)) return;
            MoveIndicesToDeleted(idxs, persistAfter: false);
            //if (removed > 0 && !string.IsNullOrEmpty(savePath)) await PersistAfterEditAsync(removed, savePath);
        }

        // start~end 프레임 범위를 삭제 목록으로 이동
        public int MoveRangeToDeleted(int start, int end, bool persistAfter = true)
        {
            var savePath = _session.CurrentCatalogPath;
            var result = _session.MoveRangeToDeleted(start, end);
            if (result == null) return 0;
            AfterFramesMovedToDeleted(result);
            if (persistAfter && !string.IsNullOrEmpty(savePath))
                _ = PersistAfterEditAsync(result.Count, savePath);
            return result.Count;
        }

        // 여러 인덱스를 삭제 목록으로 이동
        public int MoveIndicesToDeleted(IReadOnlyList<int> indices, bool persistAfter = true)
        {
            var savePath = _session.CurrentCatalogPath;
            var result = _session.MoveIndicesToDeleted(indices);
            if (result == null) return 0;
            AfterFramesMovedToDeleted(result);
            if (persistAfter && !string.IsNullOrEmpty(savePath))
                _ = PersistAfterEditAsync(result.Count, savePath);
            return result.Count;
        }

        // 삭제 목록 → 프레임 리스트 복구
        public int RestoreFromDeletedList(IReadOnlyList<int> deletedIndices, bool persistAfter = true)
        {
            var savePath = _session.CurrentCatalogPath;
            int count = _session.RestoreFromDeleted(deletedIndices);
            if (count == 0) return 0;

            _view.RequestStopPlayback();
            _view.RequestClearImageCache();
            _selection.Clear();
            _view.RequestRefreshFrameList();
            _view.SetStatusText($"복구 — {count:N0} 프레임 (삭제 목록 {_session.DeletedEntries.Count:N0})");

            if (persistAfter && !string.IsNullOrEmpty(savePath))
                _ = PersistAfterEditAsync(0, savePath);
            return count;
        }

        // 드래그앤드롭 — 삭제 목록으로
        public int HandleDropToDeleted(FrameDragPayload payload)
        {
            if (payload.Source != FrameDragPayload.SourceKind.Active) return 0;
            if (payload.HasRange)
                return MoveRangeToDeleted(payload.RangeStart, payload.RangeEnd, persistAfter: false);
            if (payload.Indices.Count > 0)
                return MoveIndicesToDeleted(payload.Indices, persistAfter: false);
            return 0;
        }

        // 드래그앤드롭 — 프레임 리스트로 복구
        public int HandleDropToActive(FrameDragPayload payload)
        {
            if (payload.Source != FrameDragPayload.SourceKind.Deleted) return 0;
            if (payload.Indices.Count == 0) return 0;
            return RestoreFromDeletedList(payload.Indices, persistAfter: false);
        }

        private void AfterFramesMovedToDeleted(CatalogSession.DeleteRangeResult result)
        {
            _view.RequestStopPlayback();
            _view.OnFramesRemoved(result.Start, result.Count);
            _selection.OnFramesRemoved(result.Start, result.Count);
            _view.RequestRefreshFrameList();
            _view.SetStatusText($"삭제 목록 — {_session.DeletedEntries.Count:N0} (방금 +{result.Count:N0})");
        }

        // start~end 프레임 범위 삭제 (persistAfter=false면 저장 생략)
        public int DeleteRange(int start, int end, bool persistAfter = true) =>
            MoveRangeToDeleted(start, end, persistAfter);

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
            return _view.ShowMessage($"{target}을(를) 삭제 목록으로 이동?\n현재 {_session.CurrentFrames.Count:N0} 프레임",
                "프레임 삭제", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }
    }
}
