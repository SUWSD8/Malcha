using Malcha.Repository;
using Malcha.UI;

namespace Malcha
{
    public partial class TestForm2 : Form
    {
        public TestForm2() => InitializeComponent();

        public void TestForm2_Load(object sender, EventArgs e)
        {
            var model = ResultRepository.Instance.FindByName("mypilot");
            ChartAdapter.InitializeLossChart(chartLoss);
            ChartAdapter.DrawLossChart(chartLoss, model?.Epochs ?? new());
        }
    }
}
