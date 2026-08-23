using KvizCommando.Shared.Contracts.Profile;
using KvizCommando.Client.Services.Audio;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Profile;

public enum ProfileAccountSection
{
    Contact,
    Security
}

public partial class ProfileAccountView
{
    [Inject] private IProfileClientService ProfileClient { get; set; } = default!;
    [Inject] private AudioService Audio { get; set; } = default!;

    [Parameter] public ProfileAccountSection Section { get; set; }
    [Parameter] public string Culture { get; set; } = "hu";

    private ProfileAccountDto? _account;
    private ProfilePhoneDto _phone = new();
    private BillingNameDto _billingName = new();
    private BillingAddressDto _billingAddress = new();
    private string _newEmail = string.Empty;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _repeatPassword = string.Empty;
    private readonly bool[] _showPassword = new bool[3];
    private bool _isLoading = true;
    private bool _isPiiBusy;
    private bool _isPreferredLocaleBusy;
    private bool _isEmailBusy;
    private bool _isPasswordBusy;
    private bool _isPhoneEditable;
    private bool _isBillingEditable;

    private bool CanChangeEmail =>
        !_isEmailBusy &&
        !string.IsNullOrWhiteSpace(_newEmail) &&
        !string.Equals(
            _newEmail.Trim(),
            _account?.Email,
            StringComparison.OrdinalIgnoreCase);

