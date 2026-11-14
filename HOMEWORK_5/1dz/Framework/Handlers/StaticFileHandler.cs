using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CustomHttpServer.Core;

namespace CustomHttpServer.Framework.Handlers
{
    public class StaticFileHandler : Handler
    {
        private readonly string _rootDirectory;

        public StaticFileHandler(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public override async Task HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                string localPath = Uri.UnescapeDataString(request.Url?.LocalPath ?? "/");
                string filePath = GetFilePath(localPath);

                Console.WriteLine($"[StaticFileHandler] {localPath} -> {filePath}");

                if (!File.Exists(filePath))
                {
                    response.StatusCode = 404;
                    byte[] buf = Encoding.UTF8.GetBytes("404 Not Found");
                    response.ContentType = "text/plain; charset=utf-8";
                    response.ContentLength64 = buf.Length;
                    await response.OutputStream.WriteAsync(buf);
                    response.OutputStream.Close();
                    return;
                }

                response.ContentType = ContentTypeHelper.GetContentType(filePath);
                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaticFileHandler] Ошибка: {ex.Message}");
                try { response.StatusCode = 500; } catch { }
            }
            finally
            {
                try { response.OutputStream.Close(); } catch { }
            }
        }

        private string GetFilePath(string localPath)
        {
            string relativePath = localPath.TrimStart('/');

            bool endsWithSlash = localPath.EndsWith('/');
            bool endsWithLogin =
                relativePath.Equals("login", StringComparison.OrdinalIgnoreCase)
                || relativePath.EndsWith("/login", StringComparison.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(relativePath) || endsWithSlash || endsWithLogin)
            {
                relativePath = "login.html";
            }

            string fullPath = Path.GetFullPath(Path.Combine(_rootDirectory, relativePath));
            string rootWithSep = Path.GetFullPath(_rootDirectory) + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
                return Path.Combine(_rootDirectory, "404.html");

            return fullPath;
        }
    }
}