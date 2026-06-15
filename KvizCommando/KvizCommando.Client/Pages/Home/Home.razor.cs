using Blazored.LocalStorage;
using KvizCommando.Client.Features.Home;
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


    private readonly string Minimal = "minimalized";
    private readonly string large = "large";
    private string bboardSize = string.Empty;
    private string[] BtnOrder = new[] 
    { "InfoBoard",
      "Question", "Shop", 
      "GameVs", "GameSolo",
      "Team", "Rankings", 
      "Statistic", "Events",
      "Community", "Messages" };

    private MarkupString BboardHTML = new();
    private bool _isReady;
    private List<ContentBoxVm>? _boxes;

    private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    private ContentBoxVm Box(string orx)
    {
        var field = typeof(HomeBtnIndex).GetField(orx)!; 
        int idx= (int)(field.GetValue(null) ?? 0);
        return _boxes![idx];
    }
    private void BuildButtons()
    {
        _boxes = HomeButtonsBuilder.Build(HomeState.HomeScreen!, Lang); 
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
        string url = $"/BulletinBoard/{culture}/bb.html";
        var lastBB = await LocalStorage.GetItemAsync<DateTime>("B.B");
        BboardHTML = new MarkupString(await Http.GetStringAsync(url));
        if ( lastBB.ToUniversalTime() < HomeState.ExtendedInfo.LastInfo)
        {
            bboardSize = large;  
        }
        else
        { 
            bboardSize = Minimal;
        }
        PageTitle.SetTitle(Lang["mainlayout.Header.Home"], 0);
        _isReady = true;
        await Loader.Hide();
        
     
    }
}

