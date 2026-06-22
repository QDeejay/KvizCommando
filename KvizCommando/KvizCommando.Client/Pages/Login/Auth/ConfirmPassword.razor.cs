using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Shared.Contracts.Auth;
using KvizCommando.Shared.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace KvizCommando.Client.Pages.Login.Auth;

public partial class ConfirmPassword : ComponentBase
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private ILanguageService Lang { get; set; } = default!;
    [Inject] private IUserService Service { get; set; } = default!;
    [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;

    private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    private bool? success = null;
    private RegisterOptionsResponse? Options { get; set; }
    public ResetPasswordForm FormData { get; set; } = new();
    public string ResultMessage { get; set; } = string.Empty;
    public string successMessage { get; set; } = string.Empty;
    private bool PasswordFiledSW { get; set; } = false;
    
    private bool CanRecover =>
       !string.IsNullOrWhiteSpace(FormData.newPassword)
       && !string.IsNullOrWhiteSpace(FormData.email)
       && !string.IsNullOrWhiteSpace(FormData.resetCode)
       && !string.IsNullOrWhiteSpace(FormData.confirmedNewPsw);

    private async Task HandleValidSubmit()
    {
        ResultMessage = string.Empty;
        successMessage = string.Empty;

        // --- Password: IdentityOptions alapján teljes ellenőrzés ---
        PasswordFiledSW = false;
        if (Options is not null)
        {
            var pwd = FormData.newPassword ?? string.Empty;

            if (pwd.Length < Options.RequiredLength)
            {
                ResultMessage = Lang["identityerrors.PasswordTooShort"].FormatSafe(Options.RequiredLength);
                PasswordFiledSW = true;
            }
            else if (Options.RequireDigit && !pwd.Any(char.IsDigit))
            {
                ResultMessage = Lang["identityerrors.PasswordRequiresDigit"];
                PasswordFiledSW = true;
            }
            else if (Options.RequireLowercase && !pwd.Any(char.IsLower))
            {
                ResultMessage = Lang["identityerrors.PasswordRequiresLower"];
                PasswordFiledSW = true;
            }
            else if (Options.RequireUppercase && !pwd.Any(char.IsUpper))
            {
                ResultMessage = Lang["identityerrors.PasswordRequiresUpper"];
                PasswordFiledSW = true;
            }
            else if (Options.RequireNonAlphanumeric && pwd.All(char.IsLetterOrDigit))
            {
                ResultMessage = Lang["identityerrors.PasswordRequiresNonAlphanumeric"];
                PasswordFiledSW = true;
            }
            else if (Options.RequiredUniqueChars > 1 && pwd.Distinct().Count() < Options.RequiredUniqueChars)
            {
                ResultMessage = Lang["identityerrors.PasswordRequiresUniqueChars"].FormatSafe(Options.RequiredLength);
                PasswordFiledSW = true;
            }
            else if (FormData.newPassword != FormData.confirmedNewPsw)
            {
                ResultMessage = Lang["identityerrors.PasswordNotMatched"];
                PasswordFiledSW = true;
            }
        }

        // Ha bármelyik mező hibás, ne küldjük a szerverre
        if (PasswordFiledSW)
            return;

        // Kérés a szerver felé
        var (response, errors) = await Service.RecoverPasswordAsync(FormData);

        success = response;
        // Hibák kezelése: a szerver identityerrors.* kulcsokat ad vissza
        if (errors is { Count: > 0 })
        {
            // csak az első hibát mutatjuk; ha több kell, join-olható
            ResultMessage = Lang[$"identityerrors.{errors[0]}"];
        }
        else
        {
            ResultMessage = Lang["identityerrors.DefaultError"];
        }
    }

    protected override async Task OnInitializedAsync()
    {
        culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        Options = await IdentityRules.GetRulesAsync();
        var uri = Nav.ToAbsoluteUri(Nav.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

        var email = query["email"];
        var code = query["code"];

        FormData.email = email ?? string.Empty;
        FormData.resetCode = code ?? string.Empty;
    }
    private void NavigateHome()
    {
        Nav.NavigateTo("/login");
    }
}

