using Blazored.SessionStorage;
using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Models.StoreModels;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Shared.Contracts.CheckIn;
using KvizCommando.Shared.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Login
{
    public partial class CheckIn : ComponentBase, IDisposable
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private IUserService Service { get; set; } = default!;
        [Inject] private ISessionStorageService Session { get; set; } = default!;
        [Inject] private UiHeaderState Ui { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;

        private KcModal? TermsModal;

        private string fullHtml = string.Empty;
        private MarkupString renderHTML;
        private MarkupString TermsHtml;
        private MarkupString PrivacyHtml;

        private const string CheckInCacheKey = "checkin:status";

        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private CheckInSessionCache cacheData = new();
      
        private RegisterOptionsResponse? Options { get; set; }

        private CheckInPostRequest FormData { get; set; } = new();
        private string ResultMessage { get; set; } = string.Empty;

        private string DynamicTitle { get; set; } = string.Empty; // Dinamikus oldal cím
        private string ModalTitle { get; set; } = "PLACEHOLDER"; // Modal cím
        private string Message { get; set; } = string.Empty; // Általános üzenet a felhasználónak
        private bool DisplayNameField { get; set; } = false;

      

        private bool isLoaded { get; set; } = false;

        private bool CanCheckIn =>
           (!string.IsNullOrWhiteSpace(FormData.DisplayName) || cacheData.needsName == false)
           && !string.IsNullOrWhiteSpace(FormData.AcceptedTermsVersion);
      
        private bool _isAccepted;

        protected bool IsAccepted
        {
            get => _isAccepted;
            set
            {
                if (_isAccepted != value)
                {
                    _isAccepted = value;
                    if (value)
                    {
                        // ✅ amikor bepipálják
                        FormData.AcceptedTermsVersion = cacheData.termsVersion;
                        OnAcceptedAsync();
                    }
                    else
                    {
                        // ❌ amikor kiveszik a pipát
                        FormData.AcceptedTermsVersion = string.Empty;
                        OnRejectedAsync();
                    }
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            Options = await IdentityRules.GetRulesAsync();
            var uri = Nav.ToAbsoluteUri(Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var SugDispName = query["name"];
            if (!string.IsNullOrEmpty(SugDispName))
            {
                FormData.DisplayName = !string.IsNullOrEmpty(SugDispName) && SugDispName.Length > 3
                                        ? SugDispName
                                        : string.Empty;
                Nav.NavigateTo(uri.GetLeftPart(UriPartial.Path), replace: true);
                await Service.CheckInStartAsync(false);
            }
            cacheData = await Session.GetItemAsync<CheckInSessionCache>(CheckInCacheKey);
            if (cacheData is not null)
            {
                fullHtml = await Http.GetStringAsync(cacheData.url);
            }

            // (opcionális) takarítsd le az URL-ből az ?error-t a címsorból:

            Ui.HideLang();
            isLoaded = true;
        }
        private static Task OnAcceptedAsync()
        {
            Console.WriteLine("Elfogadta a feltételeket!");
            // ide jöhet pl. engedélyezés
            return Task.CompletedTask;
        }

        private static Task OnRejectedAsync()
        {
            Console.WriteLine("Elutasította a feltételeket!");
            // ide jöhet pl. tiltás
            return Task.CompletedTask;
        }

        private async Task HandleValidSubmit()

        {
            ResultMessage = string.Empty;
            
            DisplayNameField = false;
            var dsp = FormData.DisplayName?.Trim() ?? string.Empty;

            if (Options is not null)
            {
                if (Options.DisplayNameMinLength>dsp.Length && cacheData.needsName==true)
                {
                    ResultMessage = Lang["identityerrors.DisplayNameTooShort"];
                    DisplayNameField = true;
                }
                else if (Options.DisplayNameMaxLength < dsp.Length & cacheData.needsName == true)
                {
                    ResultMessage = Lang["identityerrors.DisplayNameTooLong"];
                    DisplayNameField = true;
                }
                else if (IsAccepted == false)
                {
                    ResultMessage = Lang["identityerrors.TermsNotAccepted"];
                }
            }
            if ((DisplayNameField && cacheData.needsName) || IsAccepted == false)
                return;
            FormData.TeamName = FormData.DisplayName + Lang["checkin.Team.Append"];
            // Kérés a szerver felé
            var (response, errors, suggestedname) = await Service.CheckInFinishedAsync(FormData);

            if (response)
            {
                // Sikeres bejelentkezés után töröljük a cache-t
                await Session.RemoveItemAsync(CheckInCacheKey);
                Nav.NavigateTo("/home", true);
                return;
            }
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
            if (suggestedname is not null && suggestedname != string.Empty)
            {
                ResultMessage += " " + Lang["checkin.Reason.SuggestedName"] + $" '{suggestedname}'.";
                FormData.DisplayName = suggestedname;
                DisplayNameField = true;
            }

        }

        private void BuildDynamicText()
        {
            if (cacheData is null || (cacheData.needsName == false && cacheData.needsTerms == false))
            {
                Message = Lang["checkin.Reason.FallBack"];
                DynamicTitle = Lang["checkin.Title.Fallback"];
            }
            else if (cacheData.needsName == true)
            {
                cacheData.needsTerms = true;
                Message = Lang["checkin.Reason.DisplayName"];
                DynamicTitle = Lang["checkin.Title.DisplayName"];
            }
            else
            {
                Message = Lang["checkin.Reason.TermsUpdated"];
                DynamicTitle = Lang["checkin.Title.TermsOutdated"];

            }
            
        }

        private async Task OpenTerms()
        {
            TermsHtml = new MarkupString(ExtractSection(fullHtml, "terms"));
            ModalTitle = @Lang["checkin.modal.TermsTitle"];
            renderHTML = TermsHtml;
            if (TermsModal is not null)
                await TermsModal.ShowAsync();
        }
        private async Task OpenPrivacy()
        {
            
            PrivacyHtml = new MarkupString(ExtractSection(fullHtml, "privacy"));
            ModalTitle = @Lang["checkin.modal.PrivacyTitle"];
            renderHTML = PrivacyHtml;
            if (TermsModal is not null)
                await TermsModal.ShowAsync();
        }

        private async Task AcceptTerms()
        {
            //IsAccepted = true;
            //await InvokeAsync(StateHasChanged);
            await Task.Delay(5);
        }
      
        private async Task NavigateHome()
        {
            await Session.RemoveItemAsync(CheckInCacheKey);
            await Service.LogoutAsync(true);
            Nav.NavigateTo("/login");
        }
        public void Dispose()
        {
            Ui.ShowLang(); // komponens elhagyásakor VISSZAÁLL
            GC.SuppressFinalize(this);
        }

        /// Helpers
        /// 
        private static string ExtractSection(string html, string id)
        {
            var startTag = $"<div id=\"{id}\">";
            var endTag = "</div>";

            var start = html.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            if (start == -1) return string.Empty;

            start += startTag.Length;
            var end = html.IndexOf(endTag, start, StringComparison.OrdinalIgnoreCase);
            if (end == -1) return string.Empty;

            return html.Substring(start, end - start);
        }
    }


}

