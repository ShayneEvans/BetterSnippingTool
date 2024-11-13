//Wrapper for FFmpeg
using System.Diagnostics;

namespace BetterSnippingTool.Tools
{
    public static class FFmpeg
    {
        public static void run_command(string ffmpegPath, string command)
        {
            using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = ffmpegPath;
                p.StartInfo.Arguments = command;
                p.Start();
                p.WaitForExit();
            }
        }
    }
}