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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
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
            btnTest.Location = new Point(30, 28);
            btnTest.Margin = new Padding(2, 1, 2, 1);
            btnTest.Name = "btnTest";
            btnTest.Size = new Size(75, 22);
            btnTest.TabIndex = 0;
            btnTest.Text = "테스트";
            btnTest.UseVisualStyleBackColor = true;
            btnTest.Click += btnTest_Click;
            // 
            // btnTrain
            // 
            btnTrain.Location = new Point(138, 28);
            btnTrain.Margin = new Padding(2, 1, 2, 1);
            btnTrain.Name = "btnTrain";
            btnTrain.Size = new Size(75, 22);
            btnTrain.TabIndex = 1;
            btnTrain.Text = "테스트2";
            btnTrain.UseVisualStyleBackColor = true;
            btnTrain.Click += btnTrain_Click;
            // 
            // btnTest3
            // 
            btnTest3.Location = new Point(246, 28);
            btnTest3.Name = "btnTest3";
            btnTest3.Size = new Size(75, 23);
            btnTest3.TabIndex = 2;
            btnTest3.Text = "테스트3";
            btnTest3.UseVisualStyleBackColor = true;
            btnTest3.Click += btnTest3_Click;
            // 
            // chartLoss
            // 
            chartArea2.Name = "ChartArea1";
            chartLoss.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            chartLoss.Legends.Add(legend2);
            chartLoss.Location = new Point(30, 69);
            chartLoss.Name = "chartLoss";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            chartLoss.Series.Add(series2);
            chartLoss.Size = new Size(291, 249);
            chartLoss.TabIndex = 3;
            chartLoss.Text = "chart1";
            // 
            // btnTest4
            // 
            btnTest4.Location = new Point(340, 28);
            btnTest4.Name = "btnTest4";
            btnTest4.Size = new Size(75, 23);
            btnTest4.TabIndex = 4;
            btnTest4.Text = "테스트4";
            btnTest4.UseVisualStyleBackColor = true;
            btnTest4.Click += btnTest4_Click;
            // 
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(506, 349);
            Controls.Add(btnTest4);
            Controls.Add(chartLoss);
            Controls.Add(btnTest3);
            Controls.Add(btnTrain);
            Controls.Add(btnTest);
            Margin = new Padding(2, 1, 2, 1);
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