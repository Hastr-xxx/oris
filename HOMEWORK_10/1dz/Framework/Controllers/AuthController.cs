using System;
using System.Net;
using _1dz.Framework.Result;
using CustomHttpServer.Framework.Attributes;

namespace _1dz.Framework.Controllers
{
    [HttpController("auth")]
    public class AuthController : BaseController
    {
        [Post("login")]
        public ActionResult Login(string login, string password)
        {
            const string testLogin = "student";
            const string testPassword = "test123";

            if (login != testLogin || password != testPassword)
            {
                return new JsonResult(new { Success = false, Message = "Неверный логин или пароль" }, 401);
            }

            int userId = 1;
            string sessionId = SessionService.CreateSession(userId);

            var cookie = new Cookie("sessionId", sessionId, "/")
            {
                HttpOnly = true,
                Secure = Context.Request.IsSecureConnection,
                Expires = DateTime.UtcNow.AddMinutes(30)
            };

            Context.Response.SetCookie(cookie);

            return Json(new { Success = true, Message = "Вход выполнен" });
        }

        [Post("logout")]
        public ActionResult Logout()
        {
            string? sessionId = Context.Request.Cookies["sessionId"]?.Value;
            SessionService.RemoveSession(sessionId);

            var cookie = new Cookie("sessionId", string.Empty, "/")
            {
                HttpOnly = true,
                Secure = Context.Request.IsSecureConnection,
                Expires = DateTime.UtcNow.AddDays(-1)
            };

            Context.Response.SetCookie(cookie);

            return Json(new { Success = true, Message = "Выход выполнен" });
        }
    }
}