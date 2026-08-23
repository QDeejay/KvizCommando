using KvizCommando.Shared.Contracts.Auth;
using Blazored.SessionStorage;
using System.Net.Http.Json;

namespace KvizCommando.Client.Services;

public class IdentityRulesService
{
    private readonly HttpClient _http;
    private readonly ISessionStorageService _sessionStorage;

    private const string STORAGE_KEY = "identity-options";
    private RegisterOptionsResponse? _rules;

    public IdentityRulesService(
        HttpClient http,
        ISessionStorageService sessionStorage)
    {
        _http = http;
        _sessionStorage = sessionStorage;
    }

    /// <summary>
    /// Lekéri az Identity aktuális regisztrációs szabályait.
    /// </summary>
    public async Task<RegisterOptionsResponse> GetRulesAsync()
    {
        if (_rules != null)
            return _rules;

        // A sessionStorage böngészőfrissítés után is megőrzi az egyszer már lekért szabályokat.
        var stored = await _sessionStorage.GetItemAsync<RegisterOptionsResponse>(
            STORAGE_KEY);
        if (stored is not null)
        {
            _rules = stored;
            return _rules;
        }

        var rules = await _http.GetFromJsonAsync<RegisterOptionsResponse>("api/auth/options");
        if (rules == null)
            throw new InvalidOperationException("Nem sikerült lekérni az IdentityOptions beállításokat.");

        await _sessionStorage.SetItemAsync(STORAGE_KEY, rules);

        _rules = rules;
        return _rules;
    }
}
