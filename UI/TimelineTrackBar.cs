using System.Runtime.InteropServices;

namespace Malcha.UI
{
    // TrackBar native UI 위에 구간 마커를 정확한 채널 좌표로 덧그림
    internal sealed class TimelineTrackBar : TrackBar
    {
        public event PaintEventHandler? PostPaint;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref NativeRect lParam);

        private const int TBM_GETCHANNELRECT = 0x040A;

        public Rectangle ChannelRect
        {
            get
            {
                if (!IsHandleCreated)
                    return FallbackChannelRect();

                var r = new NativeRect();
                SendMessage(Handle, TBM_GETCHANNELRECT, IntPtr.Zero, ref r);
                if (r.Right <= r.Left)
                    return FallbackChannelRect();
                return Rectangle.FromLTRB(r.Left, r.Top, r.Right, r.Bottom);
            }
        }

        public int ValueToX(int value)
        {
            var ch = ChannelRect;
            if (Maximum <= Minimum)
                return ch.Left + ch.Width / 2;

            float t = (float)(value - Minimum) / (Maximum - Minimum);
            return ch.Left + (int)Math.Round(t * ch.Width);
        }

        public int XToValue(int x)
        {
            var ch = ChannelRect;
            if (ch.Width <= 0)
                return Minimum;

            float t = Math.Clamp((float)(x - ch.Left) / ch.Width, 0f, 1f);
            return Math.Clamp(Minimum + (int)Math.Round(t * (Maximum - Minimum)), Minimum, Maximum);
        }

        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != 0x000F) return; // WM_PAINT

            var handler = PostPaint;
            if (handler == null) return;

            using var g = Graphics.FromHwnd(Handle);
            handler(this, new PaintEventArgs(g, ClientRectangle));
        }

        private Rectangle FallbackChannelRect()
        {
            const int pad = 8;
            var cs = ClientSize;
            return new Rectangle(pad, 0, Math.Max(1, cs.Width - pad * 2), cs.Height);
        }
    }
}
