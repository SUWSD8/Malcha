using Malcha.Model;
using System.Windows.Forms.DataVisualization.Charting;

namespace Malcha.UI
{
    internal static class ChartAdapter
    {
        private const string AreaName = "ChartArea1";
        private const string SeriesTrainLoss = "학습 오류율";
        private const string SeriesValLoss = "검증 오류율";

        public static void InitializeLossChart(Chart chart)
        {
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            var area = new ChartArea("MainArea");
            area.AxisX.Title = "Epoch";
            area.AxisY.Title = "Loss";
            area.AxisX.Minimum = 1;
            chart.ChartAreas.Add(area);
            chart.Legends.Add(new Legend { Docking = Docking.Top });
        }

        public static void DrawLossChart(Chart chart, List<TrainingEpoch> epochs)
        {
            if (epochs.Count == 0) throw new Exception("훈련 기록 없음");
            chart.Series.Clear();
            var loss = new Series("Loss") { ChartType = SeriesChartType.Line, Color = Color.Blue };
            var val = new Series("Val_Loss") { ChartType = SeriesChartType.Line, Color = Color.Orange };
            foreach (var e in epochs)
            {
                loss.Points.AddXY(e.Epoch, e.Loss);
                val.Points.AddXY(e.Epoch, e.ValLoss);
            }
            chart.Series.Add(loss);
            chart.Series.Add(val);
        }

        // lstViewScore 옆 chtErrorrate — 반복(Ep) × 오류율(loss)
        public static void InitializeErrorRateChart(Chart chart)
        {
            chart.ChartAreas.Clear();
            chart.Legends.Clear();
            chart.Series.Clear();

            var area = new ChartArea(AreaName) { BackColor = Color.FromArgb(48, 42, 41) };
            StyleAxis(area.AxisX, "반복 (Ep)");
            StyleAxis(area.AxisY, "오류율 (loss)");
            area.AxisX.Minimum = 1;
            area.AxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            chart.ChartAreas.Add(area);

            var legend = new Legend("Legend1")
            {
                Docking = Docking.Top,
                BackColor = Color.Transparent,
                ForeColor = Color.Gainsboro
            };
            chart.Legends.Add(legend);

            chart.Series.Add(new Series(SeriesTrainLoss)
            {
                ChartArea = AreaName,
                ChartType = SeriesChartType.Line,
                Color = Color.FromArgb(208, 111, 118),
                Legend = legend.Name,
                BorderWidth = 2
            });
            chart.Series.Add(new Series(SeriesValLoss)
            {
                ChartArea = AreaName,
                ChartType = SeriesChartType.Spline,
                Color = Color.FromArgb(109, 125, 218),
                Legend = legend.Name,
                BorderWidth = 2
            });
        }

        public static void BindErrorRateChart(
            Chart chart,
            IReadOnlyList<TrainingEpoch> epochs,
            int? plannedTotal = null)
        {
            if (chart.ChartAreas.Count == 0)
                InitializeErrorRateChart(chart);

            var train = chart.Series[SeriesTrainLoss];
            var val = chart.Series[SeriesValLoss];
            train.Points.Clear();
            val.Points.Clear();

            var area = chart.ChartAreas[AreaName];
            area.AxisX.Minimum = 1;
            area.AxisX.Maximum = double.NaN;

            if (epochs.Count == 0)
            {
                chart.Invalidate();
                return;
            }

            foreach (var e in epochs)
            {
                train.Points.AddXY(e.Epoch, e.Loss);
                val.Points.AddXY(e.Epoch, e.ValLoss);
            }

            int lastEp = epochs[^1].Epoch;
            int xMax = plannedTotal ?? lastEp;
            if (xMax > 0)
                area.AxisX.Maximum = xMax;

            chart.Invalidate();
        }

        private static void StyleAxis(Axis axis, string title)
        {
            axis.Title = title;
            axis.TitleForeColor = Color.Gainsboro;
            axis.LabelStyle.ForeColor = Color.Gainsboro;
            axis.LineColor = Color.DimGray;
            axis.MajorGrid.LineColor = Color.FromArgb(64, 58, 57);
            axis.MinorGrid.LineColor = Color.FromArgb(48, 42, 41);
        }
    }
}
