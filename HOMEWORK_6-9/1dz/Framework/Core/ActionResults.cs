using System.Text;

namespace CustomHttpServer.Framework.Core
{
    public abstract class ActionResult { }

    public class HtmlResult : ActionResult
    {
        public string Html { get; }
        public HtmlResult(string html) => Html = html;
    }

    public class RedirectResult : ActionResult
    {
        public string Url { get; }
        public RedirectResult(string url) => Url = url;
    }

    public class NoContentResult : ActionResult { }

    public class ErrorResult : ActionResult
    {
        public int StatusCode { get; }
        public string Message { get; }
        public ErrorResult(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}