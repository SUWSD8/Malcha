using Malcha.Controller;
using Malcha.Model;
using Malcha.Repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

namespace Malcha.UI
{
    internal static class ChartAdapter
    {
        public static void InitializeLossChart(Chart targetChart)
        {
            targetChart.ChartAreas.Clear();
            targetChart.Legends.Clear();

            ChartArea chartArea = new ChartArea("MainArea");
            chartArea.AxisX.Title = "Epoch (에포크)";
            chartArea.AxisY.Title = "Loss (손실값)";
            chartArea.AxisX.Minimum = 1;
            targetChart.ChartAreas.Add(chartArea);

            Legend legend = new Legend("DefaultLegend");
            legend.Docking = Docking.Top;
            targetChart.Legends.Add(legend);
        }
        // 2. 실제 데이터를 차트에 그리기
        public static void DrawLossChart(Chart targetChart, List<TrainedData> history)
        {

            if (history == null || history.Count == 0)
            {
                throw new Exception("훈련 기록이 없습니다. 모델 학습이 제대로 이루어졌는지 확인하세요.");
                return;
            }

            targetChart.Series.Clear();

            Series seriesLoss = new Series("훈련 손실 (Loss)")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Blue,
                MarkerStyle = MarkerStyle.Circle
            };

            Series seriesValLoss = new Series("검증 손실 (Val_Loss)")
            {
                ChartType = SeriesChartType.Line,
                BorderWidth = 2,
                Color = Color.Orange,
                MarkerStyle = MarkerStyle.Square
            };

            foreach (var data in history)
            {
                seriesLoss.Points.AddXY(data.Epoch, data.Loss);
                seriesValLoss.Points.AddXY(data.Epoch, data.ValLoss);
            }

            targetChart.Series.Add(seriesLoss);
            targetChart.Series.Add(seriesValLoss);
        }
    }
}
