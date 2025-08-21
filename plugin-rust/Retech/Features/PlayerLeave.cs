using Retech.Utils;

namespace Retech.Features;

public class PlayerLeave(Retech retech)
{
  private readonly Retech _retech = retech;

  public void OnPlayerLeave(BasePlayer basePlayer)
  {
    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds(),
      },
      PlayerLeaveEvent = new PlayerLeaveEvent
      {
        PrimaryPlayerIdentifier = new PlayerIdentifier
        {
          Type = "steamid",
          Identifier = basePlayer.UserIDString,
        },
      }
    });
  }
}
