using System;
using RetechLibrary.Network;

namespace Retech;


class Program
{
    static void Main()
    {
        WebSocketClient webSocketClient = new();

        webSocketClient.OnConnected += () =>
        {
            Console.WriteLine("Connected");

            webSocketClient.SendAsync(Samples.RustSamples.HandshakePacket());
        };

        webSocketClient.OnDisconnected += () =>
        {
            Console.WriteLine("Disconnected");
        };

        webSocketClient.OnError += (exception) =>
        {
            Console.WriteLine(exception.Message);
        };

        webSocketClient.OnPacketReceived += (packet) =>
        {
            ushort packetId = packet.ReadUInt16();
            switch (packetId)
            {
                case 0x0000:
                    Console.WriteLine("Handshake response");
                    break;

                default:
                    Console.WriteLine("Unknown packet");
                    break;
            }
        };

        webSocketClient.ConnectAsync(new Uri("ws://localhost:3030"));

        bool exit = false;
        while (!exit)
        {
            switch (Console.ReadKey().KeyChar)
            {
                case 'a':
                    webSocketClient.SendAsync(Samples.RustSamples.PlayerTickPacket());
                    break;

                case 'q':
                    exit = true;
                    break;
            }
        }
    }
}
