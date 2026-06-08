using Malcha.Model;

using Malcha.Repository;

using Malcha.Service;

using Malcha.View;



namespace Malcha.Controller

{

    // [Controller] 모델 학습 흐름 — View ↔ Service 중재

    internal class TrainingController

    {

        private readonly ITrainingView _view;
        private readonly CatalogSession _session;
        private readonly WslDataSyncService _dataSync = WslDataSyncService.Instance;

        private readonly WslTrainingService _wsl = WslTrainingService.Instance;

        private readonly ScoreAnalyzer _analyzer = ScoreAnalyzer.Instance;

        private readonly ResultRepository _repo = ResultRepository.Instance;

        private readonly TrainingLogParser _logParser = new();
        private CancellationTokenSource? _trainCts;
        private volatile bool _trainingInProgress;
        private volatile bool _stopRequested;
        private string _activeModelName = string.Empty;
        private bool _hadFinalModelAtTrainStart;

        public TrainingController(ITrainingView view, CatalogSession session)

        {

            _view = view;
            _session = session;

            _view.ViewLoaded += async (_, _) =>
            {
                _repo.ClearCache();
                if (!_wsl.IsConfigured)
                {
                    _view.BindModelList(Array.Empty<TrainingResult>());
                    _view.ClearEpochScores(_view.SelectedModelName);
                    return;
                }
                await LoadHistoryAsync(_view.SelectedModelName, false);
            };

            _view.RunTrainingRequested += OnRunTraining;
            _view.StopTrainingRequested += OnStopTraining;

            _view.UpdateCommentRequested += OnUpdateComment;

            _view.DeleteModelRequested += OnDeleteModel;

            _view.RestoreModelFromBackupRequested += (_, _) => OnRestoreFromBackup();

            _view.ModelSelectionChanged += OnModelSelectionChanged;

        }



        private async void OnRunTraining(object? sender, EventArgs e)

