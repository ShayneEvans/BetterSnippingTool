using BetterSnippingTool.Forms;
using BetterSnippingTool.Config;
using BetterSnippingTool.Utilities;

namespace BetterSnippingTool
{
    static class Program
    {
        public static AppConfig Config { get; private set; }

        [STAThread]
        static void Main()
        {
            //Initialize the application configuration
            AppConfig config = AppConfig.Instance;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Used so that screen scaling will not have an effect on screen capture area
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.Run(new BetterSnippingToolForm(false));
        }
    }
}