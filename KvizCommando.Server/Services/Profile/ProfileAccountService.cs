using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Identity;
using KvizCommando.Shared.Contracts.Profile;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Identity;

namespace KvizCommando.Server.Services.Profile;

public sealed class ProfileAccountService : IProfileAccountService
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IUserPiiService _pii;

    public ProfileAccountService(UserManager<ApplicationUser> users, IUserPiiService pii)
    {
        _users = users;
        _pii = pii;
    }

    /// <inheritdoc />
    public async Task<ProfileAccountResponse> GetAsync(string userId, CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return new ProfileAccountResponse { State = ProfileAccountRequestState.NotFound };

        var pii = await _pii.GetProfileAsync(userId, ct);
        return Success(user.Email ?? string.Empty, pii.Phone, pii.BillingName, pii.BillingAddress);
    }

    /// <inheritdoc />
    public async Task<ProfileAccountResponse> SaveAsync(
        string userId,
        SaveProfileAccountRequest request,
        CancellationToken ct = default)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
            return new ProfileAccountResponse
            {
                State = ProfileAccountRequestState.ValidationFailed,
                Errors = errors
            };

        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return new ProfileAccountResponse { State = ProfileAccountRequestState.NotFound };

        await _pii.SetProfileAsync(
            userId,
            request.Phone ?? string.Empty,
            request.BillingName ?? string.Empty,
            request.BillingAddress ?? string.Empty,
            ct);
        return Success(user.Email ?? string.Empty, request.Phone, request.BillingName, request.BillingAddress);
    }

    private static List<string> Validate(SaveProfileAccountRequest request)
    {
        var errors = new List<string>();
        if ((request.Phone ?? string.Empty).Trim().Length > ProfileAccountRules.PHONE_MAX_LENGTH)
            errors.Add("PhoneTooLong");
        if ((request.BillingName ?? string.Empty).Trim().Length > ProfileAccountRules.BILLING_NAME_MAX_LENGTH)
            errors.Add("BillingNameTooLong");
        if ((request.BillingAddress ?? string.Empty).Trim().Length > ProfileAccountRules.BILLING_ADDRESS_MAX_LENGTH)
            errors.Add("BillingAddressTooLong");
        return errors;
    }

    private static ProfileAccountResponse Success(
        string email,
        string? phone,
        string? billingName,
        string? billingAddress) => new()
        {
            State = ProfileAccountRequestState.Success,
            Account = new ProfileAccountDto
            {
                Email = email,
                Phone = phone?.Trim() ?? string.Empty,
                BillingName = billingName?.Trim() ?? string.Empty,
                BillingAddress = billingAddress?.Trim() ?? string.Empty
            }
        };
}
