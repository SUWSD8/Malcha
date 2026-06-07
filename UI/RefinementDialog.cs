using Malcha.Model;
using System.Drawing;

namespace Malcha.UI
{
    // 필터 적용 — 초보자용 설명·미리보기·강도 프리셋
    internal sealed class RefinementDialog : Form
    {
        private enum PresetKind { Recommended, Light, Strong, Custom }

        private static readonly Color Bg = Color.FromArgb(33, 28, 29);
        private static readonly Color PanelBg = Color.FromArgb(40, 35, 36);
        private static readonly Color Accent = Color.FromArgb(255, 158, 48);
        private static readonly Color TextLight = Color.FromArgb(240, 236, 234);
        private static readonly Color TextMuted = Color.FromArgb(168, 162, 160);
        private static readonly Color PreviewBg = Color.FromArgb(48, 42, 41);

        private readonly IReadOnlyList<Frame> _frames;
        private readonly ComboBox _preset;
        private readonly Panel _advancedPanel;
        private readonly NumericUpDown _epsilon;
        private readonly NumericUpDown _spike;
        private readonly NumericUpDown _stride;
        private readonly CheckBox _userModeOnly;
        private readonly Label _lblAfter;
        private readonly Label _lblDup;
        private readonly Label _lblSpike;
        private readonly Label _lblRange;
        private readonly Label _lblMode;
        private readonly Label _lblHint;

        private RefinementDialog(IReadOnlyList<Frame> frames, FrameRefinementFilter.Options defaults)
        {
            _frames = frames;
            Text = "데이터 정제";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(500, 418);
            BackColor = Bg;
            ForeColor = TextLight;
            Font = new Font("맑은 고딕", 9.75F);

            var intro = new Label
            {
                Text =
                    "학습에 도움이 되지 않는 프레임을 자동으로 골라냅니다.\n" +
                    "· 차량이 멈춰 있는데 같은 조향·속도만 반복되는 장면\n" +
                    "· 순간적으로 튀었다가 바로 돌아오는 센서 노이즈\n" +
                    "· 허용 범위를 벗어난 잘못된 값\n\n" +
                    "직접 삭제한 구간은 그대로 유지됩니다.",
                Location = new Point(16, 12),
                Size = new Size(468, 108),
                ForeColor = TextMuted
            };

            var lblPreset = new Label
            {
                Text = "정제 강도",
                Location = new Point(16, 128),
                AutoSize = true,
                Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold),
                ForeColor = TextLight
            };

            _preset = new ComboBox
            {
                Location = new Point(100, 124),
                Width = 280,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _preset.Items.AddRange(new object[]
            {
                "권장 — 대부분의 주행 데이터에 적합",
                "약함 — 조금만 걸러냄 (데이터 많이 남김)",
                "강함 — 더 많이 걸러냄 (짧은 학습 시간)",
                "직접 설정 — 숫자를 직접 조정"
            });
            _preset.SelectedIndex = 0;

            _userModeOnly = new CheckBox
            {
                Text = "user 모드 기록만 남기기 (자율·원격 주행 제외)",
                Location = new Point(16, 152),
                Size = new Size(460, 22),
                Checked = defaults.UserModeOnly,
                ForeColor = TextMuted
            };
            _userModeOnly.CheckedChanged += (_, _) => RefreshPreview();

            var previewBox = new Panel
            {
                Location = new Point(16, 180),
                Size = new Size(468, 148),
                BackColor = PreviewBg
            };

            var lblPreviewTitle = new Label
            {
                Text = "미리보기 (실제 정제 전 예상)",
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                ForeColor = Accent
            };

            _lblAfter = CreatePreviewLine(10, 34, $"현재  {_frames.Count:N0} 프레임");
            _lblDup = CreatePreviewLine(10, 58, "· 정지·중복  —");
            _lblSpike = CreatePreviewLine(10, 80, "· 순간 노이즈  —");
            _lblRange = CreatePreviewLine(10, 102, "· 센서 오류  —");
            _lblMode = CreatePreviewLine(10, 124, "· user 외 모드  —");

            previewBox.Controls.AddRange(new Control[]
            {
                lblPreviewTitle, _lblAfter, _lblDup, _lblSpike, _lblRange, _lblMode
            });

            (_advancedPanel, _epsilon, _spike, _stride) = BuildAdvancedPanel(defaults);
            _advancedPanel.Location = new Point(16, 314);
            _advancedPanel.Visible = false;

            _lblHint = new Label
            {
                Location = new Point(16, 336),
                Size = new Size(468, 36),
                ForeColor = TextMuted,
                Text = "※ 정제 후 「정제 데이터 연동」을 다시 실행해야 학습에 반영됩니다."
            };

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(12, 8, 12, 8),
                BackColor = Bg
            };
            var run = new Button
            {
                Text = "정제 실행",
                DialogResult = DialogResult.OK,
                Width = 100,
                Height = 32,
                BackColor = Color.FromArgb(198, 100, 114),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            run.FlatAppearance.BorderSize = 0;
            var cancel = new Button { Text = "취소", DialogResult = DialogResult.Cancel, Width = 80, Height = 32 };
            buttons.Controls.Add(run);
            buttons.Controls.Add(cancel);

            Controls.Add(intro);
            Controls.Add(lblPreset);
            Controls.Add(_preset);
            Controls.Add(_userModeOnly);
            Controls.Add(previewBox);
            Controls.Add(_advancedPanel);
            Controls.Add(_lblHint);
            Controls.Add(buttons);

            AcceptButton = run;
            CancelButton = cancel;

            _preset.SelectedIndexChanged += (_, _) => OnPresetChanged();
            _epsilon.ValueChanged += (_, _) => OnCustomOptionChanged();
            _spike.ValueChanged += (_, _) => OnCustomOptionChanged();
            _stride.ValueChanged += (_, _) => OnCustomOptionChanged();

            OnPresetChanged();
        }

