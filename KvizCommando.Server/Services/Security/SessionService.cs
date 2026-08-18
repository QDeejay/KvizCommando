using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace KvizCommando.Server.Services.Security
{
    public class SessionService : ISessionService
    {
        private readonly ConcurrentDictionary<string, string> _sessionKeys = new();

        /// <inheritdoc />
        public string GenerateAndStoreSessionKey(string userId)
        {
            var keyBytes = RandomNumberGenerator.GetBytes(32);
            var key = Convert.ToBase64String(keyBytes);
            _sessionKeys[userId] = key;
            return key;
        }

        /// <inheritdoc />
        public string? GetSessionKey(string userId)
        {
            return _sessionKeys.TryGetValue(userId, out var key) ? key : null;
        }
    }

}
