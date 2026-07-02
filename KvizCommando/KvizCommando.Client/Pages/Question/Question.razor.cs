
using Blazored.LocalStorage;
using KvizCommando.Client.Features.Modal;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Layout;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Question.Components;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Question;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
using System.Reflection;



namespace KvizCommando.Client.Pages.Question
{
    public partial class Question : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        private readonly Dictionary<string, ContentBoxVm> _boxes = [];
        //private NewQuestionRequest _formData = new();
        private ModalBoxVm _qModalPar = new();
        private string[] _boxOrder = [];
        private Func<Task> _onManageSlotAsync = default!;
        private int _selectedId = 100;
        private string _boxBtnText = string.Empty;
        private string _boxBtnStyle = string.Empty;
        private string MessageFact = string.Empty;
        private string MessageUsr = string.Empty;

        private bool _locNotShowStateNew => AppStates.LocStoreStates.ChkBxNotShowNew ?? false;
        private bool _locNotShowStateDel => AppStates.LocStoreStates.ChkBxNotShowDel ?? false;

        private bool _isReady = false;
        private bool _isLoaded = false;
        //private int[] _originalCodes = new int[10];
        //private int[] _workingCodes = Array.Empty<int>();
        private string Culture => AppStates.Culture;
        private QuestionDtos QState => AppStates.Question!;
        private ContentBoxVm Box(string orx) => _boxes[orx];
        private void BuildBoxes()
        {

            var boxes = QBtnBoxBuilder.BuildBoxes(QState.ExtendedInfo!, Ui.Lang);
            foreach (var box in boxes)
            {
                _boxes[box.Key] = box.Value;
            }
            _isReady = _isLoaded;
        }
        private void OnBoxClick(int boxId)
        {
            _selectedId = 100;
            _boxBtnText = string.Empty;
            _boxBtnStyle = string.Empty;
            _onManageSlotAsync = default!;
            var title = string.Empty;
            switch (boxId)
            {
                case 1:
                    _boxOrder = QBtnBoxBuilder.Root;
                    title = (Ui.Lang["mainlayout.Header.Question"]);
                    break;
                case 101:
                    _boxOrder = QBtnBoxBuilder.SubFact;
                    title = _boxes[QBoxKeyRoot.RtBtnFactory.ToString()].Header;
                    break;
                case 102:
                    _boxOrder = QBtnBoxBuilder.SubUsr;
                    _onManageSlotAsync = OnDeleteUsrSlotAsync;
                    _boxBtnText = _locNotShowStateDel ? Ui.Lang["question.Button.Delete"] : Ui.Lang["question.Button.Handle"];
                    _boxBtnStyle = _locNotShowStateDel ? "background-color: #a64b2a" : "background-color: #4b5320";
                    title = _boxes[QBoxKeyRoot.RtBtnUsr.ToString()].Header;
                    break;
                case 103:
                    _boxOrder = QBtnBoxBuilder.SubPend;
                    _onManageSlotAsync = OnHandlePendSLotAsync;
                    _boxBtnText = Ui.Lang["question.Button.Handle"];
                    title = _boxes[QBoxKeyRoot.RtBtnPendig.ToString()].Header;
                    break;
                case 104:
                    _boxOrder = QBtnBoxBuilder.SubNew;
                    title = _boxes[QBoxKeyRoot.RtBtnNew.ToString()].Header;
                    _selectedId = Array.FindIndex(QState.PendingSlots!, x => x.Category == 0);
                    break;
                default:
                    _boxOrder = QBtnBoxBuilder.Root;
                    title = (Ui.Lang["mainlayout.Header.Question"]);
                    break;  
            }
            Ui.Header.SetTitle(title, boxId);
            Ui.Header.SetBackBtnEna(boxId>1);
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Question"],1);
            _boxOrder = QBtnBoxBuilder.Root;
            _isLoaded = true;
            if (_isReady==false)
                BuildBoxes();
            await Task.Delay(1);
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnBackBtnClicked += UpdateBckClick;
        }
        private async Task OnFactSaveAsync(int[] workCodes)
        {
            try
            {
                var (success, message) = await Api.SaveFactorySlotsAsync(new SaveFactoryRequest
                { CategorySlots = workCodes });
                MessageFact = message;
                await Ui.ReloadAsync();
                BuildBoxes(); 

            }
            catch (Exception ex) { Console.WriteLine($"Save failed: {ex.Message}"); }
            finally { }
          
           
        }
        private async Task OnSaveToFormAsync(NewQuestionRequest request)
        {
            if (!_locNotShowStateNew)
            {
                _qModalPar = MBoxBuilder.BuildParam(ModalTypes.QNewRules, Ui.Lang);
                //_qModalPar.BodyParameters.Add(nameof(QModalRender.SlotNo), _selectedId);
                var result = await Ui.Modal.ShowAsync(_qModalPar);

                if (result != ModalResult.Button1)
                    return;
            }
            
            var (suc, msg) = await Api.SendNewQuestionAsync(request);
            //_isSucces = suc;
            //_message = msg;
            await Task.Delay(2);
        }
        private async Task OnDeleteUsrSlotAsync()
        {
            if (!_locNotShowStateDel)
            {
                _qModalPar = MBoxBuilder.BuildParam(ModalTypes.QUsrDelet, Ui.Lang);
                var result = await Ui.Modal.ShowAsync(_qModalPar);

                if (result != ModalResult.Button1)
                    return;
            }
            var (suc, msg) = await Api.ManageSlotAsync(new ManageSlotRequest
            {
                SlotNo = _selectedId,
                ReqType = SlotManageType.DeleteUsr,
            });

        }
        private async Task OnHandlePendSLotAsync()
        {
            _qModalPar = MBoxBuilder.BuildParam(ModalTypes.QPendHandle, Ui.Lang);
            _qModalPar = _qModalPar with {
                ActionText2 = QState.PendingSlots[_selectedId].Status == "Approved" ? _qModalPar.ActionText2 : string.Empty,
            };
            _qModalPar.BodyParameters.Add(nameof(QModalRender.SlotNo), _selectedId);
            var result = await Ui.Modal.ShowAsync(_qModalPar);
            var reqType = new SlotManageType();
            
            if (result == ModalResult.Button1)
                reqType = SlotManageType.DeletePending;
            else if (result == ModalResult.Button2)
                reqType = SlotManageType.MovePending;
            else
                return;


                var (suc, msg) = await Api.ManageSlotAsync(new ManageSlotRequest
                {
                    SlotNo = _selectedId,
                    ReqType = (SlotManageType)reqType,
                });
        }
       
