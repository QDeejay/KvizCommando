
using System.Globalization;
using System.Net.Http.Headers;

public sealed class LanguageHandler : DelegatingHandler
{
   
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        // ha nincs még beállítva, rakjuk rá az aktuális nyelvet
        if (!request.Headers.Contains("Accept-Language"))
        {
            var culture = CultureInfo.CurrentUICulture.Name; // pl. "hu-HU" vagy "en-US"
            
            if (!string.IsNullOrWhiteSpace(culture))
            {
                request.Headers.AcceptLanguage.Clear();
                request.Headers.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse(culture));
            }
        }

        return base.SendAsync(request, ct);
    }
}
