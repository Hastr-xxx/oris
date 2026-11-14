using Microsoft.VisualStudio.TestTools.UnitTesting;
using CustomHttpServer.Services.Authentication;
using CustomHttpServer.Framework.Attributes;
using System.Reflection;

namespace CustomHttpServer.Tests
{
    [TestClass]
    public class AuthTests
    {
        [TestMethod]
        public void CreateSession_ReturnsValidGuid()
        {
            // Arrange
            var service = new UserSessionService();

            // Act
            string sessionId = service.CreateSession(1);

            // Assert
            Assert.IsNotNull(sessionId);
            Assert.AreEqual(32, sessionId.Length); // Guid "N" format length
        }

        [TestMethod]
        public void GetUserId_ValidSession_ReturnsUserId()
        {
            var service = new UserSessionService();
            string sessionId = service.CreateSession(42);

            int? userId = service.GetUserId(sessionId);

            Assert.AreEqual(42, userId);
        }

        [TestMethod]
        public void GetUserId_InvalidSession_ReturnsNull()
        {
            var service = new UserSessionService();
            int? userId = service.GetUserId("invalid_session_id");
            Assert.IsNull(userId);
        }

        [TestMethod]
        public void RemoveSession_DestroysSession()
        {
            var service = new UserSessionService();
            string sessionId = service.CreateSession(1);

            service.RemoveSession(sessionId);
            int? userId = service.GetUserId(sessionId);

            Assert.IsNull(userId);
        }

        [TestMethod]
        public void AuthorizeAttribute_CanBeAppliedToClassAndMethod()
        {
            var attrUsage = typeof(AuthorizeAttribute).GetCustomAttribute<AttributeUsageAttribute>();

            Assert.IsTrue(attrUsage.ValidOn.HasFlag(AttributeTargets.Class));
            Assert.IsTrue(attrUsage.ValidOn.HasFlag(AttributeTargets.Method));
        }
    }
}