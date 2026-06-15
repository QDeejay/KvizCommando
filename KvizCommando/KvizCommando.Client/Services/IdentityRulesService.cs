using KvizCommando.Shared.Options;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace KvizCommando.Client.Services;

public class IdentityRulesService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;

    private const string StorageKey = "identity-options";
    private RegisterOptionsResponse? _rules; // memória cache

    public IdentityRulesService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public async Task<RegisterOptionsResponse> GetRulesAsync()
    {
        // ha memóriában van
        if (_rules != null)
            return _rules;

        // sessionStorage check
        var stored = await _js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
        if (!string.IsNullOrEmpty(stored))
        {
            _rules = JsonSerializer.Deserialize<RegisterOptionsResponse>(stored)!;
            return _rules;
        }

        // ha nincs: API hívás
        var rules = await _http.GetFromJsonAsync<RegisterOptionsResponse>("api/auth/options");
        if (rules == null)
            throw new InvalidOperationException("Nem sikerült lekérni az IdentityOptions beállításokat.");

        // betesszük storage-ba
        var json = JsonSerializer.Serialize(rules);
        await _js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, json);

        _rules = rules;
        return _rules;
    }
}
