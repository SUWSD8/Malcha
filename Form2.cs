using Malcha.Controller;
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
    public partial class Form2 : Form
    {
        /*public Form2()
        {
            InitializeComponent();
            Form2_Load();

        }
        /*private async void Form2_Load()
        {
            await ButtonAdapter.ParseTrainingHistory(btnRunTraining, "mypilot");
            RefreshModelList();
        }

        private void btnDataManagement_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /*private void btnUpdateComment_Click(object sender, EventArgs e)
        {
            string name = txtMyPilot.Text;
            string newComment = txtModelMemo.Text;
            bool success = TrainModelController.Instance.UpdateModelComment(name, newComment);
            if (success)
            {
                MessageBox.Show("Model comment updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshModelList();
            }
            else
            {
                MessageBox.Show("Failed to update model comment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDeleteModel_Click(object sender, EventArgs e)
        {
            string name = txtMyPilot.Text;
            bool success = TrainModelController.Instance.DeleteModel(name);
            if (success)
            {
                MessageBox.Show("Model deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshModelList();
            }
            else
            {
                MessageBox.Show("Failed to delete model.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnShowTrainingHistory_Click(object sender, EventArgs e)
        {

            TestForm2 chartForm = new TestForm2();
            chartForm.Show();
        }

        private async void btnRunAnalysis_Click(object sender, EventArgs e)
        {
            //string name = txtMyPilot.Text;
            btnRunTraining.Enabled = false;
            try
            {
                await ButtonAdapter.RunModelTraining(btnRunTraining, name);

                await ButtonAdapter.ParseTrainingHistory(btnRunTraining, name);

                RefreshModelList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRunTraining.Enabled = true;
            }
        }
        private void RefreshModelList()
        {
            var models = TrainModelController.Instance.GetAllTrainedModels();
            dgvPilotList.AutoGenerateColumns = false;
            dgvPilotList.DataSource = null;
            dgvPilotList.DataSource = models;
        }*/
    }
}
