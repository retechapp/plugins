using System;
using Google.Protobuf;
using Retech.Utils;

namespace Retech.Features;

public class PlayerVoice(Retech retech)
{
  private readonly Retech _retech = retech;

  public void OnReceivedVoice(BasePlayer basePlayer, byte[] data)
  {
    _retech.Send(new PluginSendEnvelope
    {
      Metadata = new Metadata
      {
        Timestamp = TimeUtils.UnixTimeMilliseconds(),
      },
      PlayerVoiceEvent = new PlayerVoiceEvent
      {
        PlayerIdentifier = new PlayerIdentifier
        {
          Type = "steamid",
          Identifier = basePlayer.UserIDString,
        },
        Raw = ByteString.CopyFrom(data),
      }
    });
  }
}
