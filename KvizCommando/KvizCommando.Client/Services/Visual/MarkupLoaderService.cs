using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Services.Visual
{
    


    public class MarkupLoaderService
    {
        private readonly HttpClient _http;

        public MarkupLoaderService(HttpClient http) 
        { 
            _http = http;
        }
        public async Task<MarkupString> LoadingHtmlAsync(string culture, Html reqHtml)
        {
            var html = new MarkupString(string.Empty);
            string url = reqHtml switch
            {
                Html.Bboard => $"/BulletinBoard/{culture}/bb.html",
                Html.Terms => string.Empty,
                Html.Privacy => string.Empty,
                Html.NewQuestRules => $"/Manuals/{culture}/questionsend.html",
                _ => string.Empty,
            };
            if(string.IsNullOrEmpty(url))
                html = new MarkupString(await _http.GetStringAsync(url));
            return html;
        }
        
    }
    public enum Html
    {
        Bboard,
        Terms,
        Privacy,
        NewQuestRules

    }
}
