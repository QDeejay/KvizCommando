using Blazored.LocalStorage;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Home.Features;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Home;

public partial class Home : KcComponentBase, IDisposable
{
    [CascadingParameter]
    private AppState AppState { get; set; } = default!;
    [Inject] private ILocalStorageService LocalStorage { get; set; } = default!;
    private string Culture => AppState.Culture;
    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private HomeDTOs HState => AppState.Home!;

    private const string BOX_SIZE_MINIMAL = "minimalized";
    private const string BOX_SIZE_LARGE = "large";

    private string _bBoardSize = string.Empty;
    private string[] _boxOrder = [];
    private bool _isReady = false;
    private bool _isLoaded = false;
    private MarkupString _bBoardHTML = new();
    private ContentBoxVm Box(string orx) => _boxes![orx];

    private void BuildBoxes()
    {
        var boxes = HomeBoxBuilder.Build(HState.HomeScreen!, Ui.Lang);
        _boxOrder = HomeBoxBuilder.BtnOrder;
        foreach (var box in boxes)
        {
            _boxes[box.Key] = box.Value;
        }
        _boxes[HomeBoxKey.InfoBoard.ToString()].Size = _bBoardSize;
        _isReady = _isLoaded;
    }
    private void OnBoxClick(int boxId)
    {
        Console.WriteLine($"Box {boxId} kattintva.");
        switch ((HomeBoxKey)boxId)
        {
            case HomeBoxKey.InfoBoard:
                break;
            case HomeBoxKey.Question:
                Ui.Nav.NavigateTo("/question");
                break;
            case HomeBoxKey.Team:
                Ui.Nav.NavigateTo("/team");
                break;
            case HomeBoxKey.GameVs:
                Ui.Nav.NavigateTo("/vs-game");
                break;
            case HomeBoxKey.GameSolo:
                Ui.Nav.NavigateTo("/solo-game");
                break;
        }
    }
    private async Task CloseBBoard()
    {
        Console.WriteLine("faxom1");

        await LocalStorage.SetItemAsync("B.B", DateTime.UtcNow);

    }
    protected override async Task OnInitializedAsync()
    {

        var lastBB = AppState.LocStoreStates.LastBboardChk;
        _bBoardHTML = await MarkupLoader.LoadingHtmlAsync(Culture, Html.Bboard);

        if (lastBB.ToUniversalTime() < HState.ExtendedInfo!.LastInfo)
            _bBoardSize = BOX_SIZE_LARGE;
        else
            _bBoardSize = BOX_SIZE_MINIMAL;

        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Home"], 0);
        Ui.Header.SetBackBtnEna(false);
        _isLoaded = true;
        if (_isReady == false)
            BuildBoxes();
    }
    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += UpdateBckClick;
    }

    /// <summary>
    /// Felszabadítja a példány által használt erőforrásokat.
    /// </summary>
    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= UpdateBckClick;
        GC.SuppressFinalize(this);
    }

    private void UpdateBckClick()
    {
    }
}
