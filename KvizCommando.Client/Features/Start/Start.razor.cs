using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
using System.Threading.Tasks;


namespace KvizCommando.Client.Features.Start
{
    public partial class Start : KcComponentBase
    {
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
                var result = await _userService.CheckInStartAsync(true);

                if (result.CanNavigateHome)
                {
                    await Ui.ReloadAsync(ReqStates.AllViaCheckIn);
                    Ui.Nav.NavigateTo("/home");
                    return;
                }

                if (result.Errors.Count > 0)
                    Ui.Nav.NavigateTo("/login");

            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                Ui.Nav.NavigateTo("/login");
            }
        
        }
    }
}
