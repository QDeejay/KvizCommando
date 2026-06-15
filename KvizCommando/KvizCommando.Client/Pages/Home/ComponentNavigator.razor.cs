using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace KvizCommando.Client.Pages.Home
{
    public partial class ComponentNavigator : ComponentBase, IDisposable 
    {
        [Inject] protected IApiService Api { get; set; } = default!;

        [Parameter] public int OnComponentSelected { get; set; } = 0;
        [Parameter] public IQuestionState _qState { get; set; } = default!;
        [Parameter] public EventCallback<ModalPar> CallModal { get; set; } = default!;


        private int SelectedId { get; set; }
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        private ModalPar Modal = new();


        public void Dispose()
        {
            // Clean up any resources here
            CallModal = default!;
            GC.SuppressFinalize(this);
        }
    }
}
