using Malcha.Model;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Malcha.Service
{
    // WSL train.py 원시 출력 → UI에 필요한 줄만 추출 + Epoch 점수 수집
    internal sealed class TrainingLogParser
    {
        // Epoch N/M ... loss ... val_loss (한 줄 요약)
        private static readonly Regex CombinedEpochLoss = new(
            @"Epoch\s+(\d+)\s*/\s*(\d+).*?-\s*loss:\s*([\d.eE+-]+).*?-\s*val_loss:\s*([\d.eE+-]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex EpochHeader = new(
            @"^Epoch\s+(\d+)\s*/\s*(\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Keras 표준: "- loss: 0.5 - val_loss: 0.6" (Epoch 헤더 다음 줄)
        private static readonly Regex KerasLossLine = new(
            @"-\s*loss:\s*([\d.eE+-]+)\s*-\s*val_loss:\s*([\d.eE+-]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ValLossImproved = new(
            @"Epoch\s+(\d+)\s*:\s*val_loss improved from [\d.eE+-]+ to ([\d.eE+-]+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TensorFlowInfo = new(
            @"^\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}",
            RegexOptions.Compiled);

        // 배치 진행 줄 (1/50 ...) — epoch가 아님, 무시
        private static readonly Regex BatchProgressLine = new(
            @"^\d+\s*/\s*\d+\s*\[",
            RegexOptions.Compiled);

        private int _lastLoggedEpoch;
        private int? _pendingEpoch;
        private int? _pendingTotal;
        private double _pendingLoss;

        private readonly List<TrainingEpoch> _collectedEpochs = new();

        public bool HadPythonError { get; private set; }
        public bool SawEarlyStopping { get; private set; }
        public bool SawTrainingFinished { get; private set; }
        public IReadOnlyList<TrainingEpoch> CollectedEpochs => _collectedEpochs;
        public int? PlannedTotalEpochs { get; private set; }

        public void Reset()
        {
            _lastLoggedEpoch = 0;
            _pendingEpoch = null;
            _pendingTotal = null;
            _pendingLoss = 0;
            _collectedEpochs.Clear();
            PlannedTotalEpochs = null;
            HadPythonError = false;
            SawEarlyStopping = false;
            SawTrainingFinished = false;
        }

        public string? TryFormat(string? rawLine)
        {
            if (string.IsNullOrWhiteSpace(rawLine)) return null;
            var line = rawLine.Trim();

            if (IsImportantStatus(line)) return line;

            if (line.Contains("early stopping", StringComparison.OrdinalIgnoreCase))
            {
                SawEarlyStopping = true;
                return "[조기 종료] Early Stopping — val_loss가 patience 동안 개선되지 않아 학습 중단";
            }

            if (line.Contains("Finished training", StringComparison.OrdinalIgnoreCase))
                SawTrainingFinished = true;

            if (IsPythonError(line))
            {
                HadPythonError = true;
                return $"[오류] {line}";
            }

            if (TensorFlowInfo.IsMatch(line)) return null;

            // 배치 진행률 줄은 epoch 점수에 사용하지 않음
            if (BatchProgressLine.IsMatch(line)) return null;

            var combined = CombinedEpochLoss.Match(line);
            if (combined.Success)
                return TryEmitEpoch(
                    int.Parse(combined.Groups[1].Value),
                    int.Parse(combined.Groups[2].Value),
                    combined.Groups[3].Value,
                    combined.Groups[4].Value);

            var improved = ValLossImproved.Match(line);
            if (improved.Success)
            {
                int ep = int.Parse(improved.Groups[1].Value);
                string valStr = improved.Groups[2].Value;
                if (ep > _lastLoggedEpoch)
                {
                    var prevLoss = _collectedEpochs.Count > 0 ? _collectedEpochs[^1].Loss : 0;
                    return TryEmitEpoch(ep, PlannedTotalEpochs ?? ep,
                        prevLoss.ToString(CultureInfo.InvariantCulture), valStr);
                }
            }

            var epochHeader = EpochHeader.Match(line);
            if (epochHeader.Success)
            {
                _pendingEpoch = int.Parse(epochHeader.Groups[1].Value);
                _pendingTotal = int.Parse(epochHeader.Groups[2].Value);
                PlannedTotalEpochs ??= _pendingTotal;
                return null;
            }

            var kerasLoss = KerasLossLine.Match(line);
            if (kerasLoss.Success && _pendingEpoch.HasValue)
            {
                var msg = TryEmitEpoch(
                    _pendingEpoch.Value,
                    _pendingTotal ?? _pendingEpoch.Value,
                    kerasLoss.Groups[1].Value,
                    kerasLoss.Groups[2].Value);
                _pendingEpoch = null;
                _pendingTotal = null;
                return msg;
            }

            var epochOnly = Regex.Match(line, @"^Epoch\s+(\d+)\s*:", RegexOptions.IgnoreCase);
            if (epochOnly.Success && !improved.Success)
            {
                int ep = int.Parse(epochOnly.Groups[1].Value);
                if (ep > _lastLoggedEpoch)
                    return $"Epoch {ep} 진행 중…";
            }

            return null;
        }

        private string? TryEmitEpoch(int epoch, int total, string loss, string valLoss)
        {
            if (epoch <= _lastLoggedEpoch) return null;
            _lastLoggedEpoch = epoch;
            PlannedTotalEpochs ??= total;

            if (!double.TryParse(loss, NumberStyles.Float, CultureInfo.InvariantCulture, out var lossVal))
                lossVal = _pendingLoss;
            if (!double.TryParse(valLoss, NumberStyles.Float, CultureInfo.InvariantCulture, out var valVal))
                valVal = 0;

            _pendingLoss = lossVal;
            _collectedEpochs.Add(new TrainingEpoch { Epoch = epoch, Loss = lossVal, ValLoss = valVal });

            return $"Epoch {epoch}/{total}  Loss: {FormatLoss(loss)}  Val_Loss: {FormatLoss(valLoss)}";
        }

        private static string FormatLoss(string value) =>
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)
                ? v.ToString("F4", CultureInfo.InvariantCulture)
                : value;

        private static bool IsPythonError(string line) =>
            line.Contains("Traceback", StringComparison.OrdinalIgnoreCase)
            || line.Contains("KeyError", StringComparison.OrdinalIgnoreCase)
            || line.Contains("IndexError", StringComparison.OrdinalIgnoreCase)
            || line.Contains("FileNotFoundError", StringComparison.OrdinalIgnoreCase)
            || line.Contains("Exception:", StringComparison.OrdinalIgnoreCase)
            || line.Contains("conda: command not found", StringComparison.OrdinalIgnoreCase)
            || line.StartsWith("[오류]", StringComparison.Ordinal);

        private static bool IsImportantStatus(string line) =>
            line.Contains("Saving model", StringComparison.OrdinalIgnoreCase)
            || line.Contains("학습 완료", StringComparison.OrdinalIgnoreCase)
            || line.Contains("training complete", StringComparison.OrdinalIgnoreCase);
    }
}
