using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using Malcha.Model;

namespace Malcha
{
    internal sealed class ChartController
    {
        private readonly Chart _chart;
        private int _lastHighlightIndex = -1;

        public ChartController(Chart chart) => _chart = chart;

        public void ResetHighlight() => _lastHighlightIndex = -1;

        public void RefreshFromFrames(IReadOnlyList<Frame> frames)
        {
            try
            {
                var seriesAngle = _chart.Series["user/angle"];
                var seriesThrottle = _chart.Series["user/throttle"];
                seriesAngle.Points.Clear();
                seriesThrottle.Points.Clear();

                for (int i = 0; i < frames.Count; i++)
                {
                    var f = frames[i];
                    seriesAngle.Points.AddXY(i, f.Angle);
                    seriesThrottle.Points.AddXY(i, f.Throttle);
                }

                var area = _chart.ChartAreas.Count > 0 ? _chart.ChartAreas[0] : null;
                if (area != null)
                {
                    area.AxisX.Minimum = 0;
                    area.AxisX.Maximum = System.Math.Max(0, frames.Count - 1);
                    area.RecalculateAxesScale();
                }

                _lastHighlightIndex = -1;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"Chart update error: {ex.Message}");
            }
        }

        public void RemoveRange(int start, int count)
        {
            try
            {
                var seriesAngle = _chart.Series["user/angle"];
                var seriesThrottle = _chart.Series["user/throttle"];
                for (int i = 0; i < count; i++)
                {
                    if (seriesAngle.Points.Count > start) seriesAngle.Points.RemoveAt(start);
                    if (seriesThrottle.Points.Count > start) seriesThrottle.Points.RemoveAt(start);
                }
            }
            catch { }
        }

        public void HighlightCurrent(int index, IReadOnlyList<Frame> frames)
        {
            try
            {
                var seriesAngle = _chart.Series["user/angle"];
                var seriesThrottle = _chart.Series["user/throttle"];

                if (seriesAngle.Points.Count != frames.Count || seriesThrottle.Points.Count != frames.Count)
                    RefreshFromFrames(frames);

                if (_lastHighlightIndex >= 0 && _lastHighlightIndex < seriesAngle.Points.Count)
                {
                    seriesAngle.Points[_lastHighlightIndex].MarkerStyle = MarkerStyle.None;
                    seriesThrottle.Points[_lastHighlightIndex].MarkerStyle = MarkerStyle.None;
                }

                if (index >= 0 && index < seriesAngle.Points.Count)
                {
                    seriesAngle.Points[index].MarkerStyle = MarkerStyle.Circle;
                    seriesAngle.Points[index].MarkerSize = 8;
                    seriesAngle.Points[index].MarkerColor = System.Drawing.Color.Yellow;
                    seriesAngle.Points[index].MarkerBorderColor = System.Drawing.Color.Black;
                    seriesThrottle.Points[index].MarkerStyle = MarkerStyle.Circle;
                    seriesThrottle.Points[index].MarkerSize = 8;
                    seriesThrottle.Points[index].MarkerColor = System.Drawing.Color.Orange;
                    seriesThrottle.Points[index].MarkerBorderColor = System.Drawing.Color.Black;
                    _lastHighlightIndex = index;
                }
            }
            catch { }
        }
    }
}
