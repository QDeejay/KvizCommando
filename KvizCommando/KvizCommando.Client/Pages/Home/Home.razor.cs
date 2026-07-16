using Blazored.LocalStorage;
using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Home.Features;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
namespace KvizCommando.Client.Pages.Home;

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
        switch (boxId)
        {
            case 0:
                break;
            case 1:
                Ui.Nav.NavigateTo("/question");
                break;
            case 2:
                Ui.Nav.NavigateTo("/team");
                break;
            case 3:
                Ui.Nav.NavigateTo("/vs-game");
                break;
            case 4:
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
        UpdateLedDisplay();
        _isLoaded = true;
        if (_isReady == false)
            BuildBoxes();
    }
    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += UpdateBckClick;
    }

    private void UpdateLedDisplay()
    {
        var main = HState.UserMainData!;
        var next = HState.ExtendedInfo!.NextXp;
        int level = main.RankEnum;
        string levelStr = RankNameTable.Data[level].PublicLevel ?? "";

        var messages = new List<string>
            {
                Ui.Lang["mainlayout.Text.TeamName"].FormatSafe(main.TeamName),
                Ui.Lang["mainlayout.Text.TeamLevel"].FormatSafe(levelStr),
                Ui.Lang["mainlayout.Text.Xp"].FormatSafe(main.XP),
                Ui.Lang["mainlayout.Text.NextLevelXp"].FormatSafe(next),
                Ui.Lang["mainlayout.Text.Credit"].FormatSafe(main.Credit),
                Ui.Lang["mainlayout.Text.Voucher"].FormatSafe(main.Voucher)
            };
        Ui.HeadDisplay.SetMessages(messages);
    }

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= UpdateBckClick;
        GC.SuppressFinalize(this);
    }

    private void UpdateBckClick()
    {
    }
}
