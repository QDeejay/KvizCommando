using Blazored.LocalStorage;
using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Helpers;
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
    [Inject] private PageTitleService PageTitle { get; set; } = default!;
    [Inject] protected HttpClient Http { get; set; } = default!;
    [Inject] public IDisplayMessageState DisplayState { get; set; } = default!;


    private readonly string Minimal = "minimalized";
    private readonly string large = "large";
    private string bboardSize = string.Empty;
    private string[] BtnOrder = Array.Empty<string>();


    private MarkupString BboardHTML = new();
    private bool _isReady;
    private Dictionary<string, ContentBoxVm>? _boxesDict;
    //private List<ContentBoxVm>? _boxes;

    private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    private ContentBoxVm Box(string orx) => _boxesDict![orx];
    
    private void BuildButtons()
    {
        _boxesDict = HomeButtonsBuilder.Build(HomeState.HomeScreen!, Lang);
    }
    private void OnBoxClick(int boxId)
    {
        // kattintás kezelése
        Console.WriteLine($"Box {boxId} kattintva.");
        switch (boxId) 
        {
            case 4: Nav.NavigateTo("/solo");
                break;
            case 10: Nav.NavigateTo("/team");
                break;
            case 11: Nav.NavigateTo("/question");
                break;
        }
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
        

        BtnOrder = Enum.GetNames<HomeBoxKey>();
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
        _isReady = true;
        await Loader.Hide();
        
     
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

