using System;
using System.Drawing;
using System.Windows.Forms;

namespace Malcha
{
    internal sealed class HelpDialog : Form
    {
        private static readonly Color Bg = Color.FromArgb(33, 28, 29);
        private static readonly Color ContentBg = Color.FromArgb(40, 35, 36);
        private static readonly Color Accent = Color.FromArgb(255, 158, 48);
        private static readonly Color KeyBg = Color.FromArgb(58, 52, 53);
        private static readonly Color KeyBorder = Color.FromArgb(90, 82, 84);
        private static readonly Color TextLight = Color.FromArgb(240, 236, 234);
        private static readonly Color TextMuted = Color.FromArgb(168, 162, 160);

        private static readonly (string Title, (string[] Keys, string Description)[] Rows)[] Sections =
        {
            ("데이터", new[]
            {
                (Array.Empty<string>(), "데이터 선택 — 작업용·백업 .catalog 파일을 직접 엽니다."),
                (Array.Empty<string>(), "새로고침 — 현재 카탈로그를 디스크에서 다시 불러옵니다."),
                (Array.Empty<string>(), "정제·삭제·복구 전에 자동으로 백업이 생성됩니다 (backups/ 폴더)."),
            }),
            ("재생 · 탐색", new[]
            {
                (new[] { "Space" }, "재생 / 일시정지"),
                (new[] { "←", "→" }, "이전 / 다음 프레임"),
                (Array.Empty<string>(), "타임라인 — 드래그하여 재생 위치를 이동합니다."),
                (Array.Empty<string>(), "프레임 목록 — 항목을 클릭하면 해당 프레임으로 이동합니다."),
            }),
            ("구간 선택", new[]
            {
                (new[] { "Ctrl", "드래그" }, "타임라인에서 구간을 드래그해 선택"),
                (new[] { "Ctrl", "클릭" }, "타임라인·목록에서 구간 시작점 설정"),
                (new[] { "Ctrl", "Shift", "클릭" }, "타임라인·목록에서 구간 끝점 설정"),
                (new[] { "[" }, "현재 프레임을 구간 시작으로"),
                (new[] { "]" }, "현재 프레임을 구간 끝으로"),
                (new[] { "Esc" }, "구간 선택 해제"),
                (new[] { "Shift", "클릭" }, "타임라인에서 구간 선택 해제"),
                (Array.Empty<string>(), "선택된 구간은 타임라인·목록에 주황색으로 표시됩니다."),
            }),
            ("편집", new[]
            {
                (Array.Empty<string>(), "선택구간 삭제 — 주황색 구간 제거 (저장·backups/ 백업 포함)"),
                (Array.Empty<string>(), "필터 적용 — 중복·스파이크·범위 초과 프레임을 자동 정제합니다."),
                (Array.Empty<string>(), "복구 — 최신 백업과 정제본을 병합해 빠진 프레임을 되살립니다."),
                (Array.Empty<string>(), "백업이 없으면 직전 편집 상태(Undo)로 되돌립니다."),
                (new[] { "Delete" }, "목록에서 선택한 항목 삭제"),
                (Array.Empty<string>(), "타임라인·목록 우클릭 — 선택 구간 또는 항목 삭제"),
            }),
            ("기타", new[]
            {
                (new[] { "F1" }, "이 도움말 열기"),
                (Array.Empty<string>(), "모델 학습 — 학습 화면으로 이동합니다."),
                (Array.Empty<string>(), "데이터 관리 — 학습 데이터·모델 기록을 관리합니다."),
            }),
        };

        public static void ShowFor(IWin32Window owner)
        {
            using var dlg = new HelpDialog();
            dlg.ShowDialog(owner);
        }

        private HelpDialog()
        {
            Text = "Malcha 사용 안내";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Bg;
            ForeColor = TextLight;
            Font = new Font("맑은 고딕", 9.75F);
            ClientSize = new Size(520, 560);
            Padding = new Padding(0);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Bg,
                Padding = new Padding(20, 14, 20, 0)
            };

