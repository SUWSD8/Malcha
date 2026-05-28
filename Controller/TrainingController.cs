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

        // View 이벤트를 핸들러에 연결
        public TrainingController(ITrainingView view)
        {
            _view = view;
            _view.ViewLoaded += async (_, _) => await LoadHistoryAsync(_view.SelectedModelName, false);
            _view.RunTrainingRequested += OnRunTraining;
            _view.UpdateCommentRequested += OnUpdateComment;
            _view.DeleteModelRequested += OnDeleteModel;
            _view.ShowChartRequested += (_, _) => new TestForm2().Show();
        }

        // WSL 학습 실행 후 결과 로드
        private async void OnRunTraining(object? sender, EventArgs e)
        {
            string name = _view.SelectedModelName;
            if (string.IsNullOrWhiteSpace(name)) { _view.ShowError("모델 이름을 선택하세요."); return; }

            _view.SetTrainingButtonEnabled(false);
            _view.SetTrainingButtonText("학습 중...");
            _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 시작: {name}");

            try
            {
                if (!await _wsl.TrainAsync("data", $"{name}.h5"))
                {
                    _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 실패");
                    _view.ShowError("WSL 학습 실패");
                    return;
                }
                _view.AppendLog($"[{DateTime.Now:HH:mm:ss}] 학습 완료");
                _view.ShowInfo("완료", "모델 학습이 완료되었습니다.");
                await LoadHistoryAsync(name, true);
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
            finally
            {
                _view.SetTrainingButtonEnabled(true);
                _view.SetTrainingButtonText("학습 시작");
            }
        }

        // 선택 모델 코멘트 수정
        private void OnUpdateComment(object? sender, EventArgs e)
        {
            try
            {
                _repo.UpdateComment(_view.SelectedModelName, _view.ModelComment);
                _view.ShowInfo("완료", "코멘트 수정됨");
                _view.BindModelList(_repo.GetAll());
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }

        // 선택 모델 삭제
        private void OnDeleteModel(object? sender, EventArgs e)
        {
            try
            {
                _repo.Delete(_view.SelectedModelName);
                _view.ShowInfo("완료", "모델 삭제됨");
                _view.BindModelList(_repo.GetAll());
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }

        // database.json 분석 후 View에 점수·모델 목록 바인딩
        private async Task LoadHistoryAsync(string modelName, bool showDialog)
        {
            try
            {
                var result = await _analyzer.AnalyzeAsync(WslTrainingService.DefaultDatabasePath, modelName);
                if (result == null) return;
                var summary = _analyzer.BuildSummary(result);
                _view.BindScoreSummary(summary);
                _view.BindModelList(_repo.GetAll());
                if (showDialog) _view.ShowInfo("학습 결과", summary.ToDisplayMessage());
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
        }
    }
}
