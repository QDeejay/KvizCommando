using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Profile;

public enum ProfilePrivacySection
{
    Root,
    PrivacyPolicy,
    Terms
}

public partial class ProfilePrivacyView
{
    [Inject] private IProfileClientService ProfileClient { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;

    [Parameter] public ProfilePrivacySection Section { get; set; }
    [Parameter] public EventCallback OnReturnToRoot { get; set; }

    private MarkupString _documentMarkup;
    private string _fullHtml = string.Empty;
    private bool _isLoading;
    private bool _loadFailed;
    private ProfilePrivacySection? _loadedSection;

    protected override async Task OnParametersSetAsync()
    {
        if (Section == ProfilePrivacySection.Root)
            return;

        if (_loadedSection == Section)
            return;

        _isLoading = true;
        _loadFailed = false;

        try
        {
            if (string.IsNullOrWhiteSpace(_fullHtml))
            {
                var terms = await ProfileClient.GetLegalMetaAsync();
                if (terms is null || string.IsNullOrWhiteSpace(terms.Url))
                {
                    _loadFailed = true;
                    return;
                }

                _fullHtml = await Http.GetStringAsync(terms.Url);
            }

            var sectionId = Section == ProfilePrivacySection.Terms
                ? "terms"
                : "privacy";
            var sectionHtml = ExtractSection(_fullHtml, sectionId);

            if (string.IsNullOrWhiteSpace(sectionHtml))
            {
                _loadFailed = true;
                return;
            }

            _documentMarkup = new MarkupString(sectionHtml);
            _loadedSection = Section;
        }
        catch (HttpRequestException)
        {
            _loadFailed = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task ReturnToRootAsync()
    {
        await OnReturnToRoot.InvokeAsync();
    }

    private static string ExtractSection(string html, string id)
    {
        var startTag = $"<div id=\"{id}\">";
        const string END_TAG = "</div>";

        var start = html.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
        if (start == -1)
            return string.Empty;

        start += startTag.Length;
        var end = html.IndexOf(END_TAG, start, StringComparison.OrdinalIgnoreCase);

        return end == -1
            ? string.Empty
            : html[start..end];
    }
}
