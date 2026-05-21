using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Malcha.Model
{
    internal class Frame
    {
        // 데이터 고유 인덱스
        [JsonPropertyName("_index")]
        public int Index { get; set; }

        // 세션 식별자
        [JsonPropertyName("_session_id")]
        public string SessionId { get; set; }

        // 밀리초 단위 타임스탬프 (시간순 정렬 및 재생 속도 조절에 유용)
        [JsonPropertyName("_timestamp_ms")]
        public long TimestampMs { get; set; }

        // 이미지 파일명
        [JsonPropertyName("cam/image_array")]
        public string ImagePath { get; set; }

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
