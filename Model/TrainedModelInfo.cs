using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.Model
{
    internal class TrainedModelInfo
    {
        // --- 메타데이터 (UI 라벨/텍스트 출력용) ---
        public int Number { get; set; }
        public string Name { get; set; }
        public string Pilot { get; set; }
        public string Type { get; set; }
        public string Tubs { get; set; }
        public double Time { get; set; }
        public string Transfer { get; set; }
        public string Comment { get; set; }
        // --- 시각화 데이터 (UI 차트 바인딩용) ---
        public List<TrainedData> History { get; set; }
        public TrainedModelInfo()
        {
            History = new List<TrainedData>();
        }
    }
}
