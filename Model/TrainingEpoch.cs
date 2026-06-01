namespace Malcha.Model
{
    // [Model] Epoch별 Loss·ValLoss (차트 데이터 포인트)
    internal class TrainingEpoch
    {
        public int Epoch { get; set; }
        public double Loss { get; set; }
        public double ValLoss { get; set; }
    }
}
