using KvizCommando.Client.Components;
using KvizCommando.Client.Features.Modal;
using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Pages.Question.Components;
using KvizCommando.Client.Pages.Shared;
using KvizCommando.Client.Services.Audio;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Dto;
using KvizCommando.Client.Services.Visual;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;
using System.ComponentModel.Design;
using System.Globalization;
using System.Text.RegularExpressions;

namespace KvizCommando.Client.Pages.Team
{
    public partial class Team : KcComponentBase, IDisposable
    {
        [CascadingParameter]
        private AppState AppStates { get; set; } = default!;

        private readonly Dictionary<string, ContentBoxVm> _boxes = [];
        private string[] _boxOrder = [];
        private bool _isReady = false;
        private bool _isLoaded = false;
        private SubHeaderParams _subHead = new();
        private string Culture => AppStates.Culture;

        private int _selectedMember =0;
        private int[] _recruitMixer = { 1, 2, 3, 4, 5, 6, 7, 8 };
        private TeamNavigator _teamNav = new();
        private TeamDtos TState => AppStates.Team!;
        private ContentBoxVm Box(string orx) => _boxes[orx];
        private void OnSelectMember(int id) => _teamNav.SelectedMember = id;
        private void BuildBoxes()
        {
            /*
            var callback = new QuestionCallbacks
            {
                OnFactSave = OnFactSaveAsync,
                OnSelectId = OnSelectIdAsync,
                OnDelete = OnDeleteUsrSlotAsync,
                OnHandle = OnHandlePendSLotAsync,
                OnWatch = OnWatchQuestionAsync,
                OnSend = OnSaveToFormAsync
            };*/
            for (int i = 0; i < 9; i++)
            {
                _subHead.HTabs[i] = OrientationLocalizer.GetOrientation(i, Culture); // + ".";
                _subHead.TabEnable[i] = TState.CharCatMask[i] || true; 
                _subHead.TabDisableText[i] = "";
                //_subHead.TabEnable[i] = TState.CharCatMask[i] || (TState.Candidates[i].CanBeHire && TState.TeamInfo.TotalMembers < TState.TeamInfo.MaxMembers);
                //_subHead.TabDisableText[i] = TState.TeamInfo.TotalMembers < TState.TeamInfo.MaxMembers ? (!TState.Candidates[i].CanBeHire ? Ui.Lang["team.Label.PopUp.NotHire"] : "") : Ui.Lang["team.Label.PopUp.NoFree"];
            }
            var boxes = TBoxBuilder.BuildBoxes(TState.RootBoxInfo!, Ui.Lang);
            foreach (var box in boxes)
            {
                _boxes[box.Key] = box.Value;
            }
            _isReady = _isLoaded;
        }
        private void OnBoxClick(int boxId)
        {
            _boxOrder = TBoxBuilder.Root;
            string headerTitle;
            if (Ui.Header.PageIndex == 2)
                _teamNav.SelectedSkill = 0;

            switch (boxId)
            {
                case 2:
                    headerTitle = (Ui.Lang["mainlayout.Header.Team"]);
                    break;
                case 201:
                    _boxOrder = TBoxBuilder.SubTeam;
                    headerTitle = _boxes[TBoxKeyContent.Team.ToString()].Header;
                    break;
                case 202:
                    _boxOrder = TBoxBuilder.SubMember;
                    headerTitle = _boxes[TBoxKeyContent.Member.ToString()].Header;
                    _subHead.EnabledTabs = CheckEnable(TState.CharCatMask, true);
                    _subHead.ActiveIndex = _subHead.EnabledTabs[0];
                    break;
                case 203:
                    _boxOrder = TBoxBuilder.SubRecruit;
                    headerTitle = _boxes[TBoxKeyContent.Recruit.ToString()].Header;
                    _subHead.EnabledTabs = CheckEnable(TState.CharCatMask, false);
                    _subHead.ActiveIndex = _subHead.EnabledTabs[0];
                    break;
                default:
                    headerTitle = (Ui.Lang["mainlayout.Header.Team"]);
                    break;

            }
            Ui.Header.SetTitle(headerTitle, boxId);
            Ui.Header.SetBackBtnEna(boxId > 2);
            StateHasChanged();
        }
        protected override async Task OnInitializedAsync()
        {
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Team"], 2);
            _boxOrder = TBoxBuilder.Root;
            _recruitMixer.Shuffle();

            await Task.Delay(1);
            _isLoaded = true;
            if (_isReady == false)
                BuildBoxes();
            
        }
        protected override void OnInitialized()
        {
            Ui.Header.OnBackBtnClicked += UpdateBckClick;
        }

