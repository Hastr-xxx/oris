using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CustomHttpServer.Framework.Attributes;
using MiniHttpServer.ORM;
using MiniTemplateEngine;
using CustomHttpServer.Framework.Attributes;
using _1dz.Framework.Result;

namespace _1dz.Framework.Controllers
{
    [HttpController("user")]
    public class UserController : BaseController
    {
        private const string ConnString = "Server=localhost;Database=OrisDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;";

        [Get("list")]
        public async Task<ActionResult> List()
        {
            var orm = new OrmContext(ConnString);
            IReadOnlyList<User> users = await orm.GetAllAsync<User>("Users");

            var renderer = new HtmlTemplateRenderer();
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "User", "List.html");

            string html = renderer.RenderFromFile(templatePath, new { Users = users });
            return new HtmlResult(html);
        }

        [Get("profile")]
        [Authorize]
        public ActionResult Profile()
        {
            return Html("<h1>Личный кабинет</h1>");
        }
    }
}