using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchService
{
    Task CreateLockedMatchAsync(
        IReadOnlyList<VsRankedQueueEntry> entries,
        CancellationToken ct = default);

    Task SelectCharacterAsync(
        string connectionId,
        int slotNumber,
        CancellationToken ct = default);

    Task AssignLoadoutAsync(
        string connectionId,
        VsLoadoutAssignmentRequest request,
        CancellationToken ct = default);

    Task AssignHelpAsync(
        string connectionId,
        VsHelpAssignmentRequest request,
        CancellationToken ct = default);

    Task ResetPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    Task FinishPreparationAsync(
        string connectionId,
        CancellationToken ct = default);

    Task DisconnectAsync(
        string connectionId,
        CancellationToken ct = default);
}

/**
 * A lezárt VS meccs létrehozásának és a preparációs parancsoknak
 * a szerveroldali szerződése.
 */
