using KvizCommando.Shared.Contracts.Auth;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace KvizCommando.Client.Services;

public class IdentityRulesService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private const string STORAGE_KEY = "identity-options";
    private RegisterOptionsResponse? _rules;

    public IdentityRulesService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    /// <summary>
    /// Lekéri az Identity aktuális regisztrációs szabályait.
    /// </summary>
    public async Task<RegisterOptionsResponse> GetRulesAsync()
    {
        if (_rules != null)
            return _rules;

        // A sessionStorage böngészőfrissítés után is megőrzi az egyszer már lekért szabályokat.
        var stored = await _js.InvokeAsync<string?>("sessionStorage.getItem", STORAGE_KEY);
        if (!string.IsNullOrEmpty(stored))
        {
            _rules = JsonSerializer.Deserialize<RegisterOptionsResponse>(stored)!;
            return _rules;
        }

        var rules = await _http.GetFromJsonAsync<RegisterOptionsResponse>("api/auth/options");
        if (rules == null)
            throw new InvalidOperationException("Nem sikerült lekérni az IdentityOptions beállításokat.");

        var json = JsonSerializer.Serialize(rules);
        await _js.InvokeVoidAsync("sessionStorage.setItem", STORAGE_KEY, json);

        _rules = rules;
        return _rules;
    }
}
