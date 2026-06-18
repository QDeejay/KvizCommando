using Blazored.LocalStorage;
using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Layout;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.Visual;
using Microsoft.AspNetCore.Components;
using System.Globalization;
using System.IO;
using static System.Net.WebRequestMethods;

namespace KvizCommando.Client.Pages.Home;

public partial class Home : ComponentBase
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] public IScreenApiService HomeApi { get; set; } = default!;
    [Inject] public ILocalStorageService LocalStorage { get; set; } = default!;
    [Inject] private ILoadingService Loader { get; set; } = default!;
    [Inject] private IHomeState HomeState { get; set; } = default!;
    [Inject] private IQuestionState QuestionState { get; set; } = default!;
    [Inject] private ITeamState TeamState { get; set; } = default!;
    [Inject] private ISoloState SoloState { get; set; } = default!;


    [Inject] private PageTitleService PageTitle { get; set; } = default!;
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] public IDisplayMessageState DisplayState { get; set; } = default!;


    private readonly string Minimal = "minimalized";
    private readonly string large = "large";
    private string bboardSize = string.Empty;
    private string[] BoxOrder = Array.Empty<string>();
    private string[] BoxOrderHome= Array.Empty<string>();
    internal string[] BoxOrderQuestion = Array.Empty<string>();
    internal string[] BoxOrderSolo = Array.Empty<string>();


    private string ActualPage = "home";
    private MarkupString BboardHTML = new();
    private bool _isReady = false;
    private bool _isLoaded = false;
    private bool _btnReady = false;
    private Dictionary<string, ContentBoxVm>? _boxesDict= new Dictionary<string, ContentBoxVm>();
    private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    private ContentBoxVm Box(string orx) => _boxesDict![orx];
    private void BuildButtons(string page)
    {
        var boxes = new Dictionary<string, ContentBoxVm>();
        switch (page)
        {    
            case "home": boxes = HomeButtonsBuilder.Build(HomeState.HomeScreen!, Lang);
                break;
            case "question": boxes = QBtnBoxBuilder.BuildBoxes(QuestionState.ExtendedInfo!, Lang);
                break;
            case "solo":  boxes = SgameBtnBuilder.BuildBoxes(SoloState.Snapshot!, culture, Lang);
                break;
            default: boxes = HomeButtonsBuilder.Build(HomeState.HomeScreen!, Lang);
                break;
        }
        
        foreach (var box in boxes)
        {
            _boxesDict![box.Key]= box.Value;
        }
        _isReady = true && _isLoaded;
    }
    private void OnBoxClick(int boxId)
    {
        // kattintás kezelése
        Console.WriteLine($"Box {boxId} kattintva.");
        _isReady = false;
        var title = Lang["mainlayout.Header.Home"];
        switch (boxId) 
        {
            case 0: ActualPage = "home";
                BoxOrder = BxOrdHome.Root;
                break;
            case 1: ActualPage = "question";
                title = Lang["mainlayout.Header.Question"];
                BoxOrder = BxOrdQuest.Root;
                break;
            case 2:
                ActualPage = "team";
                title = Lang["mainlayout.Header.Team"];
                //Nav.NavigateTo("/team");
                break;
            case 3:
                ActualPage = "vs";
                //Nav.NavigateTo("/solo");
                break;
            case 4:
                ActualPage = "solo";
                title = Lang["mainlayout.Header.GameSolo"];
                BoxOrder = BxOrdSolo.Root;
                //Nav.NavigateTo("/solo");
                break;
            //Nav.NavigateTo("/question");
            case 401: BoxOrder = BxOrdSolo.Cat;
                title = _boxesDict![SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
                break;
            case 402: BoxOrder= BxOrdSolo.Ori;
                title = _boxesDict![SgameBoxKeyRoot.RtBtnOrient.ToString()].Header;
                break;
            }
        BuildButtons(ActualPage);
        PageTitle.SetTitle(title, boxId, -1);

    }
    private async Task CloseBBoard()
    {
        bboardSize = Minimal;
        await LocalStorage.SetItemAsync("B.B", DateTime.UtcNow);
    }
    protected override async Task OnInitializedAsync()
    {
        await Loader.Show();
        await HomeState.EnsureLoadedAsync();
        await QuestionState.EnsureLoadedAsync();
        await SoloState.EnsureLoadedAsync();
        await TeamState.EnsureLoadedAsync();
        string url = $"/BulletinBoard/{culture}/bb.html";
        var lastBB = await LocalStorage.GetItemAsync<DateTime>("B.B");
        BboardHTML = new MarkupString(await Http.GetStringAsync(url));
        if ( lastBB.ToUniversalTime() < HomeState.ExtendedInfo!.LastInfo)
        {
            bboardSize = large;  
        }
        else
        { 
            bboardSize = Minimal;
        }
        PageTitle.SetTitle(Lang["mainlayout.Header.Home"], 0, HomeState.UserMainData!.RankEnum);
        UpdateLedDisplay();
        await Loader.Hide();
        ActualPage = "home";
        BoxOrder = BxOrdHome.Root;
        _isLoaded = true;
        if (_isReady == false)
            BuildButtons(ActualPage);
    }
    protected override void OnInitialized()
    {
    }

    private void UpdateLedDisplay()
    {
        var main = HomeState.UserMainData!;
        var next = HomeState.ExtendedInfo!.NextXp;
        int level = main.RankEnum;
        string levelStr = RankNameTable.Data[level].PublicLevel ?? "";

        var messages = new List<string>
            {
                Lang["mainlayout.Text.TeamName"].FormatSafe(main.TeamName),
                Lang["mainlayout.Text.TeamLevel"].FormatSafe(levelStr),
                Lang["mainlayout.Text.Xp"].FormatSafe(main.XP),
                Lang["mainlayout.Text.NextLevelXp"].FormatSafe(next),
                Lang["mainlayout.Text.Credit"].FormatSafe(main.Credit),
                Lang["mainlayout.Text.Voucher"].FormatSafe(main.Voucher)
            };
        DisplayState.SetMessages(messages);
    }
}

