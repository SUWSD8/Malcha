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
            panel6 = new Panel();
            button2 = new Button();
            button1 = new Button();
            textBox2 = new TextBox();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            lblAngleTitle = new Label();
            panel3 = new Panel();
            dgvPilotList = new DataGridView();
            이름 = new DataGridViewTextBoxColumn();
            파일럿 = new DataGridViewTextBoxColumn();
            타입 = new DataGridViewTextBoxColumn();
            데이터저장소 = new DataGridViewTextBoxColumn();
            시간 = new DataGridViewTextBoxColumn();
            전이학습 = new DataGridViewTextBoxColumn();
            설명 = new DataGridViewTextBoxColumn();
            lblViewPilots = new Label();
            panel4 = new Panel();
            button10 = new Button();
            button9 = new Button();
            button8 = new Button();
            button7 = new Button();
            button6 = new Button();
            button3 = new Button();
            label5 = new Label();
            label1 = new Label();
            label7 = new Label();
            label8 = new Label();
            comboBox1 = new ComboBox();
            textBox1 = new TextBox();
            label9 = new Label();
            panel5 = new Panel();
            button4 = new Button();
            button5 = new Button();
            panel7 = new Panel();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).BeginInit();
            panel4.SuspendLayout();
            panel5.SuspendLayout();
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
            panel1.Size = new Size(978, 59);
            panel1.TabIndex = 1;
            // 
            // btnDataManagement
            // 
            btnDataManagement.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDataManagement.BackColor = Color.FromArgb(53, 48, 49);
            btnDataManagement.FlatStyle = FlatStyle.Popup;
            btnDataManagement.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnDataManagement.ForeColor = SystemColors.ButtonHighlight;
            btnDataManagement.Location = new Point(752, 10);
            btnDataManagement.Name = "btnDataManagement";
            btnDataManagement.Size = new Size(103, 23);
            btnDataManagement.TabIndex = 6;
            btnDataManagement.Text = "데이터 관리";
            btnDataManagement.UseVisualStyleBackColor = false;
            btnDataManagement.Click += btnDataManagement_Click;
            // 
            // btnTrainModel
            // 
            btnTrainModel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTrainModel.BackColor = Color.FromArgb(53, 48, 49);
            btnTrainModel.FlatStyle = FlatStyle.Popup;
            btnTrainModel.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnTrainModel.ForeColor = SystemColors.ButtonHighlight;
            btnTrainModel.Location = new Point(861, 10);
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
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(20, 20, 20);
            panel2.Controls.Add(panel6);
            panel2.Controls.Add(button2);
            panel2.Controls.Add(button1);
            panel2.Controls.Add(textBox2);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(label3);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(lblAngleTitle);
            panel2.Location = new Point(0, 61);
            panel2.Name = "panel2";
            panel2.Size = new Size(978, 106);
            panel2.TabIndex = 2;
            // 
            // panel6
            // 
            panel6.Location = new Point(0, 105);
            panel6.Name = "panel6";
            panel6.Size = new Size(638, 10);
            panel6.TabIndex = 0;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button2.BackColor = Color.FromArgb(53, 48, 49);
            button2.FlatStyle = FlatStyle.Popup;
            button2.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button2.ForeColor = SystemColors.ButtonHighlight;
            button2.Location = new Point(788, 68);
            button2.Name = "button2";
            button2.Size = new Size(176, 29);
            button2.TabIndex = 21;
            button2.Text = "myconfig 저장";
            button2.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button1.BackColor = Color.FromArgb(53, 48, 49);
            button1.FlatStyle = FlatStyle.Popup;
            button1.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button1.ForeColor = SystemColors.ButtonHighlight;
            button1.Location = new Point(238, 69);
            button1.Name = "button1";
            button1.Size = new Size(177, 29);
            button1.TabIndex = 20;
            button1.Text = "+";
            button1.UseVisualStyleBackColor = false;
            // 
            // textBox2
            // 
            textBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            textBox2.BackColor = Color.FromArgb(53, 48, 49);
            textBox2.BorderStyle = BorderStyle.FixedSingle;
            textBox2.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            textBox2.ForeColor = SystemColors.MenuBar;
            textBox2.Location = new Point(533, 70);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(247, 25);
            textBox2.TabIndex = 19;
            textBox2.Text = "1";
            textBox2.TextAlign = HorizontalAlignment.Center;
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Top;
            label4.AutoSize = true;
            label4.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label4.ForeColor = SystemColors.ButtonHighlight;
            label4.Location = new Point(448, 75);
            label4.Name = "label4";
            label4.Size = new Size(59, 20);
            label4.TabIndex = 13;
            label4.Text = "열 개수";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top;
            label3.AutoSize = true;
            label3.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label3.ForeColor = SystemColors.ButtonHighlight;
            label3.Location = new Point(72, 72);
            label3.Name = "label3";
            label3.Size = new Size(109, 20);
            label3.TabIndex = 12;
            label3.Text = "설정 항목 추가";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top;
            label2.AutoSize = true;
            label2.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label2.ForeColor = SystemColors.ButtonHighlight;
            label2.Location = new Point(566, 17);
            label2.Name = "label2";
            label2.Size = new Size(397, 45);
            label2.TabIndex = 11;
            label2.Text = "드롭다운 메뉴를 사용해 설정 파라미터를 수정하세요.\n+ 버튼으로 행(row)을 추가하여 더 많은 파라미터를 관리할 수 있습니다.\nJSON 문법을 사용하세요. 예: 문자열은 큰따옴표 사용, true/false 사용.";
            // 
            // lblAngleTitle
            // 
            lblAngleTitle.Anchor = AnchorStyles.Top;
            lblAngleTitle.AutoSize = true;
            lblAngleTitle.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblAngleTitle.Location = new Point(72, 17);
            lblAngleTitle.Name = "lblAngleTitle";
            lblAngleTitle.Size = new Size(114, 25);
            lblAngleTitle.TabIndex = 1;
            lblAngleTitle.Text = "설정 편집기";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel3.BackColor = Color.FromArgb(33, 28, 29);
            panel3.Controls.Add(dgvPilotList);
            panel3.Controls.Add(lblViewPilots);
            panel3.Location = new Point(0, 318);
            panel3.Name = "panel3";
            panel3.Size = new Size(982, 419);
            panel3.TabIndex = 3;
            // 
            // dgvPilotList
            // 
            dgvPilotList.AllowUserToAddRows = false;
            dgvPilotList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPilotList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPilotList.BackgroundColor = Color.FromArgb(103, 98, 98);
            dgvPilotList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPilotList.Columns.AddRange(new DataGridViewColumn[] { 이름, 파일럿, 타입, 데이터저장소, 시간, 전이학습, 설명 });
            dgvPilotList.Location = new Point(0, 29);
            dgvPilotList.Name = "dgvPilotList";
            dgvPilotList.RowHeadersVisible = false;
            dgvPilotList.Size = new Size(977, 381);
            dgvPilotList.TabIndex = 12;
            // 
            // 이름
            // 
            이름.HeaderText = "이름";
            이름.Name = "이름";
            // 
            // 파일럿
            // 
            파일럿.HeaderText = "파일럿";
            파일럿.Name = "파일럿";
            // 
            // 타입
            // 
            타입.HeaderText = "타입";
            타입.Name = "타입";
            // 
            // 데이터저장소
            // 
            데이터저장소.HeaderText = "데이터 저장소";
            데이터저장소.Name = "데이터저장소";
            // 
            // 시간
            // 
            시간.HeaderText = "시간";
            시간.Name = "시간";
            // 
            // 전이학습
            // 
            전이학습.HeaderText = "전이 학습";
            전이학습.Name = "전이학습";
            // 
            // 설명
            // 
            설명.HeaderText = "설명";
            설명.Name = "설명";
            // 
            // lblViewPilots
            // 
            lblViewPilots.Anchor = AnchorStyles.Top;
            lblViewPilots.AutoSize = true;
            lblViewPilots.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblViewPilots.ForeColor = SystemColors.ButtonHighlight;
            lblViewPilots.Location = new Point(415, 6);
            lblViewPilots.Name = "lblViewPilots";
            lblViewPilots.Size = new Size(124, 20);
            lblViewPilots.TabIndex = 11;
            lblViewPilots.Text = "학습된 모델 목록";
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel4.BackColor = Color.FromArgb(33, 28, 29);
            panel4.Controls.Add(button10);
            panel4.Controls.Add(button9);
            panel4.Controls.Add(button8);
            panel4.Controls.Add(button7);
            panel4.Controls.Add(button6);
            panel4.Controls.Add(button3);
            panel4.Controls.Add(label5);
            panel4.Controls.Add(label1);
            panel4.Location = new Point(0, 734);
            panel4.Name = "panel4";
            panel4.Size = new Size(982, 118);
            panel4.TabIndex = 4;
            // 
            // button10
            // 
            button10.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button10.BackColor = Color.FromArgb(53, 48, 49);
            button10.FlatStyle = FlatStyle.Popup;
            button10.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button10.ForeColor = SystemColors.ButtonHighlight;
            button10.Location = new Point(739, 70);
            button10.Name = "button10";
            button10.Size = new Size(126, 36);
            button10.TabIndex = 38;
            button10.Text = "학습 기록";
            button10.UseVisualStyleBackColor = false;
            // 
            // button9
            // 
            button9.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button9.BackColor = Color.FromArgb(53, 48, 49);
            button9.FlatStyle = FlatStyle.Popup;
            button9.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button9.ForeColor = SystemColors.ButtonHighlight;
            button9.Location = new Point(607, 69);
            button9.Name = "button9";
            button9.Size = new Size(126, 36);
            button9.TabIndex = 37;
            button9.Text = "설정 보기";
            button9.UseVisualStyleBackColor = false;
            // 
            // button8
            // 
            button8.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button8.BackColor = Color.FromArgb(53, 48, 49);
            button8.FlatStyle = FlatStyle.Popup;
            button8.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button8.ForeColor = SystemColors.ButtonHighlight;
            button8.Location = new Point(475, 70);
            button8.Name = "button8";
            button8.Size = new Size(126, 36);
            button8.TabIndex = 36;
            button8.Text = "코멘트 수정";
            button8.UseVisualStyleBackColor = false;
            button8.Click += btnUpdateComment_Click;
            // 
            // button7
            // 
            button7.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button7.BackColor = Color.FromArgb(53, 48, 49);
            button7.FlatStyle = FlatStyle.Popup;
            button7.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button7.ForeColor = SystemColors.ButtonHighlight;
            button7.Location = new Point(343, 70);
            button7.Name = "button7";
            button7.Size = new Size(126, 36);
            button7.TabIndex = 35;
            button7.Text = "파일럿 삭제";
            button7.UseVisualStyleBackColor = false;
            button7.Click += btnDeleteModel_Click;
            // 
            // button6
            // 
            button6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button6.BackColor = Color.FromArgb(53, 48, 49);
            button6.FlatStyle = FlatStyle.Popup;
            button6.Font = new Font("맑은 고딕", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button6.ForeColor = SystemColors.ButtonHighlight;
            button6.Location = new Point(245, 70);
            button6.Name = "button6";
            button6.Size = new Size(92, 36);
            button6.TabIndex = 34;
            button6.Text = " 삭제 \r\n활성화";
            button6.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button3.BackColor = Color.FromArgb(53, 48, 49);
            button3.FlatStyle = FlatStyle.Popup;
            button3.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button3.ForeColor = SystemColors.ButtonHighlight;
            button3.Location = new Point(78, 70);
            button3.Name = "button3";
            button3.Size = new Size(161, 36);
            button3.TabIndex = 33;
            button3.Text = "내 파일럿";
            button3.UseVisualStyleBackColor = false;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top;
            label5.AutoSize = true;
            label5.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label5.ForeColor = SystemColors.ButtonHighlight;
            label5.Location = new Point(585, 20);
            label5.Name = "label5";
            label5.Size = new Size(354, 45);
            label5.TabIndex = 12;
            label5.Text = "파일럿을 선택하여 설정을 확인하거나 코멘트를 수정하거나 \r\n삭제할 수 있습니다. 삭제 시 디스크 파일과 데이터베이스 항목이\r\n함께 제거됩니다.";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top;
            label1.AutoSize = true;
            label1.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label1.ForeColor = SystemColors.ButtonHighlight;
            label1.Location = new Point(72, 20);
            label1.Name = "label1";
            label1.Size = new Size(204, 25);
            label1.TabIndex = 2;
            label1.Text = "파일럿 조회 및 편집기";
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top;
            label7.AutoSize = true;
            label7.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label7.ForeColor = SystemColors.ButtonHighlight;
            label7.Location = new Point(625, 15);
            label7.Name = "label7";
            label7.Size = new Size(326, 45);
            label7.TabIndex = 25;
            label7.Text = "위의 설정 파라미터를 사용하여 파일럿(모델)을 학습합니다.\n모델 타입과 필요하면 전이 학습 모델을 선택하세요.\n설명을 위한 코멘트를 입력할 수 있습니다.";
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top;
            label8.AutoSize = true;
            label8.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label8.ForeColor = SystemColors.ButtonHighlight;
            label8.Location = new Point(72, 15);
            label8.Name = "label8";
            label8.Size = new Size(88, 25);
            label8.TabIndex = 24;
            label8.Text = "트레이너";
            // 
            // comboBox1
            // 
            comboBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            comboBox1.BackColor = Color.FromArgb(103, 98, 98);
            comboBox1.FlatStyle = FlatStyle.Popup;
            comboBox1.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            comboBox1.ForeColor = SystemColors.MenuBar;
            comboBox1.FormattingEnabled = true;
            comboBox1.Items.AddRange(new object[] { "linear", "categorical", "rnn", "imu" });
            comboBox1.Location = new Point(238, 79);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(248, 25);
            comboBox1.TabIndex = 19;
            // 
            // textBox1
            // 
            textBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            textBox1.BackColor = Color.FromArgb(103, 98, 98);
            textBox1.BorderStyle = BorderStyle.FixedSingle;
            textBox1.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            textBox1.ForeColor = SystemColors.MenuBar;
            textBox1.Location = new Point(492, 79);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(471, 25);
            textBox1.TabIndex = 21;
            textBox1.Text = "메모 / 설명";
            // 
            // label9
            // 
            label9.Anchor = AnchorStyles.Top;
            label9.AutoSize = true;
            label9.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            label9.ForeColor = SystemColors.ButtonHighlight;
            label9.Location = new Point(72, 79);
            label9.Name = "label9";
            label9.Size = new Size(109, 20);
            label9.TabIndex = 20;
            label9.Text = "모델 타입 선택";
            // 
            // panel5
            // 
            panel5.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel5.BackColor = Color.FromArgb(20, 20, 20);
            panel5.Controls.Add(button4);
            panel5.Controls.Add(button5);
            panel5.Controls.Add(label7);
            panel5.Controls.Add(comboBox1);
            panel5.Controls.Add(label8);
            panel5.Controls.Add(label9);
            panel5.Controls.Add(textBox1);
            panel5.Location = new Point(0, 173);
            panel5.Name = "panel5";
            panel5.Size = new Size(977, 150);
            panel5.TabIndex = 5;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button4.BackColor = Color.FromArgb(198, 100, 114);
            button4.FlatStyle = FlatStyle.Popup;
            button4.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button4.ForeColor = SystemColors.ButtonHighlight;
            button4.Location = new Point(493, 110);
            button4.Name = "button4";
            button4.Size = new Size(471, 29);
            button4.TabIndex = 27;
            button4.Text = "학습 시작";
            button4.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            button5.BackColor = Color.FromArgb(53, 48, 49);
            button5.FlatStyle = FlatStyle.Popup;
            button5.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            button5.ForeColor = SystemColors.ButtonHighlight;
            button5.Location = new Point(12, 110);
            button5.Name = "button5";
            button5.Size = new Size(475, 29);
            button5.TabIndex = 26;
            button5.Text = "내 파일럿";
            button5.UseVisualStyleBackColor = false;
            // 
            // panel7
            // 
            panel7.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel7.BackColor = Color.FromArgb(33, 28, 29);
            panel7.Location = new Point(2, 166);
            panel7.Name = "panel7";
            panel7.Size = new Size(973, 10);
            panel7.TabIndex = 6;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(976, 852);
            Controls.Add(panel7);
            Controls.Add(panel5);
            Controls.Add(panel4);
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
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnTrainModel;
        private Label lblTitle;
        private Button btnDataManagement;
        private Panel panel2;
        private Label lblAngleTitle;
        private Panel panel3;
        private DataGridView dgvPilotList;
        private Label lblViewPilots;
        private Panel panel4;
        private Label label2;
        private Label label4;
        private Label label3;
        private TextBox textBox2;
        private Label label7;
        private Label label8;
        private ComboBox comboBox1;
        private TextBox textBox1;
        private Label label9;
        private DataGridViewTextBoxColumn 이름;
        private DataGridViewTextBoxColumn 파일럿;
        private DataGridViewTextBoxColumn 타입;
        private DataGridViewTextBoxColumn 데이터저장소;
        private DataGridViewTextBoxColumn 시간;
        private DataGridViewTextBoxColumn 전이학습;
        private DataGridViewTextBoxColumn 설명;
        private Panel panel6;
        private Button button2;
        private Button button1;
        private Label label1;
        private Panel panel5;
        private Button button4;
        private Button button5;
        private Panel panel7;
        private Button button10;
        private Button button9;
        private Button button8;
        private Button button7;
        private Button button6;
        private Button button3;
        private Label label5;
    }
}