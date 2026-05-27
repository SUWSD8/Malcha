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
            pnlHeader = new Panel();
            btnDataManagement = new Button();
            btnTrainModel = new Button();
            lblTitle = new Label();
            pnlConfiguration = new Panel();
            panel6 = new Panel();
            btnSaveMyConfig = new Button();
            btnAddConfigItem = new Button();
            txtinputColumnCount = new TextBox();
            lblColumnCount = new Label();
            lblAddConfigItem = new Label();
            lbl1 = new Label();
            lblAngleTitle = new Label();
            pnlList = new Panel();
            dgvPilotList = new DataGridView();
            이름 = new DataGridViewTextBoxColumn();
            파일럿 = new DataGridViewTextBoxColumn();
            타입 = new DataGridViewTextBoxColumn();
            데이터저장소 = new DataGridViewTextBoxColumn();
            시간 = new DataGridViewTextBoxColumn();
            전이학습 = new DataGridViewTextBoxColumn();
            설명 = new DataGridViewTextBoxColumn();
            lblViewPilots = new Label();
            pnlEditor = new Panel();
            btnTrainingLogs = new Button();
            btnViewConfig = new Button();
            btnEditComment = new Button();
            btnDeletePilot = new Button();
            btnEnableDelete = new Button();
            btnViewMyPilot = new Button();
            lbll3 = new Label();
            lblEditor = new Label();
            lbll2 = new Label();
            lblTrainer = new Label();
            cmbModelType = new ComboBox();
            txtModelMemo = new TextBox();
            lblModelType = new Label();
            pnlTrainer = new Panel();
            btnRunTraining = new Button();
            pnlspace = new Panel();
            txtMyPilot = new TextBox();
            pnlHeader.SuspendLayout();
            pnlConfiguration.SuspendLayout();
            pnlList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).BeginInit();
            pnlEditor.SuspendLayout();
            pnlTrainer.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlHeader.BackColor = Color.FromArgb(33, 28, 29);
            pnlHeader.Controls.Add(btnDataManagement);
            pnlHeader.Controls.Add(btnTrainModel);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Location = new Point(0, 2);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(978, 59);
            pnlHeader.TabIndex = 1;
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
            // pnlConfiguration
            // 
            pnlConfiguration.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlConfiguration.BackColor = Color.FromArgb(20, 20, 20);
            pnlConfiguration.Controls.Add(panel6);
            pnlConfiguration.Controls.Add(btnSaveMyConfig);
            pnlConfiguration.Controls.Add(btnAddConfigItem);
            pnlConfiguration.Controls.Add(txtinputColumnCount);
            pnlConfiguration.Controls.Add(lblColumnCount);
            pnlConfiguration.Controls.Add(lblAddConfigItem);
            pnlConfiguration.Controls.Add(lbl1);
            pnlConfiguration.Controls.Add(lblAngleTitle);
            pnlConfiguration.Location = new Point(0, 61);
            pnlConfiguration.Name = "pnlConfiguration";
            pnlConfiguration.Size = new Size(978, 106);
            pnlConfiguration.TabIndex = 2;
            // 
            // panel6
            // 
            panel6.Location = new Point(0, 105);
            panel6.Name = "panel6";
            panel6.Size = new Size(638, 10);
            panel6.TabIndex = 0;
            // 
            // btnSaveMyConfig
            // 
            btnSaveMyConfig.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnSaveMyConfig.BackColor = Color.FromArgb(53, 48, 49);
            btnSaveMyConfig.FlatStyle = FlatStyle.Popup;
            btnSaveMyConfig.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSaveMyConfig.ForeColor = SystemColors.ButtonHighlight;
            btnSaveMyConfig.Location = new Point(788, 68);
            btnSaveMyConfig.Name = "btnSaveMyConfig";
            btnSaveMyConfig.Size = new Size(176, 29);
            btnSaveMyConfig.TabIndex = 21;
            btnSaveMyConfig.Text = "myconfig 저장";
            btnSaveMyConfig.UseVisualStyleBackColor = false;
            // 
            // btnAddConfigItem
            // 
            btnAddConfigItem.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnAddConfigItem.BackColor = Color.FromArgb(53, 48, 49);
            btnAddConfigItem.FlatStyle = FlatStyle.Popup;
            btnAddConfigItem.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnAddConfigItem.ForeColor = SystemColors.ButtonHighlight;
            btnAddConfigItem.Location = new Point(238, 69);
            btnAddConfigItem.Name = "btnAddConfigItem";
            btnAddConfigItem.Size = new Size(177, 29);
            btnAddConfigItem.TabIndex = 20;
            btnAddConfigItem.Text = "+";
            btnAddConfigItem.UseVisualStyleBackColor = false;
            // 
            // txtinputColumnCount
            // 
            txtinputColumnCount.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            txtinputColumnCount.BackColor = Color.FromArgb(53, 48, 49);
            txtinputColumnCount.BorderStyle = BorderStyle.FixedSingle;
            txtinputColumnCount.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtinputColumnCount.ForeColor = SystemColors.MenuBar;
            txtinputColumnCount.Location = new Point(533, 70);
            txtinputColumnCount.Name = "txtinputColumnCount";
            txtinputColumnCount.ReadOnly = true;
            txtinputColumnCount.Size = new Size(247, 25);
            txtinputColumnCount.TabIndex = 19;
            txtinputColumnCount.Text = "1";
            txtinputColumnCount.TextAlign = HorizontalAlignment.Center;
            // 
            // lblColumnCount
            // 
            lblColumnCount.Anchor = AnchorStyles.Top;
            lblColumnCount.AutoSize = true;
            lblColumnCount.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblColumnCount.ForeColor = SystemColors.ButtonHighlight;
            lblColumnCount.Location = new Point(448, 75);
            lblColumnCount.Name = "lblColumnCount";
            lblColumnCount.Size = new Size(59, 20);
            lblColumnCount.TabIndex = 13;
            lblColumnCount.Text = "열 개수";
            // 
            // lblAddConfigItem
            // 
            lblAddConfigItem.Anchor = AnchorStyles.Top;
            lblAddConfigItem.AutoSize = true;
            lblAddConfigItem.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAddConfigItem.ForeColor = SystemColors.ButtonHighlight;
            lblAddConfigItem.Location = new Point(72, 72);
            lblAddConfigItem.Name = "lblAddConfigItem";
            lblAddConfigItem.Size = new Size(109, 20);
            lblAddConfigItem.TabIndex = 12;
            lblAddConfigItem.Text = "설정 항목 추가";
            // 
            // lbl1
            // 
            lbl1.Anchor = AnchorStyles.Top;
            lbl1.AutoSize = true;
            lbl1.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lbl1.ForeColor = SystemColors.ButtonHighlight;
            lbl1.Location = new Point(566, 17);
            lbl1.Name = "lbl1";
            lbl1.Size = new Size(397, 45);
            lbl1.TabIndex = 11;
            lbl1.Text = "드롭다운 메뉴를 사용해 설정 파라미터를 수정하세요.\n+ 버튼으로 행(row)을 추가하여 더 많은 파라미터를 관리할 수 있습니다.\nJSON 문법을 사용하세요. 예: 문자열은 큰따옴표 사용, true/false 사용.";
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
            // pnlList
            // 
            pnlList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlList.BackColor = Color.FromArgb(33, 28, 29);
            pnlList.Controls.Add(dgvPilotList);
            pnlList.Controls.Add(lblViewPilots);
            pnlList.Location = new Point(0, 318);
            pnlList.Name = "pnlList";
            pnlList.Size = new Size(982, 419);
            pnlList.TabIndex = 3;
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
            // pnlEditor
            // 
            pnlEditor.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlEditor.BackColor = Color.FromArgb(33, 28, 29);
            pnlEditor.Controls.Add(btnTrainingLogs);
            pnlEditor.Controls.Add(btnViewConfig);
            pnlEditor.Controls.Add(btnEditComment);
            pnlEditor.Controls.Add(btnDeletePilot);
            pnlEditor.Controls.Add(btnEnableDelete);
            pnlEditor.Controls.Add(btnViewMyPilot);
            pnlEditor.Controls.Add(lbll3);
            pnlEditor.Controls.Add(lblEditor);
            pnlEditor.Location = new Point(0, 734);
            pnlEditor.Name = "pnlEditor";
            pnlEditor.Size = new Size(982, 118);
            pnlEditor.TabIndex = 4;
            // 
            // btnTrainingLogs
            // 
            btnTrainingLogs.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnTrainingLogs.BackColor = Color.FromArgb(53, 48, 49);
            btnTrainingLogs.FlatStyle = FlatStyle.Popup;
            btnTrainingLogs.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnTrainingLogs.ForeColor = SystemColors.ButtonHighlight;
            btnTrainingLogs.Location = new Point(739, 70);
            btnTrainingLogs.Name = "btnTrainingLogs";
            btnTrainingLogs.Size = new Size(126, 36);
            btnTrainingLogs.TabIndex = 38;
            btnTrainingLogs.Text = "학습 기록";
            btnTrainingLogs.UseVisualStyleBackColor = false;
            btnTrainingLogs.Click += btnShowTrainingHistory_Click;
            // 
            // btnViewConfig
            // 
            btnViewConfig.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnViewConfig.BackColor = Color.FromArgb(53, 48, 49);
            btnViewConfig.FlatStyle = FlatStyle.Popup;
            btnViewConfig.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnViewConfig.ForeColor = SystemColors.ButtonHighlight;
            btnViewConfig.Location = new Point(607, 69);
            btnViewConfig.Name = "btnViewConfig";
            btnViewConfig.Size = new Size(126, 36);
            btnViewConfig.TabIndex = 37;
            btnViewConfig.Text = "설정 보기";
            btnViewConfig.UseVisualStyleBackColor = false;
            // 
            // btnEditComment
            // 
            btnEditComment.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnEditComment.BackColor = Color.FromArgb(53, 48, 49);
            btnEditComment.FlatStyle = FlatStyle.Popup;
            btnEditComment.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnEditComment.ForeColor = SystemColors.ButtonHighlight;
            btnEditComment.Location = new Point(475, 70);
            btnEditComment.Name = "btnEditComment";
            btnEditComment.Size = new Size(126, 36);
            btnEditComment.TabIndex = 36;
            btnEditComment.Text = "코멘트 수정";
            btnEditComment.UseVisualStyleBackColor = false;
            btnEditComment.Click += btnUpdateComment_Click;
            // 
            // btnDeletePilot
            // 
            btnDeletePilot.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnDeletePilot.BackColor = Color.FromArgb(53, 48, 49);
            btnDeletePilot.FlatStyle = FlatStyle.Popup;
            btnDeletePilot.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnDeletePilot.ForeColor = SystemColors.ButtonHighlight;
            btnDeletePilot.Location = new Point(343, 70);
            btnDeletePilot.Name = "btnDeletePilot";
            btnDeletePilot.Size = new Size(126, 36);
            btnDeletePilot.TabIndex = 35;
            btnDeletePilot.Text = "파일럿 삭제";
            btnDeletePilot.UseVisualStyleBackColor = false;
            btnDeletePilot.Click += btnDeleteModel_Click;
            // 
            // btnEnableDelete
            // 
            btnEnableDelete.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnEnableDelete.BackColor = Color.FromArgb(53, 48, 49);
            btnEnableDelete.FlatStyle = FlatStyle.Popup;
            btnEnableDelete.Font = new Font("맑은 고딕", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnEnableDelete.ForeColor = SystemColors.ButtonHighlight;
            btnEnableDelete.Location = new Point(245, 70);
            btnEnableDelete.Name = "btnEnableDelete";
            btnEnableDelete.Size = new Size(92, 36);
            btnEnableDelete.TabIndex = 34;
            btnEnableDelete.Text = " 삭제 \r\n활성화";
            btnEnableDelete.UseVisualStyleBackColor = false;
            // 
            // btnViewMyPilot
            // 
            btnViewMyPilot.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnViewMyPilot.BackColor = Color.FromArgb(53, 48, 49);
            btnViewMyPilot.FlatStyle = FlatStyle.Popup;
            btnViewMyPilot.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnViewMyPilot.ForeColor = SystemColors.ButtonHighlight;
            btnViewMyPilot.Location = new Point(78, 70);
            btnViewMyPilot.Name = "btnViewMyPilot";
            btnViewMyPilot.Size = new Size(161, 36);
            btnViewMyPilot.TabIndex = 33;
            btnViewMyPilot.Text = "내 파일럿";
            btnViewMyPilot.UseVisualStyleBackColor = false;
            // 
            // lbll3
            // 
            lbll3.Anchor = AnchorStyles.Top;
            lbll3.AutoSize = true;
            lbll3.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lbll3.ForeColor = SystemColors.ButtonHighlight;
            lbll3.Location = new Point(585, 20);
            lbll3.Name = "lbll3";
            lbll3.Size = new Size(354, 45);
            lbll3.TabIndex = 12;
            lbll3.Text = "파일럿을 선택하여 설정을 확인하거나 코멘트를 수정하거나 \r\n삭제할 수 있습니다. 삭제 시 디스크 파일과 데이터베이스 항목이\r\n함께 제거됩니다.";
            // 
            // lblEditor
            // 
            lblEditor.Anchor = AnchorStyles.Top;
            lblEditor.AutoSize = true;
            lblEditor.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblEditor.ForeColor = SystemColors.ButtonHighlight;
            lblEditor.Location = new Point(72, 20);
            lblEditor.Name = "lblEditor";
            lblEditor.Size = new Size(204, 25);
            lblEditor.TabIndex = 2;
            lblEditor.Text = "파일럿 조회 및 편집기";
            // 
            // lbll2
            // 
            lbll2.Anchor = AnchorStyles.Top;
            lbll2.AutoSize = true;
            lbll2.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lbll2.ForeColor = SystemColors.ButtonHighlight;
            lbll2.Location = new Point(625, 15);
            lbll2.Name = "lbll2";
            lbll2.Size = new Size(326, 45);
            lbll2.TabIndex = 25;
            lbll2.Text = "위의 설정 파라미터를 사용하여 파일럿(모델)을 학습합니다.\n모델 타입과 필요하면 전이 학습 모델을 선택하세요.\n설명을 위한 코멘트를 입력할 수 있습니다.";
            // 
            // lblTrainer
            // 
            lblTrainer.Anchor = AnchorStyles.Top;
            lblTrainer.AutoSize = true;
            lblTrainer.Font = new Font("맑은 고딕", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblTrainer.ForeColor = SystemColors.ButtonHighlight;
            lblTrainer.Location = new Point(72, 15);
            lblTrainer.Name = "lblTrainer";
            lblTrainer.Size = new Size(88, 25);
            lblTrainer.TabIndex = 24;
            lblTrainer.Text = "트레이너";
            // 
            // cmbModelType
            // 
            cmbModelType.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            cmbModelType.BackColor = Color.FromArgb(103, 98, 98);
            cmbModelType.FlatStyle = FlatStyle.Popup;
            cmbModelType.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            cmbModelType.ForeColor = SystemColors.MenuBar;
            cmbModelType.FormattingEnabled = true;
            cmbModelType.Items.AddRange(new object[] { "linear", "categorical", "rnn", "imu" });
            cmbModelType.Location = new Point(238, 79);
            cmbModelType.Name = "cmbModelType";
            cmbModelType.Size = new Size(248, 25);
            cmbModelType.TabIndex = 19;
            // 
            // txtModelMemo
            // 
            txtModelMemo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            txtModelMemo.BackColor = Color.FromArgb(103, 98, 98);
            txtModelMemo.BorderStyle = BorderStyle.FixedSingle;
            txtModelMemo.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtModelMemo.ForeColor = SystemColors.MenuBar;
            txtModelMemo.Location = new Point(492, 79);
            txtModelMemo.Name = "txtModelMemo";
            txtModelMemo.Size = new Size(471, 25);
            txtModelMemo.TabIndex = 21;
            txtModelMemo.Text = "메모 / 설명";
            // 
            // lblModelType
            // 
            lblModelType.Anchor = AnchorStyles.Top;
            lblModelType.AutoSize = true;
            lblModelType.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModelType.ForeColor = SystemColors.ButtonHighlight;
            lblModelType.Location = new Point(72, 79);
            lblModelType.Name = "lblModelType";
            lblModelType.Size = new Size(109, 20);
            lblModelType.TabIndex = 20;
            lblModelType.Text = "모델 타입 선택";
            // 
            // pnlTrainer
            // 
            pnlTrainer.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlTrainer.BackColor = Color.FromArgb(20, 20, 20);
            pnlTrainer.Controls.Add(txtMyPilot);
            pnlTrainer.Controls.Add(btnRunTraining);
            pnlTrainer.Controls.Add(lbll2);
            pnlTrainer.Controls.Add(cmbModelType);
            pnlTrainer.Controls.Add(lblTrainer);
            pnlTrainer.Controls.Add(lblModelType);
            pnlTrainer.Controls.Add(txtModelMemo);
            pnlTrainer.Location = new Point(0, 173);
            pnlTrainer.Name = "pnlTrainer";
            pnlTrainer.Size = new Size(977, 150);
            pnlTrainer.TabIndex = 5;
            // 
            // btnRunTraining
            // 
            btnRunTraining.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            btnRunTraining.BackColor = Color.FromArgb(198, 100, 114);
            btnRunTraining.FlatStyle = FlatStyle.Popup;
            btnRunTraining.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRunTraining.ForeColor = SystemColors.ButtonHighlight;
            btnRunTraining.Location = new Point(493, 110);
            btnRunTraining.Name = "btnRunTraining";
            btnRunTraining.Size = new Size(471, 29);
            btnRunTraining.TabIndex = 27;
            btnRunTraining.Text = "학습 시작";
            btnRunTraining.UseVisualStyleBackColor = false;
            btnRunTraining.Click += btnRunAnalysis_Click;
            // 
            // pnlspace
            // 
            pnlspace.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            pnlspace.BackColor = Color.FromArgb(33, 28, 29);
            pnlspace.Location = new Point(2, 166);
            pnlspace.Name = "pnlspace";
            pnlspace.Size = new Size(973, 10);
            pnlspace.TabIndex = 6;
            // 
            // txtMyPilot
            // 
            txtMyPilot.BackColor = Color.FromArgb(53, 48, 49);
            txtMyPilot.BorderStyle = BorderStyle.None;
            txtMyPilot.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtMyPilot.ForeColor = Color.White;
            txtMyPilot.Location = new Point(13, 114);
            txtMyPilot.Name = "txtMyPilot";
            txtMyPilot.Size = new Size(474, 20);
            txtMyPilot.TabIndex = 28;
            txtMyPilot.Text = "내 파일럿";
            txtMyPilot.TextAlign = HorizontalAlignment.Center;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(976, 852);
            Controls.Add(pnlspace);
            Controls.Add(pnlTrainer);
            Controls.Add(pnlEditor);
            Controls.Add(pnlList);
            Controls.Add(pnlConfiguration);
            Controls.Add(pnlHeader);
            Name = "Form2";
            Text = "Form2";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlConfiguration.ResumeLayout(false);
            pnlConfiguration.PerformLayout();
            pnlList.ResumeLayout(false);
            pnlList.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).EndInit();
            pnlEditor.ResumeLayout(false);
            pnlEditor.PerformLayout();
            pnlTrainer.ResumeLayout(false);
            pnlTrainer.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlHeader;
        private Button btnTrainModel;
        private Label lblTitle;
        private Button btnDataManagement;
        private Panel pnlConfiguration;
        private Label lblAngleTitle;
        private Panel pnlList;
        private DataGridView dgvPilotList;
        private Label lblViewPilots;
        private Panel pnlEditor;
        private Label lbl1;
        private Label lblColumnCount;
        private Label lblAddConfigItem;
        private TextBox txtinputColumnCount;
        private Label lbll2;
        private Label lblTrainer;
        private ComboBox cmbModelType;
        private TextBox txtModelMemo;
        private Label lblModelType;
        private DataGridViewTextBoxColumn 이름;
        private DataGridViewTextBoxColumn 파일럿;
        private DataGridViewTextBoxColumn 타입;
        private DataGridViewTextBoxColumn 데이터저장소;
        private DataGridViewTextBoxColumn 시간;
        private DataGridViewTextBoxColumn 전이학습;
        private DataGridViewTextBoxColumn 설명;
        private Panel panel6;
        private Button btnSaveMyConfig;
        private Button btnAddConfigItem;
        private Label lblEditor;
        private Panel pnlTrainer;
        private Button btnRunTraining;
        private Button btnMyPilot;
        private Panel pnlspace;
        private Button btnTrainingLogs;
        private Button btnViewConfig;
        private Button btnEditComment;
        private Button btnDeletePilot;
        private Button btnEnableDelete;
        private Button btnViewMyPilot;
        private Label lbll3;
        private TextBox txtMyPilot;
    }
}