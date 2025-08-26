using System.Linq;
using Google.Protobuf.Collections;
using Retech.Utils;

namespace Retech.Features;

public class ServerPlayerSync(Retech retech)
{
  private readonly Retech _retech = retech;

  public void OnPlayerSyncResponse() => SendPlayerSync();

  public void SendPlayerSync()
  {
    RepeatedField<ServerPlayerSyncEvent.Types.Player> players = [];

    foreach (BasePlayer basePlayer in BasePlayer.activePlayerList.ToArray<BasePlayer>())
      players.Add(new ServerPlayerSyncEvent.Types.Player
      {
        PrimaryPlayerIdentifier = new PlayerIdentifier
        {
          Type = "steam",
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
      });

    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds(),
      },
      ServerPlayerSyncEvent = new ServerPlayerSyncEvent
      {
        Players = { players },
      }
    });
  }
}
