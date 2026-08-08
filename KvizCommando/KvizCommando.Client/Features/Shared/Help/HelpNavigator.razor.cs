using Blazored.LocalStorage;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Help;

public partial class HelpNavigator : KcComponentBase, IDisposable
{
    [Inject]
    private ILocalStorageService LocalStorage { get; set; } = default!;

    [Parameter]
    public AppState AppStates { get; set; } = default!;

    private MarkupString[] _pages = [];
    private int _currentPage;
    private bool _showHelp;

    private string TitleLabel => Ui.Lang["mainlayout.Help.Title"];
    private string PageLabel => Ui.Lang["mainlayout.Help.Page"];
    private string PreviousLabel =>
        Ui.Lang["mainlayout.Help.Button.Previous"];
    private string NextLabel => Ui.Lang["mainlayout.Help.Button.Next"];
    private string CloseLabel => Ui.Lang["mainlayout.Help.Button.Close"];

    protected override void OnInitialized()
    {
        Ui.Header.OnTitleChanged += OnPageChanged;
    }

    public async Task ShowAsync(bool automatic = false)
    {
        if (_showHelp)
            return;

        var pageIndex = Ui.Header.PageIndex;

        if (!HelpCollection.Pages.TryGetValue(pageIndex, out var paths) ||
            automatic && AppStates.LocStoreStates.SeenHelps.Contains(pageIndex))
        {
            return;
        }

        try
        {
            _pages = await Task.WhenAll(paths.Select(path =>
                MarkupLoader.LoadingHtmlAsync(
                    $"/Help/{AppStates.Culture}/{path}")));
        }
        catch (HttpRequestException exception)
        {
            Console.Error.WriteLine(
                $"Help content could not be loaded for page {pageIndex}: " +
                exception.Message);
            return;
        }

        _currentPage = 0;
        _showHelp = true;
        StateHasChanged();

        if (AppStates.LocStoreStates.SeenHelps.Add(pageIndex))
        {
            await LocalStorage.SetItemAsync(
                HelpCollection.SeenStorageKey,
                AppStates.LocStoreStates.SeenHelps);
        }
    }

    private void OnPageChanged()
    {
        _ = InvokeAsync(() => ShowAsync(automatic: true));
    }

    private void PreviousPage()
    {
        if (_currentPage > 0)
            _currentPage--;
    }

    private void NextPage()
    {
        if (_currentPage < _pages.Length - 1)
            _currentPage++;
    }

    private void GoToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < _pages.Length)
            _currentPage = pageIndex;
    }

    private void Close()
    {
        _showHelp = false;
        _currentPage = 0;
        _pages = [];
    }

    public void Dispose()
    {
        Ui.Header.OnTitleChanged -= OnPageChanged;
        GC.SuppressFinalize(this);
    }
}
