namespace Malcha.Model
{
    // database.json Time 필드 → 화면 표시용 변환
    internal static class TrainingTimeFormat
    {
        // 그리드용 짧은 표기 (열 너비 제한)
        public static string FormatShort(double time)
        {
            if (time <= 0) return "-";
            if (time > 1_000_000_000)
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds((long)time).ToLocalTime();
                if (dt.Date == DateTimeOffset.Now.Date)
                    return dt.ToString("HH:mm");
                return dt.ToString("M/d");
            }
            return $"{time:F0}초";
        }

        // 툴팁·상세용 전체 표기
        public static string FormatFull(double time)
        {
            if (time <= 0) return "-";
            if (time > 1_000_000_000)
                return DateTimeOffset.FromUnixTimeSeconds((long)time).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            return $"학습 소요 {time:F1}초";
        }

        public static string Format(double time) => FormatShort(time);
    }
}
