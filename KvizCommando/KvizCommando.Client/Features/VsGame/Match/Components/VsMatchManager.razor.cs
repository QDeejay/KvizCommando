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

    [Parameter]
    public Func<bool, Task>? OnMatchLockChanged { get; set; }

    private VsMatchViewBuilder _builder = default!;
    private VsQueueViewData? _queue;
    private VsMatchViewData? _match;
    private string _errorText = string.Empty;
    private bool _reportedLocked;
    private bool _disposed;

    protected override async Task OnInitializedAsync()
    {
        _builder = new VsMatchViewBuilder(Lang);
        MatchClient.OnChanged += OnMatchClientChanged;

        try
        {
            await MatchClient.StartAsync(ClassificationId);
            BuildViewData();
        }
        catch (Exception ex)
        {
            _errorText =
                Lang["vsgame.Match.Error.Connection"];

            _errorText = ex.Message;
            Console.WriteLine(ex);
        }
    }
    

    private void OnMatchClientChanged()
    {
        _ = InvokeAsync(RefreshFromSnapshotAsync);
    }

    private async Task RefreshFromSnapshotAsync()
    {
        if (_disposed)
            return;

        BuildViewData();

        var isLocked = MatchClient.MatchSnapshot is not null;

        if (_reportedLocked != isLocked)
        {
            _reportedLocked = isLocked;

            if (OnMatchLockChanged is not null)
                await OnMatchLockChanged.Invoke(isLocked);
        }

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
        MatchClient.OnChanged -= OnMatchClientChanged;
        await MatchClient.StopAsync();

        if (_reportedLocked &&
            OnMatchLockChanged is not null)
        {
            await OnMatchLockChanged.Invoke(false);
        }

        GC.SuppressFinalize(this);
    }
}

/**
 * A VS ranked DynamicComponent életciklusát kezeli: felépíti az
 * egyetlen SignalR kapcsolatot, snapshotból view modelleket készít,
 * továbbítja a preparációs parancsokat és jelzi a lapnak a
 * MatchLocked állapotot.
 */
