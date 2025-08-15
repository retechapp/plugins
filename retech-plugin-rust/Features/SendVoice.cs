using Retech.Network;
using UnityEngine;

namespace Retech.Features;

public class SendVoice
{
  public static void Execute(BasePlayer basePlayer, byte[] data)
  {
    if (Loader.Instance == null)
      return;

    PacketWriter packetWriter = new();
    packetWriter.WriteUInt16(0x0009);
    packetWriter.WriteFloat(Time.time);
    packetWriter.WriteUInt64(basePlayer.userID.Get());
    packetWriter.WriteBytes(data);
    Loader.Instance.Send(packetWriter);
  }
}
