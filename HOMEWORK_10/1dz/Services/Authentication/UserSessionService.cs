using System;
using System.Collections.Concurrent;
using CustomHttpServer.Framework.Authentication;

namespace CustomHttpServer.Services.Authentication
{
    public sealed class UserSessionService : IUserSessionService
    {
        private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();
        private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(30);

        public string CreateSession(int userId)
        {
            string sessionId = Guid.NewGuid().ToString("N");
            var session = new SessionInfo(userId, DateTimeOffset.UtcNow.Add(SessionLifetime));

            if (!_sessions.TryAdd(sessionId, session))
                throw new InvalidOperationException("Не удалось создать уникальный сеанс.");

            return sessionId;
        }

        public int? GetUserId(string? sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return null;

            if (!_sessions.TryGetValue(sessionId, out SessionInfo? session)) return null;

            if (session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _sessions.TryRemove(sessionId, out _);
                return null;
            }

            return session.UserId;
        }

        public void RemoveSession(string? sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) return;
            _sessions.TryRemove(sessionId, out _);
        }

        private sealed record SessionInfo(int UserId, DateTimeOffset ExpiresAt);
    }
}