using ConVar;
using Retech.Utils;

namespace Retech.Features;

public class PlayerChat(Retech retech)
{
  private readonly Retech _retech = retech;

  public void OnReceivedChat(Chat.ChatChannel targetChannel, ulong userId, string username, string message)
  {
    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds(),
      },
      PlayerChatEvent = new PlayerChatEvent
      {
        PrimaryPlayerIdentifier = new PlayerIdentifier
        {
          Type = "steamid",
          Identifier = userId.ToString(),
        },
        Channel = targetChannel switch
        {
          Chat.ChatChannel.Global => "global",
          Chat.ChatChannel.Team => "team",
          Chat.ChatChannel.Server => "server",
          Chat.ChatChannel.Cards => "cards",
          Chat.ChatChannel.Local => "local",
          Chat.ChatChannel.Clan => "clan",
          _ => "other"
        },
        Message = message.Length > 4096 ? message.Substring(0, 4096) : message,
      }
    });
  }
}
