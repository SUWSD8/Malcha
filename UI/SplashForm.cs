using System.Drawing.Drawing2D;

namespace Malcha.UI
{
    /// <summary>
    /// 시작 인트로. Form.Opacity 페이드는 사용하되 자식 컨트롤은 모두 불투명 배경만 사용합니다.
    /// </summary>
    internal sealed class SplashForm : Form
    {
        private const int HoldAfterVisibleMs = 2600;
        private const int MaxWaitForMainMs = 8000;
        private const int AnimStepMs = 16;
        private const double FadeInStep = 0.11;
        private const double FadeOutStep = 0.09;

        private const string AppVersion = "v1.0";

        private static readonly Color BgDeep = Color.FromArgb(10, 8, 9);
        private static readonly Color BgMid = Color.FromArgb(28, 22, 24);
        private static readonly Color AccentPink = Color.FromArgb(227, 98, 132);
        private static readonly Color AccentPinkHot = Color.FromArgb(255, 120, 150);
        private static readonly Color AccentGold = Color.FromArgb(255, 200, 72);
        private static readonly Color AccentViolet = Color.FromArgb(106, 123, 221);
        private static readonly Color TextMuted = Color.FromArgb(168, 158, 155);

        private readonly System.Windows.Forms.Timer _animTimer;
        private readonly Panel _backdrop;
        private readonly Panel _card;
        private readonly Panel _progressTrack;
        private readonly Panel _progressFill;
        private readonly Label _lblTitle;
        private readonly Label _lblBadge;
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
        private float _phase;
        private float _progress;
        private Action? _onClosed;

        public SplashForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ShowInTaskbar = false;
            TopMost = true;
            ClientSize = new Size(720, 420);
            BackColor = BgDeep;
            ForeColor = TextMuted;
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            Opacity = 0;

            _backdrop = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgDeep
            };
            _backdrop.Paint += Backdrop_Paint;

            _card = new Panel
            {
                BackColor = Color.FromArgb(24, 20, 21),
                Padding = new Padding(0)
            };
            _card.Paint += Card_Paint;

            _lblBadge = new Label
            {
                AutoSize = true,
                Text = "◆  DONKEYCAR  ◆",
                Font = new Font("맑은 고딕", 8.5F, FontStyle.Bold),
                ForeColor = AccentGold,
                BackColor = Color.FromArgb(24, 20, 21)
            };
            _lblTitle = new Label
            {
                AutoSize = true,
                Text = "Malcha",
                Font = new Font("맑은 고딕", 52F, FontStyle.Bold),
                ForeColor = AccentPink,
                BackColor = Color.FromArgb(24, 20, 21)
            };
            _lblLine = new Label
            {
                Size = new Size(200, 4),
                BackColor = AccentGold
            };
            _lblSub = new Label
            {
                AutoSize = true,
                Text = "Catalog Editor  ·  Training Studio",
                Font = new Font("맑은 고딕", 11.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(210, 200, 198),
                BackColor = Color.FromArgb(24, 20, 21)
            };
            _lblVer = new Label
            {
                AutoSize = true,
                Text = AppVersion,
                Font = new Font("맑은 고딕", 9F, FontStyle.Bold),
                ForeColor = AccentViolet,
                BackColor = Color.FromArgb(24, 20, 21)
            };
            _lblLoading = new Label
            {
                AutoSize = true,
                Text = "동키카를 깨우는 중",
                Font = new Font("맑은 고딕", 10F, FontStyle.Bold),
                ForeColor = AccentPinkHot,
                BackColor = Color.FromArgb(24, 20, 21)
            };
            _lblHint = new Label
            {
                AutoSize = true,
                Text = "아무 곳이나 클릭 · 건너뛰기",
                Font = new Font("맑은 고딕", 8.5F),
                ForeColor = Color.FromArgb(110, 100, 98),
                BackColor = BgDeep
            };

            _progressTrack = new Panel
            {
                BackColor = Color.FromArgb(42, 36, 38),
                Height = 8
            };
            _progressFill = new Panel
            {
                BackColor = AccentPink,
                Height = 8
            };
            _progressTrack.Controls.Add(_progressFill);

            Controls.Add(_backdrop);
            Controls.Add(_card);
            _card.Controls.Add(_lblBadge);
            _card.Controls.Add(_lblTitle);
            _card.Controls.Add(_lblLine);
            _card.Controls.Add(_lblSub);
            _card.Controls.Add(_lblVer);
            _card.Controls.Add(_lblLoading);
            Controls.Add(_progressTrack);
            Controls.Add(_lblHint);

            _backdrop.SendToBack();
            _lblHint.BringToFront();
            _progressTrack.BringToFront();

            LayoutSplash();
            Resize += (_, _) => LayoutSplash();
            Click += (_, _) => TryClose(force: true);
            foreach (Control c in Controls)
                c.Click += (_, _) => TryClose(force: true);
            _card.Click += (_, _) => TryClose(force: true);
            foreach (Control c in _card.Controls)
                c.Click += (_, _) => TryClose(force: true);
            _backdrop.Click += (_, _) => TryClose(force: true);

            _animTimer = new System.Windows.Forms.Timer { Interval = AnimStepMs };
            _animTimer.Tick += OnAnimTick;
            Shown += (_, _) =>
            {
                _startedAt = DateTime.UtcNow;
                LayoutSplash();
                _animTimer.Start();
            };
        }

