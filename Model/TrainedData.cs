using System;
using System.Collections.Generic;
using System.Text;

namespace Malcha.Model
{
    internal class TrainedData
    {
        // 차트의 X축에 들어갈 값 (1, 2, 3...)
        public int Epoch { get; set; }

        // 차트의 Y축 첫 번째 선에 들어갈 값 (훈련 손실)
        public double Loss { get; set; }

        // 차트의 Y축 두 번째 선에 들어갈 값 (검증 손실)
        public double ValLoss { get; set; }
    }
}
