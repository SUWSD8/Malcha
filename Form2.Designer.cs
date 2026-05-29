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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            splitContainer1 = new SplitContainer();
            groupBox3 = new GroupBox();
            lbldeletedlist = new Label();
            lstDeleted = new ListBox();
            btnChangeCleanData = new Button();
            btnHelper = new Button();
            btnRefresh = new Button();
            chtDataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            btnApplyFilter = new Button();
            btnRecover = new Button();
            btnDeleteSelection = new Button();
            btnSetEndPoint = new Button();
            btnSetStartPoint = new Button();
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
            lblModeTitle = new Label();
            lblThrottleTitle = new Label();
            lblAngleTitle = new Label();
            groupBox1 = new GroupBox();
            lblTitle = new Label();
            txtFilePath = new TextBox();
            btnSelectData = new Button();
            lstViewScore = new ListBox();
            lblloglist = new Label();
            txtModelMemo = new TextBox();
            btnconnet = new Button();
            btnEnableDelete = new Button();
            btnCrossTest = new Button();
            btnshutdown = new Button();
            btnRunTraining = new Button();
            dgvPilotList = new DataGridView();
            이름 = new DataGridViewTextBoxColumn();
            시간 = new DataGridViewTextBoxColumn();
            설명 = new DataGridViewTextBoxColumn();
            lstLog = new ListBox();
            lblStatus = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chtDataGraph).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picVideoScreen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trbTimeline).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).BeginInit();
            lblStatus.SuspendLayout();
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
            splitContainer1.Panel1.Controls.Add(groupBox3);
            splitContainer1.Panel1.Controls.Add(groupBox2);
            splitContainer1.Panel1.Controls.Add(groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = Color.FromArgb(33, 28, 29);
            splitContainer1.Panel2.Controls.Add(lstViewScore);
            splitContainer1.Panel2.Controls.Add(lblloglist);
            splitContainer1.Panel2.Controls.Add(txtModelMemo);
            splitContainer1.Panel2.Controls.Add(btnconnet);
            splitContainer1.Panel2.Controls.Add(btnEnableDelete);
            splitContainer1.Panel2.Controls.Add(btnCrossTest);
            splitContainer1.Panel2.Controls.Add(btnshutdown);
            splitContainer1.Panel2.Controls.Add(btnRunTraining);
            splitContainer1.Panel2.Controls.Add(dgvPilotList);
            splitContainer1.Panel2.Controls.Add(lstLog);
            splitContainer1.Size = new Size(1205, 925);
            splitContainer1.SplitterDistance = 790;
            splitContainer1.TabIndex = 0;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(lblStatus);
            groupBox3.Controls.Add(lbldeletedlist);
            groupBox3.Controls.Add(lstDeleted);
            groupBox3.Controls.Add(btnChangeCleanData);
            groupBox3.Controls.Add(btnHelper);
            groupBox3.Controls.Add(btnRefresh);
            groupBox3.Controls.Add(chtDataGraph);
            groupBox3.Controls.Add(btnApplyFilter);
            groupBox3.Controls.Add(btnRecover);
            groupBox3.Controls.Add(btnDeleteSelection);
            groupBox3.Controls.Add(btnSetEndPoint);
            groupBox3.Controls.Add(btnSetStartPoint);
            groupBox3.Location = new Point(3, 632);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(784, 290);
            groupBox3.TabIndex = 9;
            groupBox3.TabStop = false;
            // 
            // lbldeletedlist
            // 
            lbldeletedlist.AutoSize = true;
            lbldeletedlist.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lbldeletedlist.ForeColor = SystemColors.ButtonHighlight;
            lbldeletedlist.Location = new Point(30, 23);
            lbldeletedlist.Name = "lbldeletedlist";
            lbldeletedlist.Size = new Size(89, 20);
            lbldeletedlist.TabIndex = 25;
            lbldeletedlist.Text = "Deleted list";
            // 
            // lstDeleted
            // 
            lstDeleted.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lstDeleted.BackColor = Color.FromArgb(48, 42, 41);
            lstDeleted.ForeColor = SystemColors.MenuBar;
            lstDeleted.FormattingEnabled = true;
            lstDeleted.Location = new Point(28, 63);
            lstDeleted.Name = "lstDeleted";
            lstDeleted.Size = new Size(124, 199);
            lstDeleted.TabIndex = 24;
            // 
            // btnChangeCleanData
            // 
            btnChangeCleanData.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnChangeCleanData.BackColor = Color.FromArgb(198, 100, 114);
            btnChangeCleanData.FlatStyle = FlatStyle.Popup;
            btnChangeCleanData.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnChangeCleanData.ForeColor = SystemColors.ButtonHighlight;
            btnChangeCleanData.Location = new Point(661, 62);
            btnChangeCleanData.Name = "btnChangeCleanData";
            btnChangeCleanData.Size = new Size(91, 51);
            btnChangeCleanData.TabIndex = 15;
            btnChangeCleanData.Text = "정제 데이터 변경";
            btnChangeCleanData.UseVisualStyleBackColor = false;
            // 
            // btnHelper
            // 
            btnHelper.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelper.BackColor = Color.FromArgb(53, 48, 49);
            btnHelper.FlatStyle = FlatStyle.Popup;
            btnHelper.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnHelper.ForeColor = SystemColors.ButtonHighlight;
            btnHelper.Location = new Point(661, 176);
            btnHelper.Name = "btnHelper";
            btnHelper.Size = new Size(91, 51);
            btnHelper.TabIndex = 23;
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
            btnRefresh.Location = new Point(661, 119);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(91, 51);
            btnRefresh.TabIndex = 22;
            btnRefresh.Text = "새로고침";
            btnRefresh.UseVisualStyleBackColor = false;
            // 
            // chtDataGraph
            // 
            chtDataGraph.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            chtDataGraph.BackColor = Color.FromArgb(48, 42, 41);
            chartArea1.Name = "ChartArea1";
            chtDataGraph.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            chtDataGraph.Legends.Add(legend1);
            chtDataGraph.Location = new Point(166, 68);
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
            chtDataGraph.Size = new Size(483, 178);
            chtDataGraph.TabIndex = 21;
            chtDataGraph.Text = "chart1";
            // 
            // btnApplyFilter
            // 
            btnApplyFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApplyFilter.BackColor = Color.FromArgb(53, 48, 49);
            btnApplyFilter.FlatStyle = FlatStyle.Popup;
            btnApplyFilter.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnApplyFilter.ForeColor = SystemColors.ButtonHighlight;
            btnApplyFilter.Location = new Point(636, 23);
            btnApplyFilter.Name = "btnApplyFilter";
            btnApplyFilter.Size = new Size(103, 23);
            btnApplyFilter.TabIndex = 20;
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
            btnRecover.Location = new Point(527, 23);
            btnRecover.Name = "btnRecover";
            btnRecover.Size = new Size(103, 23);
            btnRecover.TabIndex = 19;
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
            btnDeleteSelection.Location = new Point(418, 23);
            btnDeleteSelection.Name = "btnDeleteSelection";
            btnDeleteSelection.Size = new Size(103, 23);
            btnDeleteSelection.TabIndex = 18;
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
            btnSetEndPoint.Location = new Point(309, 23);
            btnSetEndPoint.Name = "btnSetEndPoint";
            btnSetEndPoint.Size = new Size(103, 23);
            btnSetEndPoint.TabIndex = 17;
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
            btnSetStartPoint.Location = new Point(200, 23);
            btnSetStartPoint.Name = "btnSetStartPoint";
            btnSetStartPoint.Size = new Size(103, 23);
            btnSetStartPoint.TabIndex = 16;
            btnSetStartPoint.Text = "시작점 설정";
            btnSetStartPoint.UseVisualStyleBackColor = false;
            // 
            // groupBox2
            // 
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
            groupBox2.Controls.Add(lblModeTitle);
            groupBox2.Controls.Add(lblThrottleTitle);
            groupBox2.Controls.Add(lblAngleTitle);
            groupBox2.Location = new Point(3, 103);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(784, 529);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            // 
            // picVideoScreen
            // 
            picVideoScreen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picVideoScreen.BackColor = Color.Black;
            picVideoScreen.Location = new Point(133, 22);
            picVideoScreen.Name = "picVideoScreen";
            picVideoScreen.Size = new Size(482, 438);
            picVideoScreen.TabIndex = 28;
            picVideoScreen.TabStop = false;
            // 
            // trbTimeline
            // 
            trbTimeline.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trbTimeline.Location = new Point(18, 466);
            trbTimeline.Name = "trbTimeline";
            trbTimeline.Size = new Size(749, 45);
            trbTimeline.TabIndex = 29;
            // 
            // btnPlayPause
            // 
            btnPlayPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPlayPause.BackColor = Color.FromArgb(198, 100, 114);
            btnPlayPause.FlatStyle = FlatStyle.Popup;
            btnPlayPause.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnPlayPause.ForeColor = Color.White;
            btnPlayPause.Location = new Point(621, 421);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new Size(134, 39);
            btnPlayPause.TabIndex = 34;
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
            btnFastForward.Location = new Point(691, 365);
            btnFastForward.Name = "btnFastForward";
            btnFastForward.Size = new Size(64, 50);
            btnFastForward.TabIndex = 33;
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
            btnRewind.Location = new Point(621, 365);
            btnRewind.Name = "btnRewind";
            btnRewind.Size = new Size(64, 50);
            btnRewind.TabIndex = 32;
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
            btnNextFrame.Location = new Point(691, 309);
            btnNextFrame.Name = "btnNextFrame";
            btnNextFrame.Size = new Size(64, 50);
            btnNextFrame.TabIndex = 31;
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
            btnPrevFrame.Location = new Point(621, 309);
            btnPrevFrame.Name = "btnPrevFrame";
            btnPrevFrame.Size = new Size(64, 50);
            btnPrevFrame.TabIndex = 30;
            btnPrevFrame.Text = "<";
            btnPrevFrame.UseVisualStyleBackColor = false;
            // 
            // lstDataList
            // 
            lstDataList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lstDataList.BackColor = Color.FromArgb(48, 42, 41);
            lstDataList.ForeColor = SystemColors.MenuBar;
            lstDataList.FormattingEnabled = true;
            lstDataList.Location = new Point(623, 106);
            lstDataList.Name = "lstDataList";
            lstDataList.Size = new Size(134, 184);
            lstDataList.TabIndex = 18;
            // 
            // lblRecordCount
            // 
            lblRecordCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRecordCount.AutoSize = true;
            lblRecordCount.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblRecordCount.ForeColor = SystemColors.ButtonHighlight;
            lblRecordCount.Location = new Point(623, 59);
            lblRecordCount.Name = "lblRecordCount";
            lblRecordCount.Size = new Size(98, 32);
            lblRecordCount.TabIndex = 23;
            lblRecordCount.Text = "000000";
            // 
            // lblRecordTitle
            // 
            lblRecordTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRecordTitle.AutoSize = true;
            lblRecordTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblRecordTitle.ForeColor = SystemColors.ButtonHighlight;
            lblRecordTitle.Location = new Point(623, 39);
            lblRecordTitle.Name = "lblRecordTitle";
            lblRecordTitle.Size = new Size(58, 20);
            lblRecordTitle.TabIndex = 22;
            lblRecordTitle.Text = "Record";
            // 
            // lblModeValue
            // 
            lblModeValue.AutoSize = true;
            lblModeValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModeValue.ForeColor = SystemColors.ButtonHighlight;
            lblModeValue.Location = new Point(46, 190);
            lblModeValue.Name = "lblModeValue";
            lblModeValue.Size = new Size(39, 20);
            lblModeValue.TabIndex = 21;
            lblModeValue.Text = "user";
            // 
            // lblThrottleValue
            // 
            lblThrottleValue.AutoSize = true;
            lblThrottleValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblThrottleValue.ForeColor = SystemColors.ButtonHighlight;
            lblThrottleValue.Location = new Point(46, 129);
            lblThrottleValue.Name = "lblThrottleValue";
            lblThrottleValue.Size = new Size(69, 20);
            lblThrottleValue.TabIndex = 20;
            lblThrottleValue.Text = "+00.000";
            // 
            // lblAngleValue
            // 
            lblAngleValue.AutoSize = true;
            lblAngleValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleValue.ForeColor = SystemColors.ButtonHighlight;
            lblAngleValue.Location = new Point(46, 69);
            lblAngleValue.Name = "lblAngleValue";
            lblAngleValue.Size = new Size(69, 20);
            lblAngleValue.TabIndex = 19;
            lblAngleValue.Text = "+00.000";
            // 
            // lblModeTitle
            // 
            lblModeTitle.AutoSize = true;
            lblModeTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModeTitle.ForeColor = SystemColors.ButtonHighlight;
            lblModeTitle.Location = new Point(29, 160);
            lblModeTitle.Name = "lblModeTitle";
            lblModeTitle.Size = new Size(86, 20);
            lblModeTitle.TabIndex = 17;
            lblModeTitle.Text = "user/mode";
            // 
            // lblThrottleTitle
            // 
            lblThrottleTitle.AutoSize = true;
            lblThrottleTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblThrottleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblThrottleTitle.Location = new Point(29, 99);
            lblThrottleTitle.Name = "lblThrottleTitle";
            lblThrottleTitle.Size = new Size(100, 20);
            lblThrottleTitle.TabIndex = 13;
            lblThrottleTitle.Text = "user/throttle";
            // 
            // lblAngleTitle
            // 
            lblAngleTitle.AutoSize = true;
            lblAngleTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblAngleTitle.Location = new Point(29, 39);
            lblAngleTitle.Name = "lblAngleTitle";
            lblAngleTitle.Size = new Size(84, 20);
            lblAngleTitle.TabIndex = 12;
            lblAngleTitle.Text = "user/angle";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblTitle);
            groupBox1.Controls.Add(txtFilePath);
            groupBox1.Controls.Add(btnSelectData);
            groupBox1.Location = new Point(3, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(784, 100);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblTitle.ForeColor = Color.FromArgb(227, 98, 132);
            lblTitle.Location = new Point(14, 23);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(138, 32);
            lblTitle.TabIndex = 4;
            lblTitle.Text = "DonkeyCar";
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtFilePath.Location = new Point(143, 67);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(631, 23);
            txtFilePath.TabIndex = 6;
            // 
            // btnSelectData
            // 
            btnSelectData.Anchor = AnchorStyles.Left;
            btnSelectData.BackColor = Color.FromArgb(53, 48, 49);
            btnSelectData.FlatStyle = FlatStyle.Popup;
            btnSelectData.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSelectData.ForeColor = SystemColors.ButtonHighlight;
            btnSelectData.Location = new Point(11, 67);
            btnSelectData.Name = "btnSelectData";
            btnSelectData.Size = new Size(121, 23);
            btnSelectData.TabIndex = 5;
            btnSelectData.Text = "데이터 선택";
            btnSelectData.UseVisualStyleBackColor = false;
            // 
            // lstViewScore
            // 
            lstViewScore.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lstViewScore.BackColor = Color.FromArgb(48, 42, 41);
            lstViewScore.ForeColor = SystemColors.MenuBar;
            lstViewScore.FormattingEnabled = true;
            lstViewScore.Location = new Point(17, 404);
            lstViewScore.Name = "lstViewScore";
            lstViewScore.Size = new Size(382, 139);
            lstViewScore.TabIndex = 51;
            // 
            // lblloglist
            // 
            lblloglist.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblloglist.AutoSize = true;
            lblloglist.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblloglist.ForeColor = SystemColors.ButtonHighlight;
            lblloglist.Location = new Point(17, 9);
            lblloglist.Name = "lblloglist";
            lblloglist.Size = new Size(61, 20);
            lblloglist.TabIndex = 50;
            lblloglist.Text = "Log list";
            // 
            // txtModelMemo
            // 
            txtModelMemo.Anchor = AnchorStyles.Bottom;
            txtModelMemo.BackColor = Color.FromArgb(103, 98, 98);
            txtModelMemo.BorderStyle = BorderStyle.FixedSingle;
            txtModelMemo.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtModelMemo.ForeColor = SystemColors.MenuBar;
            txtModelMemo.Location = new Point(17, 830);
            txtModelMemo.Name = "txtModelMemo";
            txtModelMemo.Size = new Size(382, 25);
            txtModelMemo.TabIndex = 59;
            txtModelMemo.Text = "메모 / 설명";
            // 
            // btnconnet
            // 
            btnconnet.Anchor = AnchorStyles.Bottom;
            btnconnet.BackColor = Color.FromArgb(53, 48, 49);
            btnconnet.FlatStyle = FlatStyle.Popup;
            btnconnet.Font = new Font("맑은 고딕", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnconnet.ForeColor = SystemColors.ButtonHighlight;
            btnconnet.Location = new Point(212, 864);
            btnconnet.Name = "btnconnet";
            btnconnet.Size = new Size(187, 34);
            btnconnet.TabIndex = 58;
            btnconnet.Text = "설명 추가";
            btnconnet.UseVisualStyleBackColor = false;
            // 
            // btnEnableDelete
            // 
            btnEnableDelete.Anchor = AnchorStyles.Bottom;
            btnEnableDelete.BackColor = Color.FromArgb(53, 48, 49);
            btnEnableDelete.FlatStyle = FlatStyle.Popup;
            btnEnableDelete.Font = new Font("맑은 고딕", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnEnableDelete.ForeColor = SystemColors.ButtonHighlight;
            btnEnableDelete.Location = new Point(17, 864);
            btnEnableDelete.Name = "btnEnableDelete";
            btnEnableDelete.Size = new Size(187, 34);
            btnEnableDelete.TabIndex = 57;
            btnEnableDelete.Text = " 모델 삭제 ";
            btnEnableDelete.UseVisualStyleBackColor = false;
            // 
            // btnCrossTest
            // 
            btnCrossTest.Anchor = AnchorStyles.Bottom;
            btnCrossTest.BackColor = Color.FromArgb(106, 123, 221);
            btnCrossTest.FlatStyle = FlatStyle.Popup;
            btnCrossTest.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnCrossTest.ForeColor = SystemColors.ButtonHighlight;
            btnCrossTest.Location = new Point(211, 551);
            btnCrossTest.Name = "btnCrossTest";
            btnCrossTest.Size = new Size(188, 37);
            btnCrossTest.TabIndex = 56;
            btnCrossTest.Text = "교차 테스트";
            btnCrossTest.UseVisualStyleBackColor = false;
            // 
            // btnshutdown
            // 
            btnshutdown.Anchor = AnchorStyles.Bottom;
            btnshutdown.BackColor = Color.FromArgb(214, 71, 129);
            btnshutdown.FlatStyle = FlatStyle.Popup;
            btnshutdown.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnshutdown.ForeColor = SystemColors.ButtonHighlight;
            btnshutdown.Location = new Point(17, 358);
            btnshutdown.Name = "btnshutdown";
            btnshutdown.Size = new Size(382, 37);
            btnshutdown.TabIndex = 55;
            btnshutdown.Text = "학습 강제 종료";
            btnshutdown.UseVisualStyleBackColor = false;
            // 
            // btnRunTraining
            // 
            btnRunTraining.Anchor = AnchorStyles.Bottom;
            btnRunTraining.BackColor = Color.FromArgb(198, 100, 114);
            btnRunTraining.FlatStyle = FlatStyle.Popup;
            btnRunTraining.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRunTraining.ForeColor = SystemColors.ButtonHighlight;
            btnRunTraining.Location = new Point(17, 551);
            btnRunTraining.Name = "btnRunTraining";
            btnRunTraining.Size = new Size(188, 37);
            btnRunTraining.TabIndex = 54;
            btnRunTraining.Text = "학습 시작";
            btnRunTraining.UseVisualStyleBackColor = false;
            // 
            // dgvPilotList
            // 
            dgvPilotList.AllowUserToAddRows = false;
            dgvPilotList.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvPilotList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPilotList.BackgroundColor = Color.FromArgb(103, 98, 98);
            dgvPilotList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvPilotList.Columns.AddRange(new DataGridViewColumn[] { 이름, 시간, 설명 });
            dgvPilotList.Location = new Point(17, 594);
            dgvPilotList.Name = "dgvPilotList";
            dgvPilotList.RowHeadersVisible = false;
            dgvPilotList.RowHeadersWidth = 82;
            dgvPilotList.Size = new Size(382, 227);
            dgvPilotList.TabIndex = 53;
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
            // lstLog
            // 
            lstLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            lstLog.BackColor = Color.FromArgb(48, 42, 41);
            lstLog.ForeColor = SystemColors.MenuBar;
            lstLog.FormattingEnabled = true;
            lstLog.Location = new Point(17, 40);
            lstLog.Name = "lstLog";
            lstLog.Size = new Size(382, 304);
            lstLog.TabIndex = 52;
            // 
            // lblStatus
            // 
            lblStatus.BackColor = Color.Black;
            lblStatus.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblStatus.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            lblStatus.Location = new Point(3, 265);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(778, 22);
            lblStatus.TabIndex = 26;
            lblStatus.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.ForeColor = SystemColors.ButtonHighlight;
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(194, 17);
            toolStripStatusLabel1.Text = "동키카 준비 완료 (Donkey Ready)";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1205, 925);
            Controls.Add(splitContainer1);
            Name = "Form2";
            Text = "Form2";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chtDataGraph).EndInit();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picVideoScreen).EndInit();
            ((System.ComponentModel.ISupportInitialize)trbTimeline).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).EndInit();
            lblStatus.ResumeLayout(false);
            lblStatus.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Button btnMyPilot;
        private SplitContainer splitContainer1;
        private TextBox txtFilePath;
        private Button btnSelectData;
        private Label lblTitle;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
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
        private Label lblModeTitle;
        private Label lblThrottleTitle;
        private Label lblAngleTitle;
        private GroupBox groupBox3;
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
        private ListBox lstViewScore;
        private Label lblloglist;
        private TextBox txtModelMemo;
        private Button btnconnet;
        private Button btnEnableDelete;
        private Button btnCrossTest;
        private Button btnshutdown;
        private Button btnRunTraining;
        private DataGridView dgvPilotList;
        private DataGridViewTextBoxColumn 이름;
        private DataGridViewTextBoxColumn 시간;
        private DataGridViewTextBoxColumn 설명;
        private ListBox lstLog;
        private StatusStrip lblStatus;
        private ToolStripStatusLabel toolStripStatusLabel1;
    }
}