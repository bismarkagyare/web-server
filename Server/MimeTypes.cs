namespace SimpleWebServer.Server;

public static class MimeTypes
{
    private static readonly Dictionary<string, string> _mappings = new(
        StringComparer.InvariantCultureIgnoreCase
    )
    {
        { ".html", "text/html" },
        { ".htm", "text/html" },
        { ".css", "text/css" },
        { ".js", "application/javascript" },
        { ".json", "application/json" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".gif", "image/gif" },
        { ".svg", "image/svg+xml" },
        { ".txt", "text/plain" },
    };

    public static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        if (ext != null && _mappings.TryGetValue(ext, out var mime))
        {
            return mime;
        }
        return "application/octet-stream"; // default if unknown
    }
}
