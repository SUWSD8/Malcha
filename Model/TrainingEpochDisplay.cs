using System.Globalization;

namespace Malcha.Model
{
    internal static class TrainingEpochDisplay
    {
        private const int RecentEpochCount = 5;

        public static string NormalizeModelName(string name)
        {
            name = name.Trim();
            if (name.EndsWith(".h5", StringComparison.OrdinalIgnoreCase))
                return name[..^3];
            return name;
        }

        public static IReadOnlyList<string> ToCompactLines(
            string modelName,
            IReadOnlyList<TrainingEpoch> epochs,
            int? plannedTotal = null)
        {
            var lines = new List<string>();
            modelName = NormalizeModelName(modelName);

            if (epochs.Count == 0)
            {
                lines.Add($"[{modelName}] 점수 대기");
                return lines;
            }

            int total = plannedTotal ?? epochs[^1].Epoch;
            var last = epochs[^1];
            var best = TrainingScore.BestValEpoch(epochs)!;
            double bestScore = TrainingScore.FromLoss(best.ValLoss);
            double lastValScore = TrainingScore.FromLoss(last.ValLoss);
            double lastTrainScore = TrainingScore.FromLoss(last.Loss);

            lines.Add(string.Format(CultureInfo.InvariantCulture,
                "★ {0}  최고 {1:F1}점 (Ep{2})", modelName, bestScore, best.Epoch));
            lines.Add(string.Format(CultureInfo.InvariantCulture,
                "현재 Ep{0}  검증 {1:F1} | 학습 {2:F1}  ({3}/{4}ep)",
                last.Epoch, lastValScore, lastTrainScore, epochs.Count, total));
            return lines;
        }

        public static string ToDetailText(
            string modelName,
            IReadOnlyList<TrainingEpoch> epochs,
            int? plannedTotal = null)
        {
            modelName = NormalizeModelName(modelName);
            if (epochs.Count == 0) return $"{modelName}: 학습 기록 없음";

            int total = plannedTotal ?? epochs[^1].Epoch;
            var best = TrainingScore.BestValEpoch(epochs)!;

            var lines = new List<string>
            {
                $"모델: {modelName}  ({epochs.Count}/{total} epoch)",
                $"최고 검증 {TrainingScore.FromLoss(best.ValLoss):F1}점 (Ep{best.Epoch}, val={best.ValLoss:F4})",
                "※ 점수는 검증(val) loss 기준 — 학습 loss와 다를 수 있음",
                "",
                "최근 epoch (검증 점수):"
            };

            int start = Math.Max(0, epochs.Count - RecentEpochCount);
            for (int i = start; i < epochs.Count; i++)
            {
                var e = epochs[i];
                double sc = TrainingScore.FromLoss(e.ValLoss);
                string mark = i == epochs.Count - 1 ? " ◀ 현재" : "";
                if (Math.Abs(e.ValLoss - best.ValLoss) < 1e-9) mark += " ★최고";
                lines.Add($"  Ep {e.Epoch,3}  val {e.ValLoss:F4} → {sc,5:F1}점{mark}");
            }
            return string.Join(Environment.NewLine, lines);
        }

        public static IReadOnlyList<string> ToLines(
            string modelName,
            IReadOnlyList<TrainingEpoch> epochs,
            int? plannedTotal = null)
            => ToCompactLines(modelName, epochs, plannedTotal);
    }
}
