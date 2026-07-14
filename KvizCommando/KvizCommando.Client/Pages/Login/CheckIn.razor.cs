using Blazored.SessionStorage;
using KvizCommando.Client.Features.Home;
using KvizCommando.Client.Features.Modal;
using KvizCommando.Client.Models.StoreModels;
using KvizCommando.Client.Models.ViewModels.Ui;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.User;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.CheckIn;
using KvizCommando.Shared.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Login
{
    public partial class CheckIn : KcComponentBase, IDisposable
    {
        [Inject] private ISessionStorageService SessionStorage { get; set; } = default!;
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IdentityRulesService IdentityRules { get; set; } = default!;

        private string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private KcModal? _termsModal;
        private ModalBoxVm _termsPar = new();
        private string _fullHtml = string.Empty;
        private MarkupString _renderHTML;
        private MarkupString _termsHtml;
        private MarkupString _privacyHtml;

        private const string CHEKIN_CACHE_KEY = "checkin:status";
        private CheckInSessionCache _cacheData = new();

        private RegisterOptionsResponse? _options;

        private CheckInPostRequest _formData = new();
        private string ResultMessage { get; set; } = string.Empty;

        private string DynamicTitle { get; set; } = string.Empty; // Dinamikus oldal cím
        private string Message { get; set; } = string.Empty; // Általános üzenet a felhasználónak
        private bool DisplayNameField { get; set; } = false;


        private bool _isLoaded = false;

        private bool CanCheckIn =>
           (!string.IsNullOrWhiteSpace(_formData.DisplayName) || _cacheData.needsName == false)
           && !string.IsNullOrWhiteSpace(_formData.AcceptedTermsVersion);
      
        private bool _isAccepted;

        private bool IsAccepted
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
                        _formData.AcceptedTermsVersion = _cacheData.termsVersion;
                        OnAcceptedAsync();
                    }
                    else
                    {
                        // ❌ amikor kiveszik a pipát
                        _formData.AcceptedTermsVersion = string.Empty;
                        OnRejectedAsync();
                    }
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            _options = await IdentityRules.GetRulesAsync();
            var uri = Ui.Nav.ToAbsoluteUri(Ui.Nav.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var SugDispName = query["name"];
            if (!string.IsNullOrEmpty(SugDispName))
            {
                _formData.DisplayName = !string.IsNullOrEmpty(SugDispName) && SugDispName.Length > 3
                                        ? SugDispName
                                        : string.Empty;
                Ui.Nav.NavigateTo(uri.GetLeftPart(UriPartial.Path), replace: true);
                await User.CheckInStartAsync(false);
            }
            _cacheData = await SessionStorage.GetItemAsync<CheckInSessionCache>(CHEKIN_CACHE_KEY);
            if (_cacheData is not null)
            {
                _fullHtml = await Http.GetStringAsync(_cacheData.url);
            }

            // (opcionális) takarítsd le az URL-ből az ?error-t a címsorból:
            //_termsPar = MBoxBuilder.BuildParam(ModalTypes.Terms, Ui.Lang);
            _isLoaded = true;
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
            var dsp = _formData.DisplayName?.Trim() ?? string.Empty;

            if (_options is not null)
            {
                if (_options.DisplayNameMinLength>dsp.Length && _cacheData.needsName==true)
                {
                    ResultMessage = Ui.Lang["identityerrors.DisplayNameTooShort"];
                    DisplayNameField = true;
                }
                else if (_options.DisplayNameMaxLength < dsp.Length & _cacheData.needsName == true)
                {
                    ResultMessage = Ui.Lang["identityerrors.DisplayNameTooLong"];
                    DisplayNameField = true;
                }
                else if (IsAccepted == false)
                {
                    ResultMessage = Ui.Lang["identityerrors.TermsNotAccepted"];
                }
            }
            if ((DisplayNameField && _cacheData.needsName) || IsAccepted == false)
                return;
            _formData.TeamName = _formData.DisplayName + Ui.Lang["checkin.Team.Append"];
            // Kérés a szerver felé
            var (response, errors, suggestedname) = await User.CheckInFinishedAsync(_formData);

            if (response)
            {
                // Sikeres bejelentkezés után töröljük a cache-t
                await SessionStorage.RemoveItemAsync(CHEKIN_CACHE_KEY);
                Ui.Nav.NavigateTo("/home", true);
                return;
            }
            // Hibák kezelése: a szerver identityerrors.* kulcsokat ad vissza
            if (errors is { Count: > 0 })
            {
                // csak az első hibát mutatjuk; ha több kell, join-olható
                ResultMessage = Ui.Lang[$"identityerrors.{errors[0]}"];
            }
            else
            {
                ResultMessage = Ui.Lang["identityerrors.DefaultError"];
            }
            if (suggestedname is not null && suggestedname != string.Empty)
            {
                ResultMessage += " " + Ui.Lang["checkin.Reason.SuggestedName"] + $" '{suggestedname}'.";
                _formData.DisplayName = suggestedname;
                DisplayNameField = true;
            }

        }

        private void BuildDynamicText()
        {
            if (_cacheData is null || (_cacheData.needsName == false && _cacheData.needsTerms == false))
            {
                Message = Ui.Lang["checkin.Reason.FallBack"];
                DynamicTitle = Ui.Lang["checkin.Title.Fallback"];
            }
            else if (_cacheData.needsName == true)
            {
                _cacheData.needsTerms = true;
                Message = Ui.Lang["checkin.Reason.DisplayName"];
                DynamicTitle = Ui.Lang["checkin.Title.DisplayName"];
            }
            else
            {
                Message = Ui.Lang["checkin.Reason.TermsUpdated"];
                DynamicTitle = Ui.Lang["checkin.Title.TermsOutdated"];

            }
            _termsPar = MBoxBuilder.BuildParam(ModalTypes.Terms, Ui.Lang);
        }

        private async Task OpenTerms()
        {
            _termsHtml = new MarkupString(ExtractSection(_fullHtml, "terms"));
            _termsPar = _termsPar with {Title = Ui.Lang["checkin.modal.TermsTitle"] };
            _renderHTML = _termsHtml;
            if (_termsModal is not null)
                await _termsModal.ShowAsync(_termsPar);
        }
        private async Task OpenPrivacy()
        {
            
            _privacyHtml = new MarkupString(ExtractSection(_fullHtml, "privacy"));
            _termsPar = _termsPar with { Title = Ui.Lang["checkin.modal.PrivacyTitle"] };
            _renderHTML = _privacyHtml;
            if (_termsModal is not null)
                await _termsModal.ShowAsync(_termsPar);
        }

        private async Task AcceptTerms()
        {
            //IsAccepted = true;
            //await InvokeAsync(StateHasChanged);
            await Task.Delay(5);
        }
      
        private async Task NavigateHome()
        {
            await SessionStorage.RemoveItemAsync(CHEKIN_CACHE_KEY);
            await User.LogoutAsync(true);
            Ui.Nav.NavigateTo("/login");
        }
        public void Dispose()
        {
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

