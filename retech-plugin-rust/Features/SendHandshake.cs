using Retech.Network;

namespace Retech.Features;

public class SendHandshake
{
    public static void Execute(string token)
    {
        if (Retech.Instance == null)
            return;

        Packet packet = new();
        packet.WriteUInt16(0x0000);
        packet.WriteString("rust");
        packet.WriteString(Constants.VERSION);
        packet.WriteString(token);
        Retech.Instance.SendPacket(packet);
    }
}
