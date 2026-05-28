using Malcha.Controller;
using Malcha.Model;
using Malcha.View;

namespace Malcha
{
    // Form1 — [View] panel6 학습 Passive View (ITrainingView 구현)
    public partial class Form1 : ITrainingView
    {
        private TrainingController? _trainingController;
        private const string DefaultModelName = "mypilot";

        // TrainingController 생성 및 버튼·Load 이벤트 연결
        private void InitializeTrainingPanel()
        {
            _trainingController = new TrainingController(this);
            btnRunTraining.Click += (_, _) => RunTrainingRequested?.Invoke(this, EventArgs.Empty);
            btnconnet.Click += (_, _) => UpdateCommentRequested?.Invoke(this, EventArgs.Empty);
            btnEnableDelete.Click += (_, _) => DeleteModelRequested?.Invoke(this, EventArgs.Empty);
            btnCrossTest.Click += (_, _) => ShowChartRequested?.Invoke(this, EventArgs.Empty);
            Load += (_, _) => ViewLoaded?.Invoke(this, EventArgs.Empty);
        }

        string ITrainingView.SelectedModelName =>
            dgvPilotList.CurrentRow?.Cells["이름"].Value?.ToString()?.Trim() ?? DefaultModelName;
        string ITrainingView.ModelComment => txtModelMemo.Text;

        public event EventHandler? ViewLoaded;
        public event EventHandler? RunTrainingRequested;
        public event EventHandler? UpdateCommentRequested;
        public event EventHandler? DeleteModelRequested;
        public event EventHandler? ShowChartRequested;

        void ITrainingView.SetTrainingButtonEnabled(bool enabled) => btnRunTraining.Enabled = enabled;
        void ITrainingView.SetTrainingButtonText(string text) => btnRunTraining.Text = text;

        // dgvPilotList에 모델 목록 바인딩
        void ITrainingView.BindModelList(IReadOnlyList<TrainingResult> models)
        {
            dgvPilotList.AutoGenerateColumns = false;
            dgvPilotList.DataSource = null;
            dgvPilotList.DataSource = models.ToList();
        }

        // lstLog에 학습 로그 추가
        void ITrainingView.AppendLog(string message)
        {
            lstLog.Items.Add(message);
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        // lstViewScore에 Loss 요약 표시
        void ITrainingView.BindScoreSummary(TrainingSummary summary)
        {
            lstViewScore.Items.Clear();
            foreach (var line in summary.ToScoreLines()) lstViewScore.Items.Add(line);
        }

        void ITrainingView.ShowInfo(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        void ITrainingView.ShowError(string message) =>
            MessageBox.Show(this, message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
