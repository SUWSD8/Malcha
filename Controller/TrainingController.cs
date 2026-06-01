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

        private readonly WslTrainingService _wsl = WslTrainingService.Instance;

        private readonly ScoreAnalyzer _analyzer = ScoreAnalyzer.Instance;

        private readonly ResultRepository _repo = ResultRepository.Instance;

        private readonly TrainingLogParser _logParser = new();



        public TrainingController(ITrainingView view)

        {

            _view = view;

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

            _view.UpdateCommentRequested += OnUpdateComment;

            _view.DeleteModelRequested += OnDeleteModel;

            _view.ModelSelectionChanged += OnModelSelectionChanged;

        }



        private async void OnRunTraining(object? sender, EventArgs e)

        {

            string name = _view.SelectedModelName;

            if (string.IsNullOrWhiteSpace(name)) { _view.ShowError("모델 이름을 선택하세요."); return; }



            if (!await EnsureMycarPathAsync()) return;



            if (!WslDataSyncService.Instance.HasTrainingData())

            {

                _view.ShowError("'정제 데이터 연동' 버튼으로 WSL data를 먼저 보내 주세요.");

                return;

            }



            _view.SetTrainingButtonEnabled(false);

            _view.SetTrainingButtonText("학습 중...");

            _view.ClearLog();

            _view.ClearEpochScores(name);

            _logParser.Reset();

            _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 시작: {name}");

            _view.AppendLog($"mycar: {_wsl.CarDirectoryLinux}");

            _view.AppendLog(WslDataSyncService.Instance.DescribeSyncedData());



            var logProgress = new Progress<string>(line =>

            {

                var formatted = _logParser.TryFormat(line);

                if (formatted != null)

                {

                    _view.AppendLog(formatted);

                    if (_logParser.CollectedEpochs.Count > 0)

                        RefreshLiveScores(name);

                }

            });



            try

            {

                if (!await _wsl.TrainAsync($"{name}.h5", logProgress))

                {

                    _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 실패");

                    _view.ShowError("WSL 학습 실패");

                    return;

                }

                if (_logParser.HadPythonError)

                {

                    _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] Python 오류 발생 — data·프레임 수 확인");

                    _view.ShowError("학습 중 Python 오류(KeyError 등)가 발생했습니다.\n정제 데이터 연동 후 프레임·이미지 수를 확인하세요.");

                    return;

                }

                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 완료 — 결과 불러오는 중…");

                await LoadHistoryAsync(name, true, _logParser.CollectedEpochs.ToList());

            }

            catch (Exception ex) { _view.ShowError(ex.Message); }

            finally

            {

                _view.SetTrainingButtonEnabled(true);

                _view.SetTrainingButtonText("학습 시작");

            }

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
                        _view.BindEpochScores(modelName, liveFallback, _logParser.PlannedTotalEpochs);
                    else
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

