using ConVar;
using Retech.Network;

namespace Retech.Features;

public class SendPlayerChat
{
    public static void Execute(ulong steamId, Chat.ChatChannel targetChannel, string message)
    {
        if (Retech.Instance == null)
            return;

        Packet packet = new();
        packet.WriteUInt16(0x0008);
        packet.WriteFloat(UnityEngine.Time.time);
        packet.WriteUInt64(steamId);
        packet.WriteString(targetChannel switch
        {
            Chat.ChatChannel.Team => "team",
            Chat.ChatChannel.Global => "global",
            _ => "Other"
        });
        packet.WriteString(message);
        Retech.Instance.SendPacket(packet);
    }
}
