namespace Malcha.UI
{
    /// <summary>
    /// Application.Run은 MainForm만 자동 Show 합니다.
    /// MainForm=스플래시, Form1은 스플래시 Shown 이후 숨긴 채로 생성합니다.
    /// </summary>
    internal sealed class MalchaApplicationContext : ApplicationContext
    {
        private readonly SplashForm _splash;
        private Form? _main;
        private bool _mainRevealed;
        private bool _eventsWired;

        public MalchaApplicationContext(SplashForm splash)
        {
            _splash = splash;
            MainForm = splash;

            _splash.Shown += (_, _) => EnsureMainCreated();
            _splash.TransitionStarting += OnTransitionStarting;
            _splash.RunWhenFinished(FocusMain);

            _splash.FormClosed += (_, _) =>
            {
                EnsureMainCreated();
                if (!_mainRevealed)
                {
                    PromoteMainAsApplicationForm();
                    RevealMainUnderSplash();
                }
                FocusMain();
            };
        }

        private void EnsureMainCreated()
        {
            if (_main != null) return;

            _main = new Form1();
            _main.Visible = false;
            _main.ShowInTaskbar = false;

            if (!_eventsWired)
            {
                _eventsWired = true;
                _main.Load += (_, _) =>
                {
                    if (!_splash.IsDisposed)
                        _splash.NotifyMainReady();
                };
                _main.Shown += (_, _) =>
                {
                    if (!_splash.IsDisposed)
                        _splash.NotifyMainReady();
                };
            }

            if (!_main.IsHandleCreated)
            {
                _ = _main.Handle;
                Application.DoEvents();
            }
        }

        private void OnTransitionStarting()
        {
            EnsureMainCreated();
            PromoteMainAsApplicationForm();
            RevealMainUnderSplash();
        }

        private void PromoteMainAsApplicationForm()
        {
            if (_main == null || _main.IsDisposed) return;
            MainForm = _main;
        }

        private void RevealMainUnderSplash()
        {
            if (_main == null || _main.IsDisposed || _mainRevealed) return;
            _mainRevealed = true;

            void Apply()
            {
                if (_main == null || _main.IsDisposed) return;
                _main.ShowInTaskbar = true;
                _main.WindowState = FormWindowState.Normal;
                _main.Show();
            }

            if (_main.InvokeRequired)
                _main.BeginInvoke(Apply);
            else
                Apply();
        }

        private void FocusMain()
        {
            if (_main == null || _main.IsDisposed) return;

            void Apply()
            {
                if (_main == null || _main.IsDisposed) return;
                _main.Activate();
                _main.BringToFront();
            }

            if (_main.InvokeRequired)
                _main.BeginInvoke(Apply);
            else
                Apply();
        }
    }
}
