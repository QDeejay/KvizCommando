using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Question.Features;
using KvizCommando.Client.Pages.Shared.Modal.Dynamic;
using KvizCommando.Client.Pages.Shared.Modal.Features;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Pages.Question
{
    public partial class Question : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        private readonly Dictionary<string, ContentBoxVm> _boxes = [];

        private string[] _boxOrder = [];

        private int _selectedId = 100;

        private bool _isReady = false;
        private bool _isLoaded = false;
        private string Culture => AppStates.Culture;
        private bool LocNotShowStateNew => AppStates.LocStoreStates.ChkBxNotShowNew ?? false;
        private bool LocNotShowStateDel => AppStates.LocStoreStates.ChkBxNotShowDel ?? false;
        private QuestionDtos QState => AppStates.Question!;
        private ContentBoxVm Box(string orx) => _boxes[orx];
        private void BuildBoxes()
        {
            var callback = new QuestionCallbacks
            {
                OnFactSave = OnFactSaveAsync,
                OnSelectId = OnSelectId,
                OnDelete = OnDeleteUsrSlotAsync,
                OnHandle = OnHandlePendSLotAsync,
                OnWatch = OnWatchQuestionAsync,
                OnSend = OnSaveToFormAsync
            };
            var boxes = QBoxBuilder.BuildBoxes(QState.ExtendedInfo!, callback, Ui.Lang);
            foreach (var box in boxes)
            {
                _boxes[box.Key] = box.Value;
            }
            _isReady = _isLoaded;
        }
        private void OnBoxClick(int boxId)
        {
            _selectedId = 100;
            _boxOrder = QBoxBuilder.Root;
            var headerTitle = string.Empty;
            switch (boxId)
            {
                case 1:
                    headerTitle = (Ui.Lang["mainlayout.Header.Question"]);
                    break;
                case 101:
                    _boxOrder = QBoxBuilder.SubFact;
                    headerTitle = _boxes[QBoxKeyRoot.RtBtnFactory.ToString()].Header;
                    break;
                case 102:
                    _boxOrder = QBoxBuilder.SubUsr;
                    headerTitle = _boxes[QBoxKeyRoot.RtBtnUsr.ToString()].Header;
                    break;
                case 103:
                    _boxOrder = QBoxBuilder.SubPend;
                    headerTitle = _boxes[QBoxKeyRoot.RtBtnPendig.ToString()].Header;
                    break;
                case 104:
                    _boxOrder = QBoxBuilder.SubNew;
                    _selectedId = Array.FindIndex(QState.PendingSlots!, x => x.Category == 0);
                    headerTitle = _boxes[QBoxKeyRoot.RtBtnNew.ToString()].Header;
                    break;
                default:
                    headerTitle = (Ui.Lang["mainlayout.Header.Question"]);
                    break;
            }
            Ui.Header.SetTitle(headerTitle, boxId);
            Ui.Header.SetBackBtnEna(boxId > 1);
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Question"], 1);
            _boxOrder = QBoxBuilder.Root;
            _isLoaded = true;
            if (_isReady == false)
                BuildBoxes();
            await Task.Delay(1);
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnBackBtnClicked += UpdateBckClick;
        }
        private void OnSelectId(int id) => _selectedId = id;
        private async Task OnWatchQuestionAsync()
        {
            var vm = MBoxBuilder.BuildParam(ModalTypes.QCheckQuestion, Ui.Lang);
            vm.BodyParameters.Add(nameof(QModalRender.SlotNo), _selectedId);
            await Ui.Modal.ShowAsync(vm);
        }
        private async Task OnFactSaveAsync(int[] workCodes)
        {
            var success = await Api.SaveFactorySlotsAsync(new SaveFactoryRequest
            { CategorySlots = workCodes });

            if (!success)
                return;

            await Ui.ReloadAsync();
            BuildBoxes();

        }
        private async Task OnSaveToFormAsync(NewQuestionRequest request)
        {
            if (!LocNotShowStateNew)
            {
                var HtmlContent = await MarkupLoader.LoadingHtmlAsync(Culture, Html.NewQuestRules);
                var vm = MBoxBuilder.BuildParam(ModalTypes.QNewRules, Ui.Lang);
                vm.BodyParameters.Add(nameof(QModalRender.RenderHTML), HtmlContent);
                var result = await Ui.Modal.ShowAsync(vm);
                if (result != ModalResult.Button1)
                    return;
            }

            var success = await Api.SendNewQuestionAsync(request);
            if (!success)
                return;
            _selectedId = 100;
            await Ui.ReloadAsync();
            BuildBoxes();
            StateHasChanged();
        }
        private async Task OnDeleteUsrSlotAsync()
        {
            if (!LocNotShowStateDel)
            {
                var vm = MBoxBuilder.BuildParam(ModalTypes.QUsrDelet, Ui.Lang);
                var result = await Ui.Modal.ShowAsync(vm);

                if (result != ModalResult.Button1)
                    return;
            }

            var success = await Api.ManageSlotAsync(new ManageSlotRequest
            {
                SlotNo = _selectedId,
                ReqType = SlotManageType.DeleteUsr,
            });

            if (!success)
                return;

            await Ui.ReloadAsync();
            BuildBoxes();
            OnBoxClick(102);
        }
        private async Task OnHandlePendSLotAsync()
        {
            var vm = MBoxBuilder.BuildParam(ModalTypes.QPendHandle, Ui.Lang);
            vm = vm with
            {
                ActionText2 = QState.PendingSlots[_selectedId].Status == "Approved" ? vm.ActionText2 : string.Empty,
            };
            vm.BodyParameters.Add(nameof(QModalRender.SlotNo), _selectedId);
            var result = await Ui.Modal.ShowAsync(vm);
            SlotManageType reqType;

            if (result == ModalResult.Button1)
                reqType = SlotManageType.DeletePending;
            else if (result == ModalResult.Button2)
                reqType = SlotManageType.MovePending;
            else
                return;

            var success = await Api.ManageSlotAsync(new ManageSlotRequest
            {
                SlotNo = _selectedId,
                ReqType = (SlotManageType)reqType,
            });

            if (!success)
                return;

            await Ui.ReloadAsync();
            BuildBoxes();
        }
        private void UpdateBckClick()
        {
            if (Ui.Header.PageIndex == 1)
                Ui.Nav.NavigateTo("/home");
            else
                OnBoxClick(1);
            InvokeAsync(StateHasChanged);
        }
        public void Dispose()
        {
            Ui.Header.OnBackBtnClicked -= UpdateBckClick;
            GC.SuppressFinalize(this);
        }

    }
}