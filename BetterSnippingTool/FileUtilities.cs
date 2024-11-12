using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterSnippingTool.Utilities
{
    public class FileUtilities
    {
        public string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public string outputDir;
        public string tempDir;

        public FileUtilities() 
        {
            this.outputDir = Path.Combine(executableDirectory, "GIF");
            this.tempDir = Path.Combine(outputDir, "GIF_Temp_Screenshots");
            Directory.CreateDirectory(outputDir);
            Directory.CreateDirectory(tempDir);
        }

        //Clears temp folder of all temp files
        public void ClearTemp()
        {
            try
            {
                if (Directory.Exists(this.tempDir))
                {
                    Array.ForEach(Directory.GetFiles(this.tempDir), File.Delete);
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing temp directory: {ex.Message}");
            }
        }

        //Deletes gif file
        public void ClearGifFolder(string gifFilePath)
        {
            //Extract the directory path up to the GIF folder
            string gifFolderPath = Path.GetDirectoryName(gifFilePath);
            Array.ForEach(Directory.GetFiles(gifFolderPath), File.Delete);
        }
    }
}
