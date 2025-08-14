using System.Net;
using System.Net.Sockets;

namespace SimpleWebServer.Server;

public class HttpServer
{
    private readonly TcpListener _listener;

    private readonly int _port;

    public HttpServer(int port)
    {
        _port = port;
        _listener = new TcpListener(IPAddress.Any, _port);
    }

    //starts the server and keep accepting connections until cancelation is requested
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine($"[HttpServer] Listening on http://localhost:{_port}");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var acceptTask = _listener.AcceptTcpClientAsync();
                var completed = await Task.WhenAny(acceptTask, Task.Delay(-1, cancellationToken));

                if (completed != acceptTask)
                    break;

                var client = acceptTask.Result;

                _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
            }
        }
        finally
        {
            _listener.Stop();
            Console.WriteLine("[HttpServer] Stopped");
        }
    }

    //handle a single client connection: read the request & a minimal http response
    public async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        
    }
}
