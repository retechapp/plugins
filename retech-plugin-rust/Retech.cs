using System;
using UnityEngine;
using RetechLibrary.Network;

namespace Retech;

public class Retech : IDisposable
{
    public static Retech? Instance = null;

    public Retech()
    {
        // Handshake packet
        BinaryPacket packet = new();
        packet.WriteUInt16(0x0000); // Packet ID
        packet.WriteVarString("rust");
        packet.WriteVarString("1.0.0");
        packet.WriteVarString("EXAMPLE_AUTHORIZATION_TOKEN");
        Debug.Log($"[Retech] {BitConverter.ToString(packet.ToArray())}");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
