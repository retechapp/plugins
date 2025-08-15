using System;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Retech;

public class Config
{
  [JsonProperty("token")]
  public string Token { get; set; } = "YOUR_SERVER_TOKEN";

  [JsonProperty("worker")]
  public WorkerConfig Worker { get; set; } = new WorkerConfig();

  public class WorkerConfig
  {
    [JsonProperty("host")]
    public string Host { get; set; } = "worker.retech.app";

    [JsonProperty("port")]
    public int Port { get; set; } = 8182;
  }

  [JsonProperty("features")]
  public FeaturesConfig Features { get; set; } = new FeaturesConfig();

  public class FeaturesConfig
  {
    [JsonProperty("playerChat")]
    public bool PlayerChat { get; set; } = true;

    [JsonProperty("playerVoice")]
    public bool PlayerVoice { get; set; } = true;

    [JsonProperty("playerTick")]
    public bool PlayerTick { get; set; } = true;
  }

  public void Validate()
  {
    if (string.IsNullOrWhiteSpace(Worker.Host))
      throw new InvalidOperationException("worker.host cannot be empty.");

    if (Worker.Port < 1 || Worker.Port > 65535)
      throw new InvalidOperationException("worker.port must be between 1 and 65535.");

    if (string.Equals(Token, "YOUR_SERVER_TOKEN", StringComparison.Ordinal))
      Logger.Warning("Token is set to the default value. Please change it to a valid server token.");
  }
}

public static class ConfigStore
{
  private static readonly JsonSerializerSettings _jsonSerializerSettings = new()
  {
    MissingMemberHandling = MissingMemberHandling.Error,
    DefaultValueHandling = DefaultValueHandling.Populate,
    NullValueHandling = NullValueHandling.Include,
    ObjectCreationHandling = ObjectCreationHandling.Auto,
    Formatting = Formatting.Indented,
  };

  private static readonly SemaphoreSlim _gate = new(1, 1);
  private static readonly Encoding _utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

  public static Config LoadOrCreate(string path)
  {
    Directory.CreateDirectory(Path.GetDirectoryName(path));

    Config config = new();
    if (File.Exists(path))
    {
      try
      {
        using FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        using StreamReader streamReader = new(fileStream, Encoding.UTF8, true);
        string payload = streamReader.ReadToEnd();
        JsonConvert.PopulateObject(payload, config, _jsonSerializerSettings);
      }
      catch (Exception exception)
      {
        Logger.Error($"Failed to load config file '{path}'. Using defaults.", exception);
      }
    }
    else
    {
      Logger.Info($"Config file '{path}' not found, creating new one with defaults.");
    }

    TrySave(config, path);

    try { config.Validate(); }
    catch (Exception exception) { Logger.Error($"Invalid config '{path}'", exception); }

    return config;
  }

  public static void Save(Config config, string path) => SaveAsync(config, path);

  public static async void SaveAsync(Config config, string path)
  {
    await _gate.WaitAsync().ConfigureAwait(false);

    try
    {
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      string tmp = path + ".tmp";
      string bak = path + ".bak";

      string payload = JsonConvert.SerializeObject(config, _jsonSerializerSettings);

      using (FileStream fileStream = new FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.None))
      using (StreamWriter streamWriter = new StreamWriter(fileStream, _utf8NoBom))
      {
        await streamWriter.WriteAsync(payload).ConfigureAwait(false);
        await streamWriter.FlushAsync().ConfigureAwait(false);
        fileStream.Flush(true);
      }

      if (File.Exists(path))
        File.Replace(tmp, path, bak, true);
      else
        File.Move(tmp, path);
    }
    catch (Exception exception)
    {
      Logger.Error($"Failed to save config file '{path}'.", exception);
    }
    finally
    {
      try { _gate.Release(); } catch { }
    }
  }

  private static void TrySave(Config config, string path)
  {
    try { Save(config, path); } catch { }
  }
}