            var title = new Label
            {
                Text = "Malcha 사용 안내",
                Font = new Font("맑은 고딕", 14F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(20, 14)
            };

            var subtitle = new Label
            {
                Text = "단축키와 주요 기능을 한눈에 확인하세요",
                Font = new Font("맑은 고딕", 9F),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(22, 38)
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            var scrollHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 8, 16, 8),
                BackColor = Bg
            };

            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ContentBg,
                Padding = new Padding(4)
            };

            var content = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = ContentBg,
                Padding = new Padding(16, 12, 16, 12),
                Width = 452
            };

            foreach (var section in Sections)
                content.Controls.Add(BuildSection(section.Title, section.Rows));

            scroll.Controls.Add(content);
            scrollHost.Controls.Add(scroll);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = Bg,
                Padding = new Padding(0, 8, 20, 12)
            };

            var closeBtn = new Button
            {
                Text = "확인",
                Size = new Size(88, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 108, 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(53, 48, 49),
                ForeColor = TextLight,
                Cursor = Cursors.Hand
            };
            closeBtn.FlatAppearance.BorderColor = KeyBorder;
            closeBtn.Click += (_, _) => Close();

            footer.Controls.Add(closeBtn);

            Controls.Add(scrollHost);
            Controls.Add(footer);
            Controls.Add(header);

            AcceptButton = closeBtn;
            CancelButton = closeBtn;
        }

        private Control BuildSection(string title, (string[] Keys, string Description)[] rows)
        {
            var section = new Panel
            {
                Width = 420,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = ContentBg,
                Margin = new Padding(0, 0, 0, 18)
            };

            var header = new Label
            {
                Text = title,
                Font = new Font("맑은 고딕", 10.5F, FontStyle.Bold),
                ForeColor = Accent,
                AutoSize = true,
                Location = new Point(0, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            section.Controls.Add(header);

            int y = header.Bottom + 6;
            foreach (var row in rows)
            {
                var rowPanel = BuildRow(row.Keys, row.Description);
                rowPanel.Location = new Point(0, y);
                section.Controls.Add(rowPanel);
                y = rowPanel.Bottom + 6;
            }

            section.Height = y;
            return section;
        }

        private Control BuildRow(string[] keys, string description)
        {
            var row = new Panel
            {
                Width = 420,
                Height = 28,
                BackColor = ContentBg
            };

            int x = 0;
            if (keys.Length > 0)
            {
                foreach (var key in keys)
                {
                    if (key is "드래그" or "클릭")
                    {
                        var action = new Label
                        {
                            Text = key,
                            AutoSize = true,
                            ForeColor = TextMuted,
                            Font = new Font("맑은 고딕", 9F),
                            Location = new Point(x, 6)
                        };
                        row.Controls.Add(action);
                        x = action.Right + 4;
                        continue;
                    }

                    var badge = CreateKeyBadge(key);
                    badge.Location = new Point(x, 2);
                    row.Controls.Add(badge);
                    x = badge.Right + 4;
                }

                var plus = new Label
                {
                    Text = "→",
                    AutoSize = true,
                    ForeColor = TextMuted,
                    Font = new Font("맑은 고딕", 9F),
                    Location = new Point(x + 2, 6)
                };
                row.Controls.Add(plus);
                x = plus.Right + 8;
            }

            var desc = new Label
            {
                Text = description,
                AutoSize = false,
                ForeColor = keys.Length > 0 ? TextLight : TextMuted,
                Font = new Font("맑은 고딕", keys.Length > 0 ? 9.75F : 9.25F),
                Location = new Point(keys.Length > 0 ? x : 0, 4),
                Size = new Size(420 - (keys.Length > 0 ? x : 0), 22)
            };
            row.Controls.Add(desc);

            return row;
        }

        private Control CreateKeyBadge(string text)
        {
            var badge = new Label
            {
                Text = text,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.75F, FontStyle.Bold),
                ForeColor = TextLight,
                BackColor = KeyBg,
                Size = new Size(Math.Max(36, TextRenderer.MeasureText(text, Font).Width + 14), 24),
                Margin = new Padding(0),
                Padding = new Padding(4, 0, 4, 0)
            };

            badge.Paint += (_, e) =>
            {
                var g = e.Graphics;
                using var border = new Pen(KeyBorder);
                g.DrawRectangle(border, 0, 0, badge.Width - 1, badge.Height - 1);
            };

            return badge;
        }
    }
}