        /// <summary>페이드 아웃 시작 직후 — 메인 창을 스플래시 뒤에 먼저 표시합니다.</summary>
        public event Action? TransitionStarting;

        private void Backdrop_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = _backdrop.ClientRectangle;

            using (var bg = new LinearGradientBrush(r, BgMid, BgDeep, 72f))
                g.FillRectangle(bg, r);

            using (var vignette = new LinearGradientBrush(
                       new Rectangle(0, r.Height / 2, r.Width, r.Height / 2),
                       Color.FromArgb(0, 0, 0, 0),
                       Color.FromArgb(180, 0, 0, 0),
                       90f))
                g.FillRectangle(vignette, 0, r.Height / 2, r.Width, r.Height / 2);

            float pulse = 0.5f + 0.5f * MathF.Sin(_phase);
            float pulse2 = 0.5f + 0.5f * MathF.Sin(_phase * 1.37f + 1.2f);

            DrawGlow(g, r.Width * 0.12f, r.Height * 0.22f, 130f + 35f * pulse, AccentPink, 0.42f);
            DrawGlow(g, r.Width * 0.88f, r.Height * 0.28f, 100f + 25f * pulse2, AccentViolet, 0.32f);
            DrawGlow(g, r.Width * 0.72f, r.Height * 0.78f, 110f + 30f * pulse, AccentGold, 0.28f);
            DrawGlow(g, r.Width * 0.18f, r.Height * 0.82f, 90f + 20f * pulse2, AccentPinkHot, 0.22f);

            using var gridPen = new Pen(Color.FromArgb(18, 255, 255, 255), 1f);
            for (int x = 0; x < r.Width; x += 48)
                g.DrawLine(gridPen, x, 0, x, r.Height);
            for (int y = 0; y < r.Height; y += 48)
                g.DrawLine(gridPen, 0, y, r.Width, y);

            float scanY = (_phase * 80f) % (r.Height + 80f) - 40f;
            using var scan = new LinearGradientBrush(
                new RectangleF(0, scanY, r.Width, 60),
                Color.FromArgb(0, AccentPink),
                Color.FromArgb(0, AccentPink),
                90f);
            var blend = new ColorBlend(3)
            {
                Colors = new[] { Color.FromArgb(0, AccentPink), Color.FromArgb(55, AccentPink), Color.FromArgb(0, AccentPink) },
                Positions = new[] { 0f, 0.5f, 1f }
            };
            scan.InterpolationColors = blend;
            g.FillRectangle(scan, 0, scanY, r.Width, 60);

