using System;
using System.Collections.Generic;
using System.IO;

namespace CustomHttpServer.Core;

public static class ContentTypeHelper
{
    private static readonly Dictionary<string, string> Types = new(StringComparer.OrdinalIgnoreCase)
    {
        [".html"] = "text/html; charset=utf-8",
        [".htm"] = "text/html; charset=utf-8",
        [".css"] = "text/css; charset=utf-8",
        [".js"] = "text/javascript; charset=utf-8",
        [".mjs"] = "text/javascript; charset=utf-8",
        [".json"] = "application/json; charset=utf-8",
        [".xml"] = "application/xml; charset=utf-8",
        [".txt"] = "text/plain; charset=utf-8",
        [".csv"] = "text/csv; charset=utf-8",

        [".svg"] = "image/svg+xml",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp",
        [".ico"] = "image/x-icon",
        [".tif"] = "image/tiff",
        [".tiff"] = "image/tiff",

        [".mp3"] = "audio/mpeg",
        [".wav"] = "audio/wav",
        [".ogg"] = "audio/ogg",

        [".mp4"] = "video/mp4",
        [".webm"] = "video/webm",

        [".pdf"] = "application/pdf",
        [".zip"] = "application/zip",
        [".rar"] = "application/vnd.rar",
        [".7z"] = "application/x-7z-compressed",

        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
        [".ttf"] = "font/ttf",
        [".otf"] = "font/otf",

        [".wasm"] = "application/wasm",
    };

    public static string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath);

        if (string.IsNullOrWhiteSpace(extension))
            return "application/octet-stream";

        return Types.TryGetValue(extension, out var contentType)
            ? contentType
            : "application/octet-stream";
    }
}