    private string CurrentPreferredLocale =>
        Culture.Equals("en", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "hu-HU";

    private bool PreferredLocaleDiffers =>
        _account is not null &&
        !string.Equals(
            _account.PreferredLocale,
            CurrentPreferredLocale,
            StringComparison.OrdinalIgnoreCase);

    private bool HasPiiChanges =>
        _account is not null &&
        (!string.Equals(
             _phone.CountryCode,
             _account.Phone.CountryCode,
             StringComparison.Ordinal) ||
         !string.Equals(
             _phone.Number,
             _account.Phone.Number,
             StringComparison.Ordinal) ||
         !string.Equals(
             _billingName.LastName,
             _account.BillingName.LastName,
             StringComparison.Ordinal) ||
         !string.Equals(
             _billingName.FirstName,
             _account.BillingName.FirstName,
             StringComparison.Ordinal) ||
         !string.Equals(
             _billingAddress.PostalCode,
             _account.BillingAddress.PostalCode,
             StringComparison.Ordinal) ||
         !string.Equals(
             _billingAddress.City,
             _account.BillingAddress.City,
             StringComparison.Ordinal) ||
         !string.Equals(
             _billingAddress.AddressLine1,
             _account.BillingAddress.AddressLine1,
             StringComparison.Ordinal) ||
         !string.Equals(
             _billingAddress.AddressLine2,
             _account.BillingAddress.AddressLine2,
             StringComparison.Ordinal));

    private bool IsPhoneNumberValid =>
        string.IsNullOrEmpty(_phone.Number) ||
        _phone.Number.All(char.IsDigit);

    private bool CanSavePii =>
        !_isPiiBusy &&
        HasPiiChanges &&
        IsPhoneNumberValid;

    private bool CanEnterNewPassword =>
        !_isPasswordBusy &&
        !string.IsNullOrWhiteSpace(_currentPassword);

    private bool CanEnterRepeatedPassword =>
        CanEnterNewPassword &&
        !string.IsNullOrWhiteSpace(_newPassword);

    private bool CanChangePassword =>
        CanEnterRepeatedPassword &&
        !string.IsNullOrWhiteSpace(_repeatPassword) &&
        _newPassword == _repeatPassword;

    private string CurrentPasswordType => GetPasswordType(0);
    private string NewPasswordType => GetPasswordType(1);
    private string RepeatPasswordType => GetPasswordType(2);

    protected override async Task OnInitializedAsync()
    {
        var response = await ProfileClient.GetAccountAsync();
        if (response.State == ProfileAccountRequestState.Success &&
            response.Account is not null)
        {
            Apply(response.Account);
        }

        _isLoading = false;
    }

    private async Task SavePiiAsync()
    {
        if (!CanSavePii)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isPiiBusy = true;
        var response = await ProfileClient.SaveAccountAsync(
            new SaveProfileAccountRequest
            {
                Phone = _phone,
                BillingName = _billingName,
                BillingAddress = _billingAddress
            });
        _isPiiBusy = false;

        if (response.State == ProfileAccountRequestState.Success &&
            response.Account is not null)
        {
            Apply(response.Account);
            Ui.Toast.Success(Ui.Lang["profile.Account.SaveSuccess"]);
            return;
        }

        Ui.Toast.Error(Ui.Lang["profile.Account.Error.Save"]);
    }

    private async Task ChangeEmailAsync()
    {
        if (!CanChangeEmail)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isEmailBusy = true;
        var response = await ProfileClient.RequestEmailChangeAsync(
            _newEmail.Trim());
        _isEmailBusy = false;

        if (response.Success)
        {
            _newEmail = string.Empty;
            Ui.Toast.Success(
                Ui.Lang["profile.Account.Email.ConfirmationSent"]);
            return;
        }

        Ui.Toast.Error(Ui.Lang["profile.Account.Error.Identity"]);
    }

    private async Task UpdatePreferredLocaleAsync()
    {
        if (_isPreferredLocaleBusy || !PreferredLocaleDiffers)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isPreferredLocaleBusy = true;
        var response = await ProfileClient.UpdatePreferredLocaleAsync();
        _isPreferredLocaleBusy = false;

        if (response.State == ProfileAccountRequestState.Success &&
            response.Account is not null)
        {
            _account = response.Account;
            Ui.Toast.Success(
                Ui.Lang["profile.Account.PreferredLocale.UpdateSuccess"]);
            return;
        }

        Ui.Toast.Error(
            Ui.Lang["profile.Account.PreferredLocale.UpdateError"]);
    }

    private async Task ChangePasswordAsync()
    {
        if (!CanChangePassword)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isPasswordBusy = true;
        var response = await ProfileClient.ChangePasswordAsync(
            _currentPassword,
            _newPassword);
        _isPasswordBusy = false;

        if (response.Success)
        {
            _currentPassword = string.Empty;
            _newPassword = string.Empty;
            _repeatPassword = string.Empty;
            Array.Fill(_showPassword, false);
            Ui.Toast.Success(
                Ui.Lang["profile.Account.Password.SaveSuccess"]);
            return;
        }

        Ui.Toast.Error(Ui.Lang["profile.Account.Error.Identity"]);
    }

    private void OnCurrentPasswordInput(ChangeEventArgs args)
    {
        _currentPassword = args.Value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_currentPassword))
        {
            _newPassword = string.Empty;
            _repeatPassword = string.Empty;
            _showPassword[1] = false;
            _showPassword[2] = false;
        }
    }

    private void OnNewPasswordInput(ChangeEventArgs args)
    {
        _newPassword = args.Value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(_newPassword))
        {
            _repeatPassword = string.Empty;
            _showPassword[2] = false;
        }
    }

    private void OnPhoneChanged(ProfilePhoneDto phone)
    {
        _phone = phone;
    }

    private async Task EnablePhoneEditingAsync()
    {
        if (_isPiiBusy || _isPhoneEditable)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isPhoneEditable = true;
    }

    private async Task EnableBillingEditingAsync()
    {
        if (_isPiiBusy || _isBillingEditable)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _isBillingEditable = true;
    }

    private async Task TogglePassword(int index)
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _showPassword[index] = !_showPassword[index];
    }

    private string GetPasswordType(int index) =>
        _showPassword[index] ? "text" : "password";

    private string GetEyeIcon(int index) =>
        _showPassword[index] ? "bi bi-eye-slash" : "bi bi-eye";

    private void Apply(ProfileAccountDto account)
    {
        _account = account;
        _phone = new ProfilePhoneDto
        {
            CountryCode = account.Phone.CountryCode,
            Number = account.Phone.Number
        };
        _billingName = new BillingNameDto
        {
            LastName = account.BillingName.LastName,
            FirstName = account.BillingName.FirstName
        };
        _billingAddress = new BillingAddressDto
        {
            PostalCode = account.BillingAddress.PostalCode,
            City = account.BillingAddress.City,
            AddressLine1 = account.BillingAddress.AddressLine1,
            AddressLine2 = account.BillingAddress.AddressLine2
        };
        _isPhoneEditable = string.IsNullOrWhiteSpace(_phone.Number);
        _isBillingEditable = !HasBillingData();
    }

    private bool HasBillingData() =>
        !string.IsNullOrWhiteSpace(_billingName.LastName) ||
        !string.IsNullOrWhiteSpace(_billingName.FirstName) ||
        !string.IsNullOrWhiteSpace(_billingAddress.PostalCode) ||
        !string.IsNullOrWhiteSpace(_billingAddress.City) ||
        !string.IsNullOrWhiteSpace(_billingAddress.AddressLine1) ||
        !string.IsNullOrWhiteSpace(_billingAddress.AddressLine2);
}
