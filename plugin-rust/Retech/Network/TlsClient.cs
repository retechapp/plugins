using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Retech.Network;

public class TlsClient : IDisposable
{
  // Events
  public event Action<string, int>? OnConnecting;
  public event Action? OnConnected;
  public event Action? OnDisconnected;
  public event Action<double>? OnReconnectScheduled;
  public event Action<Exception>? OnError;
  public event Action<byte[], int>? OnData;

  // Config
  public bool SkipCertificateValidation { get; }
  public int ReceiveBufferSize { get; } = 256 * 1024;
  public int SendBufferSize { get; } = 256 * 1024;
  public int SendQueueCapacity { get; }
  public bool DropOnBackpressure { get; }
  public TimeSpan BackPressureTimeout { get; }
  public string? PinnedThumbprint { get; }
  public int MaxFrameBytes { get; } = 8 * 1024 * 1024;

  // Internals
  private BlockingCollection<ArraySegment<byte>> _sendQueue;
  private CancellationTokenSource? _cancellationTokenSource;

  private TcpClient? _tcpClient;
  private SslStream? _sslStream;
  private Task? _sendLoopTask;
  private Task? _receiveLoopTask;
  private readonly object _gate = new();
  private volatile bool _running;
  private Task? _connectionManagerTask;
  private CancellationTokenSource? _connectionManagerCancellationTokenSource;
  private string? _connectionManagerLastHost;
  private int _connectionManagerLastPort;

  public bool IsConnected => _running && _tcpClient?.Connected == true;

  public TlsClient(
    bool skipCertificateValidation = false,
    int sendQueueCapacity = 8192,
    bool dropOnBackpressure = false,
    TimeSpan? backpressureTimeout = null,
    string? pinnedThumbprint = null)
  {
    if (sendQueueCapacity <= 0)
      throw new ArgumentOutOfRangeException(nameof(sendQueueCapacity));

    SkipCertificateValidation = skipCertificateValidation;
    SendQueueCapacity = sendQueueCapacity;
    DropOnBackpressure = dropOnBackpressure;
    BackPressureTimeout = backpressureTimeout ?? TimeSpan.FromSeconds(2);
    PinnedThumbprint = pinnedThumbprint?.Replace(":", "").ToUpperInvariant();
    _sendQueue = new BlockingCollection<ArraySegment<byte>>(new ConcurrentQueue<ArraySegment<byte>>(), SendQueueCapacity);
  }

  public Task StartConnectionManager(string host, int port, CancellationToken cancellationToken = default)
  {
    if (_connectionManagerTask != null && !_connectionManagerTask.IsCompleted)
      return _connectionManagerTask;

    _connectionManagerLastHost = host;
    _connectionManagerLastPort = port;

    _connectionManagerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    _connectionManagerTask = Task.Run(() => ConnectionManagerAsync(_connectionManagerCancellationTokenSource.Token));
    return _connectionManagerTask;
  }

  public async Task StopConnectionManagerAsync()
  {
    try { _connectionManagerCancellationTokenSource?.Cancel(); } catch { }

    await DisconnectAsync().ConfigureAwait(false);
    Task? connectionManagerTask = _connectionManagerTask;
    if (connectionManagerTask != null)
      await Task.WhenAny(connectionManagerTask, Task.Delay(500)).ConfigureAwait(false);
  }

  private async Task ConnectionManagerAsync(CancellationToken cancellationToken)
  {
    if (_connectionManagerLastHost == null)
      throw new InvalidOperationException("Connection manager has not been started with a host and port.");

    int attempt = 0;
    while (!cancellationToken.IsCancellationRequested)
    {
      try
      {
        await ConnectAsync(_connectionManagerLastHost, _connectionManagerLastPort, cancellationToken).ConfigureAwait(false);
        attempt = 0;

        Task? sendLoopTask = _sendLoopTask;
        Task? receiveLoopTask = _receiveLoopTask;

        if (sendLoopTask == null && receiveLoopTask == null)
          await Task.Delay(100, cancellationToken).ConfigureAwait(false);
        else
          await Task.WhenAny(
            Task.WhenAll(sendLoopTask ?? Task.CompletedTask, receiveLoopTask ?? Task.CompletedTask),
            Task.Delay(Timeout.Infinite, cancellationToken)
          ).ConfigureAwait(false);
      }
      catch (OperationCanceledException)
      {
        break;
      }
      catch (Exception exception)
      {
        OnErrorSafe(exception);
      }
      finally
      {
        try { await DisconnectAsync().ConfigureAwait(false); } catch { }
      }

      attempt++;

      // 5s, 10s, 15s, 30s, 60s, 120s
      double seconds = Math.Min(300, attempt <= 3 ? attempt * 5 : 15 * Math.Pow(2, attempt - 3));
      TimeSpan delay = TimeSpan.FromSeconds(seconds);

      OnReconnectScheduledSafe(seconds);

      try { await Task.Delay(delay, cancellationToken).ConfigureAwait(false); }
      catch (OperationCanceledException) { break; }
    }
  }

