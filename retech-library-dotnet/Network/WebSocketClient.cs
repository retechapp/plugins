using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace RetechLibrary.Network;

public class WebSocketClient : IDisposable
{
    public Action? OnConnected;
    public Action? OnDisconnected;
    public Action<Exception>? OnError;
    public Action<Packet>? OnPacketReceived;

    private ClientWebSocket? _clientWebSocket;
    private CancellationTokenSource _cancellationTokenSource = new();
    private CancellationToken _cancellationToken;

    public WebSocketClient() { }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task ConnectAsync(Uri uri)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = _cancellationTokenSource.Token;

        _clientWebSocket = new ClientWebSocket
        {
            Options = {
                KeepAliveInterval = TimeSpan.FromSeconds(30),
            },
        };

        try
        {
            await _clientWebSocket.ConnectAsync(uri, _cancellationToken);

            if (_clientWebSocket.State == WebSocketState.Open)
            {
                OnConnected?.Invoke();
                await ReceiveAsync();
            }
        }
        catch (Exception exception)
        {
            OnError?.Invoke(exception);
            await DisconnectAsync();
        }
    }

    public async Task DisconnectAsync()
    {
        if (!_cancellationToken.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel();
        }

        if (_clientWebSocket != null && (_clientWebSocket.State == WebSocketState.Open || _clientWebSocket.State == WebSocketState.CloseReceived))
        {
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, _cancellationToken);
        }

        if (_clientWebSocket != null)
        {
            _clientWebSocket.Dispose();
            _clientWebSocket = null;
        }

        OnDisconnected?.Invoke();
    }

    public async Task SendAsync(Packet packet)
    {
        if (_clientWebSocket == null || _clientWebSocket.State != WebSocketState.Open)
        {
            return;
        }

        await _clientWebSocket.SendAsync(new ArraySegment<byte>(packet.ToArray()), WebSocketMessageType.Binary, true, _cancellationToken);
    }

    private async Task ReceiveAsync()
    {
        byte[] buffer = new byte[8192];

        try
        {
            while (!_cancellationToken.IsCancellationRequested && _clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
            {
                byte[] byteResult = [];
                WebSocketReceiveResult result;

                do
                {
                    result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await DisconnectAsync();
                        return;
                    }

                    if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        byteResult = byteResult.Concat(buffer.Take(result.Count)).ToArray();
                    }
                } while (!result.EndOfMessage);

                OnPacketReceived?.Invoke(new Packet(byteResult));
            }
        }
        catch (Exception exception)
        {
            OnError?.Invoke(exception);
            await DisconnectAsync();
        }
    }
}
