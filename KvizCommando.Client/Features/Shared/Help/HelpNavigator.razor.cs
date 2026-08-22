using Blazored.LocalStorage;
using KvizCommando.Client.Features.Home.Builders;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Help;

public partial class HelpNavigator : KcComponentBase, IDisposable
{
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Inject]
    private AudioService Audio { get; set; } = default!;

    [Parameter]
    public AppState AppStates { get; set; } = default!;

    private static readonly HomeBoxKey[] NavigationRoots =
        Enum.GetValues<HomeBoxKey>()[1..];

    private readonly HashSet<HomeBoxKey> _expandedRoots = [];
    private MarkupString[] _pages = [];
    private int _currentPage;
    private int? _selectedHelpKey;
    private string _helpTitle = string.Empty;
    private string _backgroundImage = string.Empty;
    private bool _showHelp;
    private bool _showNavigation;
    private bool _navigationOpen;
    private bool _isLandingPage;
    private bool _isAutomatic;

    private string WindowTitle =>
        $"{Ui.Lang["mainlayout.Help.Title"]} - {_helpTitle}";
    private string NavigationLabel =>
        Ui.Lang["mainlayout.Help.Navigation"];
    private string PageLabel => Ui.Lang["mainlayout.Help.Page"];
    private string PreviousLabel =>
        Ui.Lang["mainlayout.Help.Button.Previous"];
    private string NextLabel => Ui.Lang["mainlayout.Help.Button.Next"];
    private string CloseLabel => Ui.Lang["mainlayout.Help.Button.Close"];
    private string BackgroundStyle =>
        $"--kc-help-background-image: url('{_backgroundImage}');";

    protected override void OnInitialized()
    {
        Ui.Header.OnTitleChanged += OnPageChanged;
    }

    /// <summary>
    /// Automatikusan megjeleníti az aktuális oldal még nem látott súgóját.
    /// </summary>
    public async Task ShowAutomaticAsync()
    {
        if (_showHelp)
            return;

        var helpKey = HelpCollection.ResolvePackageKey(
            Ui.Header.PageIndex);

        if (!helpKey.HasValue ||
            AppStates.LocStoreStates.SeenHelps.Contains(helpKey.Value))
        {
            return;
        }

        await LoadPackageAsync(
            helpKey.Value,
            showNavigation: false,
            isAutomatic: true);
    }

    /// <summary>
    /// Felhasználói kérésre megnyitja a súgó kezdőoldalát.
    /// </summary>
    public Task ShowManualAsync() => LoadLandingPageAsync();

    /// <summary>
    /// Megjeleníti az aktuális oldalhoz tartozó súgót.
    /// </summary>
    public async Task ShowCurrentAsync()
    {
        if (_showHelp)
        {
            _isAutomatic = false;

            if (!_showNavigation)
            {
                _showNavigation = true;
                _navigationOpen = false;

                if (_selectedHelpKey is int selectedHelpKey)
                    _expandedRoots.Add(HelpCollection.Packages[selectedHelpKey].Root);

                StateHasChanged();
            }

            return;
        }

        var helpKey = HelpCollection.ResolvePackageKey(
            Ui.Header.PageIndex);

        if (helpKey.HasValue)
        {
            await LoadPackageAsync(
                helpKey.Value,
                showNavigation: true,
                isAutomatic: false);
        }
        else
        {
            await LoadLandingPageAsync();
        }
    }

    private async Task LoadPackageAsync(
        int helpKey,
        bool showNavigation,
        bool isAutomatic)
    {
        var package = HelpCollection.Packages[helpKey];
        var tokens = package.BuildTokens(AppStates);
        MarkupString[] pages;

        try
        {
            pages = await Task.WhenAll(package.Paths.Select(async path =>
            {
                var markup = await MarkupLoader.LoadingHtmlAsync(
                    $"/Help/{AppStates.Culture}/{path}");

                return new MarkupString(HelpMarkupReplacer.Replace(
                    markup.Value,
                    tokens));
            }));
        }
        catch (HttpRequestException exception)
        {
            Console.Error.WriteLine(
                $"Help content could not be loaded for page {helpKey}: " +
                exception.Message);
            return;
        }

        var isNewHelp = AppStates.LocStoreStates.SeenHelps.Add(helpKey);

        _pages = pages;
        _currentPage = 0;
        _selectedHelpKey = helpKey;
        _helpTitle = Ui.Lang[package.TitleKey];
        _backgroundImage = package.BackgroundImage;
        _showNavigation = showNavigation;
        _navigationOpen = false;
        _isLandingPage = false;
        _isAutomatic = isAutomatic;
        _showHelp = true;
        _expandedRoots.Clear();

        if (showNavigation)
            _expandedRoots.Add(package.Root);

        StateHasChanged();

        if (isNewHelp)
        {
            await LocalStorage.SetItemAsync(
                HelpCollection.SEEN_STORAGE_KEY,
                AppStates.LocStoreStates.SeenHelps);
        }
    }

    private async Task LoadLandingPageAsync()
    {
        try
        {
            _pages =
            [
                await MarkupLoader.LoadingHtmlAsync(
                    $"/Help/{AppStates.Culture}/{HelpCollection.LANDING_PATH}")
            ];
        }
        catch (HttpRequestException exception)
        {
            Console.Error.WriteLine(
                "Help landing page could not be loaded: " +
                exception.Message);
            return;
        }

        _currentPage = 0;
        _selectedHelpKey = null;
        _helpTitle = Ui.Lang["mainlayout.Help.Landing.Title"];
        _backgroundImage = HelpCollection.LANDING_BACKGROUND;
        _showNavigation = true;
        _navigationOpen = true;
        _isLandingPage = true;
        _isAutomatic = false;
        _showHelp = true;
        _expandedRoots.Clear();
        StateHasChanged();
    }

    private IEnumerable<KeyValuePair<int, HelpPackage>> GetSeenPackages(
        HomeBoxKey root) =>
        HelpCollection.Packages.Where(package =>
            package.Value.Root == root &&
            AppStates.LocStoreStates.SeenHelps.Contains(package.Key));

    private async Task ToggleNavigation()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _navigationOpen = !_navigationOpen;
    }

    private async Task ToggleRoot(HomeBoxKey root)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

        if (!_expandedRoots.Add(root))
            _expandedRoots.Remove(root);
    }

    private async Task SelectPackageAsync(int helpKey)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        await LoadPackageAsync(helpKey, showNavigation: true, isAutomatic: false);
        _navigationOpen = false;
    }

    private int? GetNextSeenHelpKey()
    {
        var currentFound = !_selectedHelpKey.HasValue;

        foreach (var package in HelpCollection.Packages)
        {
            if (!AppStates.LocStoreStates.SeenHelps.Contains(package.Key))
                continue;

            if (currentFound)
                return package.Key;

            if (package.Key == _selectedHelpKey)
                currentFound = true;
        }

        return null;
    }

    private async Task NextPackageAsync()
    {
        var nextHelpKey = GetNextSeenHelpKey();

        if (nextHelpKey.HasValue)
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            await LoadPackageAsync(nextHelpKey.Value, showNavigation: true, isAutomatic: false);
        }
    }

    private void OnPageChanged()
    {
        _ = InvokeAsync(ShowAutomaticAsync);
    }

    private async Task PreviousPage()
    {
        if (_currentPage > 0)
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            _currentPage--;
        }
    }

    private async Task NextPage()
    {
        if (_currentPage < _pages.Length - 1)
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            _currentPage++;
        }
    }

    private async Task GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < _pages.Length)
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
            _currentPage = pageIndex;
        }
    }

    private async Task Close()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _showHelp = false;
        _showNavigation = false;
        _navigationOpen = false;
        _isLandingPage = false;
        _isAutomatic = false;
        _currentPage = 0;
        _selectedHelpKey = null;
        _helpTitle = string.Empty;
        _backgroundImage = string.Empty;
        _expandedRoots.Clear();
        _pages = [];
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Ui.Header.OnTitleChanged -= OnPageChanged;
        GC.SuppressFinalize(this);
    }
}
