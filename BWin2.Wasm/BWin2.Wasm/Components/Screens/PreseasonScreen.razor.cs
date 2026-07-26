using BWin2.Wasm.Configuration;
using BWin2.Wasm.Domain;
using BWin2.Wasm.Services;
using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;

namespace BWin2.Wasm.Components.Screens;

public partial class PreseasonScreen
{
    [Inject] private IGameSession Game { get; set; } = default!;

    [Inject] private IOddsService OddsService { get; set; } = default!;

    private BetEventKind _lastKind;
    private int _selectedTeamSlot = 1;
    private int _stake = GameRules.MinimumStake;
    private string _error = string.Empty;

    private bool IsCupWinner =>
        Game.PreseasonBetKind == BetEventKind.CupWinner;

    private string Title =>
        IsCupWinner ? "Bet the cup winner" : "Bet the champion";

    private int MaximumStake =>
        Math.Min(GameRules.MaximumStake, Game.State.Credit);

    private IEnumerable<Team> Teams =>
        Enumerable.Range(
                1,
                IsCupWinner
                    ? GameRules.AllTeamCount
                    : GameRules.FirstDivisionTeamCount)
            .Select(Game.State.TeamAt);

    private int PotentialWin =>
        Qb.Int(Qb.Val(GetOdds(Game.State.TeamAt(_selectedTeamSlot))) * _stake);

    protected override void OnParametersSet()
    {
        if (_lastKind == Game.PreseasonBetKind)
            return;

        _lastKind = Game.PreseasonBetKind;
        _selectedTeamSlot = 1;
        _stake = GameRules.MinimumStake;
        _error = string.Empty;
    }

    private string GetOdds(Team team) =>
        OddsService.CalculatePreseasonOdds(
            Game.State.TeamAt(1),
            team,
            IsCupWinner);

    private void SelectTeam(int teamSlot)
    {
        _selectedTeamSlot = teamSlot;
        _error = string.Empty;
    }

    private void PlaceBet()
    {
        if (!Game.PlacePreseasonBet(_selectedTeamSlot, _stake))
        {
            _error =
                $"A tét {GameRules.MinimumStake:N0} és {MaximumStake:N0} HUF között lehet.";
            return;
        }

        _error = string.Empty;
    }
}
