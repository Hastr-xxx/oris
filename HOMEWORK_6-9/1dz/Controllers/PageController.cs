using MiniTemplateEngine;
using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Framework.Core;

namespace CustomHttpServer.Controllers
{
    [HttpController("page")]
    public class PageController : ControllerBase
    {
        //Пример запроса: http://127.0.0.1:8888/page/user?name=Вася
        [Get("user")]
        public string GetUserPage([FromQuery] string name)
        {
            var model = new { Name = name ?? "Гость", IsAdmin = true, Items = new[] { "Картошка", "Картошка побольше" } };

            string template = @"
                <h1>Привет, ${Name}!</h1>
                ${if IsAdmin}<p>Ты админ.</p>${else}<p>Ты обычный юзер.</p>${endif}
                <ul>
                    ${for item in Items}<li>${item}</li>${endfor}
                </ul>";

            var renderer = new HtmlTemplateRenderer();

            return renderer.RenderFromString(template, model);
        }
    }
}