        {

            string name = _view.SelectedModelName;

            if (string.IsNullOrWhiteSpace(name)) { _view.ShowError("모델 이름을 선택하세요."); return; }



            if (!await EnsureMycarPathAsync()) return;



            if (!WslDataSyncService.Instance.HasTrainingData())

            {

                _view.ShowError("'정제 데이터 연동' 후 프레임·이미지 수가 1:1로 일치해야 학습할 수 있습니다.\n연동 로그에서 프레임/이미지 수를 확인하세요.");

                return;

            }

            string fingerprint = TrainingDataFingerprint.Compute(_session.CurrentFrames, _session.FrameImagePaths);
            if (!_dataSync.IsSyncedWith(fingerprint))
            {
                string staleMsg =
                    "현재 화면의 카탈로그가 마지막 WSL 연동과 다릅니다.\n" +
                    "(필터·편집·삭제 후 「정제 데이터 연동」을 하지 않은 상태)\n\n" +
                    "그래도 WSL tub 데이터로 학습을 진행할까요?\n" +
                    "「아니오」를 누르고 연동 후 다시 시도하는 것을 권장합니다.";
                if (!_view.ConfirmTrainWithStaleSync(staleMsg))
                    return;
            }

            var (total, _, missingImages) = ImageCoverage.Summarize(_session.CurrentFrames, _session.FrameImagePaths);
            if (missingImages > 0 && total > 0)
            {
                _view.ShowError(
                    $"화면에 이미지 누락 프레임 {missingImages:N0}/{total:N0}개가 있습니다.\n" +
                    "정제 데이터 연동 전에 이미지 경로를 확인하세요.");
                return;
            }



            _stopRequested = false;
            _activeModelName = name;
            _trainCts?.Cancel();
            _trainCts?.Dispose();
            _trainCts = new CancellationTokenSource();
            _trainingInProgress = true;

            _view.SetTrainingButtonEnabled(false);
            _view.SetTrainingButtonText("학습 중...");
            _view.SetForceStopTrainingEnabled(true);

            _view.ClearLog();

            _view.ClearEpochScores(name);

            _logParser.Reset();

            _hadFinalModelAtTrainStart = _wsl.BeginTrainingSession(name);

            _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 시작: {name}");

            if (_hadFinalModelAtTrainStart)
            {
                _view.AppendLog(
                    $"※ 기존 models/{name}.h5 → .malcha_backup.h5 백업 · " +
                    $"staging({name}.malcha_staging.h5)에만 학습");
                _view.AppendLog("※ 강제 종료 시 새 학습분은 저장되지 않고 기존 .h5만 유지됩니다");
            }
            else
            {
                _view.AppendLog(
                    $"※ staging({name}.malcha_staging.h5)에 학습 · 완료 시 models/{name}.h5 로 저장");
                _view.AppendLog("※ 강제 종료 시에도 Ep까지 .h5 저장 → 목록·교차 테스트 가능");
            }

            _view.AppendLog($"mycar: {_wsl.CarDirectoryLinux}");

            _view.AppendLog(WslDataSyncService.Instance.DescribeSyncedData());

            if (WslDataSyncService.Instance.TryGetSyncedDataCounts(out int syncFrames, out _)
                && syncFrames > 0 && syncFrames < 2500)
            {
                _view.AppendLog(
                    $"※ data {syncFrames:N0}프 — 미정제 원본보다 적으면 val 점수·epoch가 낮게 나올 수 있음");
            }

            var logProgress = new Progress<string>(line =>
            {
                if (!string.IsNullOrWhiteSpace(line))
                    _view.AppendLog(line);

                int epochCountBefore = _logParser.CollectedEpochs.Count;
                _logParser.TryFormat(line);
                if (_logParser.CollectedEpochs.Count > epochCountBefore)
                    RefreshLiveScores(name);
            });



            try
            {
                var runResult = await _wsl.TrainAsync($"{name}.h5", logProgress, _trainCts.Token);

                if (runResult.WasCancelled)
                    return;

                if (!runResult.Succeeded)
                {
                    _wsl.DiscardStagingWeights(name);
                    _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 실패 — staging 삭제 · 기존 {name}.h5 유지");
                    string detail = _logParser.HadPythonError
                        ? "Python 오류(KeyError 등) — 정제 데이터 연동·프레임/이미지 수를 확인하세요."
                        : "WSL train.py가 0이 아닌 코드로 종료됐습니다. 학습 로그의 [오류] 줄을 확인하세요.";
                    _view.ShowError($"WSL 학습 실패\n\n{detail}");
                    return;
                }

                if (_logParser.HadPythonError)
                {
                    _wsl.DiscardStagingWeights(name);
                    _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] Python 오류 발생 — staging 삭제");
                    _view.ShowError("학습 중 Python 오류(KeyError 등)가 발생했습니다.\n정제 데이터 연동 후 프레임·이미지 수를 확인하세요.");
                    return;
                }

                try
                {
                    _wsl.PromoteStagingWeights(name);
                    _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] staging → models/{name}.h5 반영 완료");
                }
                catch (Exception ex)
                {
                    _wsl.DiscardStagingWeights(name);
                    _view.ShowError(
                        $"학습은 끝났지만 .h5 반영에 실패했습니다.\n{ex.Message}\n\n" +
                        $"기존 models/{name}.h5 · .malcha_backup.h5 는 그대로입니다.");
                    return;
                }

