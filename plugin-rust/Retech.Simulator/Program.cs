using System.Buffers;
using System.Threading.Tasks;
using Retech.Network;

namespace Retech.Simulator;

public static class Program
{
  private static readonly TlsClient _tlsClient = new(true); // @TODO: Add fingerprint on self signed certs

  public static void Main(string[] args)
  {
    if (args.Length < 2)
    {
      Console.WriteLine("Usage: Retech.Simulator <host> <port>");
      return;
    }

    string host = args[0];
    if (!int.TryParse(args[1], out int port))
    {
      Console.WriteLine("Invalid port number.");
      return;
    }

    _tlsClient.OnConnecting += OnConnecting;
    _tlsClient.OnConnected += OnConnected;
    _tlsClient.OnDisconnected += OnDisconnected;
    _tlsClient.OnReconnectScheduled += OnReconnectScheduled;
    _tlsClient.OnError += OnError;
    _tlsClient.OnData += OnData;

    _tlsClient.StartConnectionManager(host, port);

    Console.WriteLine("Press any key to exit...");
    Console.ReadKey();
  }

  private static void OnConnecting(string host, int port)
  {
    Console.WriteLine($"[Network] connecting to {host}:{port}...");
  }

  private static void OnConnected()
  {
    Console.WriteLine("[Network] connected to server.");
    _tlsClient.Send([0x00, 0xFF], 0, 2);
  }

  private static void OnDisconnected()
  {
    Console.WriteLine("[Network] disconnected from server.");
  }

  private static void OnReconnectScheduled(double seconds)
  {
    Console.WriteLine($"[Network] reconnecting in {seconds} seconds...");
  }

  private static void OnError(Exception exception)
  {
    Console.WriteLine($"[Network] error: {exception.Message}");
  }

  private static void OnData(byte[] data, int length)
  {
    string message = System.Text.Encoding.UTF8.GetString(data, 0, length);
    Console.WriteLine($"Received data: {message}");
    ArrayPool<byte>.Shared.Return(data);
    ArrayPool<byte>.Shared.Return(data);
  }
}
