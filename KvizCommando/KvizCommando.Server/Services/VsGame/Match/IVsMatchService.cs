using KvizCommando.Server.Services.VsGame.Matchmaking;
using KvizCommando.Shared.Contracts.VsGame.Match;

namespace KvizCommando.Server.Services.VsGame.Match;

public interface IVsMatchService
{
    VsMatchSession LockMatch(
        IReadOnlyList<VsRankedQueueEntry> entries);

    Task<bool> InitializeLockedMatchAsync(
        VsMatchSession match,
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
 * MÓDOSÍTÁS: a szinkron LockMatch azonnal regisztrálja a kiválasztott
 * játékosokat, az aszinkron inicializálás csak ezután foglal tétet és
 * tölt adatot. Így nincs queue és match store közötti láthatatlan
 * disconnect-időablak.
 *
 * A lezárt VS meccs létrehozásának és a preparációs parancsoknak a
 * szerveroldali szerződése.
 */
