using KvizCommando.Shared.Contracts.Profile;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Profile;

public partial class ProfileAccountView
{
    [Inject] private IProfileClientService ProfileClient { get; set; } = default!;

    private ProfileAccountDto? _account;
    private string _phone = string.Empty;
    private string _billingName = string.Empty;
    private string _billingAddress = string.Empty;
    private string _newEmail = string.Empty;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _repeatPassword = string.Empty;
    private bool _isLoading = true;
    private bool _isPiiBusy;
    private bool _isEmailBusy;
    private bool _isPasswordBusy;

    private bool CanChangeEmail => !_isEmailBusy && !string.IsNullOrWhiteSpace(_newEmail) &&
        !string.Equals(_newEmail.Trim(), _account?.Email, StringComparison.OrdinalIgnoreCase);
    private bool CanChangePassword => !_isPasswordBusy && !string.IsNullOrWhiteSpace(_currentPassword) &&
        !string.IsNullOrWhiteSpace(_newPassword) && _newPassword == _repeatPassword;

    protected override async Task OnInitializedAsync()
    {
        var response = await ProfileClient.GetAccountAsync();
        if (response.State == ProfileAccountRequestState.Success && response.Account is not null)
            Apply(response.Account);
        _isLoading = false;
    }

    private async Task SavePiiAsync()
    {
        _isPiiBusy = true;
        var response = await ProfileClient.SaveAccountAsync(new SaveProfileAccountRequest
        {
            Phone = _phone,
            BillingName = _billingName,
            BillingAddress = _billingAddress
        });
        _isPiiBusy = false;
        if (response.State == ProfileAccountRequestState.Success && response.Account is not null)
        {
            Apply(response.Account);
            Ui.Toast.Success(Ui.Lang["profile.Account.SaveSuccess"]);
        }
        else
        {
            Ui.Toast.Error(Ui.Lang["profile.Account.Error.Save"]);
        }
    }

    private async Task ChangeEmailAsync()
    {
        if (!CanChangeEmail) return;
        _isEmailBusy = true;
        var response = await ProfileClient.RequestEmailChangeAsync(_newEmail.Trim());
        _isEmailBusy = false;
        if (response.Success)
        {
            _newEmail = string.Empty;
            Ui.Toast.Success(Ui.Lang["profile.Account.Email.ConfirmationSent"]);
        }
        else Ui.Toast.Error(Ui.Lang["profile.Account.Error.Identity"]);
    }

    private async Task ChangePasswordAsync()
    {
        if (!CanChangePassword) return;
        _isPasswordBusy = true;
        var response = await ProfileClient.ChangePasswordAsync(_currentPassword, _newPassword);
        _isPasswordBusy = false;
        if (response.Success)
        {
            _currentPassword = _newPassword = _repeatPassword = string.Empty;
            Ui.Toast.Success(Ui.Lang["profile.Account.Password.SaveSuccess"]);
        }
        else Ui.Toast.Error(Ui.Lang["profile.Account.Error.Identity"]);
    }

    private void Apply(ProfileAccountDto account)
    {
        _account = account;
        _phone = account.Phone;
        _billingName = account.BillingName;
        _billingAddress = account.BillingAddress;
    }
}
