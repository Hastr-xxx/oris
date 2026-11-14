using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CustomHttpServer.Framework.Content;

namespace CustomHttpServer.Framework.Handlers
{
    public class StaticFilesHandler : RequestHandler
    {
        private readonly string _rootDirectory;

        public StaticFilesHandler(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
        }

        public override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            string localPath = Uri.UnescapeDataString(request.Url?.LocalPath ?? "/").TrimStart('/');

            if (string.IsNullOrWhiteSpace(localPath))
            {
                localPath = "index.html";
            }
            else if (localPath.EndsWith("/"))
            {
                localPath += "index.html";
            }

            string filePath = Path.Combine(_rootDirectory, localPath);

            string fullPath = Path.GetFullPath(filePath);
            string rootPath = Path.GetFullPath(_rootDirectory) + Path.DirectorySeparatorChar;

            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
            {
                if (Successor != null)
                    await Successor.HandleRequestAsync(context);
                return;
            }

            response.ContentType = MimeTypeProvider.GetMimeType(filePath);
            byte[] buffer = await File.ReadAllBytesAsync(filePath);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();
        }
    }
}