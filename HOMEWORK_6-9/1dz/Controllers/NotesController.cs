using System;
using System.Linq;
using System.Net;
using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Framework.Core;
using CustomHttpServer.Models;
using CustomHttpServer.Services;
using MiniTemplateEngine;

namespace CustomHttpServer.Controllers
{
    [HttpController("notes")]
    public class NotesController : ControllerBase
    {
        private readonly HtmlTemplateRenderer renderer = new();

        [Get("")]
        public ActionResult List([FromQuery] string title)
        {
            var notes = NoteRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(title))
                notes = notes.Where(n => n.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

            var viewModel = notes.Select(n => new
            {
                Id = n.Id,
                Title = WebUtility.HtmlEncode(n.Title),
                ShortContent = WebUtility.HtmlEncode(n.Content.Length > 50 ? n.Content.Substring(0, 50) + "..." : n.Content),
                Status = n.Status
            }).ToList();

            string template = @"
                <html><head><meta charset='utf-8'><title>Заметки</title></head><body>
                <h2>Все заметки</h2>
                <form method='GET' action='/notes'>
                    Поиск по названию: <input type='text' name='title' value='${SearchTerm}'> <button type='submit'>Найти</button>
                </form>
                <hr>
                <ul>
                ${for item in Items}
                    <li>
                        <b>[${item.Status}]</b> <a href='/notes/details?id=${item.Id}'>${item.Title}</a><br>
                        ${item.ShortContent}
                    </li>
                ${endfor}
                </ul>
                <a href='/notes/create-form'>Добавить заметку</a>
                </body></html>";

            return new HtmlResult(renderer.RenderFromString(template, new { Items = viewModel, SearchTerm = WebUtility.HtmlEncode(title ?? "") }));
        }

        [Get("details")]
        public ActionResult Details([FromQuery] string id)
        {
            if (!int.TryParse(id, out int noteId)) return new ErrorResult(400, "Некорректный ID");

            var note = NoteRepository.GetById(noteId);
            if (note == null) return new ErrorResult(404, "Заметка не найдена");

            string template = @"
                <html><head><meta charset='utf-8'></head><body>
                <h2>${Title}</h2>
                <p>Статус: <b>${Status}</b></p>
                <div style='border:1px solid #ccc; padding:10px;'>${Content}</div>
                <br><a href='/notes'>Назад к списку</a>
                </body></html>";

            return new HtmlResult(renderer.RenderFromString(template, new
            {
                Title = WebUtility.HtmlEncode(note.Title),
                Status = note.Status,
                Content = WebUtility.HtmlEncode(note.Content).Replace("\n", "<br>")
            }));
        }

        [Get("create-form")]
        public ActionResult CreateForm()
        {
            string template = @"
                <html><head><meta charset='utf-8'></head><body>
                <h2>Создать заметку</h2>
                <form method='POST' action='/notes/create'>
                    Название: <input type='text' name='title' required minlength='3' maxlength='60'><br><br>
                    Содержание:<br> <textarea name='content' required minlength='10' maxlength='500'></textarea><br><br>
                    <button type='submit'>Сохранить</button>
                </form>
                </body></html>";
            return new HtmlResult(template);
        }

        [Post("create")]
        public ActionResult Create(string title, string content)
        {
            var titleErr = NoteRepository.ValidateTitle(title);
            if (titleErr != null) return new ErrorResult(400, titleErr);

            var contentErr = NoteRepository.ValidateContent(content);
            if (contentErr != null) return new ErrorResult(400, contentErr);

            var note = NoteRepository.Create(title, content);
            return new RedirectResult($"/notes/details?id={note.Id}");
        }

        [Put("update")]
        public ActionResult Update([FromQuery] string id, string title, string content, string status)
        {
            if (!int.TryParse(id, out int noteId)) return new ErrorResult(400, "Некорректный ID");

            var titleErr = NoteRepository.ValidateTitle(title);
            if (titleErr != null) return new ErrorResult(400, titleErr);

            var contentErr = NoteRepository.ValidateContent(content);
            if (contentErr != null) return new ErrorResult(400, contentErr);

            var statusErr = NoteRepository.ValidateStatus(status);
            if (statusErr != null) return new ErrorResult(400, statusErr);

            if (!NoteRepository.Update(noteId, title, content, status, out _))
                return new ErrorResult(404, "Заметка не найдена");

            return new NoContentResult(); 
        }
    }
}