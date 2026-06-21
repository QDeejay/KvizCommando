using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages
{
    public partial class Start : ComponentBase, IDisposable
    {
        [CascadingParameter]
        protected ILanguageService Lang { get; set; } = default!;

        [CascadingParameter]
        protected NavigationManager Nav { get; set; } = default!;

        [CascadingParameter]
        protected IUserService UserService { get; set; } = default!;

        string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool _isLoaded = false;

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(500);
            _isLoaded = true;
        }
        protected async Task LoginUser() 
        {
            
            try 
            {
                await UserService.CheckInStartAsync(true);
                Nav.NavigateTo("/login");

            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                Nav.NavigateTo("/login");
            }
            finally { }
        
        }
        public void Dispose() { }
    }
}
