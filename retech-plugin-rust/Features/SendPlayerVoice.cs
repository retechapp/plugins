using Retech.Network;
using UnityEngine;

namespace Retech.Features;

public class SendPlayerVoice
{
    public static void Execute(BasePlayer basePlayer, byte[] data)
    {
        if (Retech.Instance == null)
            return;

        Packet packet = new();
        packet.WriteUInt16(0x0009);
        packet.WriteFloat(Time.time);
        packet.WriteUInt64(basePlayer.userID.Get());
        packet.WriteBytes(data);
        Retech.Instance.SendPacket(packet);
    }
}
