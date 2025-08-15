using System;
using System.Threading.Tasks;
using Retech.Network;
using UnityEngine;

namespace Retech;

public class Retech : IDisposable
{
    public static Retech? Instance = null;
    public Config config;

    private readonly TlsClient _tlsClient = new(true);
    private int _reconnectCount = 0;

    public Retech()
    {
        Logger.Setup();

        config = Config.Reload();

        _tlsClient.OnConnected += OnConnected;
        _tlsClient.OnDisconnected += OnDisconnected;
        _tlsClient.OnError += OnError;
        _tlsClient.OnData += OnData;

        Logger.Info($"Connecting to {config.WorkerHost}:{config.WorkerPort}");
        _ = _tlsClient.ConnectAsync(config.WorkerHost, config.WorkerPort);
    }

    public void Dispose()
    {
        _tlsClient.Disconnect();
        Logger.Close();
    }

    public void OnConnected()
    {
        Logger.Info($"Connected with {config.WorkerHost}:{config.WorkerPort}");

        _reconnectCount = 0;

        Features.SendHandshake.Execute(config.Token);
    }

    public void OnDisconnected()
    {
        int reconnectSeconds = _reconnectCount switch
        {
            0 => 15,
            1 => 20,
            2 => 30,
            3 => 60,
            4 => 90,
            _ => 120,
        };
        _reconnectCount++;

        Logger.Info($"Disconnected, reconnecting in {reconnectSeconds} seconds");

        _ = Task.Run(async () =>
        {
            await Task.Delay(reconnectSeconds * 1000);
            Logger.Info($"Reconnecting to {config.WorkerHost}:{config.WorkerPort}");
            await _tlsClient.ConnectAsync(config.WorkerHost, config.WorkerPort);
        });
    }

    public void OnError(Exception exception)
    {
        Logger.Error($"Network error: {exception.Message}", exception);
    }

    public void OnData(byte[] data, int size)
    {
        PacketReader packetReader = new PacketReader(data, size);
        ushort packetId = packetReader.ReadUInt16();

        switch (packetId)
        {
            case 0x0000:
                byte success = packetReader.ReadByte();
                if (success == 0x01)
                    Logger.Info("Handshake success");
                else
                    Logger.Error("Handshake failed");
                break;

            case 0x0003:
                string level = packetReader.ReadString();
                string message = packetReader.ReadString();
                Logger.Info($"[{level}] {message}");
                break;

            default:
                Logger.Warning($"Received an unknown packet id: {packetId}");
                break;
        }
    }

    public void Send(PacketWriter packetWriter) => _tlsClient.Send(packetWriter);
}
