using System.Net;
using System.Net.Sockets;
using System.Text;

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
        using (client)
        {
            try
            {
                using var stream = client.GetStream(); //grab the network stream b/n server & client
                var buffer = new byte[8192]; //i'll need a buffer to hold raw bytes
                var requestBuilder = new StringBuilder(); //i'll stitch together the bytes into a string here

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(
                        buffer,
                        0,
                        buffer.Length,
                        cancellationToken
                    );
                    if (bytesRead == 0)
                        break;

                    requestBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    if (requestBuilder.ToString().Contains("\r\n\r\n"))
                        break;

                    if (!stream.DataAvailable)
                        break;
                }

                //atp, i have the raw http request text
                string rawrequest = requestBuilder.ToString();
                var firstLine = rawrequest.Split('\n')[0].TrimEnd('\r');
                Console.WriteLine($"[HttpServer] {firstLine}");

                //craft the reply
                string body = "Hello from SimpleWebServer";
                byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

                //build the http reponse headers
                var responseBuilder = new StringBuilder();
                requestBuilder.Append("HTTP/1.1 200 OK\r\n");

                responseBuilder.Append("Content-Type: text/plain; charset=utf-8\r\n");
                responseBuilder.Append($"Content-Length: {bodyBytes.Length}\r\n");
                responseBuilder.Append("Connection: close\r\n");
                responseBuilder.Append("\r\n");

                byte[] headerBytes = Encoding.ASCII.GetBytes(responseBuilder.ToString());

                await stream.WriteAsync(headerBytes, 0, headerBytes.Length, cancellationToken);
                await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpServer] Client error: {ex.Message}");
            }
        }
    }
}
