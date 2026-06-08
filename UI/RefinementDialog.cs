using Malcha.Model;
using System.Drawing;

namespace Malcha.UI
{
    internal sealed class RefinementDialog : Form
    {
        private enum PresetKind { TrainingFocus, Smart, Light, Strong, Custom }

        private static readonly Color Bg = Color.FromArgb(33, 28, 29);
        private static readonly Color PanelBg = Color.FromArgb(40, 35, 36);
        private static readonly Color Accent = Color.FromArgb(255, 158, 48);
        private static readonly Color TextLight = Color.FromArgb(240, 236, 234);
        private static readonly Color TextMuted = Color.FromArgb(168, 162, 160);
        private static readonly Color PreviewBg = Color.FromArgb(48, 42, 41);

        private readonly IReadOnlyList<Frame> _frames;
        private readonly IReadOnlyList<string> _imagePaths;
        private readonly ComboBox _preset;
        private readonly Panel _advancedPanel;
        private readonly ComboBox _dupMode;
        private readonly NumericUpDown _imgSim;
        private readonly NumericUpDown _epsilon;
        private readonly NumericUpDown _spike;
        private readonly NumericUpDown _stride;
        private readonly CheckBox _userModeOnly;
        private readonly Label _lblAfter;
        private readonly Label _lblStationary;
        private readonly Label _lblVisual;
        private readonly Label _lblSpike;
        private readonly Label _lblRange;
        private readonly Label _lblMode;
        private readonly Label _lblHint;

