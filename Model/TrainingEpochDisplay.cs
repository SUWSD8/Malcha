using System.Globalization;

namespace Malcha.Model
{
    internal static class TrainingEpochDisplay
    {
        private const int RecentEpochCount = 5;
        private const int MaxCompactEpochRows = 12;

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
                string totalHint = plannedTotal.HasValue ? $"/{plannedTotal.Value}" : string.Empty;
                lines.Add($"[{modelName}]  반복 0{totalHint}회 — 점수 대기");
                return lines;
            }

            int total = plannedTotal ?? epochs[^1].Epoch;
            var best = TrainingScore.BestValEpoch(epochs)!;
            double bestScore = TrainingScore.FromLoss(best.ValLoss);

            lines.Add(string.Format(CultureInfo.InvariantCulture,
                "[{0}]  반복 {1}/{2}회", modelName, epochs.Count, total));
            lines.Add(string.Format(CultureInfo.InvariantCulture,
                "★ Ep {0}/{1}  최고 검증 {2:F1}점", best.Epoch, total, bestScore));

            int showFrom = epochs.Count <= MaxCompactEpochRows
                ? 0
                : epochs.Count - MaxCompactEpochRows;
            if (showFrom > 0)
                lines.Add($"— 최근 {epochs.Count - showFrom}회 (전체 {epochs.Count}회) —");

            for (int i = showFrom; i < epochs.Count; i++)
            {
                var e = epochs[i];
                double valSc = TrainingScore.FromLoss(e.ValLoss);
                double trainSc = TrainingScore.FromLoss(e.Loss);
                string mark = string.Empty;
                if (Math.Abs(e.ValLoss - best.ValLoss) < 1e-9) mark += " ★";
                if (i == epochs.Count - 1) mark += " ◀";
                lines.Add(string.Format(CultureInfo.InvariantCulture,
                    "  Ep {0,3}/{1}  검증 {2:F1} | 학습 {3:F1}{4}",
                    e.Epoch, total, valSc, trainSc, mark));
            }

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
