using System;
using System.Threading;
using System.Threading.Tasks;
using Retech.Network;

namespace Retech;

public class Retech : IDisposable
{
  private readonly Config _config;
  private readonly TlsClient _tlsClient = new(true); // @TODO: Add fingerprint on self signed certs
  private readonly SemaphoreSlim _connectGate = new(1, 1);
  private readonly CancellationTokenSource _cancellationTokenSource = new();


  private volatile bool _disposed = false;
  private volatile bool _connectedOrConnecting = false;
  private int _connectionAttempts = 0;
  private int _reconnectScheduled = 0;

  public Retech()
  {
    _config = ConfigStore.LoadOrCreate(Constants.CONFIG_FILE);

    _tlsClient.OnConnected += OnConnected;
    _tlsClient.OnDisconnected += OnDisconnected;
    _tlsClient.OnError += OnError;
    _tlsClient.OnData += OnData;

    _ = SafeConnectAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
  }

  public void Dispose()
  {
    if (_disposed)
      return;

    try { _cancellationTokenSource.Cancel(); } catch { }
    try { _tlsClient.Dispose(); } catch { }

    _connectGate.Dispose();
    _cancellationTokenSource.Dispose();
  }

  private async Task SafeConnectAsync(CancellationToken cancellationToken)
  {
    if (_disposed)
      return;

    await _connectGate.WaitAsync(cancellationToken).ConfigureAwait(false);

    try
    {
      if (_disposed || _connectedOrConnecting)
        return;

      _connectedOrConnecting = true;
      Logger.Info($"Connecting to {_config.Worker.Host}:{_config.Worker.Port}");
      await _tlsClient.ConnectAsync(_config.Worker.Host, _config.Worker.Port).ConfigureAwait(false);
    }
    catch (OperationCanceledException) { }
    catch (Exception exception)
    {
      Logger.Error("Connection failed.", exception);
      _connectedOrConnecting = false;
      ScheduleReconnect();
    }
    finally
    {
      _connectGate.Release();
    }
  }

  private void ScheduleReconnect()
  {
    if (_disposed)
      return;

    if (Interlocked.Exchange(ref _reconnectScheduled, 1) == 1)
      return;

    int cappedAttemp = Math.Min(_connectionAttempts, 7);
    double seconds = Math.Min(Math.Pow(2, cappedAttemp), 120);
    int jitterMs = new Random().Next(0, 500);
    TimeSpan delay = TimeSpan.FromSeconds(seconds).Add(TimeSpan.FromMilliseconds(jitterMs));

    int attempt = _connectionAttempts + 1;
    Logger.Info($"Scheduling reconenct attempt #{attempt} in {delay.TotalSeconds:F1} seconds");
    CancellationToken cancellationToken = _cancellationTokenSource.Token;

    _ = Task.Run(async () =>
    {
      try
      {
        await Task.Delay(delay, cancellationToken).ConfigureAwait(false);

        if (_disposed)
          return;

        Interlocked.Exchange(ref _reconnectScheduled, 0);
        Interlocked.Increment(ref _connectionAttempts);
        await SafeConnectAsync(cancellationToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException) { }
      catch (Exception exception)
      {
        Interlocked.Exchange(ref _reconnectScheduled, 0);
        Logger.Error("Reconnect attempt failed.", exception);
        ScheduleReconnect();
      }
    }, cancellationToken);
  }

  private void OnConnected()
  {
    _connectedOrConnecting = true;
    Interlocked.Exchange(ref _connectionAttempts, 0);
    Interlocked.Exchange(ref _reconnectScheduled, 0);
    Logger.Info($"Connected to {_config.Worker.Host}:{_config.Worker.Port}");

    Features.SendHandshake.Execute(_config.Token);
  }

  private void OnDisconnected()
  {
    _connectedOrConnecting = false;
    Logger.Info($"Disconnected from the server");

    if (!_disposed)
      ScheduleReconnect();
  }

  private void OnError(Exception exception)
  {
    Logger.Error("A connection error occured.", exception);
  }

  private void OnData(byte[] data, int size)
  {
    PacketReader packetReader = new(data, size);
    ushort packetId = packetReader.ReadUInt16();

    switch (packetId)
    {
      case 0x0000:
        // @TODO: Handle packet
        break;

      case 0x0003:
        // @TODO: Handle packet
        break;

      default:
        Logger.Warning($"Received an unknown packet ID: {packetId}");
        break;
    }
  }

  public void Send(PacketWriter packetWriter) => _tlsClient.Send(packetWriter);
}