        private RefinementDialog(
            IReadOnlyList<Frame> frames,
            IReadOnlyList<string> imagePaths,
            FrameRefinementFilter.Options defaults)
        {
            _frames = frames;
            _imagePaths = imagePaths;
            Text = "데이터 정제";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(520, 468);
            BackColor = Bg;
            ForeColor = TextLight;
            Font = new Font("맑은 고딕", 9.75F);

            var intro = new Label
            {
                Text =
                    "스마트 정제 — 주행 중 프레임은 유지, 정지·저속 구간만 화면 비교로 축소.\n" +
                    "· throttle>0.12 주행: 화면 비교 제거 안 함 (교차 테스트 보호)\n" +
                    "· 정지·저속에서 화면+라벨이 같을 때만 제거\n\n" +
                    "목표: 10~25% 축소. 미리보기 30% 초과면 「약함」을 사용하세요.",
                Location = new Point(16, 12),
                Size = new Size(488, 92),
                ForeColor = TextMuted
            };

            _preset = new ComboBox
            {
                Location = new Point(100, 108),
                Width = 388,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _preset.Items.AddRange(new object[]
            {
                "교차테스트 — 노이즈·오류만 (중복 유지)",
                "스마트 권장 — 정지·저속만 화면 비교 (주행 유지)",
                "약함 — 보수적 (적게 제거)",
                "강함 — 적극적 (더 많이 제거)",
                "직접 설정"
            });
            _preset.SelectedIndex = 1;

            _userModeOnly = new CheckBox
            {
                Text = "user 모드 기록만 남기기 (자율·원격 주행 제외)",
                Location = new Point(16, 136),
                Size = new Size(488, 22),
                Checked = defaults.UserModeOnly,
                ForeColor = TextMuted
            };
            _userModeOnly.CheckedChanged += (_, _) => RefreshPreview();

            var previewBox = new Panel
            {
                Location = new Point(16, 164),
                Size = new Size(488, 168),
                BackColor = PreviewBg
            };

            previewBox.Controls.Add(new Label
            {
                Text = "미리보기 (이미지 비교 포함)",
                Location = new Point(10, 8),
                AutoSize = true,
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                ForeColor = Accent
            });

            _lblAfter = CreatePreviewLine(10, 30, $"현재  {_frames.Count:N0} 프레임");
            _lblStationary = CreatePreviewLine(10, 52, "· 정지·중복  —");
            _lblVisual = CreatePreviewLine(10, 74, "· 화면·라벨 중복  —");
            _lblSpike = CreatePreviewLine(10, 96, "· 정지 노이즈  —");
            _lblRange = CreatePreviewLine(10, 118, "· 센서 오류  —");
            _lblMode = CreatePreviewLine(10, 140, "· user 외 모드  —");
            previewBox.Controls.AddRange(new Control[]
            {
                _lblAfter, _lblStationary, _lblVisual, _lblSpike, _lblRange, _lblMode
            });

            (_advancedPanel, _dupMode, _imgSim, _epsilon, _spike, _stride) = BuildAdvancedPanel(defaults);
            _advancedPanel.Location = new Point(16, 384);
            _advancedPanel.Visible = false;

            _lblHint = new Label
            {
                Location = new Point(16, 386),
                Size = new Size(488, 44),
                ForeColor = TextMuted,
                Text = "※ 정제 후 「정제 데이터 연동」 → 「학습 시작」 → 「교차 테스트」"
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
                Width = 100,
                Height = 32,
                BackColor = Color.FromArgb(198, 100, 114),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            run.FlatAppearance.BorderSize = 0;
            run.Click += (_, _) =>
            {
                if (!ConfirmHeavyRemovalIfNeeded()) return;
                DialogResult = DialogResult.OK;
                Close();
            };
            buttons.Controls.Add(run);
            buttons.Controls.Add(new Button { Text = "취소", DialogResult = DialogResult.Cancel, Width = 80, Height = 32 });

            Controls.Add(new Label
            {
                Text = "정제 강도",
                Location = new Point(16, 112),
                AutoSize = true,
                Font = new Font("맑은 고딕", 9.75F, FontStyle.Bold),
                ForeColor = TextLight
            });
            Controls.Add(intro);
            Controls.Add(_preset);
            Controls.Add(_userModeOnly);
            Controls.Add(previewBox);
            Controls.Add(_advancedPanel);
            Controls.Add(_lblHint);
            Controls.Add(buttons);

            AcceptButton = run;

            _preset.SelectedIndexChanged += (_, _) => OnPresetChanged();
            _dupMode.SelectedIndexChanged += (_, _) => OnCustomOptionChanged();
            _imgSim.ValueChanged += (_, _) => OnCustomOptionChanged();
            _epsilon.ValueChanged += (_, _) => OnCustomOptionChanged();
            _spike.ValueChanged += (_, _) => OnCustomOptionChanged();
            _stride.ValueChanged += (_, _) => OnCustomOptionChanged();

            OnPresetChanged();
        }

        public static bool TryShow(
            IWin32Window owner,
            IReadOnlyList<Frame> frames,
            IReadOnlyList<string> imagePaths,
            FrameRefinementFilter.Options defaults,
            out FrameRefinementFilter.Options result)
        {
            using var dlg = new RefinementDialog(frames, imagePaths, defaults);
            if (dlg.ShowDialog(owner) != DialogResult.OK)
            {
                result = defaults;
                return false;
            }

            result = dlg.BuildOptions();
            return true;
        }

        private static (Panel, ComboBox, NumericUpDown, NumericUpDown, NumericUpDown, NumericUpDown)
            BuildAdvancedPanel(FrameRefinementFilter.Options defaults)
        {
            var panel = new Panel { Size = new Size(488, 196), BackColor = PanelBg };

            panel.Controls.Add(new Label
            {
                Text = "중복 제거 범위",
                Location = new Point(10, 8),
                AutoSize = true,
                ForeColor = TextLight,
                Font = new Font("맑은 고딕", 8.5F)
            });
            var dupMode = new ComboBox
            {
                Location = new Point(10, 26),
                Width = 460,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            dupMode.Items.AddRange(new object[]
            {
                "없음",
                "정지 구간만",
                "스마트 — 화면+라벨 (권장)",
                "라벨 같으면 전부 (비권장)"
            });
            dupMode.SelectedIndex = DupModeToIndex(defaults.DuplicateRemoval);

            var imgSim = AddDecimal(panel, 10, 58, "화면 유사도 기준 (이상=제거)", (decimal)defaults.ImageSimilarityThreshold, 0.70m, 0.98m, 0.01m, 2);
            var epsilon = AddDecimal(panel, 250, 58, "라벨 유사 오차", (decimal)defaults.ValueEpsilon, 0.001m, 0.2m, 0.001m, 3);
            var spike = AddDecimal(panel, 10, 102, "노이즈 민감도", (decimal)defaults.SpikeThreshold, 0.05m, 1.5m, 0.05m, 2);
            var stride = AddInt(panel, 250, 102, "최소 간격(프레임)", defaults.MinKeepStride, 0, 30);

            panel.Controls.Add(new Label
            {
                Text = "화면 유사도 0.88 = 직선에서 15~35% 축소 목표. 높일수록 덜 제거.",
                Location = new Point(10, 156),
                Size = new Size(468, 32),
                ForeColor = TextMuted,
                Font = new Font("맑은 고딕", 8.5F)
            });
            panel.Controls.Add(dupMode);
            return (panel, dupMode, imgSim, epsilon, spike, stride);
        }

        private static int DupModeToIndex(FrameRefinementFilter.DuplicateRemovalMode mode) => mode switch
        {
            FrameRefinementFilter.DuplicateRemovalMode.None => 0,
            FrameRefinementFilter.DuplicateRemovalMode.StationaryOnly => 1,
            FrameRefinementFilter.DuplicateRemovalMode.AllSimilarLabels => 3,
            _ => 2
        };

        private static FrameRefinementFilter.DuplicateRemovalMode IndexToDupMode(int index) => index switch
        {
            0 => FrameRefinementFilter.DuplicateRemovalMode.None,
            1 => FrameRefinementFilter.DuplicateRemovalMode.StationaryOnly,
            3 => FrameRefinementFilter.DuplicateRemovalMode.AllSimilarLabels,
            _ => FrameRefinementFilter.DuplicateRemovalMode.SmartImage
        };

        private Label CreatePreviewLine(int x, int y, string text) => new()
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(468, 20),
            ForeColor = TextLight
        };

        private void OnPresetChanged()
        {
            bool custom = _preset.SelectedIndex == (int)PresetKind.Custom;
            _advancedPanel.Visible = custom;
            _lblHint.Location = new Point(16, custom ? 590 : 386);
            ClientSize = new Size(520, custom ? 672 : 468);

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
                PresetKind.TrainingFocus => new FrameRefinementFilter.Options
                {
                    DuplicateRemoval = FrameRefinementFilter.DuplicateRemovalMode.None,
                    SpikeThreshold = 0.55,
                    UserModeOnly = _userModeOnly.Checked
                },
                PresetKind.Smart => new FrameRefinementFilter.Options
                {
                    DuplicateRemoval = FrameRefinementFilter.DuplicateRemovalMode.SmartImage,
                    ImageSimilarityThreshold = 0.94,
                    LowSpeedVisualThreshold = 0.12,
                    ValueEpsilon = 0.015,
                    MinKeepStride = 5,
                    SpikeThreshold = 0.55,
                    UserModeOnly = _userModeOnly.Checked
                },
                PresetKind.Light => new FrameRefinementFilter.Options
                {
                    DuplicateRemoval = FrameRefinementFilter.DuplicateRemovalMode.SmartImage,
                    ImageSimilarityThreshold = 0.96,
                    LowSpeedVisualThreshold = 0.15,
                    ValueEpsilon = 0.012,
                    MinKeepStride = 6,
                    SpikeThreshold = 0.55,
                    UserModeOnly = _userModeOnly.Checked
                },
                PresetKind.Strong => new FrameRefinementFilter.Options
                {
                    DuplicateRemoval = FrameRefinementFilter.DuplicateRemovalMode.SmartImage,
                    ImageSimilarityThreshold = 0.90,
                    LowSpeedVisualThreshold = 0.08,
                    ValueEpsilon = 0.018,
                    MinKeepStride = 3,
                    SpikeThreshold = 0.5,
                    UserModeOnly = _userModeOnly.Checked
                },
                _ => new FrameRefinementFilter.Options { UserModeOnly = _userModeOnly.Checked }
            };

            _dupMode.SelectedIndex = DupModeToIndex(o.DuplicateRemoval);
            _imgSim.Value = (decimal)o.ImageSimilarityThreshold;
            _epsilon.Value = (decimal)o.ValueEpsilon;
            _spike.Value = (decimal)o.SpikeThreshold;
            _stride.Value = o.MinKeepStride;
        }

        private void RefreshPreview()
        {
            UseWaitCursor = true;
            try
            {
                var options = BuildOptions();
                var preview = FrameRefinementFilter.Preview(_frames, options, _imagePaths);
                int removed = preview.RemovedTotal;
                double pct = preview.OriginalCount > 0 ? 100.0 * removed / preview.OriginalCount : 0;
                int kept = preview.Frames.Count;

                _lblAfter.Text = removed == 0
                    ? $"현재  {preview.OriginalCount:N0}  →  그대로 {kept:N0}"
                    : $"현재  {preview.OriginalCount:N0}  →  {kept:N0}  (−{removed:N0}, {pct:F1}%)";

                _lblStationary.Text = FormatRemovalLine(
                    "정지·중복", preview.RemovedStationaryDuplicate, "throttle≈0, 같은 라벨");
                _lblVisual.Text = options.DuplicateRemoval == FrameRefinementFilter.DuplicateRemovalMode.SmartImage
                    ? FormatRemovalLine("저속·화면 중복", preview.RemovedVisualDuplicate,
                        "throttle≤0.12, 화면·라벨 유사")
                    : options.DuplicateRemoval == FrameRefinementFilter.DuplicateRemovalMode.AllSimilarLabels
                        ? FormatRemovalLine("라벨 중복(전체)", preview.RemovedVisualDuplicate + preview.RemovedStationaryDuplicate,
                            "주행 프레임 포함 — 위험")
                        : "· 화면·라벨 중복  (스마트 모드 아님)";
                _lblSpike.Text = FormatRemovalLine("정지 노이즈", preview.RemovedSpike, "정지 상태 센서 글리치");
                _lblRange.Text = FormatRemovalLine("센서 오류", preview.RemovedOutOfRange, "비정상 조향·속도");
                _lblMode.Text = FormatRemovalLine("user 외 모드", preview.RemovedWrongMode, "user가 아닌 모드");
                _lblMode.Visible = preview.RemovedWrongMode > 0 || _userModeOnly.Checked;

                if (options.DuplicateRemoval == FrameRefinementFilter.DuplicateRemovalMode.AllSimilarLabels)
                {
                    _lblHint.ForeColor = Accent;
                    _lblHint.Text = "※ 라벨 기준 전부 제거는 교차 테스트를 망가뜨릴 수 있습니다.";
                }
                else if (pct >= 30 && options.DuplicateRemoval == FrameRefinementFilter.DuplicateRemovalMode.SmartImage)
                {
                    _lblHint.ForeColor = Accent;
                    _lblHint.Text =
                        $"※ {pct:F0}% 제거 — 주행 프레임까지 지워졌을 수 있습니다.\n" +
                        "「약함」 또는 「교차테스트」 프리셋을 권장합니다.";
                }
                else if (pct >= 45)
                {
                    _lblHint.ForeColor = Accent;
                    _lblHint.Text = $"※ {pct:F0}% 제거 — 「약함」으로 낮추거나 화면 유사도 기준을 올려 보세요.";
                }
                else if (pct < 5 && options.DuplicateRemoval != FrameRefinementFilter.DuplicateRemovalMode.None)
                {
                    _lblHint.ForeColor = TextMuted;
                    _lblHint.Text = "※ 거의 제거 없음 — 「강함」 또는 화면 유사도 기준을 낮춰 보세요.";
                }
                else
                {
                    _lblHint.ForeColor = TextMuted;
                    _lblHint.Text = "※ 연동 → 학습 → 교차 테스트로 예측 화살표를 확인하세요.";
                }
            }
            finally
            {
                UseWaitCursor = false;
            }
        }

        private bool ConfirmHeavyRemovalIfNeeded()
        {
            var options = BuildOptions();
            var preview = FrameRefinementFilter.Preview(_frames, options, _imagePaths);
            double pct = preview.OriginalCount > 0
                ? 100.0 * preview.RemovedTotal / preview.OriginalCount
                : 0;

            if (options.DuplicateRemoval == FrameRefinementFilter.DuplicateRemovalMode.AllSimilarLabels)
            {
                return MessageBox.Show(this,
                    "라벨만 같으면 주행 프레임까지 삭제합니다.\n교차 테스트 예측이 멈출 수 있습니다.\n\n계속할까요?",
                    "정제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
            }

            if (options.DuplicateRemoval == FrameRefinementFilter.DuplicateRemovalMode.SmartImage
                && pct >= 30)
            {
                return MessageBox.Show(this,
                    $"약 {pct:F0}% ({preview.RemovedTotal:N0}개) 제거 예상.\n\n" +
                    "30% 넘으면 주행 데이터까지 줄어 교차 테스트·val 점수가 떨어질 수 있습니다.\n" +
                    "「약함」 프리셋을 권장합니다.\n\n그래도 계속할까요?",
                    "정제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
            }

            if (pct < 45) return true;

            return MessageBox.Show(this,
                $"약 {pct:F0}% ({preview.RemovedTotal:N0}개) 제거 예상.\n\n계속할까요?",
                "정제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        private static string FormatRemovalLine(string title, int count, string explain) =>
            count > 0 ? $"· {title}  −{count:N0}   ({explain})" : $"· {title}  없음";

        private FrameRefinementFilter.Options BuildOptions() => new()
        {
            DuplicateRemoval = IndexToDupMode(_dupMode.SelectedIndex),
            ImageSimilarityThreshold = (double)_imgSim.Value,
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
