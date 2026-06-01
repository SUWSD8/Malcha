using Malcha.UI;

namespace Malcha
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            var splash = new SplashForm();
            splash.Show();
            Application.DoEvents();

            var main = new Form1();
            main.Opacity = 0;
            main.ShowInTaskbar = false;
            main.Load += (_, _) => splash.NotifyMainReady();

            splash.RunWhenFinished(() =>
            {
                main.Opacity = 1;
                main.ShowInTaskbar = true;
                main.Show();
                main.Activate();
                splash.Dispose();
            });

            Application.Run(main);
        }
    }
}
