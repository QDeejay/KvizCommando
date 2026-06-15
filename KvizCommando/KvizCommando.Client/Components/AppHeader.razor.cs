using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Components
{
    public partial class AppHeader : ComponentBase
    {
        [Parameter] public string? DisplayName { get; set; }

        [Parameter] public string? lclRow1 { get; set; } // Profile
        [Parameter] public string? lclRow2 { get; set; } // Settings
       
        [Parameter] public string? lclRow3 { get; set; } // help
        /*
       [Parameter] public string? lclRow4 { get; set; }
       [Parameter] public string? lclRow5 { get; set; }
       [Parameter] public string? lclRow6 { get; set; }
       [Parameter] public string? lclRow7 { get; set; }
       [Parameter] public string? lclRow8 { get; set; }
       [Parameter] public string? lclRow9 { get; set; }
       */
        [Parameter] public string? lclLogout { get; set; }
        [Parameter] public EventCallback<int> OnSelected { get; set; } = default!;
    
        [Parameter] public EventCallback OnLogout { get; set; }
    }
}
