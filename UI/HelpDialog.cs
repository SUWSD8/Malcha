using System.Drawing;
using System.Windows.Forms;

namespace Malcha
{
    internal sealed class HelpDialog : Form
    {
        private const int ContentWidth = 580;
        private const int RowWidth = ContentWidth;

        private static readonly Color Bg = Color.FromArgb(33, 28, 29);
        private static readonly Color ContentBg = Color.FromArgb(40, 35, 36);
        private static readonly Color Accent = Color.FromArgb(255, 158, 48);
        private static readonly Color KeyBg = Color.FromArgb(58, 52, 53);
        private static readonly Color KeyBorder = Color.FromArgb(90, 82, 84);
        private static readonly Color TextLight = Color.FromArgb(240, 236, 234);
        private static readonly Color TextMuted = Color.FromArgb(168, 162, 160);

        private static readonly (string Title, (string[] Keys, string Description)[] Rows)[] Sections =
        {
            ("학습 준비 (이 순서대로)", new[]
            {
                (Array.Empty<string>(), "① 데이터 선택 — 주행 .catalog 파일을 엽니다."),
                (Array.Empty<string>(), "② (선택) 구간 삭제·수동 편집 — 나쁜 구간을 삭제 목록으로 옮기거나 복구합니다."),
                (Array.Empty<string>(), "③ 필터 적용 — 중복·스파이크·범위 초과 프레임을 자동 정제 (1회면 보통 충분)."),
                (Array.Empty<string>(), "④ 정제 데이터 연동 — 정제된 카탈로그·이미지를 WSL mycar/data 로 복사합니다. ★학습 전 필수"),
                (Array.Empty<string>(), "⑤ 학습 시작 — WSL에서 train.py 실행 (처음이면 mycar 폴더 선택)."),
                (Array.Empty<string>(), "※ 정제를 또 해도 프레임 수가 거의 안 줄면 이미 정제된 상태입니다."),
                (Array.Empty<string>(), "※ 연동 없이 학습하면 WSL에 남아 있는 예전 data로 학습합니다."),
            }),
            ("오른쪽 학습 패널", new[]
            {
                (Array.Empty<string>(), "학습 로그 — epoch 진행 상황 (Loss / Val_Loss)."),
                (Array.Empty<string>(), "점수 영역 — 100점 만점 요약 (검증 val loss 기준). 마우스를 올리면 epoch별 상세."),
                (Array.Empty<string>(), "모델 목록 — 이름·시간·설명. 같은 이름이 여러 개면 시간으로 구분."),
                (Array.Empty<string>(), "학습 시작 — mycar/data 로 학습. 로그에 「data: N 프레임, M 이미지」가 나오면 연동 확인."),
                (Array.Empty<string>(), "학습 강제 종료 — 진행 중인 WSL 학습 중단."),
                (Array.Empty<string>(), "교차 테스트 — 선택 모델로 카탈로그 inference, 주황(기록)·노랑(예측) 화살표 비교"),
                (Array.Empty<string>(), "설명 추가 — 선택한 모델에 메모 저장."),
                (Array.Empty<string>(), "모델 삭제 — 목록에서 행을 클릭해 선택 후 삭제 (database.json 1건 + .h5)."),
            }),
            ("점수 해석", new[]
            {
                (Array.Empty<string>(), "★ 최고 점수 — 학습 중 가장 좋았던 검증(val) 성능. epoch가 지나도 이 값은 유지됩니다."),
                (Array.Empty<string>(), "현재 Ep — 마지막 epoch의 검증·학습 점수. 검증 점수가 내려가면 과적합일 수 있습니다."),
                (Array.Empty<string>(), "공식: 점수 = 100 ÷ (1 + val_loss). loss가 줄수록 점수는 올라갑니다."),
                (Array.Empty<string>(), "같은 data로 학습을 여러 번 해도 점수가 비슷한 것은 정상입니다. data·epoch·수동 편집을 바꿀 때 차이가 납니다."),
            }),
            ("데이터", new[]
            {
                (Array.Empty<string>(), "데이터 선택 — 작업용·백업 .catalog 파일을 직접 엽니다."),
                (Array.Empty<string>(), "새로고침 — ① WSL data 다시 연동 ② WSL data 초기화 ③ 카탈로그 다시 읽기 ④ 데이터 닫기. 「정제 데이터」= WSL mycar/data."),
                (Array.Empty<string>(), "정제·삭제·복구 전에 자동으로 백업이 생성됩니다 (backups/ 폴더)."),
            }),
            ("재생 · 탐색", new[]
            {
                (new[] { "Space" }, "재생 / 일시정지"),
                (new[] { "I", "[" }, "In — 구간 시작 (재생 중 가능)"),
                (new[] { "O", "]" }, "Out — 구간 끝 (재생 중 가능)"),
                (new[] { "X", "Delete" }, "선택 구간 컷 → 삭제 목록 (재생 중: 확인 없이 계속 재생)"),
                (new[] { "0", "NumPad 0" }, "배속 0.5x (슬로우)"),
                (new[] { "1", "NumPad 1" }, "배속 1x (기본)"),
                (new[] { "2", "NumPad 2" }, "배속 1.5x"),
                (new[] { "3", "NumPad 3" }, "배속 2x"),
                (new[] { "4", "NumPad 4" }, "배속 3x"),
                (new[] { "5", "NumPad 5" }, "배속 4x"),
                (new[] { "↑" }, "배속 +0.25x (현재 값에서 증가, 최대 5x)"),
                (new[] { "↓" }, "배속 −0.25x (현재 값에서 감소, 최소 0.25x)"),
                (Array.Empty<string>(), "0~5·NumPad는 프리셋(절대 배속), ↑↓는 미세 조절(더하기). 재생 중에도 즉시 반영됩니다."),
                (Array.Empty<string>(), "배속 표시 — 재생 버튼·상태줄 오른쪽·영상 중앙(변경 시)"),
                (new[] { "←", "→" }, "이전 / 다음 프레임"),
                (Array.Empty<string>(), "타임라인 — 드래그하여 재생 위치를 이동합니다."),
                (Array.Empty<string>(), "프레임 목록 — 항목을 클릭하면 해당 프레임으로 이동합니다."),
            }),
            ("구간 선택", new[]
            {
                (new[] { "드래그" }, "프레임·삭제 목록 — 위↔아래 어느 방향이든 구간 선택 (주황색)"),
                (new[] { "드래그" }, "선택된 구간 위에서 다시 끌기 — 프레임↔삭제 목록 이동·복구"),
                (new[] { "Ctrl", "드래그" }, "타임라인에서 구간을 드래그해 선택"),
                (new[] { "Ctrl", "클릭" }, "타임라인·목록에서 구간 시작점 설정"),
                (new[] { "Ctrl", "Shift", "클릭" }, "타임라인·목록에서 구간 끝점 설정"),
                (new[] { "[" }, "현재 프레임을 구간 시작으로"),
                (new[] { "]" }, "현재 프레임을 구간 끝으로"),
                (new[] { "Esc" }, "구간 선택 해제 (프레임·삭제 목록)"),
                (new[] { "Shift", "클릭" }, "타임라인에서 구간 선택 해제"),
                (Array.Empty<string>(), "리스트 안 드래그 = 구간 선택 · 리스트 밖으로 끌기 = 이동/복구"),
            }),
            ("편집 · 삭제 목록", new[]
            {
                (Array.Empty<string>(), "선택구간 삭제 — 주황색 구간을 삭제 목록으로 이동"),
                (Array.Empty<string>(), "삭제 목록 — 아래 패널에 임시 보관. 위쪽으로 끌면 선택 항목만 복구"),
                (Array.Empty<string>(), "필터 적용 — 중복·스파이크·범위 초과 프레임을 자동 정제합니다."),
                (Array.Empty<string>(), "복구 — backups/ 백업 또는 Undo(Ctrl+Z)로 되돌립니다."),
                (new[] { "Delete" }, "목록에서 선택한 항목을 삭제 목록으로 이동"),
                (new[] { "Ctrl", "Z" }, "직전 편집 Undo (삭제 목록 상태 포함)"),
                (Array.Empty<string>(), "우클릭 — 선택 구간/항목 삭제·복구"),
            }),
            ("기타", new[]
            {
                (new[] { "F1" }, "이 도움말 열기"),
                (Array.Empty<string>(), "mycar 경로 — train.py가 있는 WSL DonkeyCar 폴더 (학습·연동 시 1회 선택, 이후 저장)."),
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
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Bg;
            ForeColor = TextLight;
            Font = new Font("맑은 고딕", 10F);
            MinimumSize = new Size(640, 520);
            ClientSize = new Size(640, 720);
            Padding = new Padding(0);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Bg,
                Padding = new Padding(20, 14, 20, 0)
            };

            var title = new Label
            {
                Text = "Malcha 사용 안내",
                Font = new Font("맑은 고딕", 16F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(20, 12)
            };

            var subtitle = new Label
            {
                Text = "데이터 정제 → WSL 연동 → 학습까지 한눈에",
                Font = new Font("맑은 고딕", 10F),
                ForeColor = TextMuted,
                AutoSize = true,
                Location = new Point(22, 40)
            };

            header.Controls.Add(title);
            header.Controls.Add(subtitle);

            var scrollHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 10, 20, 10),
                BackColor = Bg
            };

            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ContentBg,
                Padding = new Padding(6)
            };

