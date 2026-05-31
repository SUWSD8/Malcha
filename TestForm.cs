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
                if (!WslTrainingService.Instance.IsConfigured)
                {
                    MessageBox.Show("mycar 경로를 먼저 설정하세요.");
                    return;
                }
                if (await WslTrainingService.Instance.TrainAsync("mypilot.h5"))
                    MessageBox.Show("학습 완료");
            }
            finally { btnTrain.Enabled = true; }
        }

        private async void btnTest3_Click(object sender, EventArgs e)
        {
            if (!WslTrainingService.Instance.IsConfigured) { MessageBox.Show("mycar 경로 미설정"); return; }
            var r = await ScoreAnalyzer.Instance.AnalyzeAsync(WslTrainingService.Instance.DatabaseUncPath, "mypilot");
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
