using System;
using System.IO;
using System.Xml.Serialization;

public class AppConfig
{
    public string DefaultDirectory { get; set; } = string.Empty;
    public bool OptimizeGifCreation {get; set;} = true;
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
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string parentDirectory = Directory.GetParent(baseDirectory).FullName;
        string profilesPath = Path.Combine(parentDirectory, "Profiles");
        return Path.Combine(profilesPath, $"{fileName}");
    }

    private static AppConfig LoadConfig(string configName)
    {
        string filePath = GetFilePath(configName);
        Console.WriteLine(filePath);

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