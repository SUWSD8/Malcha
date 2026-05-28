using System.Windows.Forms.DataVisualization.Charting;

namespace Malcha.UI
{
    internal static class ChartAdapter
    {
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

        public static void DrawLossChart(Chart chart, List<Model.TrainingEpoch> epochs)
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
    }
}
