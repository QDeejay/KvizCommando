using KvizCommando.Client.Features.VsGame.Match.Builders;
using KvizCommando.Client.Features.VsGame.Match.Services;
using KvizCommando.Client.Features.VsGame.Match.ViewModels;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Contracts.VsGame.Match;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.VsGame.Match.Components;

public partial class VsMatchManager : IAsyncDisposable
{
    [Inject]
    private IVsMatchClientService MatchClient { get; set; } = default!;

    [Inject]
    private ILanguageService Lang { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter, EditorRequired]
    public int ClassificationId { get; set; }

    private VsMatchViewBuilder _builder = default!;
    private VsQueueViewData? _queue;
    private VsMatchViewData? _match;
    private string _errorText = string.Empty;
    private readonly CancellationTokenSource _lifetimeCts = new();
    private bool _disposed;

    protected override async Task OnInitializedAsync()
    {
        _builder = new VsMatchViewBuilder(Lang);
        MatchClient.OnChanged += OnMatchClientChanged;

        try
        {
            var result =
                await MatchClient.StartAsync(
                    ClassificationId,
                    _lifetimeCts.Token);

            if (result.IsAccepted)
            {
                BuildViewData();
            }
            else
            {
                _errorText = Lang[result.ErrorKey];
            }
        }
        catch (OperationCanceledException) when (_disposed)
        {
        }
        catch
        {
            _errorText =
                Lang["vsgame.Match.Error.Connection"];
        }
    }

    private void OnMatchClientChanged()
    {
        _ = InvokeAsync(RefreshFromSnapshot);
    }

    private void RefreshFromSnapshot()
    {
        if (_disposed)
            return;

        BuildViewData();
        StateHasChanged();
    }

    private void BuildViewData()
    {
        _queue = MatchClient.QueueSnapshot is null
            ? null
            : _builder.BuildQueue(
                MatchClient.QueueSnapshot);

        _match = MatchClient.MatchSnapshot is null
            ? null
            : _builder.Build(
                MatchClient.MatchSnapshot,
                AppStates.Culture);

        _errorText =
            string.IsNullOrWhiteSpace(
                MatchClient.ErrorMessageKey)
                ? string.Empty
                : Lang[MatchClient.ErrorMessageKey];
    }

    private Task SelectCharacterAsync(int slotNumber) =>
        MatchClient.SelectCharacterAsync(slotNumber);

    private Task AssignLoadoutAsync(
        VsLoadoutAssignmentRequest request) =>
        MatchClient.AssignLoadoutAsync(request);

    private Task AssignHelpAsync(
        VsHelpAssignmentRequest request) =>
        MatchClient.AssignHelpAsync(request);

    private Task ResetPreparationAsync() =>
        MatchClient.ResetPreparationAsync();

    private Task FinishPreparationAsync() =>
        MatchClient.FinishPreparationAsync();

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _lifetimeCts.Cancel();
        MatchClient.OnChanged -= OnMatchClientChanged;

        try
        {
            try
            {
                await MatchClient.LeaveQueueAsync();
            }
            finally
            {
                await MatchClient.StopAsync();
            }
        }
        finally
        {
            _lifetimeCts.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

/**
 * MÓDOSÍTÁS: a manager az egyetlen SignalR-kapcsolat tulajdonosa.
 * Közvetlenül feldolgozza a queue-belépési eredményt és helyben tartja
 * a lokalizált hibaállapotot; match lock- és hibacallbacket nem küld a
 * VS lap, builder vagy spec felé. Dispose során hivatalosan kilép a
 * queue-ból, majd minden esetben lezárja a kapcsolatot. A saját
 * életciklus-token megszakítja a még folyamatban lévő csatlakozást.
 *
 * A VS ranked DynamicComponent életciklusát kezeli, snapshotból view
 * modelleket készít és továbbítja a preparációs parancsokat.
 */
