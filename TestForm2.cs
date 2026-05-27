using Malcha.Repository;
using Malcha.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Malcha
{
    public partial class TestForm2 : Form
    {
        public TestForm2()
        {
            InitializeComponent();

        }
        public void TestForm2_Load(object sender, EventArgs e)
        {
            var model = DonkeyRepository.Instance.FindByName("mypilot");
            var history = model?.History;
            ChartAdapter.InitializeLossChart(chartLoss);
            ChartAdapter.DrawLossChart(chartLoss, history);
        }
    }
}
