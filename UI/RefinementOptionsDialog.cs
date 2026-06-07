using Malcha;

namespace Malcha.UI
{
    // 필터 적용 전 정제 임계값 설정
    internal sealed class RefinementOptionsDialog : Form
    {
        private readonly NumericUpDown _epsilon;
        private readonly NumericUpDown _spike;
        private readonly NumericUpDown _stride;
        private readonly CheckBox _userModeOnly;

        private RefinementOptionsDialog(FrameRefinementFilter.Options defaults)
        {
            Text = "필터 설정";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(380, 260);
            Font = new Font("맑은 고딕", 9.75F);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 6,
                Padding = new Padding(12)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            _epsilon = AddDecimalRow(layout, 0, "중복 허용 차이 (angle·throttle)",
                (decimal)defaults.ValueEpsilon, 0.001m, 0.2m, 0.001m, 3);
            _spike = AddDecimalRow(layout, 1, "스파이크 임계값",
                (decimal)defaults.SpikeThreshold, 0.05m, 1.5m, 0.05m, 2);
            _stride = AddIntRow(layout, 2, "최소 유지 간격 (프레임)",
                defaults.MinKeepStride, 0, 30);

            _userModeOnly = new CheckBox
            {
                Text = "user 모드 프레임만 유지",
                Checked = defaults.UserModeOnly,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(new Label { Text = "주행 모드", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 3);
            layout.Controls.Add(_userModeOnly, 1, 3);

            var hint = new Label
            {
                Text = "※ 간격( stride )은 중복 제거 시에도 CNN 입력 다양성을 위해\n   N프레임마다 1장은 남깁니다.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Anchor = AnchorStyles.Left
            };
            layout.SetColumnSpan(hint, 2);
            layout.Controls.Add(hint, 0, 4);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(8),
                Height = 44
            };
            var ok = new Button { Text = "적용", DialogResult = DialogResult.OK, Width = 80 };
            var cancel = new Button { Text = "취소", DialogResult = DialogResult.Cancel, Width = 80 };
            buttons.Controls.Add(ok);
            buttons.Controls.Add(cancel);
            AcceptButton = ok;
            CancelButton = cancel;

            Controls.Add(layout);
            Controls.Add(buttons);
        }

        public static bool TryShow(IWin32Window owner, FrameRefinementFilter.Options defaults, out FrameRefinementFilter.Options result)
        {
            using var dlg = new RefinementOptionsDialog(defaults);
            if (dlg.ShowDialog(owner) != DialogResult.OK)
            {
                result = defaults;
                return false;
            }

            result = new FrameRefinementFilter.Options
            {
                ValueEpsilon = (double)dlg._epsilon.Value,
                SpikeThreshold = (double)dlg._spike.Value,
                OutOfRangeLimit = defaults.OutOfRangeLimit,
                MinKeepStride = (int)dlg._stride.Value,
                UserModeOnly = dlg._userModeOnly.Checked
            };
            return true;
        }

        private static NumericUpDown AddDecimalRow(
            TableLayoutPanel layout, int row, string label,
            decimal value, decimal min, decimal max, decimal increment, int decimals)
        {
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            var num = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Increment = increment,
                DecimalPlaces = decimals,
                Value = Math.Clamp(value, min, max),
                Width = 100,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(num, 1, row);
            return num;
        }

        private static NumericUpDown AddIntRow(
            TableLayoutPanel layout, int row, string label, int value, int min, int max)
        {
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            var num = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Value = Math.Clamp(value, min, max),
                Width = 100,
                Anchor = AnchorStyles.Left
            };
            layout.Controls.Add(num, 1, row);
            return num;
        }
    }
}
