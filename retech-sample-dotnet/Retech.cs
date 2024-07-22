using System;
using System.Threading;
using System.Threading.Tasks;
using RetechLibrary.Network;

namespace Retech;

class Program
{
    static void Main()
    {
        Client client = new();

        client.OnError += (Exception exception) =>
        {
            Console.WriteLine($"Error: {exception.Message} (attemting reconnect in 1000 ms)");
            Task.Delay(1000).Wait();
            client.Connect("localhost", 13370);
        };

        client.OnConnected += () =>
        {
            Packet packet = new();
            packet.WriteUInt16(0x0000);
            packet.WriteVarString("rust");
            packet.WriteVarString("1.0.0");
            packet.WriteVarString("EXAMPLE_AUTHORIZATION_TOKEN");
            client.Send(packet);
        };

        client.OnData += (Packet packet) =>
        {
            Console.WriteLine($"Received packet ID: {packet.ReadUInt16()}");
        };

        client.Connect("localhost", 13370);

        Console.ReadKey();
    }
}
