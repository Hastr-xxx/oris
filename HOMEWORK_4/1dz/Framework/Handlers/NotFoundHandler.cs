using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CustomHttpServer.Framework.Handlers
{
    public class NotFoundHandler : RequestHandler
    {
        public override async Task HandleRequestAsync(HttpListenerContext context)
        {
            var response = context.Response;
            response.StatusCode = 404;
            response.ContentType = "text/plain; charset=utf-8";

            byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();
        }
    }
}