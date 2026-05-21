using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Malcha.Model
{
    internal class Frame
    {
        private string id { get; set; }
        private long timestamp { get; set; }
        private string imagePath { get; set; }
        private double angle { get; set; }
        private string model { get; set; }
        private double throttle { get; set; }

        // 조향각
        [JsonPropertyName("user/angle")]
        public double Angle { get; set; }

        // 쓰로틀 (속도)
        [JsonPropertyName("user/throttle")]
        public double Throttle { get; set; }

        // 주행 모드 (예: "user", "local", "local_angle" 등)
        [JsonPropertyName("user/mode")]
        public string Mode { get; set; }
    }
}
