using System.Net;
using System.Threading.Tasks;

namespace CustomHttpServer.Framework.Handlers
{
    public abstract class RequestHandler
    {
        public RequestHandler? Successor { get; set; }

        public abstract Task HandleRequestAsync(HttpListenerContext context);
    }
}