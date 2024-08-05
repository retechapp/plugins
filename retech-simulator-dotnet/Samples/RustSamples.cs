using System;
using RetechLibrary.Network;

namespace Retech.Samples;

public class RustSamples
{
    public static Random random = new();

    public static Packet HandshakePacket()
    {
        Packet packet = new();
        packet.WriteUInt16(0x0000); // Packet ID
        packet.WriteString("rust"); // Plugin name
        packet.WriteString("1.0.0"); // Plugin version
        packet.WriteString("nQMx5Rvddbx969pggQhphWH5"); // Token - this is a fake token ;-)
        return packet;
    }

    public static Packet PlayerTickPacket()
    {
        Packet packet = new();
        packet.WriteUInt16(0x000A); // Packet ID
        packet.WriteFloat(random.NextSingle()); // Game server time
        packet.WriteUInt64(76561198035501555); // Steam ID
        packet.WriteFloat(random.NextSingle()); // postion x
        packet.WriteFloat(random.NextSingle()); // postion y
        packet.WriteFloat(random.NextSingle()); // postion z
        packet.WriteFloat(random.NextSingle()); // rotation x
        packet.WriteFloat(random.NextSingle()); // rotation y
        packet.WriteFloat(random.NextSingle()); // rotation z
        packet.WriteFloat(random.NextSingle()); // velocity x
        packet.WriteFloat(random.NextSingle()); // velocity y
        packet.WriteFloat(random.NextSingle()); // velocity z
        return packet;
    }
}
