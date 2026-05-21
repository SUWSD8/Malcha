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
            SuspendLayout();
            // 
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend8 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series15 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series16 = new System.Windows.Forms.DataVisualization.Charting.Series();
            panel1 = new Panel();
            btnDataManagement = new Button();
            btnTrainModel = new Button();
            txtFilePath = new TextBox();
            btnSelectData = new Button();
            btnLoadConfig = new Button();
            lblTitle = new Label();
            panel2 = new Panel();
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
            lstDataList = new ListBox();
            lblStatus = new StatusStrip();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            btnBrowserAnalyze = new Button();
            btnRefresh = new Button();
            chtDataGraph = new System.Windows.Forms.DataVisualization.Charting.Chart();
            btnApplyFilter = new Button();
            btnRecover = new Button();
            btnDeleteSelection = new Button();
            btnSetEndPoint = new Button();
            btnSetStartPoint = new Button();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picVideoScreen).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trbTimeline).BeginInit();
            panel3.SuspendLayout();
            lblStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chtDataGraph).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            panel1.BackColor = Color.FromArgb(33, 28, 29);
            panel1.Controls.Add(btnDataManagement);
            panel1.Controls.Add(btnTrainModel);
            panel1.Controls.Add(txtFilePath);
            panel1.Controls.Add(btnSelectData);
            panel1.Controls.Add(btnLoadConfig);
            panel1.Controls.Add(lblTitle);
            panel1.Location = new Point(0, -2);
            panel1.Name = "panel1";
            panel1.Size = new Size(612, 83);
            panel1.TabIndex = 0;
            // 
            // btnDataManagement
            // 
            btnDataManagement.Anchor = AnchorStyles.Left;
            btnDataManagement.BackColor = Color.FromArgb(53, 48, 49);
            btnDataManagement.FlatStyle = FlatStyle.Popup;
            btnDataManagement.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnDataManagement.ForeColor = SystemColors.ButtonHighlight;
            btnDataManagement.Location = new Point(242, 15);
            btnDataManagement.Name = "btnDataManagement";
            btnDataManagement.Size = new Size(103, 23);
            btnDataManagement.TabIndex = 5;
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
            btnTrainModel.Location = new Point(351, 14);
            btnTrainModel.Name = "btnTrainModel";
            btnTrainModel.Size = new Size(103, 23);
            btnTrainModel.TabIndex = 4;
            btnTrainModel.Text = "모델 학습";
            btnTrainModel.UseVisualStyleBackColor = false;
            btnTrainModel.Click += btnTrainModel_Click;
            // 
            // txtFilePath
            // 
            txtFilePath.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtFilePath.Location = new Point(230, 47);
            txtFilePath.Name = "txtFilePath";
            txtFilePath.Size = new Size(368, 23);
            txtFilePath.TabIndex = 3;
            // 
            // btnSelectData
            // 
            btnSelectData.Anchor = AnchorStyles.Left;
            btnSelectData.BackColor = Color.FromArgb(53, 48, 49);
            btnSelectData.FlatStyle = FlatStyle.Popup;
            btnSelectData.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnSelectData.ForeColor = SystemColors.ButtonHighlight;
            btnSelectData.Location = new Point(121, 46);
            btnSelectData.Name = "btnSelectData";
            btnSelectData.Size = new Size(103, 23);
            btnSelectData.TabIndex = 2;
            btnSelectData.Text = "데이터 선택";
            btnSelectData.UseVisualStyleBackColor = false;
            // 
            // btnLoadConfig
            // 
            btnLoadConfig.Anchor = AnchorStyles.Left;
            btnLoadConfig.BackColor = Color.FromArgb(53, 48, 49);
            btnLoadConfig.FlatStyle = FlatStyle.Popup;
            btnLoadConfig.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnLoadConfig.ForeColor = SystemColors.ButtonHighlight;
            btnLoadConfig.Location = new Point(12, 46);
            btnLoadConfig.Name = "btnLoadConfig";
            btnLoadConfig.Size = new Size(103, 23);
            btnLoadConfig.TabIndex = 1;
            btnLoadConfig.Text = "설정 불러오기";
            btnLoadConfig.UseVisualStyleBackColor = false;
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
            panel2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel2.BackColor = Color.FromArgb(20, 20, 20);
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
            panel2.Location = new Point(0, 81);
            panel2.Name = "panel2";
            panel2.Size = new Size(612, 278);
            panel2.TabIndex = 1;
            // 
            // picVideoScreen
            // 
            picVideoScreen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            picVideoScreen.BackColor = Color.Black;
            picVideoScreen.Location = new Point(116, 6);
            picVideoScreen.Name = "picVideoScreen";
            picVideoScreen.Size = new Size(342, 219);
            picVideoScreen.TabIndex = 2;
            picVideoScreen.TabStop = false;
            // 
            // trbTimeline
            // 
            trbTimeline.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trbTimeline.Location = new Point(0, 231);
            trbTimeline.Name = "trbTimeline";
            trbTimeline.Size = new Size(609, 45);
            trbTimeline.TabIndex = 2;
            // 
            // btnPlayPause
            // 
            btnPlayPause.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPlayPause.BackColor = Color.FromArgb(198, 100, 114);
            btnPlayPause.FlatStyle = FlatStyle.Popup;
            btnPlayPause.Font = new Font("맑은 고딕", 12F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnPlayPause.ForeColor = Color.White;
            btnPlayPause.Location = new Point(464, 186);
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
            btnFastForward.Location = new Point(534, 130);
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
            btnRewind.Location = new Point(464, 130);
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
            btnNextFrame.Location = new Point(534, 74);
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
            btnPrevFrame.Location = new Point(464, 74);
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
            lblRecordCount.Location = new Point(464, 34);
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
            lblRecordTitle.Location = new Point(464, 14);
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
            panel3.Controls.Add(lstDataList);
            panel3.Controls.Add(lblStatus);
            panel3.Controls.Add(btnBrowserAnalyze);
            panel3.Controls.Add(btnRefresh);
            panel3.Controls.Add(chtDataGraph);
            panel3.Controls.Add(btnApplyFilter);
            panel3.Controls.Add(btnRecover);
            panel3.Controls.Add(btnDeleteSelection);
            panel3.Controls.Add(btnSetEndPoint);
            panel3.Controls.Add(btnSetStartPoint);
            panel3.Location = new Point(0, 359);
            panel3.Name = "panel3";
            panel3.Size = new Size(609, 264);
            panel3.TabIndex = 2;
            // 
            // lstDataList
            // 
            lstDataList.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lstDataList.FormattingEnabled = true;
            lstDataList.Location = new Point(10, 55);
            lstDataList.Name = "lstDataList";
            lstDataList.Size = new Size(120, 169);
            lstDataList.TabIndex = 3;
            // 
            // lblStatus
            // 
            lblStatus.BackColor = Color.Black;
            lblStatus.Font = new Font("맑은 고딕", 9F, FontStyle.Bold, GraphicsUnit.Point, 129);
            lblStatus.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
            lblStatus.Location = new Point(0, 242);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(609, 22);
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
            // btnBrowserAnalyze
            // 
            btnBrowserAnalyze.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowserAnalyze.BackColor = Color.FromArgb(53, 48, 49);
            btnBrowserAnalyze.FlatStyle = FlatStyle.Popup;
            btnBrowserAnalyze.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnBrowserAnalyze.ForeColor = SystemColors.ButtonHighlight;
            btnBrowserAnalyze.Location = new Point(507, 117);
            btnBrowserAnalyze.Name = "btnBrowserAnalyze";
            btnBrowserAnalyze.Size = new Size(91, 51);
            btnBrowserAnalyze.TabIndex = 11;
            btnBrowserAnalyze.Text = " 브라우저    분석";
            btnBrowserAnalyze.UseVisualStyleBackColor = false;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.FromArgb(53, 48, 49);
            btnRefresh.FlatStyle = FlatStyle.Popup;
            btnRefresh.Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 129);
            btnRefresh.ForeColor = SystemColors.ButtonHighlight;
            btnRefresh.Location = new Point(507, 60);
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
            chartArea8.Name = "ChartArea1";
            chtDataGraph.ChartAreas.Add(chartArea8);
            legend8.Name = "Legend1";
            chtDataGraph.Legends.Add(legend8);
            chtDataGraph.Location = new Point(142, 46);
            chtDataGraph.Name = "chtDataGraph";
            series15.ChartArea = "ChartArea1";
            series15.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series15.Legend = "Legend1";
            series15.Name = "user/angle";
            series16.ChartArea = "ChartArea1";
            series16.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series16.Color = Color.Red;
            series16.Legend = "Legend1";
            series16.Name = "user/throttle";
            chtDataGraph.Series.Add(series15);
            chtDataGraph.Series.Add(series16);
            chtDataGraph.Size = new Size(353, 178);
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
            btnApplyFilter.Location = new Point(483, 11);
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
            btnRecover.Location = new Point(374, 11);
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
            btnDeleteSelection.Location = new Point(265, 11);
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
            btnSetEndPoint.Location = new Point(156, 11);
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
            btnSetStartPoint.Location = new Point(47, 11);
            btnSetStartPoint.Name = "btnSetStartPoint";
            btnSetStartPoint.Size = new Size(103, 23);
            btnSetStartPoint.TabIndex = 4;
            btnSetStartPoint.Text = "시작점 설정";
            btnSetStartPoint.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(610, 623);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Name = "Form1";
            Text = "Form1";
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
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private TextBox txtFilePath;
        private Button btnSelectData;
        private Button btnLoadConfig;
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
        private Button btnBrowserAnalyze;
        private Button btnRefresh;
        private StatusStrip lblStatus;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ListBox lstDataList;
        private Button btnTrainModel;
        private Button btnDataManagement;
    }
}