            DrawCornerBrackets(g, r, 18, AccentGold, 28);
        }

        private static void DrawGlow(Graphics g, float cx, float cy, float radius, Color color, float alpha)
        {
            var rect = new RectangleF(cx - radius, cy - radius, radius * 2, radius * 2);
            using var path = new GraphicsPath();
            path.AddEllipse(rect);
            using var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.FromArgb((int)(alpha * 255), color),
                SurroundColors = new[] { Color.FromArgb(0, color) }
            };
            g.FillPath(brush, path);
        }

        private static void DrawCornerBrackets(Graphics g, Rectangle r, int inset, Color color, int len)
        {
            using var pen = new Pen(Color.FromArgb(160, color), 2f);
            // TL
            g.DrawLine(pen, inset, inset + len, inset, inset);
            g.DrawLine(pen, inset, inset, inset + len, inset);
            // TR
            g.DrawLine(pen, r.Width - inset - len, inset, r.Width - inset, inset);
            g.DrawLine(pen, r.Width - inset, inset, r.Width - inset, inset + len);
            // BL
            g.DrawLine(pen, inset, r.Height - inset - len, inset, r.Height - inset);
            g.DrawLine(pen, inset, r.Height - inset, inset + len, r.Height - inset);
            // BR
            g.DrawLine(pen, r.Width - inset - len, r.Height - inset, r.Width - inset, r.Height - inset);
            g.DrawLine(pen, r.Width - inset, r.Height - inset, r.Width - inset, r.Height - inset - len);
        }

        private void Card_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var r = _card.ClientRectangle;
            r.Inflate(-1, -1);

            using var border = new Pen(AccentPink, 1.5f);
            using var path = RoundedRect(r, 14);
            g.DrawPath(border, path);

            using var inner = new Pen(Color.FromArgb(80, AccentGold), 1f);
            var innerR = r;
            innerR.Inflate(-4, -4);
            g.DrawPath(inner, RoundedRect(innerR, 11));
        }

        private static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void LayoutSplash()
        {
            int cx = ClientSize.Width / 2;
            const int bottomMargin = 28;

            int trackW = (int)(ClientSize.Width * 0.62);
            _progressTrack.Size = new Size(trackW, 8);
            _progressTrack.Location = new Point(cx - trackW / 2, ClientSize.Height - bottomMargin - 52);

            _lblHint.Location = new Point(cx - _lblHint.Width / 2, ClientSize.Height - bottomMargin - _lblHint.Height);
            UpdateProgressBar();

            int cardW = Math.Min(520, ClientSize.Width - 80);
            int cardH = 260;
            _card.Size = new Size(cardW, cardH);
            _card.Location = new Point(cx - cardW / 2, (ClientSize.Height - cardH) / 2 - 16);

            int innerCx = cardW / 2;
            _lblBadge.Location = new Point(innerCx - _lblBadge.Width / 2, 22);
            _lblTitle.Location = new Point(innerCx - _lblTitle.Width / 2, _lblBadge.Bottom + 8);
            _lblLine.Location = new Point(innerCx - _lblLine.Width / 2, _lblTitle.Bottom + 12);
            _lblSub.Location = new Point(innerCx - _lblSub.Width / 2, _lblLine.Bottom + 14);
            _lblVer.Location = new Point(innerCx - _lblVer.Width / 2, _lblSub.Bottom + 8);
            _lblLoading.Location = new Point(innerCx - _lblLoading.Width / 2, _lblVer.Bottom + 18);
        }

        private void UpdateProgressBar()
        {
            int w = Math.Max(0, (int)(_progressTrack.ClientSize.Width * _progress));
            if (w <= 0)
            {
                _progressFill.Width = 0;
                _progressFill.BackColor = AccentPink;
                return;
            }

            _progressFill.BackColor = AccentPink;
            _progressFill.Width = w;
        }

        public void NotifyMainReady() => _mainReady = true;

        public void RunWhenFinished(Action action) => _onClosed = action;

        private void OnAnimTick(object? sender, EventArgs e)
        {
            if (_fadeIn)
            {
                Opacity = Math.Min(1, Opacity + FadeInStep);
                if (Opacity >= 1)
                {
                    _fadeIn = false;
                    _fullyVisibleAt ??= DateTime.UtcNow;
                }
                return;
            }

            if (_fadeOut)
            {
                Opacity = Math.Max(0, Opacity - FadeOutStep);
                if (Opacity <= 0.01)
                {
                    Opacity = 0;
                    _animTimer.Stop();
                    Close();
                }
                return;
            }

            _phase += 0.11f;
            _backdrop.Invalidate();

            double elapsed = (DateTime.UtcNow - _startedAt).TotalMilliseconds;
            float target = _mainReady ? 1f : (float)Math.Min(0.88, elapsed / MaxWaitForMainMs * 0.88);
            _progress += (target - _progress) * 0.12f;
            if (_mainReady && _progress > 0.98f) _progress = 1f;
            UpdateProgressBar();

            _loadingDots = (_loadingDots + 1) % 12;
            if (_loadingDots % 3 == 0)
            {
                string dots = new string('.', (_loadingDots / 3) % 4);
                _lblLoading.Text = "동키카를 깨우는 중" + dots;
                int innerCx = _card.Width / 2;
                _lblLoading.Left = innerCx - _lblLoading.Width / 2;
            }

            int hintV = 140 + (int)(35 * (0.5 + 0.5 * Math.Sin(_phase * 2f)));
            hintV = Math.Clamp(hintV, 0, 255);
            _lblHint.ForeColor = Color.FromArgb(255, hintV, Math.Max(0, hintV - 10), Math.Max(0, hintV - 12));

            TryClose(force: false);
        }

        private void TryClose(bool force)
        {
            if (_closing) return;

            if (!force)
            {
                if (!_mainReady && (DateTime.UtcNow - _startedAt).TotalMilliseconds < MaxWaitForMainMs)
                    return;
                if (_fullyVisibleAt == null) return;
                if ((DateTime.UtcNow - _fullyVisibleAt.Value).TotalMilliseconds < HoldAfterVisibleMs)
                    return;
            }

            BeginClose();
        }

        private void BeginClose()
        {
            if (_closing) return;
            _closing = true;
            _progress = 1f;
            UpdateProgressBar();
            _lblLoading.Text = "시작합니다…";
            LayoutSplash();

            try { TransitionStarting?.Invoke(); } catch { }

            _fadeOut = true;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _animTimer.Stop();
            _animTimer.Dispose();
            try { _onClosed?.Invoke(); } catch { }
            base.OnFormClosed(e);
        }
    }
}
