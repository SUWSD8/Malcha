using Malcha.Controller;
using Malcha.Data;
using Malcha.Model;
using Malcha.Repository;
using Malcha.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Malcha
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }
        

        private async void btnTest_Click(object sender, EventArgs e)
        {
            ButtonAdapter.RunCatalogAnalysis(btnTest);
        }

        private async void btnTrain_Click(object sender, EventArgs e)
        {
            ButtonAdapter.RunModelTraining(btnTrain);
        }

        private async void btnTest3_Click(object sender, EventArgs e)
        {
            ButtonAdapter.ParseTrainingHistory(btnTest3, "mypilot");
        }

        private void btnTest4_Click(object sender, EventArgs e)
        {
            var model = DonkeyRepository.Instance.FindByName("mypilot");
            var history = model?.History;
            ChartAdapter.InitializeLossChart(chartLoss);
            ChartAdapter.DrawLossChart(chartLoss, history);
            

        }
    }
}
