namespace Malcha
{
    partial class TestForm
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
            btnTest = new Button();
            btnTrain = new Button();
            btnTest3 = new Button();
            chartLoss = new System.Windows.Forms.DataVisualization.Charting.Chart();
            btnTest4 = new Button();
            ((System.ComponentModel.ISupportInitialize)chartLoss).BeginInit();
            SuspendLayout();
            // 
            // btnTest
            // 
            btnTest.Location = new Point(60, 60);
            btnTest.Margin = new Padding(4, 2, 4, 2);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(150, 47);
            btnTest.TabIndex = 0;
            btnTest.Text = "테스트";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            // 
            // btnTrain
            // 
            btnTrain.Location = new Point(276, 60);
            btnTrain.Margin = new Padding(4, 2, 4, 2);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(150, 47);
            btnTrain.TabIndex = 1;
            btnTrain.Text = "테스트2";
            btnTrain.UseVisualStyleBackColor = true;
            btnTrain.Click += btnTrain_Click;
            // 
            // btnTest3
            // 
            btnTest3.Location = new Point(492, 60);
            btnTest3.Margin = new Padding(6, 6, 6, 6);
            btnTest3.Name = "btnTest3";
            btnTest3.Size = new Size(150, 49);
            btnTest3.TabIndex = 2;
            btnTest3.Text = "테스트3";
            btnTest3.UseVisualStyleBackColor = true;
            btnTest3.Click += btnTest3_Click;
            // 
            // chartLoss
            // 
            chartArea1.Name = "ChartArea1";
            chartLoss.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            chartLoss.Legends.Add(legend1);
            chartLoss.Location = new Point(60, 147);
            chartLoss.Margin = new Padding(6, 6, 6, 6);
            chartLoss.Name = "chartLoss";
            series1.ChartArea = "ChartArea1";
            series1.Legend = "Legend1";
            series1.Name = "Series1";
            chartLoss.Series.Add(series1);
            chartLoss.Size = new Size(937, 531);
            chartLoss.TabIndex = 3;
            chartLoss.Text = "chart1";
            // 
            // btnTest4
            // 
            btnTest4.Location = new Point(680, 60);
            btnTest4.Margin = new Padding(6, 6, 6, 6);
            btnTest4.Name = "btnTest4";
            btnTest4.Size = new Size(150, 49);
            btnTest4.TabIndex = 4;
            btnTest4.Text = "테스트4";
            btnTest4.UseVisualStyleBackColor = true;
            btnTest4.Click += btnTest4_Click;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(14F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1012, 745);
            Controls.Add(btnTest4);
            Controls.Add(chartLoss);
            Controls.Add(btnTest3);
            Controls.Add(btnTrain);
            Controls.Add(btnTest);
            Margin = new Padding(4, 2, 4, 2);
            Name = "TestForm";
            Text = "TestForm";
            ((System.ComponentModel.ISupportInitialize)chartLoss).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnTest;
        private Button btnTrain;
        private Button btnTest3;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartLoss;
        private Button btnTest4;
    }
}