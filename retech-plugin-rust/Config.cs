using System;
using System.IO;
using Newtonsoft.Json;

namespace Retech;

public class Config
{
    [JsonProperty(PropertyName = "token")]
    public string Token = "YOUR_SERVER_TOKEN";

    [JsonProperty(PropertyName = "worker")]
    public string Worker = "wss://eu1.worker.retech.app";

    public static Config Reload()
    {
        Config config;

        try
        {
            string payload = File.ReadAllText(Constants.CONFIG_FILE);
            config = JsonConvert.DeserializeObject<Config>(payload);
            config ??= new();
        }
        catch
        {
            Logger.Info("Config file not found, creating new one");

            config = new();

            if (File.Exists(Constants.CONFIG_FILE))
                return config;
        }

        try
        {
            FileInfo fileInfo = new(Constants.CONFIG_FILE);

            if (!fileInfo.Directory.Exists)
                fileInfo.Directory.Create();

            string payload = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(Constants.CONFIG_FILE, payload);
        }
        catch (Exception exception)
        {
            Logger.Error("Failed to save config file", exception);
        }

        return config;
    }
}
