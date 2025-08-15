using System.Text.Json;
using SimpleWebServer.Server;

public record ServerSettings(int Port);

class Program
{
    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            cts.Cancel(); //push the stop button for the server
            Console.WriteLine("\n[Program] Shutdown requested. Stopping server...");
        };

        //figure out where the config file lives
        string configPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Config",
            "serversettings.json"
        );
        int port = 8080;

        try
        {
            //read the file if it exist
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var settings = JsonSerializer.Deserialize<ServerSettings>(json);

                if (settings is not null && settings.Port != 0)
                    port = settings.Port;
            }
            else
            {
                Console.WriteLine($"[Program] Config not found at {configPath}. Using defaults.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[Program] Failed to read config: {ex.Message}. Using default port {port}."
            );
        }

        //spin up the server instance
        var server = new HttpServer(port);

        var serverTask = server.StartAsync(cts.Token);

        Console.WriteLine("[Program] Press Ctrl+C to stop.");

        await serverTask;
    }
}
