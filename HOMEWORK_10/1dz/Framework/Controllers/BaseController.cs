using System.Net;
using _1dz.Framework.Result;
using CustomHttpServer.Framework.Authentication;

namespace _1dz.Framework.Controllers
{
    public abstract class BaseController
    {
        protected HttpListenerContext Context { get; private set; } = null!;
        protected IUserSessionService SessionService { get; private set; } = null!;

        public void Initialize(HttpListenerContext context, IUserSessionService sessionService)
        {
            Context = context;
            SessionService = sessionService;
        }

        protected ActionResult Html(string html) => new HtmlResult(html);
        protected ActionResult Json(object data, int statusCode = 200) => new JsonResult(data, statusCode);
    }
}