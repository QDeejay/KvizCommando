using KvizCommando.Server.Application.Abstractions.Security;
using KvizCommando.Server.Identity;
using KvizCommando.Shared.Contracts.Profile;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

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
        return Success(
            user.Email ?? string.Empty,
            user.PreferredLocale,
            ReadPhone(pii.Phone),
            ReadBillingName(pii.BillingName),
            ReadBillingAddress(pii.BillingAddress));
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

        var phone = Normalize(request.Phone);
        var billingName = Normalize(request.BillingName);
        var billingAddress = Normalize(request.BillingAddress);

        await _pii.SetProfileAsync(
            userId,
            HasValue(phone) ? JsonSerializer.Serialize(phone) : string.Empty,
            HasValue(billingName) ? JsonSerializer.Serialize(billingName) : string.Empty,
            HasValue(billingAddress) ? JsonSerializer.Serialize(billingAddress) : string.Empty,
            ct);
        return Success(
            user.Email ?? string.Empty,
            user.PreferredLocale,
            phone,
            billingName,
            billingAddress);
    }

    /// <inheritdoc />
    public async Task<ProfileAccountResponse> UpdatePreferredLocaleAsync(
        string userId,
        string preferredLocale,
        CancellationToken ct = default)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
            return new ProfileAccountResponse { State = ProfileAccountRequestState.NotFound };

        var normalizedLocale = NormalizeLocale(preferredLocale);
        if (!string.Equals(user.PreferredLocale, normalizedLocale, StringComparison.Ordinal))
        {
            user.PreferredLocale = normalizedLocale;
            var result = await _users.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ProfileAccountResponse
                {
                    State = ProfileAccountRequestState.IdentityError,
                    Errors = result.Errors.Select(error => error.Code).ToList()
                };
            }
        }

        return await GetAsync(userId, ct);
    }

    private static List<string> Validate(SaveProfileAccountRequest request)
    {
        var errors = new List<string>();
        var phone = request.Phone ?? new ProfilePhoneDto();
        var billingName = request.BillingName ?? new BillingNameDto();
        var billingAddress = request.BillingAddress ?? new BillingAddressDto();

        if ((phone.CountryCode ?? string.Empty).Trim().Length >
            ProfileAccountRules.PHONE_COUNTRY_CODE_MAX_LENGTH)
            errors.Add("PhoneCountryCodeTooLong");
        if ((phone.Number ?? string.Empty).Trim().Length >
            ProfileAccountRules.PHONE_NUMBER_MAX_LENGTH)
            errors.Add("PhoneTooLong");
        if ((billingName.LastName ?? string.Empty).Trim().Length >
            ProfileAccountRules.BILLING_NAME_PART_MAX_LENGTH ||
            (billingName.FirstName ?? string.Empty).Trim().Length >
            ProfileAccountRules.BILLING_NAME_PART_MAX_LENGTH)
            errors.Add("BillingNameTooLong");
        if ((billingAddress.PostalCode ?? string.Empty).Trim().Length >
            ProfileAccountRules.BILLING_POSTAL_CODE_MAX_LENGTH)
            errors.Add("BillingPostalCodeTooLong");
        if ((billingAddress.City ?? string.Empty).Trim().Length >
            ProfileAccountRules.BILLING_CITY_MAX_LENGTH)
            errors.Add("BillingCityTooLong");
        if ((billingAddress.AddressLine1 ?? string.Empty).Trim().Length >
            ProfileAccountRules.BILLING_ADDRESS_LINE_MAX_LENGTH ||
            (billingAddress.AddressLine2 ?? string.Empty).Trim().Length >
            ProfileAccountRules.BILLING_ADDRESS_LINE_MAX_LENGTH)
            errors.Add("BillingAddressTooLong");
        return errors;
    }

    private static ProfileAccountResponse Success(
        string email,
        string preferredLocale,
        ProfilePhoneDto phone,
        BillingNameDto billingName,
        BillingAddressDto billingAddress) => new()
        {
            State = ProfileAccountRequestState.Success,
            Account = new ProfileAccountDto
            {
                Email = email,
                PreferredLocale = NormalizeLocale(preferredLocale),
                Phone = Normalize(phone),
                BillingName = Normalize(billingName),
                BillingAddress = Normalize(billingAddress)
            }
        };

    private static string NormalizeLocale(string? locale) =>
        locale?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true
            ? "en-US"
            : "hu-HU";

    private static ProfilePhoneDto ReadPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new ProfilePhoneDto();

        try
        {
            return Normalize(JsonSerializer.Deserialize<ProfilePhoneDto>(value));
        }
        catch (JsonException)
        {
            return new ProfilePhoneDto { Number = value.Trim() };
        }
    }

    private static BillingNameDto ReadBillingName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new BillingNameDto();

        try
        {
            return Normalize(JsonSerializer.Deserialize<BillingNameDto>(value));
        }
        catch (JsonException)
        {
            return new BillingNameDto { LastName = value.Trim() };
        }
    }

    private static BillingAddressDto ReadBillingAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return new BillingAddressDto();

        try
        {
            return Normalize(JsonSerializer.Deserialize<BillingAddressDto>(value));
        }
        catch (JsonException)
        {
            return new BillingAddressDto { AddressLine1 = value.Trim() };
        }
    }

    private static ProfilePhoneDto Normalize(ProfilePhoneDto? phone) => new()
    {
        CountryCode = string.IsNullOrWhiteSpace(phone?.CountryCode)
            ? "+36"
            : phone.CountryCode.Trim(),
        Number = phone?.Number?.Trim() ?? string.Empty
    };

    private static BillingNameDto Normalize(BillingNameDto? billingName) => new()
    {
        LastName = billingName?.LastName?.Trim() ?? string.Empty,
        FirstName = billingName?.FirstName?.Trim() ?? string.Empty
    };

    private static BillingAddressDto Normalize(BillingAddressDto? billingAddress) => new()
    {
        PostalCode = billingAddress?.PostalCode?.Trim() ?? string.Empty,
        City = billingAddress?.City?.Trim() ?? string.Empty,
        AddressLine1 = billingAddress?.AddressLine1?.Trim() ?? string.Empty,
        AddressLine2 = billingAddress?.AddressLine2?.Trim() ?? string.Empty
    };

    private static bool HasValue(ProfilePhoneDto phone) =>
        !string.IsNullOrWhiteSpace(phone.Number);

    private static bool HasValue(BillingNameDto billingName) =>
        !string.IsNullOrWhiteSpace(billingName.LastName) ||
        !string.IsNullOrWhiteSpace(billingName.FirstName);

    private static bool HasValue(BillingAddressDto billingAddress) =>
        !string.IsNullOrWhiteSpace(billingAddress.PostalCode) ||
        !string.IsNullOrWhiteSpace(billingAddress.City) ||
        !string.IsNullOrWhiteSpace(billingAddress.AddressLine1) ||
        !string.IsNullOrWhiteSpace(billingAddress.AddressLine2);
}
