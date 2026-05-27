using System.Drawing;
using System.Drawing.Drawing2D;
using Malcha.Model;

namespace Malcha
{
    internal static class FrameOverlayRenderer
    {
        internal static void Draw(Graphics g, Frame frame, Size clientSize)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = clientSize.Width;
            int h = clientSize.Height;
            int arrowLen = System.Math.Max(64, System.Math.Min(w, h) / 3);
            int arrowWidth = System.Math.Max(16, arrowLen / 3);
            int headHeight = System.Math.Max(16, arrowLen / 3);
            float centerX = w / 2f;
            float centerY = h - (headHeight * 0.15f);
            float maxDeg = 45f;
            float angleDeg = (float)frame.Angle * maxDeg;

            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(angleDeg);

            using (var shadowBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
            using (var shadowPen = new Pen(Color.FromArgb(160, 0, 0, 0), System.Math.Max(4, arrowWidth / 4)))
            {
                g.TranslateTransform(2f, 2f);
                g.DrawLine(shadowPen, 0f, 0f, 0f, -arrowLen + headHeight);
                var triShadow = new[]
                {
                    new PointF(0f, -arrowLen),
                    new PointF(arrowWidth / 2f, -arrowLen + headHeight),
                    new PointF(-arrowWidth / 2f, -arrowLen + headHeight)
                };
                g.FillPolygon(shadowBrush, triShadow);
                g.DrawPolygon(shadowPen, triShadow);
                g.ResetTransform();
                g.TranslateTransform(centerX, centerY);
                g.RotateTransform(angleDeg);
            }

            using (var brush = new SolidBrush(Color.FromArgb(230, 255, 140, 0)))
            using (var shaftBrush = new SolidBrush(Color.FromArgb(230, 255, 140, 0)))
            {
                g.DrawLine(new Pen(shaftBrush), 0f, 0f, 0f, -arrowLen + headHeight);
                var tri = new[]
                {
                    new PointF(0f, -arrowLen),
                    new PointF(arrowWidth / 2f, -arrowLen + headHeight),
                    new PointF(-arrowWidth / 2f, -arrowLen + headHeight)
                };
                g.FillPolygon(brush, tri);
            }

            try
            {
                float barWidth = System.Math.Max(8, w / 80f);
                float barHeight = System.Math.Max(40, h / 4f);
                float margin = 10f;
                float barX = w - margin - barWidth;
                float barY = h - margin - barHeight;

                float t = System.Math.Max(-1f, System.Math.Min(1f, (float)frame.Throttle));
                float tNorm = (t + 1f) / 2f;
                float fillH = barHeight * tNorm;
                var bgRect = new RectangleF(barX, barY, barWidth, barHeight);
                var fillRect = new RectangleF(barX, barY + (barHeight - fillH), barWidth, fillH);

                g.ResetTransform();
                using (var bgBrush = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                using (var borderPen = new Pen(Color.FromArgb(220, 200, 200, 200), 1f))
                using (var fillBrush = new SolidBrush(Color.FromArgb(220, 60, 180, 75)))
                using (var txtBrush = new SolidBrush(Color.FromArgb(230, 255, 255, 255)))
                using (var font = new Font("Segoe UI", System.Math.Max(8f, w / 60f), FontStyle.Bold))
                using (var sf = new StringFormat())
                {
                    g.FillRectangle(bgBrush, bgRect);
                    g.DrawRectangle(borderPen, bgRect.X, bgRect.Y, bgRect.Width, bgRect.Height);
                    g.FillRectangle(fillBrush, fillRect);

                    sf.Alignment = StringAlignment.Far;
                    sf.LineAlignment = StringAlignment.Far;
                    var txtPt = new PointF(barX - 6f, h - margin);
                    string txt = frame.Throttle.ToString("+#0.000;-#0.000;0.000");
                    g.DrawString(txt, font, txtBrush, txtPt, sf);
                }
            }
            catch { }

            g.ResetTransform();
        }
    }
}
