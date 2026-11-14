using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using CustomHttpServer.Framework.Attributes;
using CustomHttpServer.Framework.Core;

namespace CustomHttpServer.Controllers
{
    // GET http://127.0.0.1:8888/home-work/send?to=test@ttt.ru
    [HttpController("home-work")]
    public class HomeworkController : ControllerBase
    {
        private const string SmtpHost = "smtp.yandex.ru";
        private const int SmtpPort = 587;
        private const string FromAddress = "bezumovaan@yandex.ru"; 
        private const string FromName = "Безумова Анастасия";
        private const string FromPassword = "nnccuzkrselsdedc"; 

        private const string GroupNumber = "11-541";
        private const string FullName = "Безумова Анастасия Сергеевна";

        private const string ArchiveFileName = "project.zip";

        [Get("send")]
        public string Send([FromQuery] string to)
        {
            string archivePath = FindArchive(ArchiveFileName);

            try
            {
                var from = new MailAddress(FromAddress, FromName);
                var toAddress = new MailAddress(to);

                using var message = new MailMessage(from, toAddress)
                {
                    Subject = $"Домашнее задание — {GroupNumber} — {FullName}",
                    Body =
                        $"<h2>Домашнее задание</h2>" +
                        $"<p><b>Группа:</b> {GroupNumber}</p>" +
                        $"<p><b>ФИО:</b> {FullName}</p>" +
                        $"<p>Архив с проектом во вложении.</p>",
                    IsBodyHtml = true
                };

                var attachment = new Attachment(archivePath);
                message.Attachments.Add(attachment);

                using var smtp = new SmtpClient(SmtpHost, SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(FromAddress, FromPassword)
                };

                smtp.Send(message);

                Console.WriteLine($"Письмо с архивом отправлено на {to}");
                return $"Письмо с архивом ({Path.GetFileName(archivePath)}) отправлено на {to}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки письма: {ex.Message}");
                return $"Ошибка отправки: {ex.Message}";
            }
        }

        private static string FindArchive(string fileName)
        {
            var candidates = new[]
            {
                Path.Combine(Directory.GetCurrentDirectory(), fileName),
                Path.Combine(AppContext.BaseDirectory, fileName),
                Path.Combine(AppContext.BaseDirectory, "..", "..", "..", fileName),
            };

            foreach (var path in candidates)
            {
                string full = Path.GetFullPath(path);
                if (File.Exists(full))
                    return full;
            }

            return null;
        }
    }
}