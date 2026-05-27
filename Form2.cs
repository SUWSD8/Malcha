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
        public Form2()
        {
            InitializeComponent();
        }

        private void btnDataManagement_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUpdateComment_Click(object sender, EventArgs e)
        {
            bool success = TrainModelController.Instance.UpdateModelComment("ModelName", "New comment for the model.");
            if (success)
            {
                MessageBox.Show("Model comment updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed to update model comment.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnDeleteModel_Click(object sender, EventArgs e)
        {
            bool success = TrainModelController.Instance.DeleteModel("ModelName");
            if (success)
            {
                MessageBox.Show("Model deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
}
