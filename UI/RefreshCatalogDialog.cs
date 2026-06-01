using System.Drawing;
using System.Windows.Forms;

namespace Malcha.UI
{
    internal enum RefreshChoice
    {
        ResyncWsl,
        ClearWsl,
        ReloadDisk,
        CloseAll
    }

    internal sealed class RefreshCatalogDialog : Form
    {
        internal sealed class Info
        {
            public required string CatalogSummary { get; init; }
            public required string WslDataSummary { get; init; }
            public bool HasOpenCatalog { get; init; }
            public bool WslConfigured { get; init; }
        }

        private const int ContentWidth = 488;
        private const int OuterPadding = 20;

        private static readonly Color Bg = Color.FromArgb(33, 28, 29);
        private static readonly Color PanelBg = Color.FromArgb(40, 35, 36);
        private static readonly Color CardBg = Color.FromArgb(48, 42, 43);
        private static readonly Color Accent = Color.FromArgb(255, 158, 48);
        private static readonly Color TextLight = Color.FromArgb(240, 236, 234);
        private static readonly Color TextMuted = Color.FromArgb(168, 162, 160);
        private static readonly Color KeyBorder = Color.FromArgb(90, 82, 84);

        private readonly RadioButton _optResync;
        private readonly RadioButton _optClearWsl;
        private readonly RadioButton _optReload;
        private readonly RadioButton _optClose;

        public RefreshChoice? Result { get; private set; }

        public static RefreshChoice? Show(IWin32Window owner, Info info)
        {
            using var dlg = new RefreshCatalogDialog(info);
            return dlg.ShowDialog(owner) == DialogResult.OK ? dlg.Result : null;
        }

        private RefreshCatalogDialog(Info info)
        {
            Text = "새로고침";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Bg;
            ForeColor = TextLight;
            Font = new Font("맑은 고딕", 10F);
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(ContentWidth + OuterPadding * 2, 320);
            MaximumSize = new Size(ContentWidth + OuterPadding * 2 + 8, GetMaxDialogHeight());
            Padding = new Padding(OuterPadding, 16, OuterPadding, 16);

            var root = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Bg,
                Dock = DockStyle.Top,
                Width = ContentWidth
            };
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var header = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Bg,
                Width = ContentWidth,
                Margin = new Padding(0, 0, 0, 10)
            };
            var title = new Label
            {
                Text = "어떻게 새로고침할까요?",
                Font = new Font("맑은 고딕", 12F, FontStyle.Bold),
                ForeColor = TextLight,
                AutoSize = true,
                Location = new Point(0, 0)
            };
            var catalogLine = new Label
            {
                Text = info.CatalogSummary,
                ForeColor = TextMuted,
                AutoSize = true,
                MaximumSize = new Size(ContentWidth, 0),
                Location = new Point(2, title.Bottom + 6)
            };
            var wslLine = new Label
            {
                Text = info.WslDataSummary,
                ForeColor = info.WslConfigured ? TextMuted : Accent,
                AutoSize = true,
                MaximumSize = new Size(ContentWidth, 0),
                Location = new Point(2, catalogLine.Bottom + 4),
                Font = new Font("맑은 고딕", 9.5F)
            };
            header.Controls.Add(title);
            header.Controls.Add(catalogLine);
            header.Controls.Add(wslLine);
            header.Height = wslLine.Bottom + 2;

            var (cardResync, radioResync) = CreateOptionCard(
                "WSL data 다시 연동",
                "현재 화면의 카탈로그·이미지를 WSL mycar/data 로 다시 보냅니다.\n「정제 데이터 연동」과 같으며, 학습 전 최신 상태로 맞출 때 사용합니다.",
                selected: info.HasOpenCatalog, enabled: info.HasOpenCatalog);
            _optResync = radioResync;

            var clearDesc = info.WslConfigured
                ? "WSL mycar/data 의 catalog·이미지·manifest 를 삭제합니다.\n화면의 카탈로그는 그대로이며, 학습 전 다시 연동해야 합니다."
                : "mycar 경로가 설정되지 않았습니다.\n학습 시작 또는 연동 시 경로를 먼저 선택하세요.";
            var (cardClear, radioClear) = CreateOptionCard(
                "WSL data 초기화",
                clearDesc,
                selected: !info.HasOpenCatalog && info.WslConfigured,
                enabled: info.WslConfigured);
            _optClearWsl = radioClear;

            var (cardReload, radioReload) = CreateOptionCard(
                "카탈로그 다시 읽기",
                "디스크의 .catalog 파일을 그대로 다시 불러옵니다.\n화면·선택·Undo가 초기화됩니다. WSL data는 변경하지 않습니다.",
                selected: false, enabled: info.HasOpenCatalog);
            _optReload = radioReload;

            var (cardClose, radioClose) = CreateOptionCard(
                "데이터 닫기",
                "현재 카탈로그를 닫고 화면을 비웁니다.\nWSL data는 그대로 두며, 다시 열려면 「데이터 선택」을 사용하세요.",
                selected: false, enabled: info.HasOpenCatalog);
            _optClose = radioClose;

