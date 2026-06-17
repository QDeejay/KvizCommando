
using Blazored.LocalStorage;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Features.Sologame;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Language;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Shared.Contracts.Question;
using Microsoft.AspNetCore.Components;
using System.Globalization;



namespace KvizCommando.Client.Pages.Question
{
    public partial class Question : ComponentBase, IDisposable
    {
        [Inject] protected PageTitleService PageTitle { get; set; } = default!;
        [Inject] protected ILoadingService Loader { get; set; } = default!;
        [Inject] protected ILanguageService Lang { get; set; } = default!;
        [Inject] protected IQuestionState QuestionState { get; set; } = default!;
        [Inject] protected IApiService QuestApi { get; set; } = default!;
        [Inject] protected HttpClient Http { get; set; } = default!;
        [Inject] protected ILocalStorageService LocalStorage { get; set; } = default!;

        private int SelectedId { get; set; }
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private KcModal? questionModal;
        private ModalPar qModal = new();

        private NewQuestionRequest formData = new();
        private MarkupString renderHTML = new();

        private string MessageFact = string.Empty;
        private string MessageUsr = string.Empty;
        private string BoxHeader = "PLACEHOLDER";

        private const string localNotShowNew = "notShowNew";
        private const string localNotShowDel = "notShowDel";

        private string BoxBtnText = string.Empty;
        private string BoxBtnStyle = string.Empty;
        private Func<Task> OnBoxBtnClick { get; set; } = default!;
        private bool localNotShowState { get; set; }
        private bool localNotShowStateDel { get; set; }

        private bool isSuccess = false;
        private int ScreenOption = 0;
        private int ModalOption = 0;
 
        private bool _isLoaded = false;
        private int[] OriginalCodes = new int[10];
        private int[] WorkingCodes = Array.Empty<int>();
        private string SubHeaderTitle = string.Empty;
        private List<ContentBoxVm>? _qbuttons;

       

        private void BuildButtons()
        {
            
            if (QuestionState.Snapshot != null)
            {
                //_qbuttons = QBtnBtnBuilder.BuildButtons(QuestionState.ExtendedInfo!, Lang);
                _isLoaded = true;
            }
        }
        protected override async Task OnInitializedAsync()
        {
            await Loader.Show();
            await QuestionState.EnsureLoadedAsync();
            localNotShowStateDel = await LocalStorage.GetItemAsync<bool>(localNotShowDel);
            localNotShowState = await LocalStorage.GetItemAsync<bool>(localNotShowNew);

            if (localNotShowState == false)
            {
                string Url = $"/Manuals/{culture}/questionsend.html";
                renderHTML = new MarkupString(await Http.GetStringAsync(Url));
            }
            else renderHTML = new MarkupString(string.Empty);

            PageTitle.SetTitle(Lang["mainlayout.Header.Question"],101,-1);

            (OriginalCodes, WorkingCodes) = QuestionHelper.CloneFactorySlots(QuestionState.FactorySlots);
            await Loader.Hide();
            if (_isLoaded==false)
                BuildButtons();
        }
        