        private async Task OnSaveButtonPressed(ModifySkillRequest modReq) 
        { 
        }
        private async Task OnHireButtonPressed(int hireCoord)
        { 
        }
        private async Task OnTeamManageButtonPressed(int actionReq)
        { }

        private void UpdateBckClick()
        {
            if (Ui.Header.PageIndex == 2)
                Ui.Nav.NavigateTo("/home");
            else
                OnBoxClick(2);
            InvokeAsync(StateHasChanged);
        }
        public void Dispose()
        {
            //teamModal?.Dispose();
            Ui.Header.OnBackBtnClicked -= UpdateBckClick;
            GC.SuppressFinalize(this);
        }

        internal class TeamNavigator
        {
            internal int SelectedMember { get; set; } = 0;
            internal int SelectedSkill { get; set; } = 0;
        }
        private static int[] CheckEnable(bool[] bools, bool reference)
        {
            int[] result;
            var indexes = new List<int>();

            for (int i = 1; i < bools.Length; i++)
            {
                if (bools[i]==reference || !reference)
                    indexes.Add(i);
            }

            result = [.. indexes];
            return result;
        }
    }
}
/*
 .label-title {
    align-items: center;
}
.lcd-wrapper {
    height: 100%;
    width: 95%;
}
.message-label {
    display: block;
    text-align: center;
    font-size: 16px;
    margin-top: 20px;
}

 *
  //[Inject] private PageHeaderService Header{ get; set; } = default!;
      
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private ITeamState _tState { get; set; } = default!;
        //[Inject] private IApiService Api { get; set; } = default!;

        //private IGeneralInfo _generalInfo { get; set; } = default!;
        private TeamMemberDto SelectedMember { get; set; } = new TeamMemberDto();
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private KcModal? teamModal;
        private ModalBoxVm tModal = new();
        private bool _isLoaded = false;
        private string tabTitle = string.Empty;
        private string[] hTabs { get; set; } = new string[9];
        private string[] vTabs { get; set; } = new string[4];
        private string pictureCode { get; set; } = string.Empty;
        private int activeHTab = 0;
        private int activeVTab = 0;
        private int ModalOption = 0;
        private int[] RecruitMixer = { 1, 2, 3, 4, 5, 6, 7, 8 };
        private int selectedCandidateId = 0;
        private int selectedMemberId = 0;
        private string MessageTeam = string.Empty;
        private bool isTeamSuccess = false;

        private int Selected => activeHTab;

        private (bool[], string[]) tabHEnabled => BuildTabHEnabled();
        private bool picTabEnable => pictureCode != string.Empty;
        protected override async Task OnInitializedAsync()
        {
            
            await _tState.EnsureLoadedAsync();
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Team"],2);
            //_generalInfo = _tState.TeamInfo ?? new TeamExtendedInfo();
            _isLoaded = true;
            RecruitMixer.Shuffle();
           
            
        }
        private void BoxReady()
        {
            tabTitle = Ui.Lang["team.tabPane.General.Title"];
            hTabs = new string[9];
            hTabs[0] = Ui.Lang["team.tabPane.General"] + ".";
            for (int i = 1; i < 9; i++)
            {
                hTabs[i] = OrientationLocalizer.GetOrientShort(i, culture) + ".";
            }
            vTabs[0] = Ui.Lang["team.SubBtn.OverView"];
            vTabs[1] = Ui.Lang["team.SubBtn.Helps"];
            vTabs[2] = Ui.Lang["team.SubBtn.Extra"];
            vTabs[3] = Ui.Lang["team.SubBtn.Hire"];
        }
        private void OnHTabChanged(int index)
        {
            activeHTab = index;
            selectedCandidateId= 0;
            if (index == 0)
            {
                activeVTab = 0;
                vTabs[1] = Ui.Lang["team.SubBtn.Helps"];
                pictureCode = string.Empty;
                //_generalInfo = _tState.TeamInfo;
            }
            else
            {
                if (_tState.Charmask[index])
                {
                    activeVTab = 0;
                    SelectedMember = _tState.TeamMembers[index];
                    //_generalInfo = SelectedMember;
                    //pictureCode = SelectedMember.MemberPictureCode;
                }
                else
                {
                    activeVTab = 3;
                    pictureCode = string.Empty;
                }        
                vTabs[1] = Ui.Lang["team.SubBtn.Second"];          
            }
            tabTitle = TitleManager();
        }
        private void OnVTabChanged(int index)
        {
            activeVTab = index;
        }
        private async Task OnSaveButtonPressed(ModifySkillRequest request)
        {
            try
            {
               
                var (success, message) = await Api.ModifyTeamAsync(request);
                MessageTeam = message;
                isTeamSuccess = success;
            
                if (isTeamSuccess)
                    _tState.Invalidate();
                await _tState.EnsureLoadedAsync();
                //_generalInfo = activeHTab == 0 ? _tState.TeamInfo : _tState.TeamMembers[activeHTab];
                await InvokeAsync(StateHasChanged);
                await Task.Delay(500);
                MessageTeam = string.Empty;
            }
            catch (Exception ex) { Console.WriteLine($"Save failed: {ex.Message}"); }
            finally 
            {
                
            }
            
        }
        private async Task OnHireButtonPressed()
        {
            selectedMemberId = 0;
            if (selectedCandidateId == 0) return;
            selectedMemberId = activeHTab;
            await ModalCallAsync(0);

        }
        private async Task OnTeamManagerButtonPressed(int actionreq)
        {
            selectedMemberId = 0;
            if (actionreq == 0) return;
            int action = actionreq / 100;
            if (action > 3) return;
            int member = actionreq -action * 100;
            int skill = actionreq - action * 100;
            if (member < 1 || member>9) return;
            if (actionreq > 100)
            {
                selectedMemberId = member;
                SelectedMember = _tState.TeamMembers[member];
                await ModalCallAsync(action);
            }
            else 
            {
                if (activeHTab == 0)
                    OnHTabChanged(member);
                else
                    OnVTabChanged(skill > 3 ? 2 : 1);                                
            }
            
        }
        private async Task ModalCallAsync(int btnId)
        {
            var spec = MBoxBuilder.BuildParam(ModalTypes.THandle, Ui.Lang);
            ModalOption = (int)spec.Mode;
            tModal = spec;
            if (btnId != 3)
            {
                tModal = tModal with
                {
                    //AsyncAction1 = () => ExecuteModalAsync(btnId)
                };
            }
            else
            {
                tModal = tModal with
                {
                    ActionText2 = SelectedMember.SkillPoints>0 ? tModal.ActionText2 : string.Empty,
                    //AsyncAction1 = () => ExecuteModalAsync(btnId),
                    //AsyncAction2 = () => ExecuteModalAsync(btnId+1)
                };
            }
            
            await teamModal!.ShowAsync(tModal);
        }
        private async Task ExecuteModalAsync(int mode)
        {
            try
            {
               
                var response = await Api.ManageTeamAsync(new ManageTeamRequest()
                {
                    CandidateId = selectedCandidateId,
                    ReqType = (ManageType)mode,
                    MemberNo = selectedMemberId
                });
                isTeamSuccess = response.Success;
                MessageTeam = response.Message;
                if (isTeamSuccess)
                {
                    _tState.Invalidate();
                    activeHTab = 0;
                    activeVTab = 0;
                    pictureCode = string.Empty;
                    selectedMemberId = 0;
                }
                await _tState.EnsureLoadedAsync();
                //_generalInfo = _tState.TeamInfo;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(500);
                MessageTeam = string.Empty;
            }
            catch (Exception ex) { Console.WriteLine($"Save failed: {ex.Message}"); }
            
            finally { }
            
        }
        private void OnCandidateSelected(int candidateId)
        {
            selectedCandidateId = candidateId;
            pictureCode = candidateId == 0 ? string.Empty : _tState.Candidates[activeHTab].PictureCode[candidateId - 1] ?? string.Empty;
        }
        private void CloseModalAction()
        {
            ModalOption = 0;
        }
        private (bool[], string[]) BuildTabHEnabled()
        {
            var result = (new bool[9], new string[9]);
            string[] reason = new string[9];
            result.Item1[0] = true;
            for (int i = 1; i < result.Item1.Length; i++)
            {
                result.Item1[i] = _tState.Charmask[i] || (_tState.Candidates[i].CanBeHire && _tState.TeamInfo.TotalMembers < _tState.TeamInfo.MaxMembers) ;
                result.Item2[i] = _tState.TeamInfo.TotalMembers < _tState.TeamInfo.MaxMembers ? (!_tState.Candidates[i].CanBeHire ? Ui.Lang["team.Label.PopUp.NotHire"] : "") : Ui.Lang["team.Label.PopUp.NoFree"]; 
            }
            return result;
        }
        private string TitleManager()
        {
            string title = string.Empty;
            int att1Name = activeHTab;
            if (activeHTab == 0)
            { title = Ui.Lang["team.tabPane.General.Title"]; }
            else if (activeVTab == 3)
            { title = OrientationLocalizer.GetOrientation(att1Name, culture) + " " + Ui.Lang["team.tabPane.Recruit.Title"]; }
            else
            { title = Ui.Lang["team.tabPane.Member.Title"]; }

            return title;
        }




















 <HorizontalTabPane ActiveHIndex="@activeHTab" 
                                   ActiveHIndexChanged="@OnHTabChanged"
                                   TabHEnabled="@tabHEnabled"
                                   ActiveVIndex="@activeVTab"
                                   ActiveVIndexChanged="@OnVTabChanged"
                                   TabTitle="@tabTitle"
                                   HTabs="@hTabs"
                                   VTabs="@vTabs"
                                   PicTabEnabled="@picTabEnable">
                                  

                    <Content1>
                    <CascadingValue Value="@Selected">
                        <UpperBlockDisplay />
                        <BottomBlockDisplay ActionButtonPushed="OnTeamManagerButtonPressed" />
                    </CascadingValue>
                    </Content1>
                    <Content2>
                       
                    </Content2>
                    <Content3>
                        <BottomBlockViaButtons DatatoProc="@_generalInfo"
                                               DataHelp="@_tState.Help"
                                               TabPosH="@activeHTab"
                                               TabPosV="@activeVTab"
                                               ModifySkill="OnSaveButtonPressed" />
                    </Content3>
                    <Content4>
                        <RecruitBlockDisplay RecruitData="@_tState.Candidates![activeHTab]"
                                            TabPos="@activeHTab"
                                            candidateOrder="@RecruitMixer"
                                            SelectedCandidate="OnCandidateSelected" />
                    </Content4>

                     <Content5>
                        
                    </Content5>
                    <ButtonRow>
                    @if (activeVTab == 3)
                    {
                        <button class="military-button" disabled="@(selectedCandidateId == 0)" @onclick="OnHireButtonPressed">@(Ui.Lang["team.Button.Hire"])</button>
                    }
                    </ButtonRow>
                </HorizontalTabPane>

 <KcModal @ref="teamModal"
                 Id="teamModal"
          
                 OnCloseAction="CloseModalAction"
                 @if (ModalOption>0)
                 {
                 <TModalRender mode="@ModalOption"
                                  tabPosH="@activeHTab"
                                  canNo="@selectedCandidateId"
                                  Candidate="@_tState.Candidates![activeHTab]"
                                  TeamMember="@SelectedMember" />
                 }
                 </KcModal>
 
 */