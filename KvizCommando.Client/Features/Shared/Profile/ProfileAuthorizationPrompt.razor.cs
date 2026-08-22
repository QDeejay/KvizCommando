using KvizCommando.Client.Services.Audio;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Profile;

public partial class ProfileAuthorizationPrompt
{
    [Inject] private AudioService Audio { get; set; } = default!;

    [Parameter] public bool Busy { get; set; }
    [Parameter] public string Error { get; set; } = string.Empty;
    [Parameter] public string SubmitTextKey { get; set; } = string.Empty;
    [Parameter] public string WorkingTextKey { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback OnChanged { get; set; }

    private string _password = string.Empty;
    private bool _showPassword;

    private bool CanSubmit =>
        !Busy &&
        !string.IsNullOrWhiteSpace(_password);

    private string PasswordType =>
        _showPassword ? "text" : "password";

    private string PasswordEyeIcon =>
        _showPassword ? "bi bi-eye-slash" : "bi bi-eye";

    private async Task OnPasswordInput(ChangeEventArgs args)
    {
        _password = args.Value?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(_password))
            _showPassword = false;
        await OnChanged.InvokeAsync();
    }

    private async Task TogglePasswordAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        _showPassword = !_showPassword;
    }

    private async Task SubmitAsync()
    {
        if (!CanSubmit)
            return;

        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        await OnSubmit.InvokeAsync(_password);
    }

    private async Task CancelAsync()
    {
        await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);
        await OnCancel.InvokeAsync();
    }
}
