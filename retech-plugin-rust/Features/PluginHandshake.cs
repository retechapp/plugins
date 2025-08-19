using System;
using System.Diagnostics;

namespace Retech.Features;

public static class PluginHandshake
{
  public static void Request(string token)
  {
    PluginEnvelope pluginEnvelope = new()
    {
      Timestamp = Stopwatch.GetTimestamp(),
      PluginHandshakeRequest = new PluginHandshakeRequest
      {
        Game = "rust",
        Version = Constants.VERSION,
        Token = token
      }
    };

    Logger.Info($"Plugin Envelope: {pluginEnvelope.ToString()}");
  }

  public static void Response(WorkerEnvelope workerEnvelope)
  {
    if (workerEnvelope.PluginHandshakeResponse.Success)
      Logger.Info($"Handshake successful: {workerEnvelope.PluginHandshakeResponse.Message}");
    else
      Logger.Error($"Handshake failed: {workerEnvelope.PluginHandshakeResponse.Message}");
  }
}
