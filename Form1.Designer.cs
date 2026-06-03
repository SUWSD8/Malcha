namespace Malcha
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            splitContainer1 = new SplitContainer();
            splitContainer3 = new SplitContainer();
            groupBox1 = new GroupBox();
            btnSaveCatalog = new Button();
            lblTitle = new Label();
            txtFilePath = new TextBox();
            btnSelectData = new Button();
            groupBox2 = new GroupBox();
            picVideoScreen = new PictureBox();
            trbTimeline = new TrackBar();
            btnPlayPause = new Button();
            btnFastForward = new Button();
            btnRewind = new Button();
            btnNextFrame = new Button();
            btnPrevFrame = new Button();
            lstDataList = new ListBox();
            lblRecordCount = new Label();
            lblRecordTitle = new Label();
            lblModeValue = new Label();
            lblThrottleValue = new Label();
            lblAngleValue = new Label();
            lblModelValue = new Label();
            lblModelAngle = new Label();
            lblModeTitle = new Label();
            lblThrottleTitle = new Label();
            lblAngleTitle = new Label();
            groupBox3 = new GroupBox();
            btnUndo = new Button();
            lblStatus = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabelPlaybackSpeed = new ToolStripStatusLabel();
            lbldeletedlist = new Label();
            lstDeleted = new ListBox();
            btnChangeCleanData = new Button();
            btnHelper = new Button();
            btnRefresh = new Button();
            btnApplyFilter = new Button();
            btnRecover = new Button();
            btnDeleteSelection = new Button();
            btnSetEndPoint = new Button();
            btnSetStartPoint = new Button();
            chtDataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            splitContainer2 = new SplitContainer();
            groupBox4 = new GroupBox();
            lblloglist = new Label();
            btnshutdown = new Button();
            lstLog = new ListBox();
            groupBox5 = new GroupBox();
            tableLayoutPanel2 = new TableLayoutPanel();
            btnRunTraining = new Button();
            btnCrossTest = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnEnableDelete = new Button();
            btnconnet = new Button();
            lstViewScore = new ListBox();
            txtModelMemo = new TextBox();
            dgvPilotList = new DataGridView();
            이름 = new DataGridViewTextBoxColumn();
            시간 = new DataGridViewTextBoxColumn();
            설명 = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picVideoScreen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trbTimeline).BeginInit();
            groupBox3.SuspendLayout();
            lblStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chtDataGraph).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.BackColor = Color.FromArgb(33, 28, 29);
            splitContainer1.Panel1.Controls.Add(splitContainer3);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = Color.FromArgb(33, 28, 29);
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1205, 791);
            splitContainer1.SplitterDistance = 692;
            splitContainer1.TabIndex = 0;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(groupBox1);
            splitContainer3.Panel1.Controls.Add(groupBox2);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(groupBox3);
            splitContainer3.Size = new Size(692, 791);
            splitContainer3.SplitterDistance = 493;
            splitContainer3.TabIndex = 59;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            groupBox1.Controls.Add(btnSaveCatalog);
            groupBox1.Controls.Add(lblTitle);
            groupBox1.Controls.Add(txtFilePath);
            groupBox1.Controls.Add(btnSelectData);
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(686, 90);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            // 
            // btnSaveCatalog
            // 
            btnSaveCatalog.Anchor = AnchorStyles.Left;
            btnSaveCatalog.BackColor = Color.FromArgb(53, 48, 49);
            btnSaveCatalog.FlatStyle = FlatStyle.Popup;
            btnSaveCatalog.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSaveCatalog.ForeColor = SystemColors.ButtonHighlight;
            btnSaveCatalog.Location = new Point(141, 56);
            btnSaveCatalog.Margin = new Padding(2);
            btnSaveCatalog.Name = "btnSaveCatalog";
            btnSaveCatalog.Size = new Size(121, 23);
            btnSaveCatalog.TabIndex = 10;
            btnSaveCatalog.Text = "저장";
            btnSaveCatalog.UseVisualStyleBackColor = false;
            btnSaveCatalog.Click += btnSaveCatalog_Click;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblTitle.ForeColor = Color.FromArgb(227, 98, 132);
            lblTitle.Location = new Point(17, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(138, 32);
            lblTitle.TabIndex = 7;
            lblTitle.Text = "DonkeyCar";
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtFilePath.Location = new Point(268, 56);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(405, 23);
            txtFilePath.TabIndex = 9;
            // 
            // btnSelectData
            // 
            btnSelectData.Anchor = AnchorStyles.Left;
            btnSelectData.BackColor = Color.FromArgb(53, 48, 49);
            btnSelectData.FlatStyle = FlatStyle.Popup;
            btnSelectData.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSelectData.ForeColor = SystemColors.ButtonHighlight;
            btnSelectData.Location = new Point(14, 56);
            btnSelectData.Name = "btnSelectData";
            btnSelectData.Size = new Size(121, 23);
            btnSelectData.TabIndex = 8;
            btnSelectData.Text = "데이터 선택";
            btnSelectData.UseVisualStyleBackColor = false;
            // 
            // groupBox2
            // 
            groupBox2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox2.Controls.Add(picVideoScreen);
            groupBox2.Controls.Add(trbTimeline);
            groupBox2.Controls.Add(btnPlayPause);
            groupBox2.Controls.Add(btnFastForward);
            groupBox2.Controls.Add(btnRewind);
            groupBox2.Controls.Add(btnNextFrame);
            groupBox2.Controls.Add(btnPrevFrame);
            groupBox2.Controls.Add(lstDataList);
            groupBox2.Controls.Add(lblRecordCount);
            groupBox2.Controls.Add(lblRecordTitle);
            groupBox2.Controls.Add(lblModeValue);
            groupBox2.Controls.Add(lblThrottleValue);
            groupBox2.Controls.Add(lblAngleValue);
            groupBox2.Controls.Add(lblModelValue);
            groupBox2.Controls.Add(lblModelAngle);
            groupBox2.Controls.Add(lblModeTitle);
            groupBox2.Controls.Add(lblThrottleTitle);
            groupBox2.Controls.Add(lblAngleTitle);
            groupBox2.Location = new Point(3, 88);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(686, 404);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            // 
            // picVideoScreen
            // 
            picVideoScreen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picVideoScreen.BackColor = Color.Black;
            picVideoScreen.Location = new Point(136, 21);
            picVideoScreen.Name = "picVideoScreen";
            picVideoScreen.Size = new Size(378, 318);
            picVideoScreen.TabIndex = 44;
            picVideoScreen.TabStop = false;
            // 
            // trbTimeline
            // 
            trbTimeline.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trbTimeline.Location = new Point(17, 346);
            trbTimeline.Name = "trbTimeline";
            trbTimeline.Size = new Size(645, 45);
            trbTimeline.TabIndex = 45;
            // 
            // btnPlayPause
            // 
            btnPlayPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPlayPause.BackColor = Color.FromArgb(198, 100, 114);
            btnPlayPause.FlatStyle = FlatStyle.Popup;
            btnPlayPause.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnPlayPause.ForeColor = Color.White;
            btnPlayPause.Location = new Point(521, 291);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new Size(134, 39);
            btnPlayPause.TabIndex = 50;
            btnPlayPause.Text = "재생 / 정지";
            btnPlayPause.UseVisualStyleBackColor = false;
            // 
            // btnFastForward
            // 
            btnFastForward.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnFastForward.BackColor = Color.FromArgb(56, 52, 52);
            btnFastForward.FlatStyle = FlatStyle.Popup;
            btnFastForward.Font = new Font("바탕", 14.25F, FontStyle.Bold);
            btnFastForward.ForeColor = Color.White;
            btnFastForward.Location = new Point(591, 236);
            btnFastForward.Name = "btnFastForward";
            btnFastForward.Size = new Size(64, 50);
            btnFastForward.TabIndex = 49;
            btnFastForward.Text = ">>";
            btnFastForward.UseVisualStyleBackColor = false;
            // 
            // btnRewind
            // 
            btnRewind.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnRewind.BackColor = Color.FromArgb(56, 52, 52);
            btnRewind.FlatStyle = FlatStyle.Popup;
            btnRewind.Font = new Font("바탕", 14.25F, FontStyle.Bold);
            btnRewind.ForeColor = Color.White;
            btnRewind.Location = new Point(521, 236);
            btnRewind.Name = "btnRewind";
            btnRewind.Size = new Size(64, 50);
            btnRewind.TabIndex = 48;
            btnRewind.Text = "<<";
            btnRewind.UseVisualStyleBackColor = false;
            // 
            // btnNextFrame
            // 
            btnNextFrame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnNextFrame.BackColor = Color.FromArgb(56, 52, 52);
            btnNextFrame.FlatStyle = FlatStyle.Popup;
            btnNextFrame.Font = new Font("바탕", 14.25F, FontStyle.Bold);
            btnNextFrame.ForeColor = Color.White;
            btnNextFrame.Location = new Point(591, 179);
            btnNextFrame.Name = "btnNextFrame";
            btnNextFrame.Size = new Size(64, 50);
            btnNextFrame.TabIndex = 47;
            btnNextFrame.Text = ">";
            btnNextFrame.UseVisualStyleBackColor = false;
            // 
            // btnPrevFrame
            // 
            btnPrevFrame.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPrevFrame.BackColor = Color.FromArgb(56, 52, 52);
            btnPrevFrame.FlatStyle = FlatStyle.Popup;
            btnPrevFrame.Font = new Font("바탕", 14.25F, FontStyle.Bold);
            btnPrevFrame.ForeColor = Color.White;
            btnPrevFrame.Location = new Point(521, 179);
            btnPrevFrame.Name = "btnPrevFrame";
            btnPrevFrame.Size = new Size(64, 50);
            btnPrevFrame.TabIndex = 46;
            btnPrevFrame.Text = "<";
            btnPrevFrame.UseVisualStyleBackColor = false;
            // 
            // lstDataList
            // 
            lstDataList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lstDataList.BackColor = Color.FromArgb(48, 42, 41);
            lstDataList.ForeColor = SystemColors.MenuBar;
            lstDataList.FormattingEnabled = true;
            lstDataList.Location = new Point(521, 81);
            lstDataList.Name = "lstDataList";
            lstDataList.Size = new Size(134, 79);
            lstDataList.TabIndex = 38;
            // 
            // lblRecordCount
            // 
            lblRecordCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRecordCount.AutoSize = true;
            lblRecordCount.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblRecordCount.ForeColor = SystemColors.ButtonHighlight;
            lblRecordCount.Location = new Point(520, 40);
            lblRecordCount.Name = "lblRecordCount";
            lblRecordCount.Size = new Size(98, 32);
            lblRecordCount.TabIndex = 43;
            lblRecordCount.Text = "000000";
            // 
            // lblRecordTitle
            // 
            lblRecordTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRecordTitle.AutoSize = true;
            lblRecordTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblRecordTitle.ForeColor = SystemColors.ButtonHighlight;
            lblRecordTitle.Location = new Point(520, 21);
            lblRecordTitle.Name = "lblRecordTitle";
            lblRecordTitle.Size = new Size(58, 20);
            lblRecordTitle.TabIndex = 42;
            lblRecordTitle.Text = "Record";
            // 
            // lblModeValue
            // 
            lblModeValue.AutoSize = true;
            lblModeValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModeValue.ForeColor = SystemColors.ButtonHighlight;
            lblModeValue.Location = new Point(49, 189);
            lblModeValue.Name = "lblModeValue";
            lblModeValue.Size = new Size(39, 20);
            lblModeValue.TabIndex = 41;
            lblModeValue.Text = "user";
            // 
            // lblThrottleValue
            // 
            lblThrottleValue.AutoSize = true;
            lblThrottleValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblThrottleValue.ForeColor = SystemColors.ButtonHighlight;
            lblThrottleValue.Location = new Point(49, 128);
            lblThrottleValue.Name = "lblThrottleValue";
            lblThrottleValue.Size = new Size(69, 20);
            lblThrottleValue.TabIndex = 40;
            lblThrottleValue.Text = "+00.000";
            // 
            // lblAngleValue
            // 
            lblAngleValue.AutoSize = true;
            lblAngleValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleValue.ForeColor = SystemColors.ButtonHighlight;
            lblAngleValue.Location = new Point(49, 68);
            lblAngleValue.Name = "lblAngleValue";
            lblAngleValue.Size = new Size(69, 20);
            lblAngleValue.TabIndex = 39;
            lblAngleValue.Text = "+00.000";
            // 
            // lblModelValue
            // 
            lblModelValue.AutoSize = true;
            lblModelValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModelValue.ForeColor = Color.FromArgb(255, 215, 64);
            lblModelValue.Location = new Point(49, 248);
            lblModelValue.Name = "lblModelValue";
            lblModelValue.Size = new Size(69, 20);
            lblModelValue.TabIndex = 65;
            lblModelValue.Text = "+00.000";
            lblModelValue.Visible = false;
            // 
            // lblModelAngle
            // 
            lblModelAngle.AutoSize = true;
            lblModelAngle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModelAngle.ForeColor = Color.FromArgb(255, 215, 64);
            lblModelAngle.Location = new Point(32, 218);
            lblModelAngle.Name = "lblModelAngle";
            lblModelAngle.Size = new Size(98, 20);
            lblModelAngle.TabIndex = 64;
            lblModelAngle.Text = "model/angle";
            lblModelAngle.Visible = false;
            // 
            // lblModeTitle
            // 
            lblModeTitle.AutoSize = true;
            lblModeTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModeTitle.ForeColor = SystemColors.ButtonHighlight;
            lblModeTitle.Location = new Point(32, 159);
            lblModeTitle.Name = "lblModeTitle";
            lblModeTitle.Size = new Size(86, 20);
            lblModeTitle.TabIndex = 37;
            lblModeTitle.Text = "user/mode";
            // 
            // lblThrottleTitle
            // 
            lblThrottleTitle.AutoSize = true;
            lblThrottleTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblThrottleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblThrottleTitle.Location = new Point(32, 98);
            lblThrottleTitle.Name = "lblThrottleTitle";
            lblThrottleTitle.Size = new Size(100, 20);
            lblThrottleTitle.TabIndex = 36;
            lblThrottleTitle.Text = "user/throttle";
            // 
            // lblAngleTitle
            // 
            lblAngleTitle.AutoSize = true;
            lblAngleTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblAngleTitle.Location = new Point(32, 38);
            lblAngleTitle.Name = "lblAngleTitle";
            lblAngleTitle.Size = new Size(84, 20);
            lblAngleTitle.TabIndex = 35;
            lblAngleTitle.Text = "user/angle";
            // 
            // groupBox3
            // 
            groupBox3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox3.Controls.Add(btnUndo);
            groupBox3.Controls.Add(lblStatus);
            groupBox3.Controls.Add(lbldeletedlist);
            groupBox3.Controls.Add(lstDeleted);
            groupBox3.Controls.Add(btnChangeCleanData);
            groupBox3.Controls.Add(btnHelper);
            groupBox3.Controls.Add(btnRefresh);
            groupBox3.Controls.Add(btnApplyFilter);
            groupBox3.Controls.Add(btnRecover);
            groupBox3.Controls.Add(btnDeleteSelection);
            groupBox3.Controls.Add(btnSetEndPoint);
            groupBox3.Controls.Add(btnSetStartPoint);
            groupBox3.Controls.Add(chtDataGraph);
            groupBox3.Location = new Point(0, 4);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(691, 289);
            groupBox3.TabIndex = 2;
            groupBox3.TabStop = false;
            // 
            // btnUndo
            // 
            btnUndo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUndo.BackColor = Color.FromArgb(53, 48, 49);
            btnUndo.FlatStyle = FlatStyle.Popup;
            btnUndo.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnUndo.ForeColor = SystemColors.ButtonHighlight;
            btnUndo.Location = new Point(450, 19);
            btnUndo.Margin = new Padding(2);
            btnUndo.Name = "btnUndo";
            btnUndo.Size = new Size(99, 23);
            btnUndo.TabIndex = 39;
            btnUndo.Text = "되돌리기";
            btnUndo.UseVisualStyleBackColor = false;
            btnUndo.Click += btnUndo_Click;
            // 
            // lblStatus
            // 
            lblStatus.BackColor = Color.Black;
            lblStatus.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblStatus.ImageScalingSize = new Size(20, 20);
            lblStatus.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1, toolStripStatusLabelPlaybackSpeed });
            lblStatus.Location = new Point(3, 264);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(685, 22);
            lblStatus.TabIndex = 38;
            lblStatus.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.ForeColor = SystemColors.ButtonHighlight;
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(670, 17);
            toolStripStatusLabel1.Spring = true;
            toolStripStatusLabel1.Text = "동키카 준비 완료 (Donkey Ready)";
            // 
            // toolStripStatusLabelPlaybackSpeed
            // 
            toolStripStatusLabelPlaybackSpeed.Alignment = ToolStripItemAlignment.Right;
            toolStripStatusLabelPlaybackSpeed.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            toolStripStatusLabelPlaybackSpeed.ForeColor = Color.FromArgb(255, 210, 90);
            toolStripStatusLabelPlaybackSpeed.Name = "toolStripStatusLabelPlaybackSpeed";
            toolStripStatusLabelPlaybackSpeed.Size = new Size(36, 17);
            toolStripStatusLabelPlaybackSpeed.Text = "▶ 1x";
            toolStripStatusLabelPlaybackSpeed.Visible = false;
            // 
            // lbldeletedlist
            // 
            lbldeletedlist.AutoSize = true;
            lbldeletedlist.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lbldeletedlist.ForeColor = SystemColors.ButtonHighlight;
            lbldeletedlist.Location = new Point(35, 22);
            lbldeletedlist.Name = "lbldeletedlist";
            lbldeletedlist.Size = new Size(74, 20);
            lbldeletedlist.TabIndex = 37;
            lbldeletedlist.Text = "삭제 목록";
            // 
            // lstDeleted
            // 
            lstDeleted.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            lstDeleted.BackColor = Color.FromArgb(48, 42, 41);
            lstDeleted.ForeColor = SystemColors.MenuBar;
            lstDeleted.FormattingEnabled = true;
            lstDeleted.Location = new Point(20, 67);
            lstDeleted.Name = "lstDeleted";
            lstDeleted.Size = new Size(125, 154);
            lstDeleted.TabIndex = 36;
            // 
            // btnChangeCleanData
            // 
            btnChangeCleanData.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChangeCleanData.BackColor = Color.FromArgb(198, 100, 114);
            btnChangeCleanData.FlatStyle = FlatStyle.Popup;
            btnChangeCleanData.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnChangeCleanData.ForeColor = SystemColors.ButtonHighlight;
            btnChangeCleanData.Location = new Point(562, 61);
            btnChangeCleanData.Name = "btnChangeCleanData";
            btnChangeCleanData.Size = new Size(91, 51);
            btnChangeCleanData.TabIndex = 27;
            btnChangeCleanData.Text = "정제 데이터 연동";
            btnChangeCleanData.UseVisualStyleBackColor = false;
            // 
            // btnHelper
            // 
            btnHelper.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelper.BackColor = Color.FromArgb(53, 48, 49);
            btnHelper.FlatStyle = FlatStyle.Popup;
            btnHelper.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnHelper.ForeColor = SystemColors.ButtonHighlight;
            btnHelper.Location = new Point(562, 218);
            btnHelper.Name = "btnHelper";
            btnHelper.Size = new Size(91, 51);
            btnHelper.TabIndex = 35;
            btnHelper.Text = " 도움말";
            btnHelper.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.FromArgb(53, 48, 49);
            btnRefresh.FlatStyle = FlatStyle.Popup;
            btnRefresh.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRefresh.ForeColor = SystemColors.ButtonHighlight;
            btnRefresh.Location = new Point(562, 161);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(91, 51);
            btnRefresh.TabIndex = 34;
            btnRefresh.Text = "새로고침";
            btnRefresh.UseVisualStyleBackColor = false;
            // 
            // btnApplyFilter
            // 
            btnApplyFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApplyFilter.BackColor = Color.FromArgb(53, 48, 49);
            btnApplyFilter.FlatStyle = FlatStyle.Popup;
            btnApplyFilter.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnApplyFilter.ForeColor = SystemColors.ButtonHighlight;
            btnApplyFilter.Location = new Point(562, 117);
            btnApplyFilter.Margin = new Padding(2);
            btnApplyFilter.Name = "btnApplyFilter";
            btnApplyFilter.Size = new Size(91, 39);
            btnApplyFilter.TabIndex = 32;
            btnApplyFilter.Text = "필터 적용";
            btnApplyFilter.UseVisualStyleBackColor = false;
            // 
            // btnRecover
            // 
            btnRecover.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRecover.BackColor = Color.FromArgb(53, 48, 49);
            btnRecover.FlatStyle = FlatStyle.Popup;
            btnRecover.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRecover.ForeColor = SystemColors.ButtonHighlight;
            btnRecover.Location = new Point(553, 19);
            btnRecover.Margin = new Padding(2);
            btnRecover.Name = "btnRecover";
            btnRecover.Size = new Size(103, 23);
            btnRecover.TabIndex = 31;
            btnRecover.Text = "복구";
            btnRecover.UseVisualStyleBackColor = false;
            // 
            // btnDeleteSelection
            // 
            btnDeleteSelection.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDeleteSelection.BackColor = Color.FromArgb(53, 48, 49);
            btnDeleteSelection.FlatStyle = FlatStyle.Popup;
            btnDeleteSelection.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnDeleteSelection.ForeColor = SystemColors.ButtonHighlight;
            btnDeleteSelection.Location = new Point(343, 19);
            btnDeleteSelection.Margin = new Padding(2);
            btnDeleteSelection.Name = "btnDeleteSelection";
            btnDeleteSelection.Size = new Size(103, 23);
            btnDeleteSelection.TabIndex = 30;
            btnDeleteSelection.Text = "선택구간 삭제";
            btnDeleteSelection.UseVisualStyleBackColor = false;
            // 
            // btnSetEndPoint
            // 
            btnSetEndPoint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSetEndPoint.BackColor = Color.FromArgb(53, 48, 49);
            btnSetEndPoint.FlatStyle = FlatStyle.Popup;
            btnSetEndPoint.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSetEndPoint.ForeColor = SystemColors.ButtonHighlight;
            btnSetEndPoint.Location = new Point(236, 19);
            btnSetEndPoint.Margin = new Padding(2);
            btnSetEndPoint.Name = "btnSetEndPoint";
            btnSetEndPoint.Size = new Size(103, 23);
            btnSetEndPoint.TabIndex = 29;
            btnSetEndPoint.Text = "끝점 설정";
            btnSetEndPoint.UseVisualStyleBackColor = false;
            // 
            // btnSetStartPoint
            // 
            btnSetStartPoint.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSetStartPoint.BackColor = Color.FromArgb(53, 48, 49);
            btnSetStartPoint.FlatStyle = FlatStyle.Popup;
            btnSetStartPoint.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSetStartPoint.ForeColor = SystemColors.ButtonHighlight;
            btnSetStartPoint.Location = new Point(129, 19);
            btnSetStartPoint.Margin = new Padding(2);
            btnSetStartPoint.Name = "btnSetStartPoint";
            btnSetStartPoint.Size = new Size(103, 23);
            btnSetStartPoint.TabIndex = 28;
            btnSetStartPoint.Text = "시작점 설정";
            btnSetStartPoint.UseVisualStyleBackColor = false;
            // 
            // chtDataGraph
            // 
            chtDataGraph.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            chtDataGraph.BackColor = Color.FromArgb(48, 42, 41);
            chartArea1.Name = "ChartArea1";
            chtDataGraph.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            chtDataGraph.Legends.Add(legend1);
            chtDataGraph.Location = new Point(166, 55);
            chtDataGraph.Name = "chtDataGraph";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Legend = "Legend1";
            series1.Name = "user/angle";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Color = Color.Red;
            series2.Legend = "Legend1";
            series2.Name = "user/throttle";
            chtDataGraph.Series.Add(series1);
            chtDataGraph.Series.Add(series2);
            chtDataGraph.Size = new Size(384, 195);
            chtDataGraph.TabIndex = 33;
            chtDataGraph.Text = "chart1";
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(groupBox4);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(groupBox5);
            splitContainer2.Size = new Size(509, 791);
            splitContainer2.SplitterDistance = 258;
            splitContainer2.TabIndex = 69;
            // 
            // groupBox4
            // 
            groupBox4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            groupBox4.Controls.Add(lblloglist);
            groupBox4.Controls.Add(btnshutdown);
            groupBox4.Controls.Add(lstLog);
            groupBox4.Location = new Point(3, -2);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(502, 257);
            groupBox4.TabIndex = 0;
            groupBox4.TabStop = false;
            // 
            // lblloglist
            // 
            lblloglist.AutoSize = true;
            lblloglist.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblloglist.ForeColor = SystemColors.ButtonHighlight;
            lblloglist.Location = new Point(8, 15);
            lblloglist.Name = "lblloglist";
            lblloglist.Size = new Size(61, 20);
            lblloglist.TabIndex = 56;
            lblloglist.Text = "Log list";
            // 
            // btnshutdown
            // 
            btnshutdown.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnshutdown.BackColor = Color.FromArgb(214, 71, 129);
            btnshutdown.FlatStyle = FlatStyle.Popup;
            btnshutdown.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnshutdown.ForeColor = SystemColors.ButtonHighlight;
            btnshutdown.Location = new Point(8, 207);
            btnshutdown.Name = "btnshutdown";
            btnshutdown.Size = new Size(485, 37);
            btnshutdown.TabIndex = 58;
            btnshutdown.Text = "학습 강제 종료";
            btnshutdown.UseVisualStyleBackColor = false;
            // 
            // lstLog
            // 
            lstLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstLog.BackColor = Color.FromArgb(48, 42, 41);
            lstLog.ForeColor = SystemColors.MenuBar;
            lstLog.FormattingEnabled = true;
            lstLog.Location = new Point(8, 42);
            lstLog.Name = "lstLog";
            lstLog.Size = new Size(485, 154);
            lstLog.TabIndex = 57;
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(tableLayoutPanel2);
            groupBox5.Controls.Add(tableLayoutPanel1);
            groupBox5.Controls.Add(lstViewScore);
            groupBox5.Controls.Add(txtModelMemo);
            groupBox5.Controls.Add(dgvPilotList);
            groupBox5.Dock = DockStyle.Fill;
            groupBox5.Location = new Point(0, 0);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(509, 529);
            groupBox5.TabIndex = 1;
            groupBox5.TabStop = false;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(btnRunTraining, 0, 0);
            tableLayoutPanel2.Controls.Add(btnCrossTest, 1, 0);
            tableLayoutPanel2.Location = new Point(3, 173);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Size = new Size(505, 36);
            tableLayoutPanel2.TabIndex = 68;
            // 
            // btnRunTraining
            // 
            btnRunTraining.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnRunTraining.BackColor = Color.FromArgb(198, 100, 114);
            btnRunTraining.FlatStyle = FlatStyle.Popup;
            btnRunTraining.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRunTraining.ForeColor = SystemColors.ButtonHighlight;
            btnRunTraining.Location = new Point(3, 3);
            btnRunTraining.Name = "btnRunTraining";
            btnRunTraining.Size = new Size(246, 30);
            btnRunTraining.TabIndex = 62;
            btnRunTraining.Text = "학습 시작";
            btnRunTraining.UseVisualStyleBackColor = false;
            // 
            // btnCrossTest
            // 
            btnCrossTest.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnCrossTest.BackColor = Color.FromArgb(106, 123, 221);
            btnCrossTest.FlatStyle = FlatStyle.Popup;
            btnCrossTest.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnCrossTest.ForeColor = SystemColors.ButtonHighlight;
            btnCrossTest.Location = new Point(255, 3);
            btnCrossTest.Name = "btnCrossTest";
            btnCrossTest.Size = new Size(247, 30);
            btnCrossTest.TabIndex = 63;
            btnCrossTest.Text = "교차 테스트";
            btnCrossTest.UseVisualStyleBackColor = false;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(btnEnableDelete, 0, 0);
            tableLayoutPanel1.Controls.Add(btnconnet, 1, 0);
            tableLayoutPanel1.Location = new Point(3, 484);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(505, 36);
            tableLayoutPanel1.TabIndex = 67;
            // 
            // btnEnableDelete
            // 
            btnEnableDelete.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnEnableDelete.BackColor = Color.FromArgb(53, 48, 49);
            btnEnableDelete.FlatStyle = FlatStyle.Popup;
            btnEnableDelete.Font = new Font("맑은 고딕", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnEnableDelete.ForeColor = SystemColors.ButtonHighlight;
            btnEnableDelete.Location = new Point(3, 3);
            btnEnableDelete.Name = "btnEnableDelete";
            btnEnableDelete.Size = new Size(246, 30);
            btnEnableDelete.TabIndex = 64;
            btnEnableDelete.Text = " 모델 삭제 ";
            btnEnableDelete.UseVisualStyleBackColor = false;
            // 
            // btnconnet
            // 
            btnconnet.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnconnet.BackColor = Color.FromArgb(53, 48, 49);
            btnconnet.FlatStyle = FlatStyle.Popup;
            btnconnet.Font = new Font("맑은 고딕", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnconnet.ForeColor = SystemColors.ButtonHighlight;
            btnconnet.Location = new Point(255, 3);
            btnconnet.Name = "btnconnet";
            btnconnet.Size = new Size(247, 30);
            btnconnet.TabIndex = 65;
            btnconnet.Text = "설명 추가";
            btnconnet.UseVisualStyleBackColor = false;
            // 
            // lstViewScore
            // 
            lstViewScore.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstViewScore.BackColor = Color.FromArgb(48, 42, 41);
            lstViewScore.ForeColor = SystemColors.MenuBar;
            lstViewScore.FormattingEnabled = true;
            lstViewScore.Location = new Point(8, 18);
            lstViewScore.Name = "lstViewScore";
            lstViewScore.Size = new Size(492, 139);
            lstViewScore.TabIndex = 60;
            // 
            // txtModelMemo
            // 
            txtModelMemo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtModelMemo.BackColor = Color.FromArgb(103, 98, 98);
            txtModelMemo.BorderStyle = BorderStyle.FixedSingle;
            txtModelMemo.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtModelMemo.ForeColor = SystemColors.MenuBar;
            txtModelMemo.Location = new Point(8, 450);
            txtModelMemo.Name = "txtModelMemo";
            txtModelMemo.Size = new Size(493, 25);
            txtModelMemo.TabIndex = 66;
            txtModelMemo.Text = "메모 / 설명";
            // 
            // dgvPilotList
            // 
            dgvPilotList.AllowUserToAddRows = false;
            dgvPilotList.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPilotList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPilotList.BackgroundColor = Color.FromArgb(103, 98, 98);
            dgvPilotList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPilotList.Columns.AddRange(new DataGridViewColumn[] { 이름, 시간, 설명 });
            dgvPilotList.Location = new Point(8, 215);
            dgvPilotList.Name = "dgvPilotList";
            dgvPilotList.RowHeadersVisible = false;
            dgvPilotList.RowHeadersWidth = 82;
            dgvPilotList.Size = new Size(492, 227);
            dgvPilotList.TabIndex = 61;
            // 
            // 이름
            // 
            이름.DataPropertyName = "Name";
            이름.HeaderText = "이름";
            이름.MinimumWidth = 10;
            이름.Name = "이름";
            이름.ReadOnly = true;
            // 
            // 시간
            // 
            시간.DataPropertyName = "Time";
            시간.HeaderText = "시간";
            시간.MinimumWidth = 10;
            시간.Name = "시간";
            시간.ReadOnly = true;
            // 
            // 설명
            // 
            설명.DataPropertyName = "Comment";
            설명.HeaderText = "설명";
            설명.MinimumWidth = 10;
            설명.Name = "설명";
            설명.ReadOnly = true;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1205, 791);
            Controls.Add(splitContainer1);
            Name = "Form1";
            Text = "Malcha v1.0";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picVideoScreen).EndInit();
            ((System.ComponentModel.ISupportInitialize)trbTimeline).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            lblStatus.ResumeLayout(false);
            lblStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chtDataGraph).EndInit();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            groupBox5.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label lblTitle;
        private TextBox txtFilePath;
        private Button btnSelectData;
        private PictureBox picVideoScreen;
        private TrackBar trbTimeline;
        private Button btnPlayPause;
        private Button btnFastForward;
        private Button btnRewind;
        private Button btnNextFrame;
        private Button btnPrevFrame;
        private ListBox lstDataList;
        private Label lblRecordCount;
        private Label lblRecordTitle;
        private Label lblModeValue;
        private Label lblThrottleValue;
        private Label lblAngleValue;
        private Label lblModelValue;
        private Label lblModelAngle;
        private Label lblModeTitle;
        private Label lblThrottleTitle;
        private Label lblAngleTitle;
        private GroupBox groupBox3;
        private StatusStrip lblStatus;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel toolStripStatusLabelPlaybackSpeed;
        private Label lbldeletedlist;
        private ListBox lstDeleted;
        private Button btnChangeCleanData;
        private Button btnHelper;
        private Button btnRefresh;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtDataGraph;
        private Button btnApplyFilter;
        private Button btnRecover;
        private Button btnDeleteSelection;
        private Button btnSetEndPoint;
        private Button btnSetStartPoint;
        private Label lblloglist;
        private Button btnshutdown;
        private ListBox lstLog;
        public GroupBox groupBox4;
        private GroupBox groupBox5;
        private ListBox lstViewScore;
        private TextBox txtModelMemo;
        private Button btnconnet;
        private Button btnEnableDelete;
        private Button btnCrossTest;
        private Button btnRunTraining;
        private DataGridView dgvPilotList;
        private DataGridViewTextBoxColumn 이름;
        private DataGridViewTextBoxColumn 시간;
        private DataGridViewTextBoxColumn 설명;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel1;
        private Button btnSaveCatalog;
        private Button btnUndo;
        private SplitContainer splitContainer2;
        private SplitContainer splitContainer3;
    }
}
