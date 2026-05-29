namespace Malcha.Model
{
    // Loss → 100점 만점 (loss↓ = 점수↑, 단조 증가)
    internal static class TrainingScore
    {
        public static double FromLoss(double loss)
        {
            if (loss <= 0) return 100;
            return Math.Round(Math.Clamp(100.0 / (1.0 + loss), 0, 100), 1);
        }

        // epoch 기록 중 최저 val_loss (= 가장 좋은 검증 성능)
        public static double BestValScore(IReadOnlyList<TrainingEpoch> epochs)
        {
            if (epochs.Count == 0) return 0;
            var bestVal = epochs.Min(e => e.ValLoss);
            return FromLoss(bestVal);
        }

        public static TrainingEpoch? BestValEpoch(IReadOnlyList<TrainingEpoch> epochs)
        {
            if (epochs.Count == 0) return null;
            return epochs.OrderBy(e => e.ValLoss).ThenByDescending(e => e.Epoch).First();
        }
    }
}
