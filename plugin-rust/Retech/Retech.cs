using System;
using Retech.Network;
using System.Buffers;
using Google.Protobuf;
using Retech.Features;

namespace Retech;

public class Retech : IDisposable
{
  public readonly Config Config;
  private readonly TlsClient _tlsClient = new(true);
  private volatile bool _disposed = false;

  // Features
  public readonly PluginHandshake? PluginHandshake;
  public readonly ServerHealth? ServerHealth;
  public readonly PlayerVoice? PlayerVoice;
  public readonly PlayerChat? PlayerChat;
  public readonly Features.PlayerJoin? PlayerJoin;
  public readonly PlayerLeave? PlayerLeave;
  public readonly PlayerTick? PlayerTick;

  public Retech()
  {
    Config = ConfigStore.LoadOrCreate(Constants.CONFIG_FILE);

    _tlsClient.OnConnecting += OnConnecting;
    _tlsClient.OnConnected += OnConnected;
    _tlsClient.OnDisconnected += OnDisconnected;
    _tlsClient.OnReconnectScheduled += OnReconnectScheduled;
    _tlsClient.OnError += OnError;
    _tlsClient.OnData += OnData;

    PluginHandshake = new PluginHandshake(this);

    if (Config.Features.ServerHealth)
      ServerHealth = new ServerHealth(this);

    if (Config.Features.PlayerVoice)
      PlayerVoice = new PlayerVoice(this);

    if (Config.Features.PlayerChat)
      PlayerChat = new PlayerChat(this);

    PlayerJoin = new Features.PlayerJoin(this);

    PlayerLeave = new PlayerLeave(this);

    if (Config.Features.PlayerTick)
      PlayerTick = new PlayerTick(this);

    _tlsClient.StartConnectionManager(Config.Worker.Host, Config.Worker.Port);
  }

  public void Dispose()
  {
    if (_disposed)
      return;

    _disposed = true;

    try { _tlsClient.Dispose(); } catch { }
  }

  private void OnConnecting(string host, int port)
  {
    Logger.Log(Logger.LogLevel.Info, "NETWORK", $"Connecting to worker {host}:{port}");
  }

  private void OnConnected()
  {
    Logger.Log(Logger.LogLevel.Info, "NETWORK", $"Connected to worker");

    PluginHandshake?.OnConnected();
  }

  private void OnDisconnected()
  {
    Logger.Log(Logger.LogLevel.Warning, "NETWORK", $"Disconnected from worker");
  }

  private void OnReconnectScheduled(double seconds)
  {
    Logger.Log(Logger.LogLevel.Warning, "NETWORK", $"Attempting to reconnect in {seconds} seconds");
  }

  private void OnError(Exception exception)
  {
    Logger.Log(Logger.LogLevel.Error, "NETWORK", $"Error", exception);
  }

  private void OnData(byte[] data, int size)
  {
    if (_disposed || data.Length < 2 || size < 2)
    {
      ArrayPool<byte>.Shared.Return(data);
      return;
    }

    ushort packetId = (ushort)((data[0] << 8) | (data[1] & 0xFF));
    try
    {
      switch (packetId)
      {
        case 0x000F:
          PluginReceiveEnvelope pluginReceiveEnvelope = PluginReceiveEnvelope.Parser.ParseFrom(data.AsSpan(2, size - 2));
          switch (pluginReceiveEnvelope.PayloadCase)
          {
            case PluginReceiveEnvelope.PayloadOneofCase.PluginHandshakeResponse:
              PluginHandshake?.OnPluginHandshakeResponse(pluginReceiveEnvelope.Metadata, pluginReceiveEnvelope.PluginHandshakeResponse);
              break;

            default:
              Logger.Log(Logger.LogLevel.Warning, "NETWORK", $"Received unknown payload type: {pluginReceiveEnvelope.PayloadCase}");
              break;
          }
          break;

        default:
          Logger.Log(Logger.LogLevel.Warning, "NETWORK", $"Received unknown packet ID: {packetId}");
          break;
      }
    }
    catch (Exception exception)
    {
      Logger.Log(Logger.LogLevel.Error, "NETWORK", $"Error processing packet: {packetId}", exception);
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(data);
    }
  }

  public bool Send(PluginSendEnvelope pluginSendEnvelope)
  {
    ushort packetId = 0x000F;

    int size = 2 + pluginSendEnvelope.CalculateSize();
    byte[] buffer = ArrayPool<byte>.Shared.Rent(size);

    // Write packet id
    buffer[0] = (byte)(packetId >> 8);
    buffer[1] = (byte)(packetId & 0xFF);

    // Write protobuf payload
    pluginSendEnvelope.WriteTo(buffer.AsSpan(2, size - 2));

    bool sent = _tlsClient.Send(buffer, 0, size);
    ArrayPool<byte>.Shared.Return(buffer);
    return sent;
  }
}
