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
            btnTest = new Button();
            btnTrain = new Button();
            btnTest3 = new Button();
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
            // TestForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 211);
            Controls.Add(btnTest3);
            Controls.Add(btnTrain);
            Controls.Add(btnTest);
            Margin = new Padding(2, 1, 2, 1);
            Name = "TestForm";
            Text = "TestForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btnTest;
        private Button btnTrain;
        private Button btnTest3;
    }
}