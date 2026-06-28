using Blazored.LocalStorage;
using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Layout;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Home;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.IO;
using static System.Net.WebRequestMethods;
namespace KvizCommando.Client.Pages.Home;

public partial class Home : KcComponentBase, IDisposable
{
  
    [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
    [Inject] private IHomeState _homeState { get; set; } = default!;
    [Inject] private HttpClient _http { get; set; } = default!;
   
    private Dictionary<string, ContentBoxVm>? _boxesDict = new Dictionary<string, ContentBoxVm>();

    private const string BOX_SIZE_MINIMAL = "minimalized";
    private const string BOX_SIZE_LARGE = "large";
    private string _bBoardSize = string.Empty;
    private string[] _boxOrder = Array.Empty<string>();
    private bool _isReady = false;
    private bool _isLoaded = false;
    private MarkupString _bBoardHTML = new();
    private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    private ContentBoxVm Box(string orx) => _boxesDict![orx];
    private void BuildBoxes()
    {
        var boxes = new Dictionary<string, ContentBoxVm>();
        boxes = HomeButtonsBuilder.Build(_homeState.HomeScreen!, Ui.Lang);
        _boxOrder = HomeButtonsBuilder.BtnOrder;
        foreach (var box in boxes)
        {
            _boxesDict![box.Key] = box.Value;
        }
        _boxesDict![HomeBoxKey.InfoBoard.ToString()].Size = _bBoardSize;
        _isReady = _isLoaded;
    }
    private void OnBoxClick(int boxId)
    {
        Console.WriteLine($"Box {boxId} kattintva.");
        switch (boxId)
        {
            case 0: 
                break;
            case 1: Ui.Nav.NavigateTo("/question");
                break;
            case 2: Ui.Nav.NavigateTo("/team");
                break;
            case 3: Ui.Nav.NavigateTo("/vsgame");
                break;
            case 4: Ui.Nav.NavigateTo("/sologame");
                break;
        }
    }
    private async Task CloseBBoard()
    {
        Console.WriteLine("faxom1");
        
        await _localStorage.SetItemAsync("B.B", DateTime.UtcNow);
       
    }
    protected override async Task OnInitializedAsync()
    {
        await _homeState.EnsureLoadedAsync();
        string url = $"/BulletinBoard/{culture}/bb.html";
        var lastBB = await _localStorage.GetItemAsync<DateTime>("B.B");
        _bBoardHTML = new MarkupString(await _http.GetStringAsync(url));

        if (lastBB.ToUniversalTime() < _homeState.ExtendedInfo!.LastInfo)
            _bBoardSize = BOX_SIZE_LARGE;
        else
            _bBoardSize = BOX_SIZE_MINIMAL;
       
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Home"],0);
        Ui.Header.SetRank (_homeState.UserMainData!.RankEnum);
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
    private void UpdateBckClick()
    {
        Console.WriteLine($"Navigate to: kurva anyád");
        //_isReady = false;
        //InvokeAsync(StateHasChanged);
    }
    private void UpdateLedDisplay()
    {
        var main = _homeState.UserMainData!;
        var next = _homeState.ExtendedInfo!.NextXp;
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
}

/*
 .home-selector {
    
}
.question-selector {
    width: 440px;
}
.solo-selector {
    width: 860px;
}
.solo-selector,
.question-selector,

    // [Inject] private ILanguageService Lang { get; set; } = default!;
    //[Inject] private NavigationManager Nav { get; set; } = default;
    //[Inject] private PageHeaderService Header { get; set; } = default!;
    //[Inject] private IDisplayMessageState Display { get; set; } = default!;
 */