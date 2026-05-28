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
            panel1 = new Panel();
            txtFilePath = new TextBox();
            btnSelectData = new Button();
            lblTitle = new Label();
            btnChangeCleanData = new Button();
            panel2 = new Panel();
            lstDataList = new ListBox();
            picVideoScreen = new PictureBox();
            trbTimeline = new TrackBar();
            btnPlayPause = new Button();
            btnFastForward = new Button();
            btnRewind = new Button();
            btnNextFrame = new Button();
            btnPrevFrame = new Button();
            lblRecordCount = new Label();
            lblRecordTitle = new Label();
            lblModeValue = new Label();
            lblThrottleValue = new Label();
            lblAngleValue = new Label();
            lblModeTitle = new Label();
            lblThrottleTitle = new Label();
            lblAngleTitle = new Label();
            panel3 = new Panel();
            lbldeletedlist = new Label();
            lstDeleted = new ListBox();
            lblStatus = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            btnHelper = new Button();
            btnRefresh = new Button();
            chtDataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            btnApplyFilter = new Button();
            btnRecover = new Button();
            btnDeleteSelection = new Button();
            btnSetEndPoint = new Button();
            btnSetStartPoint = new Button();
            panel4 = new Panel();
            lstLog = new ListBox();
            panel6 = new Panel();
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
            btnSaveCatalog = new Button();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picVideoScreen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trbTimeline).BeginInit();
            panel3.SuspendLayout();
            lblStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chtDataGraph).BeginInit();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.FromArgb(33, 28, 29);
            panel1.Controls.Add(btnSaveCatalog);
            panel1.Controls.Add(txtFilePath);
            panel1.Controls.Add(btnSelectData);
            panel1.Controls.Add(lblTitle);
            panel1.Location = new Point(-3, 1);
            panel1.Name = "panel1";
            panel1.Size = new Size(752, 83);
            panel1.TabIndex = 0;
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtFilePath.Location = new Point(142, 47);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(483, 23);
            txtFilePath.TabIndex = 3;
            // 
            // btnSelectData
            // 
            btnSelectData.Anchor = AnchorStyles.Left;
            btnSelectData.BackColor = Color.FromArgb(53, 48, 49);
            btnSelectData.FlatStyle = FlatStyle.Popup;
            btnSelectData.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSelectData.ForeColor = SystemColors.ButtonHighlight;
            btnSelectData.Location = new Point(10, 47);
            btnSelectData.Name = "btnSelectData";
            btnSelectData.Size = new Size(121, 23);
            btnSelectData.TabIndex = 2;
            btnSelectData.Text = "데이터 선택";
            btnSelectData.UseVisualStyleBackColor = false;
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
            // btnChangeCleanData
            // 
            btnChangeCleanData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            btnChangeCleanData.BackColor = Color.FromArgb(198, 100, 114);
            btnChangeCleanData.FlatStyle = FlatStyle.Popup;
            btnChangeCleanData.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnChangeCleanData.ForeColor = SystemColors.ButtonHighlight;
            btnChangeCleanData.Location = new Point(647, 50);
            btnChangeCleanData.Name = "btnChangeCleanData";
            btnChangeCleanData.Size = new Size(91, 51);
            btnChangeCleanData.TabIndex = 1;
            btnChangeCleanData.Text = "정제 데이터 연동";
            btnChangeCleanData.UseVisualStyleBackColor = false;
            // 
            // panel2
            // 
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(20, 20, 20);
            panel2.Controls.Add(lstDataList);
            panel2.Controls.Add(picVideoScreen);
            panel2.Controls.Add(trbTimeline);
            panel2.Controls.Add(btnPlayPause);
            panel2.Controls.Add(btnFastForward);
            panel2.Controls.Add(btnRewind);
            panel2.Controls.Add(btnNextFrame);
            panel2.Controls.Add(btnPrevFrame);
            panel2.Controls.Add(lblRecordCount);
            panel2.Controls.Add(lblRecordTitle);
            panel2.Controls.Add(lblModeValue);
            panel2.Controls.Add(lblThrottleValue);
            panel2.Controls.Add(lblAngleValue);
            panel2.Controls.Add(lblModeTitle);
            panel2.Controls.Add(lblThrottleTitle);
            panel2.Controls.Add(lblAngleTitle);
            panel2.Location = new Point(-3, 88);
            panel2.Name = "panel2";
            panel2.Size = new Size(752, 501);
            panel2.TabIndex = 1;
            // 
            // lstDataList
            // 
            lstDataList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            lstDataList.BackColor = Color.FromArgb(48, 42, 41);
            lstDataList.ForeColor = SystemColors.MenuBar;
            lstDataList.FormattingEnabled = true;
            lstDataList.Location = new Point(604, 86);
            lstDataList.Name = "lstDataList";
            lstDataList.Size = new Size(134, 199);
            lstDataList.TabIndex = 3;
            // 
            // picVideoScreen
            // 
            picVideoScreen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picVideoScreen.BackColor = Color.Black;
            picVideoScreen.Location = new Point(116, 6);
            picVideoScreen.Name = "picVideoScreen";
            picVideoScreen.Size = new Size(482, 442);
            picVideoScreen.TabIndex = 2;
            picVideoScreen.TabStop = false;
            // 
            // trbTimeline
            // 
            trbTimeline.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trbTimeline.Location = new Point(0, 454);
            trbTimeline.Name = "trbTimeline";
            trbTimeline.Size = new Size(749, 45);
            trbTimeline.TabIndex = 2;
            // 
            // btnPlayPause
            // 
            btnPlayPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPlayPause.BackColor = Color.FromArgb(198, 100, 114);
            btnPlayPause.FlatStyle = FlatStyle.Popup;
            btnPlayPause.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnPlayPause.ForeColor = Color.White;
            btnPlayPause.Location = new Point(604, 409);
            btnPlayPause.Name = "btnPlayPause";
            btnPlayPause.Size = new Size(134, 39);
            btnPlayPause.TabIndex = 11;
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
            btnFastForward.Location = new Point(674, 353);
            btnFastForward.Name = "btnFastForward";
            btnFastForward.Size = new Size(64, 50);
            btnFastForward.TabIndex = 10;
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
            btnRewind.Location = new Point(604, 353);
            btnRewind.Name = "btnRewind";
            btnRewind.Size = new Size(64, 50);
            btnRewind.TabIndex = 9;
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
            btnNextFrame.Location = new Point(674, 297);
            btnNextFrame.Name = "btnNextFrame";
            btnNextFrame.Size = new Size(64, 50);
            btnNextFrame.TabIndex = 8;
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
            btnPrevFrame.Location = new Point(604, 297);
            btnPrevFrame.Name = "btnPrevFrame";
            btnPrevFrame.Size = new Size(64, 50);
            btnPrevFrame.TabIndex = 2;
            btnPrevFrame.Text = "<";
            btnPrevFrame.UseVisualStyleBackColor = false;
            // 
            // lblRecordCount
            // 
            lblRecordCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRecordCount.AutoSize = true;
            lblRecordCount.Font = new Font("맑은 고딕", 18F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblRecordCount.ForeColor = SystemColors.ButtonHighlight;
            lblRecordCount.Location = new Point(604, 34);
            lblRecordCount.Name = "lblRecordCount";
            lblRecordCount.Size = new Size(98, 32);
            lblRecordCount.TabIndex = 7;
            lblRecordCount.Text = "000000";
            // 
            // lblRecordTitle
            // 
            lblRecordTitle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRecordTitle.AutoSize = true;
            lblRecordTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblRecordTitle.ForeColor = SystemColors.ButtonHighlight;
            lblRecordTitle.Location = new Point(604, 14);
            lblRecordTitle.Name = "lblRecordTitle";
            lblRecordTitle.Size = new Size(58, 20);
            lblRecordTitle.TabIndex = 6;
            lblRecordTitle.Text = "Record";
            // 
            // lblModeValue
            // 
            lblModeValue.AutoSize = true;
            lblModeValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModeValue.ForeColor = SystemColors.ButtonHighlight;
            lblModeValue.Location = new Point(27, 165);
            lblModeValue.Name = "lblModeValue";
            lblModeValue.Size = new Size(39, 20);
            lblModeValue.TabIndex = 5;
            lblModeValue.Text = "user";
            // 
            // lblThrottleValue
            // 
            lblThrottleValue.AutoSize = true;
            lblThrottleValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblThrottleValue.ForeColor = SystemColors.ButtonHighlight;
            lblThrottleValue.Location = new Point(27, 104);
            lblThrottleValue.Name = "lblThrottleValue";
            lblThrottleValue.Size = new Size(69, 20);
            lblThrottleValue.TabIndex = 4;
            lblThrottleValue.Text = "+00.000";
            // 
            // lblAngleValue
            // 
            lblAngleValue.AutoSize = true;
            lblAngleValue.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleValue.ForeColor = SystemColors.ButtonHighlight;
            lblAngleValue.Location = new Point(27, 44);
            lblAngleValue.Name = "lblAngleValue";
            lblAngleValue.Size = new Size(69, 20);
            lblAngleValue.TabIndex = 3;
            lblAngleValue.Text = "+00.000";
            // 
            // lblModeTitle
            // 
            lblModeTitle.AutoSize = true;
            lblModeTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblModeTitle.ForeColor = SystemColors.ButtonHighlight;
            lblModeTitle.Location = new Point(10, 135);
            lblModeTitle.Name = "lblModeTitle";
            lblModeTitle.Size = new Size(86, 20);
            lblModeTitle.TabIndex = 2;
            lblModeTitle.Text = "user/mode";
            // 
            // lblThrottleTitle
            // 
            lblThrottleTitle.AutoSize = true;
            lblThrottleTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblThrottleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblThrottleTitle.Location = new Point(10, 74);
            lblThrottleTitle.Name = "lblThrottleTitle";
            lblThrottleTitle.Size = new Size(100, 20);
            lblThrottleTitle.TabIndex = 1;
            lblThrottleTitle.Text = "user/throttle";
            // 
            // lblAngleTitle
            // 
            lblAngleTitle.AutoSize = true;
            lblAngleTitle.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblAngleTitle.ForeColor = SystemColors.ButtonHighlight;
            lblAngleTitle.Location = new Point(10, 14);
            lblAngleTitle.Name = "lblAngleTitle";
            lblAngleTitle.Size = new Size(84, 20);
            lblAngleTitle.TabIndex = 0;
            lblAngleTitle.Text = "user/angle";
            // 
            // panel3
            // 
            panel3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel3.BackColor = Color.FromArgb(33, 28, 29);
            panel3.Controls.Add(lbldeletedlist);
            panel3.Controls.Add(lstDeleted);
            panel3.Controls.Add(btnChangeCleanData);
            panel3.Controls.Add(lblStatus);
            panel3.Controls.Add(btnHelper);
            panel3.Controls.Add(btnRefresh);
            panel3.Controls.Add(chtDataGraph);
            panel3.Controls.Add(btnApplyFilter);
            panel3.Controls.Add(btnRecover);
            panel3.Controls.Add(btnDeleteSelection);
            panel3.Controls.Add(btnSetEndPoint);
            panel3.Controls.Add(btnSetStartPoint);
            panel3.Location = new Point(-3, 585);
            panel3.Name = "panel3";
            panel3.Size = new Size(752, 270);
            panel3.TabIndex = 2;
            // 
            // lbldeletedlist
            // 
            lbldeletedlist.AutoSize = true;
            lbldeletedlist.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lbldeletedlist.ForeColor = SystemColors.ButtonHighlight;
            lbldeletedlist.Location = new Point(19, 11);
            lbldeletedlist.Name = "lbldeletedlist";
            lbldeletedlist.Size = new Size(89, 20);
            lbldeletedlist.TabIndex = 14;
            lbldeletedlist.Text = "Deleted list";
            // 
            // lstDeleted
            // 
            lstDeleted.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lstDeleted.BackColor = Color.FromArgb(48, 42, 41);
            lstDeleted.ForeColor = SystemColors.MenuBar;
            lstDeleted.FormattingEnabled = true;
            lstDeleted.Location = new Point(17, 36);
            lstDeleted.Name = "lstDeleted";
            lstDeleted.Size = new Size(124, 199);
            lstDeleted.TabIndex = 13;
            // 
            // lblStatus
            // 
            lblStatus.BackColor = Color.Black;
            lblStatus.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblStatus.ImageScalingSize = new Size(20, 20);
            lblStatus.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            lblStatus.Location = new Point(0, 248);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(752, 22);
            lblStatus.TabIndex = 12;
            lblStatus.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.ForeColor = SystemColors.ButtonHighlight;
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(194, 17);
            toolStripStatusLabel1.Text = "동키카 준비 완료 (Donkey Ready)";
            // 
            // btnHelper
            // 
            btnHelper.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnHelper.BackColor = Color.FromArgb(53, 48, 49);
            btnHelper.FlatStyle = FlatStyle.Popup;
            btnHelper.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnHelper.ForeColor = SystemColors.ButtonHighlight;
            btnHelper.Location = new Point(647, 164);
            btnHelper.Name = "btnHelper";
            btnHelper.Size = new Size(91, 51);
            btnHelper.TabIndex = 11;
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
            btnRefresh.Location = new Point(647, 107);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(91, 51);
            btnRefresh.TabIndex = 10;
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
            chtDataGraph.Location = new Point(158, 50);
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
            chtDataGraph.Size = new Size(480, 178);
            chtDataGraph.TabIndex = 9;
            chtDataGraph.Text = "chart1";
            // 
            // btnApplyFilter
            // 
            btnApplyFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnApplyFilter.BackColor = Color.FromArgb(53, 48, 49);
            btnApplyFilter.FlatStyle = FlatStyle.Popup;
            btnApplyFilter.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnApplyFilter.ForeColor = SystemColors.ButtonHighlight;
            btnApplyFilter.Location = new Point(622, 11);
            btnApplyFilter.Name = "btnApplyFilter";
            btnApplyFilter.Size = new Size(103, 23);
            btnApplyFilter.TabIndex = 8;
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
            btnRecover.Location = new Point(513, 11);
            btnRecover.Name = "btnRecover";
            btnRecover.Size = new Size(103, 23);
            btnRecover.TabIndex = 7;
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
            btnDeleteSelection.Location = new Point(404, 11);
            btnDeleteSelection.Name = "btnDeleteSelection";
            btnDeleteSelection.Size = new Size(103, 23);
            btnDeleteSelection.TabIndex = 6;
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
            btnSetEndPoint.Location = new Point(295, 11);
            btnSetEndPoint.Name = "btnSetEndPoint";
            btnSetEndPoint.Size = new Size(103, 23);
            btnSetEndPoint.TabIndex = 5;
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
            btnSetStartPoint.Location = new Point(186, 11);
            btnSetStartPoint.Name = "btnSetStartPoint";
            btnSetStartPoint.Size = new Size(103, 23);
            btnSetStartPoint.TabIndex = 4;
            btnSetStartPoint.Text = "시작점 설정";
            btnSetStartPoint.UseVisualStyleBackColor = false;
            // 
            // panel4
            // 
            panel4.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel4.BackColor = Color.FromArgb(198, 100, 114);
            panel4.Location = new Point(0, 79);
            panel4.Name = "panel4";
            panel4.Size = new Size(749, 20);
            panel4.TabIndex = 12;
            // 
            // lstLog
            // 
            lstLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            lstLog.BackColor = Color.FromArgb(48, 42, 41);
            lstLog.ForeColor = SystemColors.MenuBar;
            lstLog.FormattingEnabled = true;
            lstLog.Location = new Point(17, 37);
            lstLog.Name = "lstLog";
            lstLog.Size = new Size(195, 304);
            lstLog.TabIndex = 35;
            // 
            // panel6
            // 
            panel6.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            panel6.BackColor = Color.FromArgb(33, 28, 29);
            panel6.Controls.Add(lstViewScore);
            panel6.Controls.Add(lblloglist);
            panel6.Controls.Add(txtModelMemo);
            panel6.Controls.Add(btnconnet);
            panel6.Controls.Add(btnEnableDelete);
            panel6.Controls.Add(btnCrossTest);
            panel6.Controls.Add(btnshutdown);
            panel6.Controls.Add(btnRunTraining);
            panel6.Controls.Add(dgvPilotList);
            panel6.Controls.Add(lstLog);
            panel6.Location = new Point(755, 1);
            panel6.Name = "panel6";
            panel6.Size = new Size(232, 854);
            panel6.TabIndex = 0;
            // 
            // lstViewScore
            // 
            lstViewScore.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lstViewScore.BackColor = Color.FromArgb(48, 42, 41);
            lstViewScore.ForeColor = SystemColors.MenuBar;
            lstViewScore.FormattingEnabled = true;
            lstViewScore.Location = new Point(17, 397);
            lstViewScore.Name = "lstViewScore";
            lstViewScore.Size = new Size(195, 79);
            lstViewScore.TabIndex = 15;
            // 
            // lblloglist
            // 
            lblloglist.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblloglist.AutoSize = true;
            lblloglist.Font = new Font("맑은 고딕", 11.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblloglist.ForeColor = SystemColors.ButtonHighlight;
            lblloglist.Location = new Point(17, 10);
            lblloglist.Name = "lblloglist";
            lblloglist.Size = new Size(61, 20);
            lblloglist.TabIndex = 12;
            lblloglist.Text = "Log list";
            // 
            // txtModelMemo
            // 
            txtModelMemo.Anchor = AnchorStyles.Bottom;
            txtModelMemo.BackColor = Color.FromArgb(103, 98, 98);
            txtModelMemo.BorderStyle = BorderStyle.FixedSingle;
            txtModelMemo.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            txtModelMemo.ForeColor = SystemColors.MenuBar;
            txtModelMemo.Location = new Point(17, 763);
            txtModelMemo.Name = "txtModelMemo";
            txtModelMemo.Size = new Size(195, 25);
            txtModelMemo.TabIndex = 49;
            txtModelMemo.Text = "메모 / 설명";
            // 
            // btnconnet
            // 
            btnconnet.Anchor = AnchorStyles.Bottom;
            btnconnet.BackColor = Color.FromArgb(53, 48, 49);
            btnconnet.FlatStyle = FlatStyle.Popup;
            btnconnet.Font = new Font("맑은 고딕", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnconnet.ForeColor = SystemColors.ButtonHighlight;
            btnconnet.Location = new Point(120, 794);
            btnconnet.Name = "btnconnet";
            btnconnet.Size = new Size(92, 34);
            btnconnet.TabIndex = 48;
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
            btnEnableDelete.Location = new Point(17, 794);
            btnEnableDelete.Name = "btnEnableDelete";
            btnEnableDelete.Size = new Size(92, 34);
            btnEnableDelete.TabIndex = 47;
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
            btnCrossTest.Location = new Point(119, 487);
            btnCrossTest.Name = "btnCrossTest";
            btnCrossTest.Size = new Size(93, 37);
            btnCrossTest.TabIndex = 46;
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
            btnshutdown.Location = new Point(17, 351);
            btnshutdown.Name = "btnshutdown";
            btnshutdown.Size = new Size(195, 37);
            btnshutdown.TabIndex = 45;
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
            btnRunTraining.Location = new Point(17, 487);
            btnRunTraining.Name = "btnRunTraining";
            btnRunTraining.Size = new Size(93, 37);
            btnRunTraining.TabIndex = 44;
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
            dgvPilotList.Location = new Point(17, 530);
            dgvPilotList.Name = "dgvPilotList";
            dgvPilotList.RowHeadersVisible = false;
            dgvPilotList.RowHeadersWidth = 82;
            dgvPilotList.Size = new Size(195, 227);
            dgvPilotList.TabIndex = 40;
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
            // btnSaveCatalog
            // 
            btnSaveCatalog.Anchor = AnchorStyles.Left;
            btnSaveCatalog.BackColor = Color.FromArgb(53, 48, 49);
            btnSaveCatalog.FlatStyle = FlatStyle.Popup;
            btnSaveCatalog.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSaveCatalog.ForeColor = SystemColors.ButtonHighlight;
            btnSaveCatalog.Location = new Point(642, 47);
            btnSaveCatalog.Name = "btnSaveCatalog";
            btnSaveCatalog.Size = new Size(107, 23);
            btnSaveCatalog.TabIndex = 4;
            btnSaveCatalog.Text = "저장";
            btnSaveCatalog.UseVisualStyleBackColor = false;
            btnSaveCatalog.Click += btnSaveCatalog_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(991, 791);
            Controls.Add(panel6);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(panel4);
            Name = "Form1";
            Text = "Malcha v0.2";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picVideoScreen).EndInit();
            ((System.ComponentModel.ISupportInitialize)trbTimeline).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            lblStatus.ResumeLayout(false);
            lblStatus.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chtDataGraph).EndInit();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvPilotList).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private TextBox txtFilePath;
        private Button btnSelectData;
        private Button btnChangeCleanData;
        private Label lblTitle;
        private Panel panel2;
        private Button btnPlayPause;
        private Button btnFastForward;
        private Button btnRewind;
        private Button btnNextFrame;
        private Button btnPrevFrame;
        private Label lblRecordCount;
        private Label lblRecordTitle;
        private Label lblModeValue;
        private Label lblThrottleValue;
        private Label lblAngleValue;
        private Label lblModeTitle;
        private Label lblThrottleTitle;
        private Label lblAngleTitle;
        private PictureBox picVideoScreen;
        private TrackBar trbTimeline;
        private Panel panel3;
        private Button btnApplyFilter;
        private Button btnRecover;
        private Button btnDeleteSelection;
        private Button btnSetEndPoint;
        private Button btnSetStartPoint;
        private System.Windows.Forms.DataVisualization.Charting.Chart chtDataGraph;
        private Button btnHelper;
        private Button btnRefresh;
        private StatusStrip lblStatus;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ListBox lstDataList;
        private Panel panel4;
        private Panel panel6;
        private ListBox lstLog;
        private DataGridView dgvPilotList;
        private DataGridViewTextBoxColumn 이름;
        private DataGridViewTextBoxColumn 시간;
        private DataGridViewTextBoxColumn 설명;
        private TextBox txtModelMemo;
        private Button btnconnet;
        private Button btnEnableDelete;
        private Button btnCrossTest;
        private Button btnshutdown;
        private Button btnRunTraining;
        private Label lblloglist;
        private Label lbldeletedlist;
        private ListBox lstDeleted;
        private ListBox lstViewScore;
        private Button btnSaveCatalog;
    }
}
