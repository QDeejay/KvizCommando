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
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private IUserService UserService { get; set; } = default!;

        private readonly string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private bool _isLoaded = false;

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(500);
            _isLoaded = true;
        }
        private async Task LoginUser() 
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
