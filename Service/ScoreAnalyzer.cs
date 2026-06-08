using Malcha.Model;
using Malcha.Repository;

namespace Malcha.Service
{
    // [Service] WSL database.json 학습 Loss 분석 (UI 의존성 없음)
    internal class ScoreAnalyzer
    {
        private static readonly ScoreAnalyzer _instance = new();
        public static ScoreAnalyzer Instance => _instance;

        private readonly ResultRepository _repository = ResultRepository.Instance;
        private ScoreAnalyzer() { }

        // database.json에서 모델 학습 기록 로드 후 Repository에 저장
        public async Task<TrainingResult?> AnalyzeAsync(string databasePath, string modelName)
        {
            var result = await _repository.LoadFromDatabaseAsync(databasePath, modelName);
            if (result != null) _repository.AddOrReplace(result);
            return result;
        }

        // 로그에서 수집한 epoch로 UI 요약 생성 (강제 종료·database 미갱신 시)
        public TrainingSummary BuildSummaryFromLive(string modelName, IReadOnlyList<TrainingEpoch> epochs)
        {
            if (epochs.Count == 0)
                throw new InvalidOperationException("학습 기록(Epoch) 없음");
            var last = epochs[^1];
            var best = TrainingScore.BestValEpoch(epochs)!;
            return new TrainingSummary
            {
                ModelName = modelName,
                TotalEpochs = last.Epoch,
                FinalLoss = last.Loss,
                FinalValLoss = best.ValLoss
            };
        }

        // 최종 Epoch 기준으로 UI 표시용 요약 생성
        public TrainingSummary BuildSummary(TrainingResult result)
        {
            if (result.Epochs.Count == 0)
                throw new InvalidOperationException("학습 기록(Epoch) 없음");
            var last = result.Epochs[^1];
            var best = TrainingScore.BestValEpoch(result.Epochs)!;
            return new TrainingSummary
            {
                ModelName = result.Name,
                TotalEpochs = result.Epochs.Count,
                FinalLoss = last.Loss,
                FinalValLoss = best.ValLoss
            };
        }
    }
}
