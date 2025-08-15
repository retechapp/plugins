using System;
using ConVar;
using Retech.Network;

namespace Retech.Features;

public class SendChat
{
    public static void Execute(ulong steamId, Chat.ChatChannel targetChannel, string message)
    {
        if (Retech.Instance == null)
            return;

        PacketWriter packetWriter = new();
        packetWriter.WriteUInt16(0x0008);
        packetWriter.WriteFloat(UnityEngine.Time.time);
        packetWriter.WriteUInt64(steamId);
        packetWriter.WriteString(targetChannel switch
        {
            Chat.ChatChannel.Team => "team",
            Chat.ChatChannel.Global => "global",
            _ => "other"
        });
        packetWriter.WriteString(message.Substring(0, Math.Min(message.Length, 4096)));
        Retech.Instance.Send(packetWriter);
    }
}
