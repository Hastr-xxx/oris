using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.ORM;
using System;

namespace MiniHttpServer.ORM.UnitTests
{
    [TestClass]
    public class OrmContextTests
    {
        private const string DummyConnStr = "Server=dummy;Database=dummy;";

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_EmptyConnectionString_ThrowsException()
        {
            var context = new OrmContext("");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GetAllAsync_InvalidTableName_ThrowsException()
        {
            var context = new OrmContext(DummyConnStr);
            await context.GetAllAsync<object>("Users; DROP TABLE Users;--");
        }

        [TestMethod]
        public async Task InsertAsync_InvalidTableName_ThrowsException()
        {
            var context = new OrmContext(DummyConnStr);
            var user = new { Id = 1, Name = "Test" }; 
            try
            {
                await context.InsertAsync("Users 123", user); 
                Assert.Fail("Должно было выброситься исключение");
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual("Недопустимое имя SQL-идентификатора.", ex.Message);
            }
        }
    }
}