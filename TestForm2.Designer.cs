namespace Malcha
{
    partial class TestForm2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            chartLoss = new System.Windows.Forms.DataVisualization.Charting.Chart();
            ((System.ComponentModel.ISupportInitialize)chartLoss).BeginInit();
            SuspendLayout();
            // 
            // chartLoss
            // 
            chartArea1.Name = "ChartArea1";
            chartLoss.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            chartLoss.Legends.Add(legend1);
            chartLoss.Location = new Point(52, 66);
            chartLoss.Margin = new Padding(6, 6, 6, 6);
            chartLoss.Name = "chartLoss";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chartLoss.Series.Add(series1);
            chartLoss.Size = new Size(1226, 523);
            chartLoss.TabIndex = 4;
            chartLoss.Text = "chart1";
            // 
            // TestForm2
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1600, 960);
            Controls.Add(chartLoss);
            Margin = new Padding(6, 6, 6, 6);
            Name = "TestForm2";
            Text = "TestForm2";
            Load += TestForm2_Load;
            ((System.ComponentModel.ISupportInitialize)chartLoss).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chartLoss;
    }
}