//Used to manage and organize file system paths
namespace BetterSnippingTool.Utilities
{
    public class FileUtilities
    {
        public string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
        public string outputDir;
        public string tempDir;
        public string ffmpegDir;
        public string resourcesDir;
        public string solutionDir;
        public string profilesDir;
        public Dictionary<string, string> buttonImagePaths { get; }

        public FileUtilities() 
        {
            this.solutionDir = Path.GetFullPath(Path.Combine(executableDirectory, @"..\..\..\"));
            this.ffmpegDir = GetFFmpegDir();
            this.outputDir = GetGifOutputDir();
            this.tempDir = GetGifTempDir();
            this.resourcesDir = GetResourceDir();
            this.profilesDir = GetProfileDir();
            this.buttonImagePaths = new Dictionary<string, string>
            {
                {"BS_Logo", Path.Combine(resourcesDir, "BS_Logo.ico")},
                {"green_crosshair", Path.Combine(resourcesDir, "green_crosshair.png")},
                {"New_Snip_Button", Path.Combine(resourcesDir, "Button_Images", "New_Snip_Button.png")},
                {"New_GIF_Button", Path.Combine(resourcesDir, "Button_Images", "New_GIF_Button.png")},
                {"Save_Button", Path.Combine(resourcesDir, "Button_Images", "Save_Button.png")},
                {"Trim_Button", Path.Combine(resourcesDir, "Button_Images", "Trim_Button.png")},
                {"Paint_Button_Off", Path.Combine(resourcesDir, "Button_Images", "Paint_Button_Off.png")},
                {"Paint_Button_On", Path.Combine(resourcesDir, "Button_Images", "Paint_Button_On.png")},
                {"Trim_Button_Off", Path.Combine(resourcesDir, "Button_Images", "Trim_Button_Off.png")},
                {"Trim_Button_On", Path.Combine(resourcesDir, "Button_Images", "Trim_Button_On.png")},
                {"Play_Button", Path.Combine(resourcesDir, "Button_Images", "Play_Button.png")},
                {"Pause_Button", Path.Combine(resourcesDir, "Button_Images", "Pause_Button.png")},
                {"Stop_Button", Path.Combine(resourcesDir, "Button_Images", "Stop_Button.png")},
                {"New_GIF_Button_REDO", Path.Combine(resourcesDir, "Button_Images", "New_GIF_Button_REDO.png")},
                {"New_Snip_Button_REDO", Path.Combine(resourcesDir, "Button_Images", "New_Snip_Button_REDO.png")},
                {"Settings_Button", Path.Combine(resourcesDir, "Button_Images", "Settings_Button.png")},
                {"Exit_Button", Path.Combine(resourcesDir, "Button_Images", "Exit_Button.png")},
            };
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

        public string GetFFmpegDir()
        {
            string ffmpegPath = Path.Combine(solutionDir, "ffmpeg", "ffmpeg.exe");
            return ffmpegPath;
        }

        public string GetResourceDir()
        {
            string resourcesPath = Path.Combine(solutionDir, "Resources");
            return resourcesPath;
        }

        public string GetGifOutputDir()
        {
            string gifOutputPath = Path.Combine(solutionDir, "GIF");
            return gifOutputPath;
        }

        public string GetGifTempDir()
        {
            string gifOutputPath = Path.Combine(solutionDir, "GIF\\GIF_Temp_Screenshots");
            return gifOutputPath;
        }

        public string GetProfileDir()
        {
            string profilesPath = Path.Combine(solutionDir, "Profiles");
            return profilesPath;
        }
    }
}