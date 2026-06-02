namespace Malcha.UI
{
    /// <summary>
    /// 시작 인트로 화면.
    /// Form.Opacity != 1 이면 GDI+ MeasureString/DrawString이 실패하므로 Label만 사용합니다.
    /// </summary>
    internal sealed class SplashForm : Form
    {
        private const int HoldAfterVisibleMs = 2800;
        private const int MaxWaitForMainMs = 8000;
        private const int FadeStepMs = 16;
        private const double FadeDelta = 0.06;

        private const string AppVersion = "v1.0";

        private static readonly Color BgDeep = Color.FromArgb(14, 12, 13);
        private static readonly Color AccentPink = Color.FromArgb(227, 98, 132);
        private static readonly Color AccentGold = Color.FromArgb(255, 158, 48);
        private static readonly Color TextMuted = Color.FromArgb(148, 142, 140);

        private readonly System.Windows.Forms.Timer _animTimer;
        private readonly Label _lblTitle;
        private readonly Label _lblLine;
        private readonly Label _lblSub;
        private readonly Label _lblVer;
        private readonly Label _lblLoading;
        private readonly Label _lblHint;

        private DateTime _startedAt;
        private DateTime? _fullyVisibleAt;
        private bool _fadeIn = true;
        private bool _fadeOut;
        private bool _mainReady;
        private bool _closing;
        private int _loadingDots;
        private Action? _onClosed;

        public SplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            TopMost = true;
            ClientSize = new Size(640, 360);
            BackColor = BgDeep;
            ForeColor = TextMuted;
            DoubleBuffered = true;
            Opacity = 0;

            _lblTitle = new Label
            {
                AutoSize = true,
                Text = "Malcha",
                Font = new Font("맑은 고딕", 48F, FontStyle.Bold),
                ForeColor = AccentPink,
                BackColor = BgDeep
            };
            _lblLine = new Label
            {
                Size = new Size(140, 3),
                BackColor = AccentGold,
                BorderStyle = BorderStyle.None
            };
            _lblSub = new Label
            {
                AutoSize = true,
                Text = "DonkeyCar Catalog Editor",
                Font = new Font("맑은 고딕", 12F),
                ForeColor = TextMuted,
                BackColor = BgDeep
            };
            _lblVer = new Label
            {
                AutoSize = true,
                Text = AppVersion,
                Font = new Font("맑은 고딕", 9F),
                ForeColor = Color.FromArgb(100, TextMuted),
                BackColor = BgDeep
            };
            _lblLoading = new Label
            {
                AutoSize = true,
                Text = "Loading",
                Font = new Font("맑은 고딕", 9F),
                ForeColor = Color.FromArgb(160, AccentGold),
                BackColor = BgDeep
            };
            _lblHint = new Label
            {
                AutoSize = true,
                Text = "클릭하여 건너뛰기",
                Font = new Font("맑은 고딕", 8.5F),
                ForeColor = Color.FromArgb(80, 255, 255, 255),
                BackColor = BgDeep
            };

            Controls.Add(_lblTitle);
            Controls.Add(_lblLine);
            Controls.Add(_lblSub);
            Controls.Add(_lblVer);
            Controls.Add(_lblLoading);
            Controls.Add(_lblHint);
            // 하단(로딩·힌트)이 위에 그려지도록
            _lblLoading.BringToFront();
            _lblHint.BringToFront();

            LayoutSplash();
            Resize += (_, _) => LayoutSplash();
            Click += (_, _) => TryClose(force: true);
            foreach (Control c in Controls)
                c.Click += (_, _) => TryClose(force: true);

            _animTimer = new System.Windows.Forms.Timer { Interval = FadeStepMs };
            _animTimer.Tick += OnAnimTick;
            Shown += (_, _) =>
            {
                _startedAt = DateTime.UtcNow;
                LayoutSplash();
                _animTimer.Start();
            };
        }

        private void LayoutSplash()
        {
            int cx = ClientSize.Width / 2;
            const int bottomMargin = 36;
            const int gap = 12;

            _lblHint.Location = new Point(cx - _lblHint.Width / 2, ClientSize.Height - bottomMargin - _lblHint.Height);
            _lblLoading.Location = new Point(cx - _lblLoading.Width / 2, _lblHint.Top - gap - _lblLoading.Height);

            int clusterBottomMax = _lblLoading.Top - gap * 2;

            _lblTitle.Location = new Point(cx - _lblTitle.Width / 2, (int)(ClientSize.Height * 0.22));
            _lblLine.Location = new Point(cx - _lblLine.Width / 2, _lblTitle.Bottom + 14);
            _lblSub.Location = new Point(cx - _lblSub.Width / 2, _lblLine.Bottom + 14);
            _lblVer.Location = new Point(cx - _lblVer.Width / 2, _lblSub.Bottom + 6);

            if (_lblVer.Bottom > clusterBottomMax)
            {
                int shift = _lblVer.Bottom - clusterBottomMax;
                _lblTitle.Top -= shift;
                _lblLine.Top -= shift;
                _lblSub.Top -= shift;
                _lblVer.Top -= shift;
            }
        }

        public void NotifyMainReady() => _mainReady = true;

        public void RunWhenFinished(Action action) => _onClosed = action;

        private void OnAnimTick(object? sender, EventArgs e)
        {
            if (!_fadeIn && !_fadeOut)
            {
                _loadingDots = (_loadingDots + 1) % 12;
                if (_loadingDots % 3 == 0)
                {
                    _lblLoading.Text = "Loading" + new string('.', (_loadingDots / 3) % 4);
                    int cx = ClientSize.Width / 2;
                    _lblLoading.Left = cx - _lblLoading.Width / 2;
                }
            }

            if (_fadeIn)
            {
                Opacity = Math.Min(1, Opacity + FadeDelta);
                if (Opacity >= 1)
                {
                    _fadeIn = false;
                    _fullyVisibleAt ??= DateTime.UtcNow;
                }
                TryClose(force: false);
                return;
            }

            if (!_fadeOut)
            {
                TryClose(force: false);
                return;
            }

            Opacity = Math.Max(0, Opacity - FadeDelta);
            if (Opacity <= 0)
            {
                _animTimer.Stop();
                _closing = true;
                Close();
            }
        }

        private void TryClose(bool force)
        {
            if (_closing || _fadeOut) return;

            if (!force)
            {
                if (!_mainReady && (DateTime.UtcNow - _startedAt).TotalMilliseconds < MaxWaitForMainMs)
                    return;
                if (_fullyVisibleAt == null) return;
                if ((DateTime.UtcNow - _fullyVisibleAt.Value).TotalMilliseconds < HoldAfterVisibleMs)
                    return;
            }

            _fadeIn = false;
            _fadeOut = true;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _animTimer.Stop();
            _animTimer.Dispose();
            _onClosed?.Invoke();
            base.OnFormClosed(e);
        }
    }
}
