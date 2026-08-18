namespace KvizCommando.Server.Services.Security
{
    public interface ISessionService
    {
        string GenerateAndStoreSessionKey(string userId);
        string? GetSessionKey(string userId);
    }
}
