using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Components;
using KvizCommando.Client.Services.Visual.UiService;
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
    private enum ProfilePrivacyAction
    {
        None,
        Export,
        Delete
    }

    [Inject] private IProfileClientService ProfileClient { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public ProfilePrivacySection Section { get; set; }
    [Parameter] public EventCallback OnReturnToRoot { get; set; }

    private MarkupString _documentMarkup;
    private string _fullHtml = string.Empty;
    private string _authorizationError = string.Empty;
    private bool _isLoading;
    private bool _loadFailed;
    private bool _isAuthorizationBusy;
    private ProfilePrivacyAction _activeAction;
    private ProfilePrivacySection? _loadedSection;

    protected override async Task OnParametersSetAsync()
    {
        if (Section == ProfilePrivacySection.Root)
            return;

        ClearAuthorization();

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

    private async Task OpenAuthorizationAsync(ProfilePrivacyAction action)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _activeAction = action;
        _authorizationError = string.Empty;
    }

    private Task CloseAuthorizationAsync()
    {
        ClearAuthorization();
        return Task.CompletedTask;
    }

    private void ClearAuthorizationError()
    {
        _authorizationError = string.Empty;
    }

    private Task AuthorizeAsync(string currentPassword)
    {
        return _activeAction switch
        {
            ProfilePrivacyAction.Export => ExportDataAsync(currentPassword),
            ProfilePrivacyAction.Delete => DeleteAccountAsync(currentPassword),
            _ => Task.CompletedTask
        };
    }

    private async Task ExportDataAsync(string currentPassword)
    {
        _isAuthorizationBusy = true;
        _authorizationError = string.Empty;

        try
        {
            var result = await ProfileClient.ExportDataAsync(currentPassword);
            if (result.State == ProfileDataExportState.Success)
            {
                await DownloadAsync(result);
                ClearAuthorization();
                Ui.Toast.Success(Ui.Lang["profile.Privacy.Export.Success"]);
                return;
            }

            _authorizationError = Ui.Lang[result.State switch
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
            _authorizationError = Ui.Lang["profile.Privacy.Export.Error"];
        }
        finally
        {
            _isAuthorizationBusy = false;
        }
    }

    private async Task DeleteAccountAsync(string currentPassword)
    {
        var modal = MBoxBuilder.BuildParam(
            ModalTypes.DialogConfirm,
            Ui.Lang);
        modal.BodyParameters.Add(
            nameof(DBoxModalRender.DialogBoxType),
            DBoxConfirmTypes.AccountDeletionConfirm);

        if (await Ui.Modal.ShowAsync(modal) != ModalResult.Button1)
            return;

        _isAuthorizationBusy = true;
        _authorizationError = string.Empty;

        try
        {
            var state = await UserService.ProfileDeleteAsync(currentPassword);
            if (state == ProfileAccountDeletionState.Success)
                return;

            _authorizationError = Ui.Lang[state switch
            {
                ProfileAccountDeletionState.InvalidPassword =>
                    "profile.Privacy.Delete.InvalidPassword",
                ProfileAccountDeletionState.RateLimited =>
                    "profile.Privacy.Delete.RateLimited",
                _ => "profile.Privacy.Delete.Error"
            }];
        }
        finally
        {
            _isAuthorizationBusy = false;
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

    private void ClearAuthorization()
    {
        _activeAction = ProfilePrivacyAction.None;
        _authorizationError = string.Empty;
        _isAuthorizationBusy = false;
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
