
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages
{
    public partial class Start : KcComponentBase, IDisposable
    {
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        //[Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private IUserService _userService { get; set; } = default!;

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
                await _userService.CheckInStartAsync(true);
                Ui.Nav.NavigateTo("/login");

            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                Ui.Nav.NavigateTo("/login");
            }
            finally { }
        
        }
        public void Dispose() { }
    }
}
