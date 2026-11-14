using System;
using System.Collections.Generic;
using System.Linq;
using CustomHttpServer.Models;

namespace CustomHttpServer.Services
{
    public static class NoteRepository
    {
        private static readonly List<Note> notes = new();
        private static int nextId = 1;

        public static IEnumerable<Note> GetAll() => notes;

        public static Note? GetById(int id) => notes.FirstOrDefault(n => n.Id == id);

        public static Note Create(string title, string content)
        {
            var note = new Note { Id = nextId++, Title = title, Content = content, Status = "normal" };
            notes.Add(note);
            return note;
        }

        public static bool Update(int id, string title, string content, string status, out Note? updatedNote)
        {
            var note = GetById(id);
            if (note == null) { updatedNote = null; return false; }

            note.Title = title;
            note.Content = content;
            note.Status = status;
            updatedNote = note;
            return true;
        }

        public static string? ValidateTitle(string title) =>
            (title.Length >= 3 && title.Length <= 60) ? null : "Название должно быть от 3 до 60 символов.";

        public static string? ValidateContent(string content) =>
            (content.Length >= 10 && content.Length <= 500) ? null : "Содержание должно быть от 10 до 500 символов.";

        public static string? ValidateStatus(string status) =>
            (status == "normal" || status == "pinned") ? null : "Статус должен быть normal или pinned.";
    }
}