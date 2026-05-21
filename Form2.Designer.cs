namespace Malcha
{
    partial class Form2
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
            panel1 = new Panel();
            btnDataManagement = new Button();
            btnTrainModel = new Button();
            lblTitle = new Label();
            panel2 = new Panel();
            lblAngleTitle = new Label();
            textBox1 = new TextBox();
            textBox2 = new TextBox();
            label1 = new Label();
            textBox3 = new TextBox();
            comboBox1 = new ComboBox();
            textBox4 = new TextBox();
            button1 = new Button();
            button2 = new Button();
            panel3 = new Panel();
            label2 = new Label();
            dataGridView1 = new DataGridView();
            textBox5 = new TextBox();
            button3 = new Button();
            번호 = new DataGridViewTextBoxColumn();
            모델이름 = new DataGridViewTextBoxColumn();
            모델종류 = new DataGridViewTextBoxColumn();
            사용한주행데이터폴더 = new DataGridViewTextBoxColumn();
            생성시간 = new DataGridViewTextBoxColumn();
            메모 = new DataGridViewTextBoxColumn();
            어떤모델을기반으로학습했는지 = new DataGridViewTextBoxColumn();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.FromArgb(33, 28, 29);
            panel1.Controls.Add(btnDataManagement);
            panel1.Controls.Add(btnTrainModel);
            panel1.Controls.Add(lblTitle);
            panel1.Location = new Point(0, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(636, 59);
            panel1.TabIndex = 1;
            // 
            // btnDataManagement
            // 
            btnDataManagement.Anchor = AnchorStyles.Left;
            btnDataManagement.BackColor = Color.FromArgb(53, 48, 49);
            btnDataManagement.FlatStyle = FlatStyle.Popup;
            btnDataManagement.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnDataManagement.ForeColor = SystemColors.ButtonHighlight;
            btnDataManagement.Location = new Point(242, 12);
            btnDataManagement.Name = "btnDataManagement";
            btnDataManagement.Size = new Size(103, 23);
            btnDataManagement.TabIndex = 6;
            btnDataManagement.Text = "데이터 관리";
            btnDataManagement.UseVisualStyleBackColor = false;
            // 
            // btnTrainModel
            // 
            btnTrainModel.Anchor = AnchorStyles.Left;
            btnTrainModel.BackColor = Color.FromArgb(53, 48, 49);
            btnTrainModel.FlatStyle = FlatStyle.Popup;
            btnTrainModel.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnTrainModel.ForeColor = SystemColors.ButtonHighlight;
            btnTrainModel.Location = new Point(351, 12);
            btnTrainModel.Name = "btnTrainModel";
            btnTrainModel.Size = new Size(103, 23);
            btnTrainModel.TabIndex = 4;
            btnTrainModel.Text = "모델 학습";
            btnTrainModel.UseVisualStyleBackColor = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblTitle.ForeColor = Color.FromArgb(227, 98, 132);
            lblTitle.Location = new Point(12, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(138, 32);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "DonkeyCar";
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(20, 20, 20);
            panel2.Controls.Add(button2);
            panel2.Controls.Add(button1);
            panel2.Controls.Add(textBox4);
            panel2.Controls.Add(comboBox1);
            panel2.Controls.Add(textBox3);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(textBox2);
            panel2.Controls.Add(textBox1);
            panel2.Controls.Add(lblAngleTitle);
            panel2.Location = new Point(0, 61);
            panel2.Name = "panel2";
            panel2.Size = new Size(636, 172);
            panel2.TabIndex = 2;
            // 
            // lblAngleTitle
            // 
            lblAngleTitle.AutoSize = true;
            lblAngleTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblAngleTitle.Location = new Point(242, 10);
            lblAngleTitle.Name = "lblAngleTitle";
            lblAngleTitle.Size = new Size(119, 20);
            lblAngleTitle.TabIndex = 1;
            lblAngleTitle.Text = "설정값 덮어쓰기";
            // 
            // textBox1
            // 
            textBox1.BackColor = Color.FromArgb(53, 48, 49);
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            textBox1.ForeColor = SystemColors.MenuBar;
            textBox1.Location = new Point(3, 37);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(311, 25);
            textBox1.TabIndex = 3;
            textBox1.Text = "최대 학습 반복 횟수(Epoch): 2";
            // 
            // textBox2
            // 
            textBox2.BackColor = Color.FromArgb(103, 98, 98);
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox2.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            textBox2.ForeColor = SystemColors.MenuBar;
            textBox2.Location = new Point(322, 37);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(307, 25);
            textBox2.TabIndex = 4;
            textBox2.Text = "새 값 입력";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.ButtonHighlight;
            label1.Location = new Point(242, 75);
            label1.Name = "label1";
            label1.Size = new Size(130, 20);
            label1.TabIndex = 5;
            label1.Text = "AI 주행 모델 학습";
            // 
            // textBox3
            // 
            textBox3.BackColor = Color.FromArgb(103, 98, 98);
            textBox3.BorderStyle = BorderStyle.FixedSingle;
            textBox3.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            textBox3.ForeColor = SystemColors.MenuBar;
            textBox3.Location = new Point(322, 101);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(307, 25);
            textBox3.TabIndex = 7;
            textBox3.Text = "메모 / 설명";
            // 
            // comboBox1
            // 
            comboBox1.BackColor = Color.FromArgb(103, 98, 98);
            comboBox1.FlatStyle = FlatStyle.Popup;
            comboBox1.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            comboBox1.ForeColor = SystemColors.MenuBar;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "linear", "", "categorical", "", "rnn", "", "imu" });
            comboBox1.Location = new Point(165, 101);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(148, 25);
            comboBox1.TabIndex = 3;
            // 
            // textBox4
            // 
            textBox4.BackColor = Color.FromArgb(53, 48, 49);
            textBox4.BorderStyle = BorderStyle.FixedSingle;
            textBox4.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            textBox4.ForeColor = SystemColors.MenuBar;
            textBox4.Location = new Point(6, 101);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(155, 25);
            textBox4.TabIndex = 8;
            textBox4.Text = "모델 종류";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Left;
            button1.BackColor = Color.FromArgb(53, 48, 49);
            button1.FlatStyle = FlatStyle.Popup;
            button1.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button1.ForeColor = SystemColors.ButtonHighlight;
            button1.Location = new Point(3, 140);
            button1.Name = "button1";
            button1.Size = new Size(311, 23);
            button1.TabIndex = 9;
            button1.Text = "전이 학습 모델 선택";
            button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Left;
            button2.BackColor = Color.FromArgb(198, 100, 114);
            button2.FlatStyle = FlatStyle.Popup;
            button2.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button2.ForeColor = SystemColors.ButtonHighlight;
            button2.Location = new Point(322, 140);
            button2.Name = "button2";
            button2.Size = new Size(307, 23);
            button2.TabIndex = 10;
            button2.Text = "학습 시작";
            button2.UseVisualStyleBackColor = false;
            // 
            // panel3
            // 
            panel3.BackColor = Color.FromArgb(33, 28, 29);
            panel3.Controls.Add(button3);
            panel3.Controls.Add(textBox5);
            panel3.Controls.Add(dataGridView1);
            panel3.Controls.Add(label2);
            panel3.Location = new Point(0, 230);
            panel3.Name = "panel3";
            panel3.Size = new Size(636, 395);
            panel3.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.ButtonHighlight;
            label2.Location = new Point(242, 6);
            label2.Name = "label2";
            label2.Size = new Size(124, 20);
            label2.TabIndex = 11;
            label2.Text = "학습된 모델 목록";
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Columns.AddRange(new DataGridViewColumn[] { 번호, 모델이름, 모델종류, 사용한주행데이터폴더, 생성시간, 메모, 어떤모델을기반으로학습했는지 });
            dataGridView1.Location = new Point(0, 29);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Size = new Size(631, 311);
            dataGridView1.TabIndex = 12;
            // 
            // textBox5
            // 
            textBox5.BackColor = Color.FromArgb(53, 48, 49);
            textBox5.BorderStyle = BorderStyle.FixedSingle;
            textBox5.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            textBox5.ForeColor = SystemColors.MenuBar;
            textBox5.Location = new Point(7, 345);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(311, 25);
            textBox5.TabIndex = 13;
            textBox5.Text = "여러 데이터셋(Tub) 묶기";
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Left;
            button3.BackColor = Color.FromArgb(103, 98, 98);
            button3.FlatStyle = FlatStyle.Popup;
            button3.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button3.ForeColor = SystemColors.ButtonHighlight;
            button3.Location = new Point(324, 347);
            button3.Name = "button3";
            button3.Size = new Size(307, 23);
            button3.TabIndex = 11;
            button3.Text = "비활성화";
            button3.UseVisualStyleBackColor = false;
            // 
            // 번호
            // 
            번호.HeaderText = "번호";
            번호.Name = "번호";
            // 
            // 모델이름
            // 
            모델이름.HeaderText = "모델이름";
            모델이름.Name = "모델이름";
            // 
            // 모델종류
            // 
            모델종류.HeaderText = "모델종류";
            모델종류.Name = "모델종류";
            // 
            // 사용한주행데이터폴더
            // 
            사용한주행데이터폴더.HeaderText = "데이터폴더";
            사용한주행데이터폴더.Name = "사용한주행데이터폴더";
            // 
            // 생성시간
            // 
            생성시간.HeaderText = "생성시간";
            생성시간.Name = "생성시간";
            // 
            // 메모
            // 
            메모.HeaderText = "메모";
            메모.Name = "메모";
            // 
            // 어떤모델을기반으로학습했는지
            // 
            어떤모델을기반으로학습했는지.HeaderText = "모델기반";
            어떤모델을기반으로학습했는지.Name = "어떤모델을기반으로학습했는지";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(634, 623);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "Form2";
            Text = "Form2";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnTrainModel;
        private Label lblTitle;
        private Button btnDataManagement;
        private Panel panel2;
        private Label lblAngleTitle;
        private TextBox textBox2;
        private TextBox textBox1;
        private ComboBox comboBox1;
        private TextBox textBox3;
        private Label label1;
        private Button button2;
        private Button button1;
        private TextBox textBox4;
        private Panel panel3;
        private DataGridView dataGridView1;
        private Label label2;
        private Button button3;
        private TextBox textBox5;
        private DataGridViewTextBoxColumn 번호;
        private DataGridViewTextBoxColumn 모델이름;
        private DataGridViewTextBoxColumn 모델종류;
        private DataGridViewTextBoxColumn 사용한주행데이터폴더;
        private DataGridViewTextBoxColumn 생성시간;
        private DataGridViewTextBoxColumn 메모;
        private DataGridViewTextBoxColumn 어떤모델을기반으로학습했는지;
    }
}