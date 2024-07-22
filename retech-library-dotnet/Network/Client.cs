using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace RetechLibrary.Network;

public class Client : IDisposable
{
    public Action<Exception>? OnError;
    public Action? OnConnected;
    public Action<Packet>? OnData;

    public Client()
    { }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public void Connect(string host, int port, int timeout = 10_000)
    {
        //
    }

    public void Send(Packet packet)
    {
        //
    }
}
