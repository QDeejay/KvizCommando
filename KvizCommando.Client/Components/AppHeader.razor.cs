using Microsoft.AspNetCore.Components;
using KvizCommando.Client.Services.Audio;

namespace KvizCommando.Client.Components
{
    public partial class AppHeader
    {
        [Inject] private AudioService Audio { get; set; } = default!;
        [Parameter] public string? DisplayName { get; set; }

        [Parameter] public string? lclRow1 { get; set; } // Profile
        [Parameter] public string? lclRow2 { get; set; } // Settings
       
        [Parameter] public string? lclRow3 { get; set; } // help
        [Parameter] public string? lclLogout { get; set; }
        [Parameter] public EventCallback OnProfile { get; set; }
        [Parameter] public EventCallback OnSettings { get; set; }
        [Parameter] public EventCallback OnHelp { get; set; }
        [Parameter] public EventCallback OnLogout { get; set; }

        private async Task HandleProfileAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

            if (OnProfile.HasDelegate)
                await OnProfile.InvokeAsync();
        }

        private async Task HandleSettingsAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

            if (OnSettings.HasDelegate)
                await OnSettings.InvokeAsync();
        }

        private async Task HandleHelpAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

            if (OnHelp.HasDelegate)
                await OnHelp.InvokeAsync();
        }

        private async Task HandleLogoutAsync()
        {
            await Audio.PlaySfxAsync(AudioService.SFX_UI_TOUCH);

            if (OnLogout.HasDelegate)
                await OnLogout.InvokeAsync();
        }
    }
}