        private void OnHeadButtonClick() { OnRootBtnClick(1); }
        private void OnRootBtnClick(int btnId)
        {
            Console.WriteLine($"megnyomta a paraszt következő gombot: {btnId}");
            ModalOption = 0;
            SelectedId =100;
            SubHeaderTitle = string.Empty;
            BoxBtnText = string.Empty;
            BoxBtnStyle = string.Empty;
            OnBoxBtnClick = default!;
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
                    OnBoxBtnClick = () => ModalCallAsync(0);
                    BoxBtnText= localNotShowStateDel ? Lang["question.Button.Delete"] : Lang["question.Button.Handle"];
                    BoxBtnStyle = localNotShowStateDel ? "background-color: #a64b2a" : "background-color: #4b5320";
                    break;
                case 13:
                    ScreenOption = 3;
                    BoxHeader = Lang["question.Box.Title.PendingSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedPendingSlot, QuestionState.ExtendedInfo.AvailableUserSlot >> 1);
                    SubHeaderTitle = _qbuttons?[2].Header ?? string.Empty;
                    OnBoxBtnClick = () => ModalCallAsync(1);
                    BoxBtnText = Lang["question.Button.Handle"];
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
        private async Task OnFactSaveAsync()
        {
            try
            {
                var (success, message) = await QuestApi.SaveFactorySlotsAsync(new SaveFactoryRequest
                { CategorySlots = WorkingCodes });
                MessageFact = message;
                isSuccess = success;
                QuestionState.Invalidate();
            }
            catch (Exception ex) { Console.WriteLine($"Save failed: {ex.Message}"); }
            finally { }
            for (var i = 0; i < WorkingCodes.Length; i++)
            {
                OriginalCodes[i] = WorkingCodes[i];
            }
            QuestionState.Invalidate();
            await QuestionState.EnsureLoadedAsync();
            await InvokeAsync(StateHasChanged);
            await Task.Delay(1000);
            MessageFact = string.Empty;
            OnRootBtnClick(11);
        }
        private async Task OnSelectId(int id)
        {
            SelectedId = id;
            await Task.Delay(1);
            
        }
        private async Task ModalCallAsync(int btnId)
        {
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
        }
        private async Task ExecuteModalAsync(int execute)
        {
            bool isSucces = false;
            string message = string.Empty;
            Console.WriteLine($"ScreenOption execute: {ScreenOption}");
            if (execute < 3)
            {
                var (suc, msg) = await QuestApi.ManageSlotAsync(new ManageSlotRequest
                {
                    SlotNo = SelectedId,
                    ReqType = (SlotManageType)execute,
                });
                isSucces = suc;
                message = msg;
            }
            else
            {
                int index = Array.FindIndex(QuestionState.PendingSlots!, x => x.Category == 0);
                var (suc, msg) = await QuestApi.SendNewQuestionAsync( new NewQuestionRequest
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
                (OriginalCodes, WorkingCodes) = QuestionHelper.CloneFactorySlots(QuestionState.FactorySlots);
                QuestionState.Invalidate();
                await QuestionState.EnsureLoadedAsync();
                isSuccess = true;
            }
            else
            {
                MessageUsr = message;
            }
            await InvokeAsync(StateHasChanged);

            if (MessageUsr != string.Empty) { await Task.Delay(1000); MessageUsr = string.Empty; }
            ModalOption = 0;
            OnRootBtnClick(ScreenOption+10);
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
        public void Dispose()
        {
            questionModal?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}


/*
 * 
 *   private async Task ExecuteUsrDel(int selectedid)
        {
            if (selectedid != 100)
            {
                SelectedId = selectedid;
                await ExecuteModalAsync(0);
            }
        }
 *  private async Task CallUsrModal(int selectedid)
        {
            if (selectedid != 100)
            {
                SelectedId = selectedid;
                await ModalCallAsync(0);
            }
        }
 BoxHeader = ScreenOption == 1
                    ? Lang["question.Box.Title.UsrSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedUserSlot, QuestionState.ExtendedInfo.AvailableUserSlot)
                    : Lang["question.Box.Title.PendingSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedPendingSlot, QuestionState.ExtendedInfo.AvailablePendingSlot);
               
                ScreenOption = ScreenOption == 3 ? 2 : ScreenOption; // Frissítjük a képernyőt  



 *  if (QuestionState.ExtendedInfo.UserSlotEnable)
            {
                ScreenOption = 1;
                BoxHeader = Lang["question.Box.Title.UsrSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedUserSlot, QuestionState.ExtendedInfo.AvailableUserSlot);
            }
            else
            {
                ScreenOption = 0;
                BoxHeader = Lang["question.Box.Title.UsrSlots.NoData"];
            }
  private void GotoNextScreen(int screenno)
        {
            ModalOption = 0;
            qModal = qModal with { Mode = 0 };
            if (screenno > 2)
            {
                NewQuestionSlotNo = screenno - 3;
                ScreenOption = 3;
            }
            else
            {
                ScreenOption = screenno;
            }

            switch (ScreenOption)
            {
                case 1:
                    BoxHeader = Lang["question.Box.Title.UsrSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedUserSlot, QuestionState.ExtendedInfo.AvailableUserSlot);
                    break;
                case 2:
                    BoxHeader = Lang["question.Box.Title.PendingSlots"].FormatSafe(QuestionState.ExtendedInfo.OccupiedPendingSlot, QuestionState.ExtendedInfo.AvailableUserSlot >> 1);
                    break;
                case 3:
                    BoxHeader = Lang["question.Modal.Title.New"];
                    break;
            }
        }
 * 
 * 
 * 
  <div class="box-row">
                <div class="box">
                    <ContentBox Header=@Lang["question.Box.Title.FactorySlots"] Footer="" FooterDisplay=false Size="halflarge" Resizable="false">
                        <div class="lcd-wrapper">
                            
                       
                        </div>
                    </ContentBox>
                </div>

 
 
 */