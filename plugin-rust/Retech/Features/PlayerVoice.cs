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
        PrimaryPlayerIdentifier = new PlayerIdentifier
        {
          Type = "steamid",
          Value = basePlayer.UserIDString,
        },
        Raw = ByteString.CopyFrom(data),
      }
    });
  }
}
