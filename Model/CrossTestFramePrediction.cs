namespace Malcha.Model
{
    // 교차 테스트 — 프레임별 모델 예측값
    internal sealed class CrossTestFramePrediction
    {
        public int Index { get; init; }
        public double Angle { get; init; }
        public double Throttle { get; init; }
    }
}
