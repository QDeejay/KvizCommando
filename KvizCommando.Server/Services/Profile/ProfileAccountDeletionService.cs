using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Application.Security;
using KvizCommando.Server.Identity;
using KvizCommando.Server.Infrastructure.Email;
using KvizCommando.Server.Infrastructure.Persistence;
using KvizCommando.Server.Services.Players;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KvizCommando.Server.Services.Profile;

public sealed class ProfileAccountDeletionService : IProfileAccountDeletionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _applicationDb;
    private readonly GameDbContext _gameDb;
    private readonly IRegistrationBenefitClaimService _benefitClaims;
    private readonly IPlayerService _players;
    private readonly IAccountNotificationSender _notifications;
    private readonly ILogger<ProfileAccountDeletionService> _logger;

    public ProfileAccountDeletionService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDb,
        GameDbContext gameDb,
        IRegistrationBenefitClaimService benefitClaims,
        IPlayerService players,
        IAccountNotificationSender notifications,
        ILogger<ProfileAccountDeletionService> logger)
    {
        _userManager = userManager;
        _applicationDb = applicationDb;
        _gameDb = gameDb;
        _benefitClaims = benefitClaims;
        _players = players;
        _notifications = notifications;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProfileAccountDeletionServiceState> DeleteAsync(
        string userId,
        string currentPassword,
        CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return ProfileAccountDeletionServiceState.NotFound;

        if (!await _userManager.CheckPasswordAsync(user, currentPassword))
            return ProfileAccountDeletionServiceState.InvalidPassword;

        try
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                await _benefitClaims.RecordAsync(
                    user.Email,
                    DateTime.UtcNow.AddDays(
                        RegistrationBenefitRules.BENEFIT_BLOCK_DAYS),
                    ct);
            }

            var player = await _applicationDb.Players
                .Where(player => player.UserId == userId)
                .Select(player => new
                {
                    player.PlayerId,
                    player.RankEnum
                })
                .SingleOrDefaultAsync(ct);

            if (player is not null)
            {
                await _players.RemoveForAccountDeletionAsync(
                    userId,
                    player.PlayerId,
                    ct);
                await DeleteQuestionDataAsync(player.PlayerId, ct);
            }

            await DeleteAccountDataAsync(user, player?.PlayerId, ct);
            await TrySendAccountDeletedAsync(user, player?.RankEnum ?? 0);
            return ProfileAccountDeletionServiceState.Success;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Account deletion failed. UserId={UserId}",
                userId);
            return ProfileAccountDeletionServiceState.ServerError;
        }
    }

    private async Task TrySendAccountDeletedAsync(
        ApplicationUser user,
        int rankEnum)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
            return;

        try
        {
            await _notifications.SendAccountDeletedAsync(
                user,
                rankEnum,
                CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Account deletion notification failed. UserId={UserId}",
                user.Id);
        }
    }

    private async Task DeleteQuestionDataAsync(
        int playerId,
        CancellationToken ct)
    {
        await using var transaction = await _gameDb.Database.BeginTransactionAsync(ct);
        await _gameDb.PendingQuestions
            .Where(question => question.PlayerId == playerId)
            .ExecuteDeleteAsync(ct);
        await _gameDb.UserQuestions
            .Where(question => question.PlayerId == playerId)
            .ExecuteDeleteAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private async Task DeleteAccountDataAsync(
        ApplicationUser user,
        int? playerId,
        CancellationToken ct)
    {
        await using var transaction = await _applicationDb.Database.BeginTransactionAsync(ct);

        if (playerId.HasValue)
        {
            await _applicationDb.PlayerCharacters
                .Where(value => value.PlayerId == playerId.Value)
                .ExecuteDeleteAsync(ct);
            await _applicationDb.PlayerLoadouts
                .Where(value => value.PlayerId == playerId.Value)
                .ExecuteDeleteAsync(ct);
            await _applicationDb.PlayerCategoryStats
                .Where(value => value.PlayerId == playerId.Value)
                .ExecuteDeleteAsync(ct);
            await _applicationDb.PlayerOrientStat
                .Where(value => value.PlayerId == playerId.Value)
                .ExecuteDeleteAsync(ct);
            await _applicationDb.PlayerAskStats
                .Where(value => value.PlayerId == playerId.Value)
                .ExecuteDeleteAsync(ct);
            await _applicationDb.TeamStatistics
                .Where(value => value.PlayerId == playerId.Value)
                .ExecuteDeleteAsync(ct);
            await _applicationDb.Players
                .Where(value => value.PlayerId == playerId.Value)
                .ExecuteDeleteAsync(ct);
        }

        await _applicationDb.UserPaymentMethods
            .Where(value => value.UserId == user.Id)
            .ExecuteDeleteAsync(ct);
        await _applicationDb.MarketingConsents
            .Where(value => value.UserId == user.Id)
            .ExecuteDeleteAsync(ct);
        await _applicationDb.TermsConsents
            .Where(value => value.UserId == user.Id)
            .ExecuteDeleteAsync(ct);
        await _applicationDb.UserPii
            .Where(value => value.UserId == user.Id)
            .ExecuteDeleteAsync(ct);

        var identityResult = await _userManager.DeleteAsync(user);
        if (!identityResult.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    identityResult.Errors.Select(error => error.Code)));
        }

        await transaction.CommitAsync(ct);
    }
}
