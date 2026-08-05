using KvizCommando.Client.Features.Shared.Modal.Builders;
using KvizCommando.Client.Features.Shared.Modal.Components;
using KvizCommando.Client.Features.VsGame.Builders;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame;

public partial class VsGame : KcComponentBase, IDisposable
{
    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    private readonly Dictionary<string, ContentBoxVm> _boxes = [];

    private string[] _boxOrder = [];
    private int _classificationId;
    private bool _isReady;
    private bool _requiresQuitConfirmation;
    private int _newTeamLevel;

    private string Culture => AppStates.Culture;
    private VsGameDtos VsData => AppStates.VsGame!;

    protected override void OnInitialized()
    {
        Ui.Header.OnBackBtnClicked += HandleBack;
        Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.GameVs"], 3);
        _boxOrder = VsBoxBuilder.Root;
    }

    private ContentBoxVm Box(string key) => _boxes[key];

    private void BuildBoxes()
    {
        var parameters = new VsComponentParameters
        {
            OnTeamSaved = RefreshRankedAsync,
            OnQuitConfirmationChanged =
                EventCallback.Factory.Create<bool>(
                    this,
                    SetQuitConfirmation),
            OnTeamLevelChanged =
                EventCallback.Factory.Create<int>(
                    this,
                    SetNewTeamLevel),
            ClassificationId = _classificationId
        };

        foreach (var box in VsBoxBuilder.BuildBoxes(
                     VsData,
                     parameters,
                     Ui.Lang))
        {
            _boxes[box.Key] = box.Value;
        }

        _isReady = true;
    }

    private void OnBoxClick(int boxId)
    {
        _boxOrder = VsBoxBuilder.Root;
        var headerTitle =
            Ui.Lang["mainlayout.Header.GameVs"];

        if (boxId == 303)
        {
            _boxOrder = VsBoxBuilder.Ranked;
            headerTitle = _boxes[
                VsBoxKeyRoot.RtBtnRankedBattlefields
                    .ToString()].Header;
        }
        else if (boxId is >= 311 and <= 315)
        {
            _newTeamLevel = 0;
            _classificationId = boxId - 310;
            BuildBoxes();
            _boxOrder = VsBoxBuilder.Match;
            headerTitle = _boxes[
                $"{VsBoxKeyRanked.Classification}" +
                $"{_classificationId}"].Header;
        }

        Ui.Header.SetTitle(headerTitle, boxId);
        Ui.Header.SetBackBtnEna(boxId != 3);
        StateHasChanged();
    }

    private Task RefreshRankedAsync()
    {
        BuildBoxes();
        OnBoxClick(303);
        return Task.CompletedTask;
    }

    private async void HandleBack()
    {
        if (Ui.Header.PageIndex is >= 311 and <= 315)
        {
            if (_requiresQuitConfirmation)
            {
                var modal = MBoxBuilder.BuildParam(
                    ModalTypes.DialogConfirm,
                    Ui.Lang);

                modal.BodyParameters.Add(
                    nameof(DBoxModalRender.DialogBoxType),
                    DBoxConfirmTypes.VsGameQuitConfirm);

                if (await Ui.Modal.ShowAsync(modal) !=
                    ModalResult.Button1)
                {
                    return;
                }
            }

            if (_newTeamLevel > 0)
            {
                await Ui.Lang.LoadModuleAsync(Culture, "team");

                var modal = MBoxBuilder.BuildParam(
                    ModalTypes.TPromoteTeam,
                    Ui.Lang);

                modal.BodyParameters.Add(
                    nameof(TModalRender.AchievedTeamLevel),
                    _newTeamLevel);

                await Ui.Modal.ShowAsync(modal);
                _newTeamLevel = 0;
            }

            _requiresQuitConfirmation = false;
            BuildBoxes();
            OnBoxClick(303);
            return;
        }

        if (Ui.Header.PageIndex == 3)
        {
            Ui.Nav.NavigateTo("/home");
            return;
        }

        BuildBoxes();
        OnBoxClick(3);
    }

    private void SetQuitConfirmation(bool value) =>
        _requiresQuitConfirmation = value;

    private void SetNewTeamLevel(int value) =>
        _newTeamLevel = value;

    public void Dispose()
    {
        Ui.Header.OnBackBtnClicked -= HandleBack;
        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: a VS lap többé nem tulajdonosa a SignalR-kapcsolatnak és
 * nem közvetít match lock- vagy hibacallbacket. A manager jelzi, hogy
 * az aktuális fázis igényel-e kilépési megerősítést, illetve a reward
 * során ténylegesen létrejött-e csapatszintlépés;
 * queue-ban és befejezett meccsnél a visszalépés közvetlen.
 * Visszalépéskor
 * eltávolítja a DynamicComponentet; annak DisposeAsync metódusa
 * végzi a hivatalos queue-kilépést és kapcsolatlezárást.
 *
 * A fájl a VS menü dobozsorrendjét és navigációs állapotát kezeli.
 * Befejezett meccs után szintlépéskor a meglévő csapat-előléptetési
 * modalt jeleníti meg a visszalépés folytatása előtt.
 */
