using System;
using System.IO;
using System.Xml.Serialization;

public class AppConfig
{
    public string DefaultDirectory { get; set; }

    private static AppConfig instance;

    public static AppConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = LoadConfig();
            }
            return instance;
        }
    }

    private static AppConfig LoadConfig()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");

        if (File.Exists(filePath))
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
            using (StreamReader reader = new StreamReader(filePath))
            {
                instance = (AppConfig)serializer.Deserialize(reader);
            }
        }
        else
        {
            instance = new AppConfig(); // Return a new instance with default values if the file does not exist
        }
        return instance;
    }

    public void SaveConfig()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.xml");

        XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            serializer.Serialize(writer, this);
        }
    }
}