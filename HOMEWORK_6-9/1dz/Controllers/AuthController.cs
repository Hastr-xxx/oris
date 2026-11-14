using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Framework.Core;
using System;

namespace CustomHttpServer.Controllers
{
    [HttpController("auth")]
    public class AuthController : ControllerBase
    {
        [Get("login")]
        public string Login()
        {
            return "Use POST /auth/login to submit the form.";
        }

        [Post("login")]
        public string Login(string login, string password)
        {
            Console.WriteLine();
            Console.WriteLine("Получены данные из формы:");
            Console.WriteLine($"Логин: {login}");
            Console.WriteLine($"Пароль: {password}");
            Console.WriteLine();


            return $"OK. Login={login}, Password={password}";
        }
    }
}