            var content = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = ContentBg,
                Padding = new Padding(18, 14, 18, 14),
                Width = ContentWidth + 36
            };

            foreach (var section in Sections)
                content.Controls.Add(BuildSection(section.Title, section.Rows));

            scroll.Controls.Add(content);
            scrollHost.Controls.Add(scroll);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 56,
                BackColor = Bg,
                Padding = new Padding(0, 8, 20, 12)
            };

            var closeBtn = new Button
            {
                Text = "확인",
                Size = new Size(96, 34),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 116, 10),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(53, 48, 49),
                ForeColor = TextLight,
                Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
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
                Width = RowWidth,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = ContentBg,
                Margin = new Padding(0, 0, 0, 20)
            };

            var header = new Label
            {
                Text = title,
                Font = new Font("맑은 고딕", 11.5F, FontStyle.Bold),
                ForeColor = Accent,
                AutoSize = true,
                Location = new Point(0, 0),
                Margin = new Padding(0, 0, 0, 8)
            };
            section.Controls.Add(header);

            int y = header.Bottom + 8;
            foreach (var row in rows)
            {
                var rowPanel = BuildRow(row.Keys, row.Description);
                rowPanel.Location = new Point(0, y);
                section.Controls.Add(rowPanel);
                y = rowPanel.Bottom + 8;
            }

            section.Height = y;
            return section;
        }

        private Control BuildRow(string[] keys, string description)
        {
            int descX = 0;
            int descWidth = RowWidth;

            if (keys.Length > 0)
            {
                int keyRowHeight = 28;
                var keyRow = new Panel
                {
                    Width = RowWidth,
                    Height = keyRowHeight,
                    BackColor = ContentBg
                };

                int x = 0;
                foreach (var key in keys)
                {
                    if (key is "드래그" or "클릭")
                    {
                        var action = new Label
                        {
                            Text = key,
                            AutoSize = true,
                            ForeColor = TextMuted,
                            Font = new Font("맑은 고딕", 9.5F),
                            Location = new Point(x, 6)
                        };
                        keyRow.Controls.Add(action);
                        x = action.Right + 4;
                        continue;
                    }

                    var badge = CreateKeyBadge(key);
                    badge.Location = new Point(x, 2);
                    keyRow.Controls.Add(badge);
                    x = badge.Right + 4;
                }

                var arrow = new Label
                {
                    Text = "→",
                    AutoSize = true,
                    ForeColor = TextMuted,
                    Font = new Font("맑은 고딕", 9.5F),
                    Location = new Point(x + 2, 6)
                };
                keyRow.Controls.Add(arrow);
                descX = arrow.Right + 10;
                descWidth = RowWidth - descX;

                var row = new Panel
                {
                    Width = RowWidth,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    BackColor = ContentBg
                };
                keyRow.Location = new Point(0, 0);
                row.Controls.Add(keyRow);

                var desc = CreateDescriptionLabel(description, descX, descWidth, TextLight, 10F);
                desc.Location = new Point(descX, 2);
                row.Controls.Add(desc);
                row.Height = Math.Max(keyRowHeight, desc.Bottom + 4);
                return row;
            }

            var textRow = new Panel
            {
                Width = RowWidth,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = ContentBg
            };

            var textDesc = CreateDescriptionLabel(description, 0, RowWidth, TextMuted, 10F);
            textDesc.Location = new Point(0, 0);
            textRow.Controls.Add(textDesc);
            textRow.Height = textDesc.Height + 2;
            return textRow;
        }

        private static Label CreateDescriptionLabel(string text, int x, int width, Color color, float fontSize)
        {
            var label = new Label
            {
                Text = text,
                AutoSize = true,
                ForeColor = color,
                Font = new Font("맑은 고딕", fontSize),
                MaximumSize = new Size(width, 0)
            };
            return label;
        }

        private Control CreateKeyBadge(string text)
        {
            var badge = new Label
            {
                Text = text,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = TextLight,
                BackColor = KeyBg,
                Size = new Size(Math.Max(40, TextRenderer.MeasureText(text, Font).Width + 16), 26),
                Margin = new Padding(0),
                Padding = new Padding(4, 0, 4, 0)
            };

            badge.Paint += (_, e) =>
            {
                using var border = new Pen(KeyBorder);
                e.Graphics.DrawRectangle(border, 0, 0, badge.Width - 1, badge.Height - 1);
            };

            return badge;
        }
    }
}