        public static bool TryShow(
            IWin32Window owner,
            IReadOnlyList<Frame> frames,
            FrameRefinementFilter.Options defaults,
            out FrameRefinementFilter.Options result)
        {
            using var dlg = new RefinementDialog(frames, defaults);
            if (dlg.ShowDialog(owner) != DialogResult.OK)
            {
                result = defaults;
                return false;
            }

            result = dlg.BuildOptions();
            return true;
        }

        private static (Panel panel, NumericUpDown epsilon, NumericUpDown spike, NumericUpDown stride)
            BuildAdvancedPanel(FrameRefinementFilter.Options defaults)
        {
            var panel = new Panel { Size = new Size(468, 118), BackColor = PanelBg };

            var epsilon = AddDecimal(panel, 10, 8, "중복 민감도", (decimal)defaults.ValueEpsilon, 0.001m, 0.2m, 0.001m, 3);
            var spike = AddDecimal(panel, 240, 8, "노이즈 민감도", (decimal)defaults.SpikeThreshold, 0.05m, 1.5m, 0.05m, 2);
            var stride = AddInt(panel, 10, 44, "최소 간격(프레임)", defaults.MinKeepStride, 0, 30);

            var advHint = new Label
            {
                Text = "숫자가 클수록 더 많이 남깁니다 (중복·노이즈 기준).",
                Location = new Point(10, 82),
                AutoSize = true,
                ForeColor = TextMuted,
                Font = new Font("맑은 고딕", 8.5F)
            };
            panel.Controls.Add(advHint);
            return (panel, epsilon, spike, stride);
        }

        private Label CreatePreviewLine(int x, int y, string text) => new()
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(448, 20),
            ForeColor = TextLight
        };

        private void OnPresetChanged()
        {
            bool custom = _preset.SelectedIndex == (int)PresetKind.Custom;
            _advancedPanel.Visible = custom;
            _advancedPanel.Location = new Point(16, 334);
            _lblHint.Location = new Point(16, custom ? 458 : 334);
            ClientSize = new Size(500, custom ? 540 : 418);

            if (!custom)
                ApplyPreset((PresetKind)_preset.SelectedIndex);

            RefreshPreview();
        }

        private void OnCustomOptionChanged()
        {
            if (_preset.SelectedIndex == (int)PresetKind.Custom)
                RefreshPreview();
        }

