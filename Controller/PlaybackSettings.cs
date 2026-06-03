namespace Malcha.Controller
{
    internal static class PlaybackSettings
    {
        public const int BaseIntervalMs = 100;
        public const int MinIntervalMs = 16;

        /// <summary>키보드 상단 0~5 또는 NumPad 0~5 → 배속. 0=0.5x, 1=1x, … 5=4x.</summary>
        public static float SpeedFromDigitKey(Keys key) => key switch
        {
            Keys.D0 or Keys.NumPad0 => 0.5f,
            Keys.D1 or Keys.NumPad1 => 1.0f,
            Keys.D2 or Keys.NumPad2 => 1.5f,
            Keys.D3 or Keys.NumPad3 => 2.0f,
            Keys.D4 or Keys.NumPad4 => 3.0f,
            Keys.D5 or Keys.NumPad5 => 4.0f,
            _ => 0f
        };

        public static int DelayMs(float speed) =>
            Math.Max(MinIntervalMs, (int)Math.Round(BaseIntervalMs / speed));

        public static string FormatSpeed(float speed) =>
            speed == 1f ? "1" : speed.ToString("0.#");

        public static string FormatSpeedLabel(float speed) => $"{FormatSpeed(speed)}x";
    }
}
