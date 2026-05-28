namespace Malcha.Model
{
    // [Model] 학습 결과 UI 요약 (최종 Epoch 기준)
    internal class TrainingSummary
    {
        public string ModelName { get; set; } = string.Empty;
        public int TotalEpochs { get; set; }
        public double FinalLoss { get; set; }
        public double FinalValLoss { get; set; }

        // MessageBox용 한 줄 요약 문자열
        public string ToDisplayMessage() =>
            $"[모델] {ModelName}\nEpoch: {TotalEpochs}\nLoss: {FinalLoss:F4} / Val_Loss: {FinalValLoss:F4}";

        // lstViewScore 바인딩용 줄 목록
        public IReadOnlyList<string> ToScoreLines() => new[]
        {
            $"모델: {ModelName}", $"Epoch: {TotalEpochs}",
            $"Loss: {FinalLoss:F4}", $"Val_Loss: {FinalValLoss:F4}"
        };
    }
}
