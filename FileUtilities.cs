using System.Diagnostics;

namespace BetterSnippingTool.Utilities
{
    public static class FileUtilities
    {
        public static string ExecutableDir { get; private set; }
        public static string BaseDir { get; private set; }
        public static string BaseTempDir { get; private set; }
        public static string GifTempDir { get; private set; }
        public static string GifTempScreenshotsDir { get; private set; }
        public static string FFmpegDir { get; private set; }
        public static string ResourcesDir { get; private set; }
        public static string ProfilesDir { get; private set; }
        public static Dictionary<string, string> ButtonImagePaths { get; private set; }

        static FileUtilities()
        {
            //Initialize the base directory and depdency directories
            ExecutableDir = AppDomain.CurrentDomain.BaseDirectory;
            BaseDir = GetBaseDir();
            FFmpegDir = Path.Combine(BaseDir, "ffmpeg", "ffmpeg.exe");
            ResourcesDir = Path.Combine(BaseDir, "Resources");

            //Temp Directories
            BaseTempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BetterSnippingTool_Temp");
            GifTempDir = Path.Combine(BaseTempDir, "GIF");
            GifTempScreenshotsDir = Path.Combine(GifTempDir, "GIF_Temp_Screenshots");
            ProfilesDir = Path.Combine(BaseTempDir, "Profiles");

            //Create necessary temp folders if not already created
            CreateTempFolders();

            //Initialize the button image paths
            ButtonImagePaths = InitializeButtonImagePaths();
        }

        //Method to determine base directory depending on the environment
        private static string GetBaseDir()
        {
            string executableDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //Check if running in a development environment
            if (IsDevelopmentEnvironment(executableDir))
            {
                //Running in Visual Studio (Debug or Release mode)
                return Path.GetFullPath(Path.Combine(executableDir, @"..\..\..\"));
            }
            else
            {
                //Running in production (published app)
                return Path.GetFullPath(executableDir);
            }
        }

        // Method to initialize button image paths
        private static Dictionary<string, string> InitializeButtonImagePaths()
        {
            return new Dictionary<string, string>
            {
                { "BS_Logo", Path.Combine(ResourcesDir, "BS_Logo.ico") },
                { "green_crosshair", Path.Combine(ResourcesDir, "green_crosshair.png") },
                { "New_Snip_Button", Path.Combine(ResourcesDir, "Button_Images", "New_Snip_Button.png") },
                { "New_GIF_Button", Path.Combine(ResourcesDir, "Button_Images", "New_GIF_Button.png") },
                { "Save_Button", Path.Combine(ResourcesDir, "Button_Images", "Save_Button.png") },
                { "Trim_Button", Path.Combine(ResourcesDir, "Button_Images", "Trim_Button.png") },
                { "Paint_Button_Off", Path.Combine(ResourcesDir, "Button_Images", "Paint_Button_Off.png") },
                { "Paint_Button_On", Path.Combine(ResourcesDir, "Button_Images", "Paint_Button_On.png") },
                { "Trim_Button_Off", Path.Combine(ResourcesDir, "Button_Images", "Trim_Button_Off.png") },
                { "Trim_Button_On", Path.Combine(ResourcesDir, "Button_Images", "Trim_Button_On.png") },
                { "Play_Button", Path.Combine(ResourcesDir, "Button_Images", "Play_Button.png") },
                { "Pause_Button", Path.Combine(ResourcesDir, "Button_Images", "Pause_Button.png") },
                { "Stop_Button", Path.Combine(ResourcesDir, "Button_Images", "Stop_Button.png") },
                { "New_GIF_Button_REDO", Path.Combine(ResourcesDir, "Button_Images", "New_GIF_Button_REDO.png") },
                { "New_Snip_Button_REDO", Path.Combine(ResourcesDir, "Button_Images", "New_Snip_Button_REDO.png") },
                { "Settings_Button", Path.Combine(ResourcesDir, "Button_Images", "Settings_Button.png") },
                { "Exit_Button", Path.Combine(ResourcesDir, "Button_Images", "Exit_Button.png") }
            };
        }

        //Creates temp directories if they don't exist
        private static void CreateTempFolders()
        {
            try
            {
                //Create all required directories if they don't exist
                Directory.CreateDirectory(BaseTempDir);
                Directory.CreateDirectory(GifTempDir);
                Directory.CreateDirectory(Path.Combine(GifTempDir, "GIF_Temp_Screenshots"));
                Directory.CreateDirectory(ProfilesDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating temp directories: {ex.Message}");
            }
        }

        //Clears the temp directory of all temp files
        public static void ClearTemp()
        {
            try
            {
                if (Directory.Exists(GifTempScreenshotsDir))
                {
                    Array.ForEach(Directory.GetFiles(GifTempScreenshotsDir), File.Delete);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing temp directory: {ex.Message}");
            }
        }

        //Deletes all files in the specified GIF folder
        public static void ClearGifFolder(string gifFilePath)
        {
            try
            {
                string gifFolderPath = Path.GetDirectoryName(gifFilePath);
                Array.ForEach(Directory.GetFiles(gifFolderPath), File.Delete);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing GIF folder: {ex.Message}");
            }
        }

        private static bool IsDevelopmentEnvironment(string executableDir)
        {
            return executableDir.Contains(@"\bin\Debug\") || executableDir.Contains(@"\bin\Release\");
        }
    }
}