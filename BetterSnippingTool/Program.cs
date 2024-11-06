using BetterSnippingTool.Forms;
using BetterSnippingTool.Config;

namespace BetterSnippingTool
{
    static class Program
    {
        public static AppConfig Config { get; private set; }

        [STAThread]
        static void Main()
        {
            // Initialize the application configuration
            AppConfig config = AppConfig.Instance;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BetterSnippingToolForm(false));
        }
    }
}