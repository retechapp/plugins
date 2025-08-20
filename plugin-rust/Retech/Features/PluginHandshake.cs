using Retech.Utils;

namespace Retech.Features;

public class PluginHandshake(Retech retech)
{
  private readonly Retech _retech = retech;

  public void OnConnected()
  {
    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds()
      },
      PluginHandshakeEvent = new PluginHandshakeEvent
      {
        Game = Constants.NAME,
        Version = Constants.VERSION,
        Token = _retech.Config.Token
      }
    });
  }

  public void OnPluginHandshakeResponse(Metadata metadata, PluginHandshakeResponse response)
  {
    if (response.Success)
      Logger.Log(Logger.LogLevel.Info, "PLUGIN-HANDSHAKE", $"Plugin handshake successful: {response.Message}");
    else
      Logger.Log(Logger.LogLevel.Error, "PLUGIN-HANDSHAKE", $"Plugin handshake failed: {response.Message}");
  }
}

