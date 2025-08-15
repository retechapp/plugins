using Retech.Network;

namespace Retech.Features;

public class SendHandshake
{
    public static void Execute(string token)
    {
        if (Loader.Instance == null)
            return;

        PacketWriter packetWriter = new();
        packetWriter.WriteUInt16(0x0000);
        packetWriter.WriteString("rust");
        packetWriter.WriteString(Constants.VERSION);
        packetWriter.WriteString(token);
        Loader.Instance.Send(packetWriter);
    }
}
