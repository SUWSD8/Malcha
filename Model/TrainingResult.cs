namespace Malcha.Model
{
    // [Model] WSL database.json 단일 모델 학습 결과
    internal class TrainingResult
    {
        public int Number { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pilot { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Tubs { get; set; } = string.Empty;
        public double Time { get; set; }
        public string Transfer { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<TrainingEpoch> Epochs { get; set; } = new();
    }
}