  public async Task ConnectAsync(string host, int port, CancellationToken linkCancellationToken = default)
  {
    await DisconnectAsync().ConfigureAwait(false);

    OnConnectingSafe(host, port);

    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(linkCancellationToken);
    CancellationToken cancellationToken = _cancellationTokenSource.Token;

    try
    {
      TcpClient tcpClient = new()
      {
        NoDelay = true,
        ReceiveBufferSize = ReceiveBufferSize,
        SendBufferSize = SendBufferSize,
      };

      await tcpClient.ConnectAsync(host, port).ConfigureAwait(false);

      SslStream sslStream = new(tcpClient.GetStream(), false, RemoteCertificateValidation);
      await sslStream.AuthenticateAsClientAsync(
        host,
        null,
        SslProtocols.Tls12 | SslProtocols.Tls13,
        !SkipCertificateValidation
      ).ConfigureAwait(false);

      lock (_gate)
      {
        _tcpClient = tcpClient;
        _sslStream = sslStream;
        _running = true;
      }

      _sendLoopTask = Task.Run(() => SendLoopAsync(cancellationToken));
      _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(cancellationToken));

      OnConnectedSafe();
    }
    catch (Exception exception)
    {
      OnErrorSafe(exception);
      Disconnect();
      throw;
    }
  }

  public void Disconnect() => DisconnectAsync().GetAwaiter().GetResult();

  public async Task DisconnectAsync()
  {
    lock (_gate)
    {
      if (!_running && _cancellationTokenSource == null)
        return;

      _running = false;
    }

    try { _cancellationTokenSource?.Cancel(); } catch { }

    try { _sendQueue.CompleteAdding(); } catch { }
    try { _sslStream?.Dispose(); } catch { }
    try { _tcpClient?.Dispose(); } catch { }

    _sslStream = null;
    _tcpClient = null;

    Task[] tasks = new[] { _sendLoopTask, _receiveLoopTask }.Where((task) => task != null).Select(task => task!).ToArray();
    await Task.WhenAny(Task.WhenAll(tasks), Task.Delay(500)).ConfigureAwait(false);

    _sendLoopTask = null;
    _receiveLoopTask = null;

    _sendQueue = new BlockingCollection<ArraySegment<byte>>(new ConcurrentQueue<ArraySegment<byte>>(), SendQueueCapacity);
    _cancellationTokenSource?.Dispose();
    _cancellationTokenSource = null;

    OnDisconnectedSafe();
  }

  public bool Send(byte[] payload, int offset, int count)
  {
    if (payload == null)
      throw new ArgumentNullException(nameof(payload));

    if (offset < 0 || count < 0 || offset + count > payload.Length)
      throw new ArgumentOutOfRangeException();

    byte[] buffer = ArrayPool<byte>.Shared.Rent(count + 8);

    buffer[0] = (byte)(count >> 24);
    buffer[1] = (byte)(count >> 16);
    buffer[2] = (byte)(count >> 8);
    buffer[3] = (byte)count;

    Buffer.BlockCopy(payload, offset, buffer, 4, count);

    buffer[count + 4] = 0;
    buffer[count + 5] = 0;
    buffer[count + 6] = 0;
    buffer[count + 7] = 0;

    ArraySegment<byte> segment = new(buffer, 0, count + 8);

    if (!IsConnected)
    {
      ArrayPool<byte>.Shared.Return(buffer);
      return false;
    }

    if (DropOnBackpressure)
    {
      if (_sendQueue.TryAdd(segment))
        return true;

      ArrayPool<byte>.Shared.Return(buffer);
      return false;
    }

    double ms = BackPressureTimeout.TotalMilliseconds;
    int timeout = ms <= 0 ? 0 : ms >= int.MaxValue ? int.MaxValue : (int)ms;

    if (_sendQueue.TryAdd(segment, timeout))
      return true;

    ArrayPool<byte>.Shared.Return(buffer);
    OnErrorSafe(new IOException("Failed to send data due to backpressure."));
    return false;
  }

  public void Dispose() => Disconnect();

  private async Task SendLoopAsync(CancellationToken cancellationToken)
  {
    SslStream? sslStream = _sslStream;
    if (sslStream == null)
      return;

    try
    {
      foreach (ArraySegment<byte> segment in _sendQueue.GetConsumingEnumerable(cancellationToken))
      {
        try
        {
          byte[] array = segment.Array!;
          int offset = segment.Offset;
          int remaining = segment.Count;
          await sslStream.WriteAsync(array, offset, remaining, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
          if (segment.Array != null)
            ArrayPool<byte>.Shared.Return(segment.Array);
        }
      }

      await sslStream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }
    catch (OperationCanceledException) { }
    catch (Exception exception)
    {
      OnErrorSafe(exception);
    }
    finally
    {
      while (_sendQueue.TryTake(out ArraySegment<byte> leftover))
        if (leftover.Array != null)
          ArrayPool<byte>.Shared.Return(leftover.Array);
    }
  }

  private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
  {
    SslStream? sslStream = _sslStream;
    if (sslStream == null)
      return;

    byte[] readBuffer = ArrayPool<byte>.Shared.Rent(ReceiveBufferSize);
    byte[] parseBuffer = ArrayPool<byte>.Shared.Rent(ReceiveBufferSize);
    int parseLength = 0;
    bool remoteClosed = false;

    try
    {
      while (true)
      {
        int read = await sslStream.ReadAsync(readBuffer, 0, readBuffer.Length, cancellationToken).ConfigureAwait(false);
        if (read == 0)
        {
          remoteClosed = true;
          break;
        }

        if (parseLength + read > parseBuffer.Length)
        {
          int needed = parseLength + read;
          int next = parseBuffer.Length;
          while (next < needed)
            next <<= 1;

          byte[] grown = ArrayPool<byte>.Shared.Rent(next);
          Buffer.BlockCopy(parseBuffer, 0, grown, 0, parseLength);
          ArrayPool<byte>.Shared.Return(parseBuffer);
          parseBuffer = grown;
        }

        Buffer.BlockCopy(readBuffer, 0, parseBuffer, parseLength, read);
        parseLength += read;

        int cursor = 0;
        while (true)
        {
          if (parseLength - cursor < 4)
            break;

          int length = (parseBuffer[cursor] << 24)
            | (parseBuffer[cursor + 1] << 16)
            | (parseBuffer[cursor + 2] << 8)
            | parseBuffer[cursor + 3];

          if (length > (uint)MaxFrameBytes)
            throw new IOException($"Received frame is too large ({length} > {MaxFrameBytes}).");

          int total = 4 + length + 4;
          if (parseLength - cursor < total)
            break;

          int chk = cursor + 4 + length;
          if ((parseBuffer[chk] | parseBuffer[chk + 1] | parseBuffer[chk + 2] | parseBuffer[chk + 3]) != 0)
            throw new IOException("Checksum must be zero.");

          byte[] payload = ArrayPool<byte>.Shared.Rent(length);
          Buffer.BlockCopy(parseBuffer, cursor + 4, payload, 0, length);

          try { OnData?.Invoke(payload, length); }
          catch (Exception exception) { OnErrorSafe(exception); }

          cursor += total;
        }

        if (cursor > 0)
        {
          int remaining = parseLength - cursor;
          if (remaining > 0)
            Buffer.BlockCopy(parseBuffer, cursor, parseBuffer, 0, remaining);
          parseLength = remaining;
        }
      }
    }
    catch (OperationCanceledException) { }
    catch (Exception exception)
    {
      OnErrorSafe(exception);
    }
    finally
    {
      ArrayPool<byte>.Shared.Return(readBuffer);
      ArrayPool<byte>.Shared.Return(parseBuffer);

      if (remoteClosed && _running)
        _ = Task.Run(() => DisconnectAsync());
    }
  }

  private bool RemoteCertificateValidation(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
  {
    if (!string.IsNullOrEmpty(PinnedThumbprint) && certificate is X509Certificate2 c2)
      return string.Equals(c2.Thumbprint?.Replace(":", ""), PinnedThumbprint, StringComparison.OrdinalIgnoreCase);

    if (SkipCertificateValidation)
      return true;


    return sslPolicyErrors == SslPolicyErrors.None;
  }

  private void OnConnectingSafe(string host, int port)
  {
    try { OnConnecting?.Invoke(host, port); } catch (Exception exception) { OnErrorSafe(exception); }
  }

  private void OnConnectedSafe()
  {
    try { OnConnected?.Invoke(); } catch (Exception exception) { OnErrorSafe(exception); }
  }

  private void OnDisconnectedSafe()
  {
    try { OnDisconnected?.Invoke(); } catch (Exception exception) { OnErrorSafe(exception); }
  }

  private void OnReconnectScheduledSafe(double seconds)
  {
    try { OnReconnectScheduled?.Invoke(seconds); } catch (Exception exception) { OnErrorSafe(exception); }
  }

  private void OnErrorSafe(Exception exception)
  {
    try { OnError?.Invoke(exception); } catch { }
  }
}