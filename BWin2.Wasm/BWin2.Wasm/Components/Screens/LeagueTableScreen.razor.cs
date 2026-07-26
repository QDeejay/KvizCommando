using BWin2.Wasm.Domain;
using BWin2.Wasm.Services;
using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BWin2.Wasm.Components.Screens;

public partial class LeagueTableScreen
{
    [Inject] private IGameSession Game { get; set; } = default!;

    [Inject] private ILeagueService LeagueService { get; set; } = default!;

    private ElementReference _host;

    private IReadOnlyList<Team> Table =>
        LeagueService.GetTable(Game.State);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            await _host.FocusAsync();
    }

    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "ArrowLeft")
            Game.NavigateScreen(-1);
        else if (args.Key == "ArrowRight")
            Game.NavigateScreen(1);
    }

    private string RowClass(Team team, int place)
    {
        var classes = new List<string>();

        if (place == 1)
            classes.Add("leader");
        if (place > 15)
            classes.Add("relegation");
        if (team.Id == Game.State.ChampionBetTeamSlot)
            classes.Add("selected-team-row");

        return string.Join(" ", classes);
    }

    private static string Signed(int value) =>
        value > 0 ? $"+{value}" : value.ToString();

    private static string RecentForm(Team team)
    {
        string history = team.Statistics.ResultHistory;
        return history.Length <= 6 ? history : history[^6..];
    }
}
