using Retech.Utils;

namespace Retech.Features;

public class PlayerJoin(Retech retech)
{
  private readonly Retech _retech = retech;

  public void OnPlayerJoin(BasePlayer basePlayer)
  {
    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds(),
      },
      PlayerJoinEvent = new PlayerJoinEvent
      {
        PrimaryPlayerIdentifier = new PlayerIdentifier
        {
          Type = "steamid",
          Value = basePlayer.UserIDString,
        },
        AdditionalPlayerIdentifiers = {
          new PlayerIdentifier
          {
            Type = "displayname",
            Value = basePlayer.displayName,
          },
          new PlayerIdentifier
          {
            Type = "ipaddress",
            Value = basePlayer.Connection.IPAddressWithoutPort(),
          },
        }
      }
    });
  }
}
