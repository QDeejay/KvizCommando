using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Components
{
    public partial class AppHeader
    {
        [Parameter] public string? DisplayName { get; set; }

        [Parameter] public string? lclRow1 { get; set; } // Profile
        [Parameter] public string? lclRow2 { get; set; } // Settings
       
        [Parameter] public string? lclRow3 { get; set; } // help
        [Parameter] public string? lclLogout { get; set; }
        [Parameter] public EventCallback OnHelp { get; set; }
        [Parameter] public EventCallback OnLogout { get; set; }
    }
}
