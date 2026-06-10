using System.Runtime.InteropServices;

namespace Malcha.UI
{
    // TrackBar native UI 위에 구간 마커를 정확한 채널 좌표로 덧그림
    internal sealed class TimelineTrackBar : TrackBar
    {
        public event PaintEventHandler? PostPaint;
        public event EventHandler? ThumbDragStarted;
        public event EventHandler? ThumbDragEnded;

        /// <summary>썸을 드래그 중 (WM_HSCROLL TB_THUMBTRACK ~ TB_ENDTRACK)</summary>
        public bool IsThumbDragging { get; private set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref NativeRect lParam);

        private const int WM_HSCROLL = 0x0114;
        private const int TBM_GETCHANNELRECT = 0x040A;
        private const int TB_THUMBTRACK = 5;
        private const int TB_ENDTRACK = 8;

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
            if (m.Msg == WM_HSCROLL)
            {
                int code = m.WParam.ToInt32() & 0xFFFF;
                if (code == TB_THUMBTRACK && !IsThumbDragging)
                {
                    IsThumbDragging = true;
                    ThumbDragStarted?.Invoke(this, EventArgs.Empty);
                }
            }

            base.WndProc(ref m);

            if (m.Msg == WM_HSCROLL)
            {
                int code = m.WParam.ToInt32() & 0xFFFF;
                if (code == TB_ENDTRACK && IsThumbDragging)
                {
                    IsThumbDragging = false;
                    ThumbDragEnded?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (m.Msg == 0x000F) // WM_PAINT
            {
                var handler = PostPaint;
                if (handler == null) return;

                using var g = Graphics.FromHwnd(Handle);
                handler(this, new PaintEventArgs(g, ClientRectangle));
            }
        }

        private Rectangle FallbackChannelRect()
        {
            const int pad = 8;
            var cs = ClientSize;
            return new Rectangle(pad, 0, Math.Max(1, cs.Width - pad * 2), cs.Height);
        }
    }
}
