using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Solo.Features;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;


namespace KvizCommando.Client.Pages.Solo
{
    public partial class SoloGame : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        private readonly Dictionary<string, ContentBoxVm> _boxes = [];

        private string[] _boxOrder = [];

        private bool _isReady = false;
        private bool _isLoaded = false;

        private string SelectorCSS => _boxOrder != SgameBoxBuilder.Root ? "kc-solo-selector-sub" : "kc-solo-selector-root";

        private string Culture => AppStates.Culture;
        private SoloGameDtos SState => AppStates.SoloGame!;

        private ContentBoxVm Box(string orx) => _boxes[orx];


        private void BuildBoxes()
        {
            /*
            var callback = new SoloCallbacks
            {

            };*/
            var boxes = SgameBoxBuilder.BuildBoxes(SState, Culture, Ui.Lang);

            foreach (var box in boxes)
            {
                _boxes[box.Key] = box.Value;
            }
            _isReady = _isLoaded;
        }

        private void OnBoxClick(int boxId)
        {
            _boxOrder = SgameBoxBuilder.Root;
            var headerTitle = string.Empty;
            switch (boxId)
            {
                case 4:
                    headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];
                    break;
                case 401:
                    _boxOrder = SgameBoxBuilder.SubCat;
                    headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCategory.ToString()].Header;
                    break;
                case 402:
                    _boxOrder = SgameBoxBuilder.SubOri;
                    headerTitle = _boxes[SgameBoxKeyRoot.RtBtnOrient.ToString()].Header;
                    break;
                case 403:
                    headerTitle = _boxes[SgameBoxKeyRoot.RtBtnCampaign.ToString()].Header;
                    break;
                default:
                    headerTitle = Ui.Lang["mainlayout.Header.GameSolo"];
                    break;
            }
            Ui.Header.SetTitle(headerTitle, boxId);
            Ui.Header.SetBackBtnEna(boxId > 4);
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.GameSolo"], 4);

            _boxOrder = SgameBoxBuilder.Root;

            _isLoaded = true;

            if (_isReady == false)
                BuildBoxes();

            await Task.Delay(1);
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnBackBtnClicked += UpdateBckClick;
        }
        private void UpdateBckClick()
        {
            if (Ui.Header.PageIndex == 4)
                Ui.Nav.NavigateTo("/home");
            else
                OnBoxClick(4);
            InvokeAsync(StateHasChanged);
        }
        public void Dispose()
        {
            Ui.Header.OnBackBtnClicked -= UpdateBckClick;
            GC.SuppressFinalize(this);
        }
    }
}



/*
 
 
 
 
 */