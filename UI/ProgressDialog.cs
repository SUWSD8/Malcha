using System;
using System.Drawing;
using System.Windows.Forms;

namespace Malcha
{
    internal sealed class ProgressDialog : Form
    {
        private readonly ProgressBar _progressBar;
        private readonly Label _statusLabel;
        private readonly Button _cancelButton;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public CancellationToken Token => _cts.Token;

        public ProgressDialog(string title = "데이터 정제")
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 110);
            BackColor = Color.FromArgb(33, 28, 29);
            ForeColor = SystemColors.ButtonHighlight;

            _statusLabel = new Label
            {
                AutoSize = false,
                Location = new Point(16, 14),
                Size = new Size(388, 22),
                ForeColor = SystemColors.ButtonHighlight,
                Text = "준비 중…"
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(16, 42),
                Size = new Size(388, 22),
                Style = ProgressBarStyle.Continuous,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            _cancelButton = new Button
            {
                Text = "취소",
                Location = new Point(329, 72),
                Size = new Size(75, 28),
                FlatStyle = FlatStyle.Popup,
                BackColor = Color.FromArgb(53, 48, 49),
                ForeColor = SystemColors.ButtonHighlight
            };
            _cancelButton.Click += (_, _) =>
            {
                _cancelButton.Enabled = false;
                _statusLabel.Text = "취소 중…";
                _cts.Cancel();
            };

            Controls.Add(_statusLabel);
            Controls.Add(_progressBar);
            Controls.Add(_cancelButton);
        }

        public void ShowFor(Form owner)
        {
            Owner = owner;
            StartPosition = FormStartPosition.CenterParent;
            TopMost = true;
            Show(owner);
            Activate();
            BringToFront();
        }

        public void Report(int percent, string message)
        {
            void Apply()
            {
                _progressBar.Value = Math.Clamp(percent, 0, 100);
                if (!string.IsNullOrEmpty(message))
                    _statusLabel.Text = message;
            }

            if (InvokeRequired)
                BeginInvoke(Apply);
            else
                Apply();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_cts.IsCancellationRequested && e.CloseReason == CloseReason.UserClosing)
                _cts.Cancel();
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _cts.Dispose();
            base.Dispose(disposing);
        }
    }
}
