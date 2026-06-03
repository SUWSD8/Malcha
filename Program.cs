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
            Application.Run(new MalchaApplicationContext(splash));
        }
    }
}
