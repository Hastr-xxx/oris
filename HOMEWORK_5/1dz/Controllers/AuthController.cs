using System;
using CustomHttpServer.Framework.Attributes;

namespace CustomHttpServer.Controllers
{
    [HttpController("auth")]
    public class AuthController
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