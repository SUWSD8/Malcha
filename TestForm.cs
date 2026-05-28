using Malcha.Model;
using Malcha.Repository;
using Malcha.Service;
using Malcha.UI;

namespace Malcha
{
    public partial class TestForm : Form
    {
        public TestForm() => InitializeComponent();

        private void btnTest_Click(object sender, EventArgs e) =>
            ButtonAdapter.RunCatalogAnalysis(btnTest);

        private async void btnTrain_Click(object sender, EventArgs e)
        {
            btnTrain.Enabled = false;
            try
            {
                if (await WslTrainingService.Instance.TrainAsync("data", "mypilot.h5"))
                    MessageBox.Show("학습 완료");
            }
            finally { btnTrain.Enabled = true; }
        }

        private async void btnTest3_Click(object sender, EventArgs e)
        {
            var r = await ScoreAnalyzer.Instance.AnalyzeAsync(WslTrainingService.DefaultDatabasePath, "mypilot");
            if (r != null) MessageBox.Show(ScoreAnalyzer.Instance.BuildSummary(r).ToDisplayMessage());
        }

        private void btnTest4_Click(object sender, EventArgs e)
        {
            var model = ResultRepository.Instance.FindByName("mypilot");
            ChartAdapter.InitializeLossChart(chartLoss);
            ChartAdapter.DrawLossChart(chartLoss, model?.Epochs ?? new List<TrainingEpoch>());
        }
    }
}
