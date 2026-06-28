
using Blazored.LocalStorage;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Question;
using Microsoft.AspNetCore.Components;
using System.Globalization;



namespace KvizCommando.Client.Pages.Question
{
    public partial class Question : KcComponentBase, IDisposable
    {
        //[Inject] private PageHeaderService Header { get; set; } = default!;
        //[Inject] private NavigationManager Nav { get; set; } = default!;
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private IQuestionState _qState { get; set; } = default!;
        //[Inject] private IApiService Api { get; set; } = default!;
        [Inject] private HttpClient _http { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;


        private string _culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
  
        private Dictionary<string, ContentBoxVm>? _boxesDict = new Dictionary<string, ContentBoxVm>();
        private string[] _boxOrder = Array.Empty<string>();
        private NewQuestionRequest formData = new();
        private const string LOCAL_NOT_SHOW_NEW = "notShowNew";
        private const string LOCAL_NOT_SHOW_DEL = "notShowDel";
        private Func<Task> _onBoxBtnClick { get; set; } = default!;
        private int SelectedId { get; set; }

        private string _boxBtnText = string.Empty;
        private string _boxBtnStyle = string.Empty;

        private string MessageFact = string.Empty;
        private string MessageUsr = string.Empty;

        private int ScreenOption;
        
        private bool _locNotShowStateNew { get; set; }
        private bool _locNotShowStateDel { get; set; }

        private bool _isSuccess = false;
        private int _subPageNo = 0;
        private int ModalOption = 0;
 
        private bool _isReady = false;
        private bool _isLoaded = false;
        private int[] _originalCodes = new int[10];
        private int[] _workingCodes = Array.Empty<int>();
        private ContentBoxVm Box(string orx) => _boxesDict![orx];
        private void BuildBoxes()
        {

            var boxes = new Dictionary<string, ContentBoxVm>();
            boxes = QBtnBoxBuilder.BuildBoxes(_qState.ExtendedInfo!, Ui.Lang);
            
            foreach (var box in boxes)
            {
                _boxesDict![box.Key] = box.Value;
            }
            _isReady = _isLoaded;
        }
        private void OnBoxClick(int boxId)
        {
            SelectedId = 100;
            _boxBtnText = string.Empty;
            _boxBtnStyle = string.Empty;
            _onBoxBtnClick = default!;
            var title = string.Empty;
            switch (boxId)
            {
                case 1:
                    _boxOrder = QBtnBoxBuilder.Root;
                    title = (Ui.Lang["mainlayout.Header.Question"]);
                    break;
                case 101:
                    _boxOrder = QBtnBoxBuilder.SubFact;
                    title = _boxesDict![QBoxKeyRoot.RtBtnFactory.ToString()].Header;
                    break;
                case 102:
                    _boxOrder = QBtnBoxBuilder.SubUsr;
                    _onBoxBtnClick = () => ModalCallAsync(0);
                    _boxBtnText = _locNotShowStateDel ? Ui.Lang["question.Button.Delete"] : Ui.Lang["question.Button.Handle"];
                    _boxBtnStyle = _locNotShowStateDel ? "background-color: #a64b2a" : "background-color: #4b5320";
                    title = _boxesDict![QBoxKeyRoot.RtBtnUsr.ToString()].Header;
                    break;
                case 103:
                    _boxOrder = QBtnBoxBuilder.SubPend;
                    _onBoxBtnClick = () => ModalCallAsync(1);
                    _boxBtnText = Ui.Lang["question.Button.Handle"];
                    title = _boxesDict![QBoxKeyRoot.RtBtnPendig.ToString()].Header;
                    break;
                case 104:
                    _boxOrder = QBtnBoxBuilder.SubNew;
                    title = _boxesDict![QBoxKeyRoot.RtBtnNew.ToString()].Header;
                    break;
                default:
                    _boxOrder = QBtnBoxBuilder.Root;
                    title = (Ui.Lang["mainlayout.Header.Question"]);
                    break;  
            }
            Ui.Header.SetTitle(title, boxId);
            Ui.Header.SetBackBtnEna(boxId>1);
        }
        protected override async Task OnInitializedAsync()
        {
            await _qState.EnsureLoadedAsync();
            _locNotShowStateDel = await _localStorage.GetItemAsync<bool>(LOCAL_NOT_SHOW_DEL);
            _locNotShowStateNew = await _localStorage.GetItemAsync<bool>(LOCAL_NOT_SHOW_NEW);
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Question"],1);

            (_originalCodes, _workingCodes) = QuestionHelper.CloneFactorySlots(_qState.FactorySlots);
            _isLoaded= true;
            if (_isReady==false)
                BuildBoxes();
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnBackBtnClicked += UpdateBckClick;
        }
        private async Task OnFactSaveAsync()
        {
            try
            {
                var (success, message) = await Api.SaveFactorySlotsAsync(new SaveFactoryRequest
                { CategorySlots = _workingCodes });
                MessageFact = message;
                _isSuccess = success;
                _qState.Invalidate();
            }
            catch (Exception ex) { Console.WriteLine($"Save failed: {ex.Message}"); }
            finally { }
            for (var i = 0; i < _workingCodes.Length; i++)
            {
                _originalCodes[i] = _workingCodes[i];
            }
            await _qState.EnsureLoadedAsync();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(1000);
            MessageFact = string.Empty;
           
        }
        private async Task OnSelectId(int id)
        {
            SelectedId = id;
            await Task.Delay(1);
            
        }
        private async Task ModalCallAsync(int btnId)
        {
           await Task.Delay(1);
        }
        private async Task ExecuteModalAsync(int execute)
        {
            bool isSucces = false;
            string message = string.Empty;
            Console.WriteLine($"ScreenOption execute: {ScreenOption}");
            if (execute < 3)
            {
                var (suc, msg) = await Api.ManageSlotAsync(new ManageSlotRequest
                {
                    SlotNo = SelectedId,
                    ReqType = (SlotManageType)execute,
                });
                isSucces = suc;
                message = msg;
            }
            else
            {
                int index = Array.FindIndex(_qState.PendingSlots!, x => x.Category == 0);
                var (suc, msg) = await Api.SendNewQuestionAsync( new NewQuestionRequest
                 {
                     SlotNo = index,
                     Category = formData.Category,
                     Question = formData.Question,
                     Answers = formData.Answers
                 });
                isSucces = suc;
                message = msg;
            }
            Console.WriteLine($"ScreenOption after execute: {ScreenOption}");
            if (isSucces)
            {
                (_originalCodes, _workingCodes) = QuestionHelper.CloneFactorySlots(_qState.FactorySlots);
                _qState.Invalidate();
                await _qState.EnsureLoadedAsync();
                _isSuccess = true;
            }
            else
            {
                MessageUsr = message;
            }
            await InvokeAsync(StateHasChanged);

            if (MessageUsr != string.Empty) { await Task.Delay(1000); MessageUsr = string.Empty; }
            ModalOption = 0;
            //OnRootBtnClick(ScreenOption+10);
            await Task.Delay(1);

        }
        private async Task SaveToForm(NewQuestionRequest request)
        {
            formData = request;
            await ModalCallAsync(2);
        }  
        private async Task CallPendModal(int selectedid)
        {
            if (selectedid != 100)
            {
                SelectedId = selectedid;
                await ModalCallAsync(1);
            }
        }

        private void UpdateBckClick()
        {
            Console.WriteLine($"Navigate to: kurva anyád");
            if (Ui.Header.PageIndex == 1)
                Ui.Nav.NavigateTo("/home");
            else
                OnBoxClick(1);
            
            //_isReady = false;
            InvokeAsync(StateHasChanged);
        }
        public void Dispose()
        {
            //_questionModal?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}


/*
  <KcModal @ref="questionModal"
                         Id="questionModal"
                         Title="@Lang[qModal.Title]"
                         SizeClass="@qModal.Size"
                         ActionText1="@Lang[qModal.ActionText1]"
                         ActionText2="@Lang[qModal.ActionText2]"
                         CloseText="@Lang[qModal.CloseText]"
                         OnAction1="@qModal.AsyncAction1"
                         OnAction2="@qModal.AsyncAction2"
                         ActionButtonStyle1="@qModal.ActionStyle1"
                         CheckBottom="@qModal.CheckBottom"
                         CheckBoxText="@Lang[qModal.CheckBoxText]"
                         CheckBoxKey="@qModal.CheckBoxKey"
                         CheckBoxAction="@qModal.AsyncCheckBoxAction">
                    @if (ModalOption != 0 && SelectedId != 100)
                    {
                        <ModalRender Mode="@ModalOption"
                                     Slot="@QuestionState.PendingSlots[ModalOption==2 ? SelectedId:1]"
                                     renderHTML="@renderHTML" />
                    }
                </KcModal>


        private KcModal? questionModal;
        private ModalPar qModal = new();
private MarkupString renderHTML = new();
 if (_locNotShowStateNew == false)
            {
                string Url = $"/Manuals/{culture}/questionsend.html";
                renderHTML = new MarkupString(await Http.GetStringAsync(Url));
            }
            else renderHTML = new MarkupString(string.Empty);
private void OnRootBtnClick(int btnId)
        {
            Console.WriteLine($"megnyomta a paraszt következő gombot: {btnId}");
            ModalOption = 0;
            
            
            qModal = qModal with { Mode = 0 };
            switch (btnId)
            {
                case 1:
                    ScreenOption = 0;
                    isSuccess = false;
                    BuildButtons();
                    break;
                case 11:
                    ScreenOption = 1;
                    BoxHeader = Lang["question.Box.Title.FactorySlots"];
                    SubHeaderTitle = _qbuttons?[0].Header ?? string.Empty;             
                    break;
                case 12:
                    ScreenOption = 2;
                    BoxHeader = Lang["question.Box.Title.UsrSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedUserSlot, QuestionState.ExtendedInfo.AvailableUserSlot);
                    SubHeaderTitle = _qbuttons?[1].Header ?? string.Empty;
                    
                    break;
                case 13:
                    ScreenOption = 3;
                    BoxHeader = Lang["question.Box.Title.PendingSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedPendingSlot, QuestionState.ExtendedInfo.AvailableUserSlot >> 1);
                    SubHeaderTitle = _qbuttons?[2].Header ?? string.Empty;
                   
                    break;
                case 14:
                    ScreenOption = 4;
                    BoxHeader = Lang["question.Modal.Title.New"];
                    SubHeaderTitle = _qbuttons?[3].Header ?? string.Empty;
                    break;
                default:
                    ScreenOption = 0;
                    break;
            }
        } 

 var spec = ModalSpecs.SpecsQ[btnId];
            ModalOption = spec.Mode;
            qModal = spec;
            switch (btnId)
            {
                case 0:
                    var notShowDel = await LocalStorage.GetItemAsync<bool>(localNotShowDel);
                    if (notShowDel) { await ExecuteModalAsync(0); return; }
                    qModal = qModal with
                    {
                        AsyncAction1 = () => ExecuteModalAsync(0),
                        AsyncCheckBoxAction = () => localNotShowStateDel = true,
                        CheckBoxKey = localNotShowDel
                    };
                    break;
                case 1:
                    qModal = qModal with
                    {
                        ActionText2 = QuestionState.PendingSlots[SelectedId].Status == "Approved" ? qModal.ActionText2 : string.Empty,
                        AsyncAction1 = () => ExecuteModalAsync(1),
                        AsyncAction2 = () => ExecuteModalAsync(2),
                        CheckBoxKey = string.Empty
                    };
                    break;
                case 2:
                    var notShowNew = await LocalStorage.GetItemAsync<bool>(localNotShowNew);
                    if (notShowNew) { await ExecuteModalAsync(3); return; }
                    qModal = qModal with
                    {
                        AsyncAction1 = () => ExecuteModalAsync(3),
                        AsyncCheckBoxAction = () => localNotShowState = true,
                        CheckBoxKey = localNotShowNew
                    };
                    break;
            }
            ;
            Console.WriteLine($"ScreenOption before modal: {ScreenOption}");
            await questionModal.ShowAsync();
            Console.WriteLine($"ScreenOption after show: {ScreenOption}");

 */