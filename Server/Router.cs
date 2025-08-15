namespace SimpleWebServer.Server;

public static class Router
{
    private static readonly string _wwwRoot = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot"
    );

    // resolves a request path to (body, contentType)
    public static (string body, string contentType) Resolve(string path)
    {
        // if the user requests "/", serve index.html
        if (path == "/")
            path = "/index.html";

        string filePath = Path.Combine(_wwwRoot, path.TrimStart('/'));

        if (File.Exists(filePath))
        {
            string body = File.ReadAllText(filePath);
            string contentType = MimeTypes.GetMimeType(filePath);
            return (body, contentType);
        }
        else
        {
            return ("<h1>404 Not Found</h1>", "text/html");
        }
    }
}