        private void OnSelectId(int id)
        {
            _selectedId = id;  
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
            Ui.Header.OnBackBtnClicked -= UpdateBckClick;
            GC.SuppressFinalize(this);
        }
    }
}


/*     
 *     
  private async Task ModalCallAsync(int btnId)
        {
           await Task.Delay(1);
        }
        private async Task ExecuteModalAsync(int execute)
        {
           
            Console.WriteLine($"ScreenOption execute: {ScreenOption}");
            if (execute < 3)
            {
                
                isSucces = suc;
                message = msg;
            }
            else
            {
                
                
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


//private bool LocNotShowStateDel => AppStates.LocStoreStates.ChkBxNotShowDel ?? false;





            @if (ScreenOption == 0)
            {
                <div class="quest-selector">
                    @foreach (var btn in _qbuttons!)
                    {
                        <ContentBox Header="@btn.Header"
                                    Footer="@btn.Footer"
                                    FooterDisplay="@btn.FooterDisplay"
                                    Size="@btn.Size"
                                    ShowImage="true"
                                    ImageSrc="@btn.ImageSrc"
                                    IsClickable="@btn.IsEnabled"
                                    IsEnabled="@btn.IsEnabled"
                                    ClickId="@btn.ClickId"
                                    OnClick="OnRootBtnClick" />
                    }
                </div>
            }
            else
            {
               
                <div class="box-container">
                   

                    <ContentBox Header=@BoxHeader 
                        Footer="" FooterDisplay=false 
                        Size="@(ScreenOption == 1 ? "halflarge" : "large")" 
                        Resizable="false">
                            <div class="lcd-wrapper">
                                @switch (ScreenOption)
                                {
                                    case 1:
                                        
                                        break;
                                    case 2:
                                       
                                        break;
                                    case 3:
                                       
                                        break;
                                    case 4:
                                       
                                        break;
                                    default:
                                        break;
                                }
                               
                            </div>
                       
                    </ContentBox>
                    
                </div>

            } 
 
 *      private async Task CallPendModal(int selectedid)
        {
            if (selectedid != 100)
            {
                SelectedId = selectedid;
                await ModalCallAsync(1);
            }
        }

 *     
 *       ExtInfo="@_qState.ExtendedInfo"
                                                MessageUsr="@MessageUsr"
                                                isSuccess="@_isSuccess"
 *       [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
 ExtInfo="@_qState.ExtendedInfo"
                                                Slots="@_qState.PendingSlots"
                                                HandleSlot="CallPendModal" 
 *       
   // 
 *       ExtInfo="@_qState.ExtendedInfo"
                                             Slots="@_qState.Userlots"
                                             NotShowDel="@_locNotShowStateDel"
 *    ExtInfo="@_qState.ExtendedInfo"
                                              WorkingCodes="@_workingCodes"
                                              OriginalCodes="@_originalCodes"
                                              MessageFact="@MessageFact"
                                              isSuccess="@_isSuccess"   
 *       //[Inject] private PageHeaderService Header { get; set; } = default!;
        //[Inject] private NavigationManager Nav { get; set; } = default!;
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        //[Inject] private IQuestionState _qState { get; set; } = default!;
        //[Inject] private IApiService Api { get; set; } = default!;
 * 
 *   await _qState.EnsureLoadedAsync();
            _locNotShowStateDel = await _localStorage.GetItemAsync<bool>(LOCAL_NOT_SHOW_DEL);
            _locNotShowStateNew = await _localStorage.GetItemAsync<bool>(LOCAL_NOT_SHOW_NEW);
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