            var optionsHost = StackCards(cardResync, cardClear, cardReload, cardClose);

            var note = new Label
            {
                Text = "※ 「정제 데이터」는 WSL mycar/data 폴더에 연동된 학습용 tub 입니다.",
                ForeColor = Accent,
                AutoSize = true,
                MaximumSize = new Size(ContentWidth, 0),
                Font = new Font("맑은 고딕", 9.5F),
                Margin = new Padding(0, 14, 0, 0),
                BackColor = Bg
            };

            var btnRow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                WrapContents = false,
                Width = ContentWidth,
                Margin = new Padding(0, 12, 0, 0),
                BackColor = Bg
            };

            var cancelBtn = CreateButton("취소", DialogResult.Cancel);
            var okBtn = CreateButton("확인", DialogResult.OK);
            okBtn.Margin = new Padding(0, 0, 8, 0);
            okBtn.Click += (_, _) =>
            {
                Result = _optResync.Checked ? RefreshChoice.ResyncWsl
                    : _optClearWsl.Checked ? RefreshChoice.ClearWsl
                    : _optReload.Checked ? RefreshChoice.ReloadDisk
                    : RefreshChoice.CloseAll;
            };

            btnRow.Controls.Add(cancelBtn);
            btnRow.Controls.Add(okBtn);

            var footer = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 2,
                Width = ContentWidth,
                BackColor = Bg,
                Margin = new Padding(0, 12, 0, 0)
            };
            footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            footer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            footer.Controls.Add(note, 0, 0);
            footer.Controls.Add(btnRow, 0, 1);

            LinkRadios(_optResync, _optClearWsl, _optReload, _optClose);

            root.Controls.Add(header, 0, 0);
            root.Controls.Add(optionsHost, 0, 1);
            root.Controls.Add(footer, 0, 2);
            Controls.Add(root);

            AcceptButton = okBtn;
            CancelButton = cancelBtn;
        }

        private static int GetMaxDialogHeight()
        {
            var screen = Screen.FromPoint(Cursor.Position);
            return screen.WorkingArea.Height - 40;
        }

        private static Panel StackCards(params Panel[] cards)
        {
            var host = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = PanelBg,
                Width = ContentWidth,
                Padding = new Padding(10),
                Margin = new Padding(0, 4, 0, 0)
            };

            int y = 10;
            foreach (var card in cards)
            {
                card.Location = new Point(10, y);
                host.Controls.Add(card);
                y = card.Bottom + 8;
            }

            host.Height = y + 2;
            return host;
        }

        private static (Panel Card, RadioButton Radio) CreateOptionCard(string title, string description, bool selected, bool enabled)
        {
            const int indent = 24;
            const int innerWidth = ContentWidth - 44;

            var card = new Panel
            {
                Width = ContentWidth - 20,
                AutoSize = false,
                BackColor = CardBg,
                Padding = new Padding(10, 8, 10, 10)
            };

            var radio = new RadioButton
            {
                Text = title,
                AutoSize = true,
                MaximumSize = new Size(innerWidth, 0),
                ForeColor = enabled ? TextLight : TextMuted,
                Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
                Location = new Point(4, 4),
                Checked = selected,
                Enabled = enabled,
                BackColor = CardBg
            };

            var desc = new Label
            {
                Text = description,
                ForeColor = enabled ? TextMuted : Color.FromArgb(120, 115, 113),
                AutoSize = true,
                MaximumSize = new Size(innerWidth - indent, 0),
                BackColor = CardBg,
                Font = new Font("맑은 고딕", 9.5F)
            };

            void LayoutCard()
            {
                desc.Location = new Point(indent, radio.Bottom + 4);
                card.Height = desc.Bottom + 12;
            }

            card.Controls.Add(radio);
            card.Controls.Add(desc);
            LayoutCard();
            radio.SizeChanged += (_, _) => LayoutCard();

            if (enabled)
            {
                void SelectOption(object? s, EventArgs e)
                {
                    if (s is RadioButton rb && rb == radio) return;
                    radio.Checked = true;
                }
                card.Click += SelectOption;
                desc.Click += SelectOption;
            }

            card.Paint += (_, e) =>
            {
                using var border = new Pen(KeyBorder);
                e.Graphics.DrawRectangle(border, 0, 0, card.Width - 1, card.Height - 1);
            };

            return (card, radio);
        }

        private static void LinkRadios(params RadioButton[] radios)
        {
            foreach (var radio in radios)
            {
                radio.CheckedChanged += (_, _) =>
                {
                    if (!radio.Checked) return;
                    foreach (var other in radios)
                    {
                        if (other != radio && other.Checked)
                            other.Checked = false;
                    }
                };
            }
        }

        private static Button CreateButton(string text, DialogResult result)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(88, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(53, 48, 49),
                ForeColor = TextLight,
                Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
                DialogResult = result,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = KeyBorder;
            return btn;
        }
    }
}
