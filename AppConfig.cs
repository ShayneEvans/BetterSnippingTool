//Configuration file used to load default values or other user created profiles.
using System.Xml.Serialization;
using BetterSnippingTool.Utilities;

namespace BetterSnippingTool.Config
{
    public class AppConfig
    {
        public string DefaultDirectory { get; set; } = string.Empty;
        public enum OptimizeGifCreation
        {
            Optimize = 0,   //Optimizes gif creation by scaling down aspet ratio
            SnippedRes = 1,      //Use snipped resolution
            CustomRes = 2    //Choose from set of custom resolutions
        }

        public OptimizeGifCreation OptimizationLevel { get; set; } = OptimizeGifCreation.Optimize;
        public (int, int) gifOutputResolution { get; set; }
        public string gifDirectory { get; set; }
        private int fps = 24;
        public int FPS
        {
            get => fps;
            set => fps = value >= 1 && value <= 60
                ? value
                : 24;
        }
        private int seconds = 5;
        public int Seconds
        {
            get => seconds;
            set => seconds = value >= 1 && value <= 20
                ? value
                : 5;
        }

        private static AppConfig instance;
        private string currentFileName;

        public static AppConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = LoadConfig("default.xml");
                }
                return instance;
            }
        }

        private static string GetFilePath(string fileName)
        {
            string profilesDir = FileUtilities.ProfilesDir;
            return Path.Combine(profilesDir, $"{fileName}");
        }

        private static AppConfig LoadConfig(string configName)
        {
            string filePath = GetFilePath(configName);

            if (File.Exists(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
                using (StreamReader reader = new StreamReader(filePath))
                {
                    instance = (AppConfig)serializer.Deserialize(reader);
                    instance.currentFileName = configName;
                }
            }
            else
            {
                instance = new AppConfig();
                instance.SaveConfig(configName);
            }
            return instance;
        }

        public void SaveConfig(string fileName)
        {
            string filePath = GetFilePath(fileName);

            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, this);
            }

            currentFileName = fileName;
        }

        public void SwitchToConfig(string newFileName)
        {
            LoadConfig(newFileName);
        }

        public string GetCurrentFileName()
        {
            return currentFileName;
        }
    }
}