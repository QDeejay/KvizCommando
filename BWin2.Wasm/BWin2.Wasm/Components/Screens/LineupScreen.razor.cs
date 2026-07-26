using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;
using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BWin2.Wasm.Components.Screens;

public partial class LineupScreen
{
    [Inject] private IGameSession Game { get; set; } = default!;

    private ElementReference _host;
    private int _teamSlot;
    private bool _showScorers;

    protected override void OnInitialized()
    {
        _teamSlot = Game.State.ChampionBetTeamSlot;
    }

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
        else if (args.Key == "ArrowUp")
            _teamSlot = Math.Max(_teamSlot - 1, 1);
        else if (args.Key == "ArrowDown")
        {
            _teamSlot = Math.Min(
                _teamSlot + 1,
                GameRules.FirstDivisionTeamCount);
        }
        else if (args.Key == "Enter")
            _showScorers = !_showScorers;
    }

    private void SelectTeam(int teamSlot)
    {
        _teamSlot = teamSlot;
        _showScorers = false;
    }

    private void SetView(bool scorers)
    {
        _showScorers = scorers;
    }
}
