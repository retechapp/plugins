using System;
using System.Threading.Tasks;
using Retech.Network;

namespace Retech;

public class Retech : IDisposable
{
    public static Retech? Instance = null;
    public Config config;

    private readonly WebSocketClient? _webSocketClient;

    public Retech()
    {
        config = Config.Reload();

        _webSocketClient = new WebSocketClient();
        _webSocketClient.OnConnected += OnConnected;
        _webSocketClient.OnDisconnected += OnDisconnected;
        _webSocketClient.OnError += OnError;
        _webSocketClient.OnPacketReceived += OnPacketReceived;

        _ = _webSocketClient.ConnectAsync(new Uri(config.Worker));
    }

    public void Dispose()
    {
        _webSocketClient?.Dispose();
        Logger.Close();
    }

    public void OnConnected()
    {
        Logger.Info("Connected");
    }

    public async void OnDisconnected()
    {
        if (_webSocketClient == null)
            return;

        Logger.Info("Disconnected (reconnecing in 15 seconds)");

        await Task.Delay(15_000);
        await _webSocketClient.ConnectAsync(new Uri(config.Worker));
    }

    public void OnError(Exception exception)
    {
        Logger.Error("Network error", exception);
    }

    public void OnPacketReceived(Packet packet)
    {
        //
    }

    public void SendPacket(Packet packet)
    {
        if (_webSocketClient == null)
            return;

        _ = _webSocketClient.SendAsync(packet);
    }
}
