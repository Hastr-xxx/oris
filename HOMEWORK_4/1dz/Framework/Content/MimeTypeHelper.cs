using System;
using System.Collections.Generic;
using System.IO;

namespace CustomHttpServer.Framework.Content 
{
    public static class MimeTypeProvider 
    {
        private static readonly Dictionary<string, string> Types = new(StringComparer.OrdinalIgnoreCase)
        {
            [".html"] = "text/html; charset=utf-8",
            [".css"] = "text/css; charset=utf-8",
            [".js"] = "text/javascript; charset=utf-8",
            [".png"] = "image/png",
            [".jpg"] = "image/jpeg",
            [".svg"] = "image/svg+xml",
            [".ico"] = "image/x-icon"
        };

        public static string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(extension)) return "application/octet-stream";

            return Types.TryGetValue(extension, out var mimeType)
                ? mimeType
                : "application/octet-stream";
        }
    }
}