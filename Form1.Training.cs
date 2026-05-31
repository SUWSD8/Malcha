using Malcha.Controller;
using Malcha.Model;
using Malcha.View;
using Malcha.UI;

namespace Malcha
{
    public partial class Form1 : ITrainingView
    {
        private TrainingController? _trainingController;
        private const string DefaultModelName = "mypilot";
        private List<TrainingResult> _gridModels = new();
        private ToolTip? _scoreToolTip;

        private void InitializeTrainingPanel()
        {
            _trainingController = new TrainingController(this);
            _scoreToolTip = new ToolTip();
            dgvPilotList.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPilotList.MultiSelect = false;
            btnRunTraining.Click += (_, _) => RunTrainingRequested?.Invoke(this, EventArgs.Empty);
            btnconnet.Click += (_, _) => UpdateCommentRequested?.Invoke(this, EventArgs.Empty);
            btnEnableDelete.Click += (_, _) => DeleteModelRequested?.Invoke(this, EventArgs.Empty);
            btnCrossTest.Click += (_, _) => ShowChartRequested?.Invoke(this, EventArgs.Empty);
            dgvPilotList.SelectionChanged += (_, _) => ModelSelectionChanged?.Invoke(this, EventArgs.Empty);
            dgvPilotList.CellToolTipTextNeeded += OnPilotListCellToolTipNeeded;
            Load += (_, _) => ViewLoaded?.Invoke(this, EventArgs.Empty);
        }

        TrainingResult? ITrainingView.SelectedModel => GetSelectedModel();

        string ITrainingView.SelectedModelName =>
            GetSelectedModel()?.Name.Trim() ?? DefaultModelName;

        int ITrainingView.SelectedModelNumber =>
            GetSelectedModel()?.Number ?? -1;

        private TrainingResult? GetSelectedModel()
        {
            if (dgvPilotList.SelectedRows.Count > 0)
            {
                var tag = dgvPilotList.SelectedRows[0].Tag as TrainingResult;
                if (tag != null) return tag;
            }
            if (dgvPilotList.CurrentRow?.Tag is TrainingResult current)
                return current;
            return null;
        }

        string ITrainingView.ModelComment => txtModelMemo.Text;
        IWin32Window ITrainingView.Owner => this;

        public event EventHandler? ViewLoaded;
        public event EventHandler? RunTrainingRequested;
        public event EventHandler? UpdateCommentRequested;
        public event EventHandler? DeleteModelRequested;
        public event EventHandler? ShowChartRequested;
        public event EventHandler? ModelSelectionChanged;

        void ITrainingView.SetTrainingButtonEnabled(bool enabled) => btnRunTraining.Enabled = enabled;
        void ITrainingView.SetTrainingButtonText(string text) => btnRunTraining.Text = text;

        void ITrainingView.ClearLog()
        {
            if (InvokeRequired) { BeginInvoke(() => ((ITrainingView)this).ClearLog()); return; }
            lstLog.Items.Clear();
        }

        void ITrainingView.BindModelList(IReadOnlyList<TrainingResult> models, int? selectModelNumber = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => ((ITrainingView)this).BindModelList(models, selectModelNumber));
                return;
            }

            _gridModels = models.OrderByDescending(m => m.Time).ThenByDescending(m => m.Number).ToList();
            dgvPilotList.Rows.Clear();
            foreach (var m in _gridModels)
            {
                int rowIdx = dgvPilotList.Rows.Add(m.Name, TrainingTimeFormat.FormatShort(m.Time), m.Comment);
                dgvPilotList.Rows[rowIdx].Tag = m;
            }

            if (!selectModelNumber.HasValue && _gridModels.Count > 0)
                selectModelNumber = _gridModels[0].Number;

            if (!selectModelNumber.HasValue) return;

            foreach (DataGridViewRow row in dgvPilotList.Rows)
            {
                if (row.Tag is TrainingResult m && m.Number == selectModelNumber.Value)
                {
                    row.Selected = true;
                    dgvPilotList.CurrentCell = row.Cells["이름"];
                    break;
                }
            }
        }

        void ITrainingView.AppendLog(string message)
        {
            if (InvokeRequired) { BeginInvoke(() => ((ITrainingView)this).AppendLog(message)); return; }
            lstLog.Items.Add(message);
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        void ITrainingView.BindEpochScores(string modelName, IReadOnlyList<TrainingEpoch> epochs, int? plannedTotal = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => ((ITrainingView)this).BindEpochScores(modelName, epochs, plannedTotal));
                return;
            }

            lstViewScore.BeginUpdate();
            lstViewScore.Items.Clear();
            foreach (var line in TrainingEpochDisplay.ToCompactLines(modelName, epochs, plannedTotal))
                lstViewScore.Items.Add(line);
            lstViewScore.EndUpdate();

            _scoreToolTip?.SetToolTip(lstViewScore,
                TrainingEpochDisplay.ToDetailText(modelName, epochs, plannedTotal));
        }

        void ITrainingView.ClearEpochScores(string modelName) =>
            ((ITrainingView)this).BindEpochScores(modelName, Array.Empty<TrainingEpoch>());

        void ITrainingView.ShowInfo(string title, string message) =>
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        void ITrainingView.ShowError(string message) =>
            MessageBox.Show(this, message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);

        bool ITrainingView.ConfirmDeleteModel(TrainingResult model, string timeLabel) =>
            MessageBox.Show(this,
                $"#{model.Number}  {model.Name}\n{timeLabel}\n\n· database.json 기록 1건 삭제\n· 같은 이름 기록이 없으면 .h5도 삭제",
                "모델 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes;

        string? ITrainingView.PromptMycarFolder(string? suggestedUncPath) =>
            MycarFolderDialog.Show(this, suggestedUncPath);

        private void OnPilotListCellToolTipNeeded(object? sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex < 0 || dgvPilotList.Rows[e.RowIndex].Tag is not TrainingResult model) return;

            var col = dgvPilotList.Columns[e.ColumnIndex].Name;

            if (col == "시간")
            {
                e.ToolTipText = TrainingTimeFormat.FormatFull(model.Time);
                return;
            }
            if (col == "이름" && model.Epochs.Count > 0)
            {
                double sc = TrainingScore.BestValScore(model.Epochs);
                e.ToolTipText = $"#{model.Number}  {model.Name}\n최고 검증 {sc:F1}점 / 100\n{TrainingTimeFormat.FormatFull(model.Time)}";
            }
        }
    }
}
