using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace KvizCommando.Server.Services.Security
{
    public class SessionService : ISessionService
    {
        private readonly ConcurrentDictionary<string, string> _sessionKeys = new();

        public string GenerateAndStoreSessionKey(string userId)
        {
            var keyBytes = RandomNumberGenerator.GetBytes(32); // 256 bit
            var key = Convert.ToBase64String(keyBytes);
            _sessionKeys[userId] = key;
            return key;
        }

        public string? GetSessionKey(string userId)
        {
            return _sessionKeys.TryGetValue(userId, out var key) ? key : null;
        }
    }

}