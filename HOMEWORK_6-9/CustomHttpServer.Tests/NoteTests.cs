using Microsoft.VisualStudio.TestTools.UnitTesting;
using CustomHttpServer.Services;
using CustomHttpServer.Models;

namespace CustomHttpServer.Tests
{
    [TestClass]
    public class NoteTests
    {
        [TestMethod]
        public void ValidateTitleTooShortReturnsErrorMessage()
        {
            string invalidTitle = "ab"; 

            string? error = NoteRepository.ValidateTitle(invalidTitle);

            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void ValidateTitleValidLengthReturnsNull()
        {
            string validTitle = "Купить хлеб"; 

            string? error = NoteRepository.ValidateTitle(validTitle);

            Assert.IsNull(error);
        }

        [TestMethod]
        public void ValidateTitleTooLongReturnsErrorMessage()
        {
            string longTitle = new string('A', 61);

            string? error = NoteRepository.ValidateTitle(longTitle);

            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void ValidateContentTooShortReturnsErrorMessage()
        {
            string shortContent = "Привет"; 

            string? error = NoteRepository.ValidateContent(shortContent);

            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void ValidateStatusInvalidStatusReturnsErrorMessage()
        {
            string invalidStatus = "deleted"; 

            string? error = NoteRepository.ValidateStatus(invalidStatus);

            Assert.IsNotNull(error);
        }

        [TestMethod]
        public void CreateNoteValidDataSetsDefaultStatusToNormal()
        {
            string title = "Тестовая заметка";
            string content = "Этот контент содержит больше 10 символов.";

            Note newNote = NoteRepository.Create(title, content);

            Assert.AreEqual("normal", newNote.Status);
            Assert.AreEqual(title, newNote.Title);
            Assert.IsTrue(newNote.Id > 0);
        }
    }
}