using System;
using System.Net;
using _1dz.Framework.Result;
using CustomHttpServer.Framework.Attributes;

namespace _1dz.Framework.Controllers
{
    [HttpController("cookie")]
    public class CookieController : BaseController
    {
        [Get("set")]
        public ActionResult Set()
        {
            var cookie = new Cookie("testCookie", "HelloWorld", "/")
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };
            Context.Response.SetCookie(cookie);
            return Html("<h1>Cookie установлена</h1>");
        }

        [Get("get")]
        public ActionResult Get()
        {
            string? value = Context.Request.Cookies["testCookie"]?.Value;
            return Json(new { Value = value });
        }

        [Get("delete")]
        public ActionResult Delete()
        {
            var cookie = new Cookie("testCookie", string.Empty, "/")
            {
                Expires = DateTime.UtcNow.AddDays(-1)
            };
            Context.Response.SetCookie(cookie);
            return Html("<h1>Cookie удалена</h1>");
        }
    }
}