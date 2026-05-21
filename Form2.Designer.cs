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
            txtMaxEpochs = new TextBox();
            txtConfigValue = new TextBox();
            label1 = new Label();
            txtComment = new TextBox();
            cmbComment = new ComboBox();
            txtModelType = new TextBox();
            btnChooseTransferModel = new Button();
            btnStartTrain = new Button();
            panel3 = new Panel();
            lblViewPilots = new Label();
            dgvPilotList = new DataGridView();
            lblGroupTubs = new TextBox();
            btnGroupTubs = new Button();
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
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).BeginInit();
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
            panel2.Controls.Add(btnStartTrain);
            panel2.Controls.Add(btnChooseTransferModel);
            panel2.Controls.Add(txtModelType);
            panel2.Controls.Add(cmbComment);
            panel2.Controls.Add(txtComment);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(txtConfigValue);
            panel2.Controls.Add(txtMaxEpochs);
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
            // txtMaxEpochs
            // 
            txtMaxEpochs.BackColor = Color.FromArgb(53, 48, 49);
            txtMaxEpochs.BorderStyle = BorderStyle.FixedSingle;
            txtMaxEpochs.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtMaxEpochs.ForeColor = SystemColors.MenuBar;
            txtMaxEpochs.Location = new Point(3, 37);
            txtMaxEpochs.Name = "txtMaxEpochs";
            txtMaxEpochs.Size = new Size(311, 25);
            txtMaxEpochs.TabIndex = 3;
            txtMaxEpochs.Text = "최대 학습 반복 횟수(Epoch): 2";
            // 
            // txtConfigValue
            // 
            txtConfigValue.BackColor = Color.FromArgb(103, 98, 98);
            txtConfigValue.BorderStyle = BorderStyle.FixedSingle;
            txtConfigValue.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtConfigValue.ForeColor = SystemColors.MenuBar;
            txtConfigValue.Location = new Point(322, 37);
            txtConfigValue.Name = "txtConfigValue";
            txtConfigValue.Size = new Size(307, 25);
            txtConfigValue.TabIndex = 4;
            txtConfigValue.Text = "새 값 입력";
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
            // txtComment
            // 
            txtComment.BackColor = Color.FromArgb(103, 98, 98);
            txtComment.BorderStyle = BorderStyle.FixedSingle;
            txtComment.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtComment.ForeColor = SystemColors.MenuBar;
            txtComment.Location = new Point(319, 101);
            txtComment.Name = "txtComment";
            txtComment.Size = new Size(307, 25);
            txtComment.TabIndex = 7;
            txtComment.Text = "메모 / 설명";
            // 
            // cmbComment
            // 
            cmbComment.BackColor = Color.FromArgb(103, 98, 98);
            cmbComment.FlatStyle = FlatStyle.Popup;
            cmbComment.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            cmbComment.ForeColor = SystemColors.MenuBar;
            cmbComment.FormattingEnabled = true;
            cmbComment.Items.AddRange(new object[] { "linear", "", "categorical", "", "rnn", "", "imu" });
            cmbComment.Location = new Point(165, 101);
            cmbComment.Name = "cmbComment";
            cmbComment.Size = new Size(148, 25);
            cmbComment.TabIndex = 3;
            // 
            // txtModelType
            // 
            txtModelType.BackColor = Color.FromArgb(53, 48, 49);
            txtModelType.BorderStyle = BorderStyle.FixedSingle;
            txtModelType.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtModelType.ForeColor = SystemColors.MenuBar;
            txtModelType.Location = new Point(6, 101);
            txtModelType.Name = "txtModelType";
            txtModelType.Size = new Size(155, 25);
            txtModelType.TabIndex = 8;
            txtModelType.Text = "모델 종류";
            // 
            // btnChooseTransferModel
            // 
            btnChooseTransferModel.Anchor = AnchorStyles.Left;
            btnChooseTransferModel.BackColor = Color.FromArgb(53, 48, 49);
            btnChooseTransferModel.FlatStyle = FlatStyle.Popup;
            btnChooseTransferModel.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnChooseTransferModel.ForeColor = SystemColors.ButtonHighlight;
            btnChooseTransferModel.Location = new Point(3, 140);
            btnChooseTransferModel.Name = "btnChooseTransferModel";
            btnChooseTransferModel.Size = new Size(311, 23);
            btnChooseTransferModel.TabIndex = 9;
            btnChooseTransferModel.Text = "전이 학습 모델 선택";
            btnChooseTransferModel.UseVisualStyleBackColor = false;
            // 
            // btnStartTrain
            // 
            btnStartTrain.Anchor = AnchorStyles.Left;
            btnStartTrain.BackColor = Color.FromArgb(198, 100, 114);
            btnStartTrain.FlatStyle = FlatStyle.Popup;
            btnStartTrain.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnStartTrain.ForeColor = SystemColors.ButtonHighlight;
            btnStartTrain.Location = new Point(322, 140);
            btnStartTrain.Name = "btnStartTrain";
            btnStartTrain.Size = new Size(307, 23);
            btnStartTrain.TabIndex = 10;
            btnStartTrain.Text = "학습 시작";
            btnStartTrain.UseVisualStyleBackColor = false;
            // 
            // panel3
            // 
            panel3.BackColor = Color.FromArgb(33, 28, 29);
            panel3.Controls.Add(btnGroupTubs);
            panel3.Controls.Add(lblGroupTubs);
            panel3.Controls.Add(dgvPilotList);
            panel3.Controls.Add(lblViewPilots);
            panel3.Location = new Point(0, 230);
            panel3.Name = "panel3";
            panel3.Size = new Size(636, 395);
            panel3.TabIndex = 3;
            // 
            // lblViewPilots
            // 
            lblViewPilots.AutoSize = true;
            lblViewPilots.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblViewPilots.ForeColor = SystemColors.ButtonHighlight;
            lblViewPilots.Location = new Point(242, 6);
            lblViewPilots.Name = "lblViewPilots";
            lblViewPilots.Size = new Size(124, 20);
            lblViewPilots.TabIndex = 11;
            lblViewPilots.Text = "학습된 모델 목록";
            // 
            // dgvPilotList
            // 
            dgvPilotList.AllowUserToAddRows = false;
            dgvPilotList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPilotList.BackgroundColor = Color.FromArgb(103, 98, 98);
            dgvPilotList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPilotList.Columns.AddRange(new DataGridViewColumn[] { 번호, 모델이름, 모델종류, 사용한주행데이터폴더, 생성시간, 메모, 어떤모델을기반으로학습했는지 });
            dgvPilotList.Location = new Point(0, 29);
            dgvPilotList.Name = "dgvPilotList";
            dgvPilotList.RowHeadersVisible = false;
            dgvPilotList.Size = new Size(631, 311);
            dgvPilotList.TabIndex = 12;
            // 
            // lblGroupTubs
            // 
            lblGroupTubs.BackColor = Color.FromArgb(53, 48, 49);
            lblGroupTubs.BorderStyle = BorderStyle.FixedSingle;
            lblGroupTubs.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblGroupTubs.ForeColor = SystemColors.MenuBar;
            lblGroupTubs.Location = new Point(7, 345);
            lblGroupTubs.Name = "lblGroupTubs";
            lblGroupTubs.Size = new Size(311, 25);
            lblGroupTubs.TabIndex = 13;
            lblGroupTubs.Text = "여러 데이터셋(Tub) 묶기";
            // 
            // btnGroupTubs
            // 
            btnGroupTubs.Anchor = AnchorStyles.Left;
            btnGroupTubs.BackColor = Color.FromArgb(103, 98, 98);
            btnGroupTubs.FlatStyle = FlatStyle.Popup;
            btnGroupTubs.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnGroupTubs.ForeColor = SystemColors.ButtonHighlight;
            btnGroupTubs.Location = new Point(324, 347);
            btnGroupTubs.Name = "btnGroupTubs";
            btnGroupTubs.Size = new Size(307, 23);
            btnGroupTubs.TabIndex = 11;
            btnGroupTubs.Text = "비활성화";
            btnGroupTubs.UseVisualStyleBackColor = false;
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
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnTrainModel;
        private Label lblTitle;
        private Button btnDataManagement;
        private Panel panel2;
        private Label lblAngleTitle;
        private TextBox txtConfigValue;
        private TextBox txtMaxEpochs;
        private ComboBox cmbComment;
        private TextBox txtComment;
        private Label label1;
        private Button btnStartTrain;
        private Button btnChooseTransferModel;
        private TextBox txtModelType;
        private Panel panel3;
        private DataGridView dgvPilotList;
        private Label lblViewPilots;
        private Button btnGroupTubs;
        private TextBox lblGroupTubs;
        private DataGridViewTextBoxColumn 번호;
        private DataGridViewTextBoxColumn 모델이름;
        private DataGridViewTextBoxColumn 모델종류;
        private DataGridViewTextBoxColumn 사용한주행데이터폴더;
        private DataGridViewTextBoxColumn 생성시간;
        private DataGridViewTextBoxColumn 메모;
        private DataGridViewTextBoxColumn 어떤모델을기반으로학습했는지;
    }
}