        private void ApplyPreset(PresetKind kind)
        {
            var o = kind switch
            {
                PresetKind.Light => new FrameRefinementFilter.Options
                {
                    ValueEpsilon = 0.008,
                    SpikeThreshold = 0.55,
                    MinKeepStride = 2,
                    UserModeOnly = _userModeOnly.Checked
                },
                PresetKind.Strong => new FrameRefinementFilter.Options
                {
                    ValueEpsilon = 0.028,
                    SpikeThreshold = 0.32,
                    MinKeepStride = 6,
                    UserModeOnly = _userModeOnly.Checked
                },
                _ => new FrameRefinementFilter.Options { UserModeOnly = _userModeOnly.Checked }
            };

            _epsilon.Value = (decimal)o.ValueEpsilon;
            _spike.Value = (decimal)o.SpikeThreshold;
            _stride.Value = o.MinKeepStride;
        }

        private void RefreshPreview()
        {
            var preview = FrameRefinementFilter.Preview(_frames, BuildOptions());
            int removed = preview.RemovedTotal;
            double pct = preview.OriginalCount > 0 ? 100.0 * removed / preview.OriginalCount : 0;
            int kept = preview.Frames.Count;

            _lblAfter.Text = removed == 0
                ? $"현재  {preview.OriginalCount:N0} 프레임  →  그대로 {kept:N0} (제거 없음)"
                : $"현재  {preview.OriginalCount:N0}  →  {kept:N0} 프레임  (−{removed:N0}, {pct:F1}%)";

            _lblDup.Text = FormatRemovalLine(
                "정지·중복",
                preview.RemovedDuplicate,
                "멈춰 있거나 조향·속도가 거의 같은 연속 장면");
            _lblSpike.Text = FormatRemovalLine(
                "순간 노이즈",
                preview.RemovedSpike,
                "한 프레임만 급격히 튀었다 되돌아온 경우");
            _lblRange.Text = FormatRemovalLine(
                "센서 오류",
                preview.RemovedOutOfRange,
                "조향·속도 값이 비정상 범위");
            _lblMode.Text = FormatRemovalLine(
                "user 외 모드",
                preview.RemovedWrongMode,
                "자율·원격 등 user가 아닌 모드");
            _lblMode.Visible = preview.RemovedWrongMode > 0 || _userModeOnly.Checked;

            if (pct >= 50)
            {
                _lblHint.ForeColor = Accent;
                _lblHint.Text = "※ 50% 이상 제거 예상 — 「약함」 프리셋을 고려해 보세요.";
            }
            else
            {
                _lblHint.ForeColor = TextMuted;
                _lblHint.Text = "※ 정제 후 「정제 데이터 연동」을 다시 실행해야 학습에 반영됩니다.";
            }
        }

        private static string FormatRemovalLine(string title, int count, string plainExplain) =>
            count > 0
                ? $"· {title}  −{count:N0}   ({plainExplain})"
                : $"· {title}  없음";

        private FrameRefinementFilter.Options BuildOptions() => new()
        {
            ValueEpsilon = (double)_epsilon.Value,
            SpikeThreshold = (double)_spike.Value,
            OutOfRangeLimit = 1.25,
            MinKeepStride = (int)_stride.Value,
            UserModeOnly = _userModeOnly.Checked
        };

        private static NumericUpDown AddDecimal(
            Panel panel, int x, int y, string label,
            decimal value, decimal min, decimal max, decimal increment, int decimals)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = TextLight,
                Font = new Font("맑은 고딕", 8.5F)
            });
            var num = new NumericUpDown
            {
                Location = new Point(x, y + 18),
                Width = 90,
                Minimum = min,
                Maximum = max,
                Increment = increment,
                DecimalPlaces = decimals,
                Value = Math.Clamp(value, min, max)
            };
            panel.Controls.Add(num);
            return num;
        }

        private static NumericUpDown AddInt(Panel panel, int x, int y, string label, int value, int min, int max)
        {
            panel.Controls.Add(new Label
            {
                Text = label,
                Location = new Point(x, y),
                AutoSize = true,
                ForeColor = TextLight,
                Font = new Font("맑은 고딕", 8.5F)
            });
            var num = new NumericUpDown
            {
                Location = new Point(x, y + 18),
                Width = 90,
                Minimum = min,
                Maximum = max,
                Value = Math.Clamp(value, min, max)
            };
            panel.Controls.Add(num);
            return num;
        }
    }
}
