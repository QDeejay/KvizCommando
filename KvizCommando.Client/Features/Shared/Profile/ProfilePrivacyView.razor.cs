using KvizCommando.Client.Services.Audio;
using KvizCommando.Shared.Contracts.Profile;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

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
    [Inject] private AudioService Audio { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public ProfilePrivacySection Section { get; set; }
    [Parameter] public EventCallback OnReturnToRoot { get; set; }

    private MarkupString _documentMarkup;
    private string _fullHtml = string.Empty;
    private string _exportPassword = string.Empty;
    private string _exportError = string.Empty;
    private bool _isLoading;
    private bool _loadFailed;
    private bool _isExportAuthorizationOpen;
    private bool _isExporting;
    private bool _showExportPassword;
    private ProfilePrivacySection? _loadedSection;

    private bool CanExport =>
        !_isExporting &&
        !string.IsNullOrWhiteSpace(_exportPassword);

    private string ExportPasswordType =>
        _showExportPassword ? "text" : "password";

    private string ExportPasswordEyeIcon =>
        _showExportPassword ? "bi bi-eye-slash" : "bi bi-eye";

    protected override async Task OnParametersSetAsync()
    {
        if (Section == ProfilePrivacySection.Root)
            return;

        ClearExport();

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

    private async Task OpenExportAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isExportAuthorizationOpen = true;
    }

    private async Task CancelExportAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        ClearExport();
    }

    private async Task ToggleExportPasswordAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _showExportPassword = !_showExportPassword;
    }

    private void OnExportPasswordInput(ChangeEventArgs args)
    {
        _exportPassword = args.Value?.ToString() ?? string.Empty;
        _exportError = string.Empty;
        if (string.IsNullOrEmpty(_exportPassword))
            _showExportPassword = false;
    }

    private async Task ExportDataAsync()
    {
        if (!CanExport)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isExporting = true;
        _exportError = string.Empty;

        try
        {
            var result = await ProfileClient.ExportDataAsync(_exportPassword);
            if (result.State == ProfileDataExportState.Success)
            {
                await DownloadAsync(result);
                ClearExport();
                Ui.Toast.Success(Ui.Lang["profile.Privacy.Export.Success"]);
                return;
            }

            _exportError = Ui.Lang[result.State switch
            {
                ProfileDataExportState.InvalidPassword =>
                    "profile.Privacy.Export.InvalidPassword",
                ProfileDataExportState.RateLimited =>
                    "profile.Privacy.Export.RateLimited",
                _ => "profile.Privacy.Export.Error"
            }];
        }
        catch (JSException)
        {
            _exportError = Ui.Lang["profile.Privacy.Export.Error"];
        }
        finally
        {
            _isExporting = false;
        }
    }

    private async Task DownloadAsync(ProfileDataExportResult result)
    {
        await using var module = await JS.InvokeAsync<IJSObjectReference>(
            "import",
            "./js/profileDataDownload.js");
        using var stream = new MemoryStream(result.Content);
        using var streamReference = new DotNetStreamReference(stream);
        await module.InvokeVoidAsync(
            "downloadFromStream",
            result.FileName,
            streamReference);
    }

    private void ClearExport()
    {
        _exportPassword = string.Empty;
        _exportError = string.Empty;
        _isExportAuthorizationOpen = false;
        _isExporting = false;
        _showExportPassword = false;
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