                AppendTrainingCompletionSummary(name);

                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 완료 — 결과 불러오는 중…");
                await LoadHistoryAsync(name, true, _logParser.CollectedEpochs.ToList());
            }
            catch (OperationCanceledException)
            {
                // FinalizeStoppedTrainingAsync는 finally에서 처리
            }
            catch (Exception ex)
            {
                if (_stopRequested) return;
                _view.ShowError(ex.Message);
            }
            finally
            {
                try
                {
                    if (_stopRequested && !string.IsNullOrEmpty(_activeModelName))
                        await FinalizeStoppedTrainingAsync(_activeModelName);
                }
                finally
                {
                    _trainingInProgress = false;
                    _stopRequested = false;
                    _trainCts?.Dispose();
                    _trainCts = null;
                    ResetTrainingUi();
                }
            }
        }

        private void OnStopTraining(object? sender, EventArgs e)
        {
            if (!_trainingInProgress || _trainCts == null) return;
            _stopRequested = true;
            _view.SetForceStopTrainingEnabled(false);
            _view.SetTrainingButtonText("종료 중…");
            _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 종료 요청… (WSL 중단 · 가중치 저장 확인 중)");
            _trainCts.Cancel();
        }

        private async Task FinalizeStoppedTrainingAsync(string modelName)
        {
            _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 프로세스 종료됨 — 결과 정리 중…");

            var epochs = _logParser.CollectedEpochs.ToList();
            int lastEp = epochs.Count > 0 ? epochs[^1].Epoch : 0;
            var outcome = await _wsl.WaitForStoppedWeightsAsync(modelName, _hadFinalModelAtTrainStart);

            _view.AppendLog(
                $"  · staging({modelName}.malcha_staging.h5): " +
                (_wsl.StagingWeightsExist(modelName) ? "있음" : "없음"));
            _view.AppendLog(
                $"  · final(models/{modelName}.h5): " +
                (_wsl.ModelWeightsExist(modelName) ? "있음" : "없음"));

            if (_hadFinalModelAtTrainStart)
            {
                _wsl.DiscardStagingWeights(modelName);
                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 중단 — Epoch {lastEp} (기존 {modelName}.h5 유지)");
                _view.AppendLog($"  ★ models/{modelName}.h5 는 덮어쓰지 않았습니다");
                _view.AppendLog("  ※ 처음부터 새로 저장하려면 WSL models/ 에서 기존 .h5를 지우고 다시 학습하세요");

                if (_wsl.BackupWeightsExist(modelName))
                    _view.AppendLog($"  · 백업: models/{modelName}.malcha_backup.h5");

                await ReloadPreservedModelFromDatabaseAsync(modelName);

                string restoreHint = _wsl.BackupWeightsExist(modelName)
                    ? $"\n\n※ 교차 테스트가 이상하면 「백업에서 복구」를 시도하세요.\n" +
                      $"(models/{modelName}.malcha_backup.h5)"
                    : string.Empty;

                _view.ShowInfo("학습 중단",
                    $"Epoch {lastEp}에서 학습을 중단했습니다.\n\n" +
                    $"기존 models/{modelName}.h5 가 있어 새 학습분은 저장하지 않았습니다.\n" +
                    "완전히 새 모델로 저장하려면 models/ 폴더의 .h5를 삭제 후 다시 학습하세요.\n\n" +
                    "아래 목록에서 모델을 선택 → 「교차 테스트」" +
                    restoreHint);
                return;
            }

            // 처음 학습(기존 .h5 없음) — 중단 시점까지 저장
            if (epochs.Count == 0)
            {
                _wsl.DiscardStagingWeights(modelName);
                _view.ShowInfo("학습 중단",
                    "학습을 중단했지만 완료된 epoch가 없습니다.\n" +
                    "모델(.h5)은 epoch 1이 끝난 뒤 val_loss 저장 시점부터 생깁니다.");
                return;
            }

            if (outcome == StoppedWeightsOutcome.None)
            {
                _wsl.DiscardStagingWeights(modelName);
                _view.ShowInfo("학습 중단",
                    $"Epoch {lastEp}까지 로그는 있지만 .h5 파일을 확인하지 못했습니다.\n\n" +
                    "epoch가 끝나기 전에 종료했거나, WSL 파일 반영이 늦었을 수 있습니다.\n" +
                    "한 epoch 더 진행한 뒤 다시 시도해 보세요.");
                return;
            }

            try
            {
                if (!_wsl.TryCommitStoppedWeights(modelName, outcome))
                    throw new InvalidOperationException("가중치 파일 반영 실패");

                string via = outcome == StoppedWeightsOutcome.StagingReady
                    ? $"staging → models/{modelName}.h5"
                    : $"models/{modelName}.h5 (직접 저장됨)";
                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] Epoch {lastEp}까지 저장 — {via}");
            }
            catch (Exception ex)
            {
                _wsl.DiscardStagingWeights(modelName);
                _view.ShowError($"가중치 저장 실패:\n{ex.Message}");
                return;
            }

            RefreshLiveScores(modelName);
            int? selectNumber = await RegisterLiveModelInListAsync(modelName, epochs, weightsOnDisk: true);
            _view.AppendLog(
                selectNumber.HasValue
                    ? $"  → 아래 모델 목록에 「{modelName}」 반영됨 — 선택 후 「교차 테스트」"
                    : $"  → 모델 목록 갱신됨 — 「{modelName}」 선택 후 「교차 테스트」");

            var summary = _analyzer.BuildSummaryFromLive(modelName, epochs);
            var best = TrainingScore.BestValEpoch(epochs)!;
            _view.ShowInfo("학습 중단 · 저장됨",
                $"{summary.ToDisplayMessage()}\n(Ep{best.Epoch}까지 저장)\n\n" +
                "아래 목록에서 이 모델을 선택한 뒤 「교차 테스트」를 실행하세요.");
        }

        private async Task<int?> RegisterLiveModelInListAsync(
            string modelName,
            IReadOnlyList<TrainingEpoch> epochs,
            bool weightsOnDisk)
        {
            if (!_wsl.IsConfigured) return null;

            try
            {
                await _repo.LoadAllFromDatabaseAsync(_wsl.DatabaseUncPath);
            }
            catch
            {
                // database.json 없어도 세션 결과는 목록에 표시
            }

            var live = _repo.UpsertLiveSession(modelName, epochs, weightsOnDisk);
            _view.BindModelList(_repo.GetAll(), live.Number);
            return live.Number;
        }

        private async Task ReloadPreservedModelFromDatabaseAsync(string modelName)
        {
            if (!_wsl.IsConfigured) return;

            try
            {
                await _repo.LoadAllFromDatabaseAsync(_wsl.DatabaseUncPath);
            }
            catch
            {
                // database 없어도 목록은 유지
            }

            var existing = _repo.FindByName(modelName);
            _view.BindModelList(_repo.GetAll(), existing?.Number);

            if (existing?.Epochs.Count > 0)
                _view.BindEpochScores(modelName, existing.Epochs, existing.Epochs.Count);
            else
                _view.ClearEpochScores(modelName);
        }

        private void OnRestoreFromBackup()
        {
            string name = _view.SelectedModelName;
            if (string.IsNullOrWhiteSpace(name))
            {
                _view.ShowError("복구할 모델을 목록에서 선택하세요.");
                return;
            }

            if (!_view.ConfirmRestoreFromBackup(name))
                return;

            TryRestoreModelFromBackup(name);
        }

        private void TryRestoreModelFromBackup(string modelName)
        {
            if (!_wsl.TryRestoreFinalFromBackup(modelName))
            {
                _view.ShowError(
                    $"백업 파일이 없습니다.\nmodels/{modelName}.malcha_backup.h5\n\n" +
                    "이전 버전에서 epoch 1 중 강제 종료로 덮어씌워진 경우,\n" +
                    "앱 백업이 없으면 WSL models/ 폴더에 다른 .h5 사본이 있는지 확인하거나\n" +
                    "데이터 연동 후 학습을 처음부터 다시 실행해야 합니다.");
                return;
            }

            _view.ShowInfo("복구 완료",
                $"models/{modelName}.h5 를 .malcha_backup.h5 내용으로 되돌렸습니다.\n" +
                "「교차 테스트」로 확인하세요.");
        }

        private void ResetTrainingUi()
        {
            _view.SetTrainingButtonEnabled(true);
            _view.SetTrainingButtonText("학습 시작");
            _view.SetForceStopTrainingEnabled(false);
        }

        private void AppendTrainingCompletionSummary(string modelName)
        {
            int last = _logParser.CollectedEpochs.Count > 0
                ? _logParser.CollectedEpochs[^1].Epoch
                : 0;
            int? planned = _logParser.PlannedTotalEpochs;
            string epochPart = planned.HasValue && planned.Value > 0
                ? $"Epoch {last}/{planned.Value}"
                : $"Epoch {last}";

            if (_logParser.SawEarlyStopping)
            {
                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] {epochPart} — DonkeyCar Early Stopping으로 종료");
                _view.AppendLog("  ※ train loss는 아직 하강 중일 수 있음 (val_loss 개선 정체 시 중단)");
                return;
            }

            if (planned.HasValue && last > 0 && last < planned.Value)
            {
                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] {epochPart} — 계획 epoch 미만 종료 (로그에서 [오류]·Traceback 확인)");
                return;
            }

            if (last > 0)
                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] {epochPart} — 전체 epoch 완료");
        }



        private void RefreshLiveScores(string modelName) =>

            _view.BindEpochScores(modelName, _logParser.CollectedEpochs, _logParser.PlannedTotalEpochs);



        private void OnModelSelectionChanged(object? sender, EventArgs e)
        {
            var model = _view.SelectedModel;
            if (model != null)
                _view.BindEpochScores(model.Name, model.Epochs);
            else
                _view.ClearEpochScores(_view.SelectedModelName);
        }



        private Task<bool> EnsureMycarPathAsync()

        {

            try

            {

                string? suggested = null;

                try

                {

                    var probe = WslPathHelper.ToUncPath(WslTrainingService.DefaultDistro, "/home/heejun/mycar");

                    if (Directory.Exists(probe)) suggested = probe;

                }

                catch { }



                return Task.FromResult(_wsl.TryConfigure(() => _view.PromptMycarFolder(suggested)));

            }

            catch (Exception ex)

            {

                _view.ShowError(ex.Message);

                return Task.FromResult(false);

            }

        }



        private void OnUpdateComment(object? sender, EventArgs e)

        {

            try

            {

                _repo.UpdateComment(_view.SelectedModelName, _view.ModelComment);

                _view.ShowInfo("완료", "코멘트 수정됨");

                _view.BindModelList(_repo.GetAll(), _view.SelectedModelNumber);

            }

            catch (Exception ex) { _view.ShowError(ex.Message); }

        }



        private async void OnDeleteModel(object? sender, EventArgs e)
        {
            var model = _view.SelectedModel;
            if (model == null)
            {
                _view.ShowError("삭제할 모델을 목록에서 클릭해 선택하세요.");
                return;
            }

            string timeLabel = TrainingTimeFormat.FormatFull(model.Time);
            if (!_view.ConfirmDeleteModel(model, timeLabel)) return;

            try
            {
                if (!_wsl.IsConfigured)
                {
                    _view.ShowError("mycar 경로가 설정되지 않았습니다.");
                    return;
                }
                await _repo.DeleteFromDatabaseAsync(_wsl.DatabaseUncPath, model);
                bool deleteH5 = _repo.ShouldDeleteModelFile(model.Name);
                if (deleteH5)
                    _wsl.DeleteModelFile(model.Name);
                _view.ShowInfo("삭제 완료",
                    $"#{model.Number} {model.Name}\n· database.json 기록 1건 삭제" +
                    (deleteH5 ? "\n· models/*.h5 삭제" : "\n· .h5 유지 (같은 이름 기록 남음)"));
                _view.BindModelList(_repo.GetAll());
                _view.ClearEpochScores(model.Name);
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }



        // database.json 전체 로드 후 선택 모델 점수·목록 갱신

        private async Task LoadHistoryAsync(

            string modelName,

            bool showDialog,

            List<TrainingEpoch>? liveFallback = null)

        {

            if (!_wsl.IsConfigured)
            {
                _repo.ClearCache();
                _view.BindModelList(Array.Empty<TrainingResult>());
                return;
            }

            try
            {
                TrainingResult? result = null;

                for (int attempt = 0; attempt < 6; attempt++)
                {
                    await _repo.LoadAllFromDatabaseAsync(_wsl.DatabaseUncPath);
                    result = _repo.FindByName(modelName);
                    if (result?.Epochs.Count > 0) break;
                    if (attempt < 5) await Task.Delay(500);
                }

                _view.BindModelList(_repo.GetAll(), result?.Number);

                if (result == null)
                {
                    if (liveFallback?.Count > 0)
                    {
                        bool weightsOk = _wsl.ModelWeightsExist(modelName);
                        var live = _repo.UpsertLiveSession(modelName, liveFallback, weightsOk);
                        _view.BindModelList(_repo.GetAll(), live.Number);
                        _view.BindEpochScores(modelName, liveFallback, _logParser.PlannedTotalEpochs);
                        if (showDialog)
                        {
                            if (weightsOk)
                            {
                                var summary = _analyzer.BuildSummaryFromLive(modelName, liveFallback);
                                _view.ShowInfo("학습 결과", summary.ToDisplayMessage());
                            }
                            else
                                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] .h5 확인됨 — database.json 기록은 아직 없음");
                        }
                        return;
                    }

                    _view.ClearEpochScores(modelName);
                    if (showDialog)
                        _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] database.json에 '{modelName}' 기록 없음");
                    return;
                }

                _view.BindEpochScores(modelName, result.Epochs, result.Epochs.Count);
                if (showDialog)
                {
                    var summary = _analyzer.BuildSummary(result);
                    _view.ShowInfo("학습 결과", summary.ToDisplayMessage());
                }
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }
    }
}

