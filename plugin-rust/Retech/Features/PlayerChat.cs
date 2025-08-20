using System;
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
        PlayerIdentifier = new PlayerIdentifier
        {
          Type = "steamid",
          Identifier = userId.ToString(),
        },
        Channel = targetChannel switch
        {
          Chat.ChatChannel.Team => "team",
          Chat.ChatChannel.Global => "global",
          _ => "other"
        },
        Message = message.Substring(0, Math.Min(message.Length, 4096)),
      }
    });
  }
}