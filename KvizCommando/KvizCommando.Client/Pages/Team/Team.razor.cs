using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels;
using KvizCommando.Client.Models.ViewModels.Ui;
using KvizCommando.Client.Pages.Shared.Modal.Dynamic;
using KvizCommando.Client.Pages.Shared.Modal.Features;
using KvizCommando.Client.Pages.Team.Features;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Team;
using KvizCommando.Shared.Models.Dtos;
using Microsoft.AspNetCore.Components;

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

        private int _selectedPos = 0;
        private static readonly int[] _recruitMixer = [1, 2, 3, 4, 5, 6, 7, 8];

        private string Culture => AppStates.Culture;
        private bool[] TCharmask => AppStates.Team!.CharCatMask;
        private TeamExtendedInfo TInfo => AppStates.Team!.TeamInfo;
        private TeamRootBoxInfo TRootBoxInfo => AppStates.Team!.RootBoxInfo;

        private ContentBoxVm Box(string orx) => _boxes[orx];
        private void BuildBoxes()
        {
            var callback = new TeamCallbacks
            {
                OnHire = OnHireAsync,
                OnManage = OnTeamManagedAsync,
                OnModify = OnSavedAsync,
                OnShuffledIds = _recruitMixer
            };
            var boxes = TBoxBuilder.BuildBoxes(TRootBoxInfo, callback, Ui.Lang);
            foreach (var box in boxes)
            {
                _boxes[box.Key] = box.Value;
            }
            _isReady = _isLoaded;
        }
        private void OnBoxClick(int boxId)
        {
            var list = new List<SubHeaderVm>();
            string headerTitle;
            string[] boxOrder;
            int selectedPos = 0;
            switch (boxId)
            {
                case 2:
                    headerTitle = (Ui.Lang["mainlayout.Header.Team"]);
                    boxOrder = TBoxBuilder.Root;
                    break;
                case 201:
                    boxOrder = TBoxBuilder.SubTeam;
                    headerTitle = _boxes[TBoxKeyContent.Team.ToString()].Header;
                    break;
                case 202:
                    boxOrder = TBoxBuilder.SubMember;
                    headerTitle = _boxes[TBoxKeyContent.Member.ToString()].Header;
                    selectedPos = Array.FindIndex(TCharmask, 1, x => x);
                    list = THelpers.SubHeaderResolver(TCharmask[1..9], Culture);
                    break;
                case 203:
                    boxOrder = TBoxBuilder.SubRecruit;
                    headerTitle = _boxes[TBoxKeyContent.Recruit.ToString()].Header;
                    selectedPos = Array.FindIndex(TInfo.AbleToHireMask, 1, x => x);
                    list = THelpers.SubHeaderResolver(TInfo.AbleToHireMask[1..9], Culture);
                    break;
                default:
                    boxOrder = TBoxBuilder.Root;
                    headerTitle = (Ui.Lang["mainlayout.Header.Team"]);
                    break;

            }
            ShowBoxes(headerTitle, boxId, selectedPos, boxOrder, list);
        }
        private void ShowBoxes(string title, int bxId, int startPos, string[] order, List<SubHeaderVm> list)
        {
            _selectedPos = startPos;
            _boxOrder = order;
            Ui.Header.SetTitle(title, bxId);
            Ui.Header.SetBackBtnEna(bxId > 2);
            Ui.SubHeader.Show(list, _selectedPos);
            StateHasChanged();
        }

        protected override async Task OnInitializedAsync()
        {
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Team"], 2);
            _boxOrder = TBoxBuilder.Root;
            _selectedPos = 0;
            _recruitMixer.Shuffle();
            await Task.Delay(1);
            _isLoaded = true;
            if (_isReady == false)
                BuildBoxes();

        }
        protected override void OnInitialized()
        {
            Ui.Header.OnBackBtnClicked += UpdateBckClick;
            Ui.SubHeader.OnButtonClicked += OnSubHeaderClicked;
        }

        private async Task OnSavedAsync(ModifySkillRequest modReq)
        {

            var success = await Api.ModifyTeamAsync(modReq);

            if (!success)
                return;

            await Ui.ReloadAsync();
            BuildBoxes();
        }
        private async Task OnHireAsync(int hireCoord)
        {
            var selectedPos = hireCoord / 100;
            var selectedCandidate = hireCoord % 100;

            var mVm = MBoxBuilder.BuildParam(ModalTypes.THire, Ui.Lang);

            mVm.BodyParameters.Add(nameof(TModalRender.SelectedMember), selectedPos);
            mVm.BodyParameters.Add(nameof(TModalRender.CanDidateNo), selectedCandidate);

            var result = await Ui.Modal.ShowAsync(mVm);

            if (result != ModalResult.Button1)
                return;

            var success = await Api.ManageTeamAsync(
                new ManageTeamRequest
                {
                    ReqType = ManageType.Hire,
                    MemberNo = selectedPos,
                    CandidateId = selectedCandidate
                });

            if (!success)
                return;

            await Ui.ReloadAsync();
            BuildBoxes();

            ShowBoxes(
              _boxes[TBoxKeyContent.Team.ToString()].Header,
                     201,
                     0,
                     TBoxBuilder.SubTeam,
                     []
                    );
        }
        private async Task OnTeamManagedAsync(int actionReq)
        {
            if (actionReq > 50 && actionReq < 100)
            {
                ShowBoxes(
                    _boxes[TBoxKeyContent.Member.ToString()].Header,
                     203,
                     actionReq % 50,
                     TBoxBuilder.SubMember,
                     THelpers.SubHeaderResolver(TCharmask[1..9], Culture)
                    );
                return;
            }
            var modalAction = actionReq / 100;
            var selectedMember = actionReq % 100;
            var modalType = modalAction switch
            {
                1 => ModalTypes.TPromote,
                2 => ModalTypes.TRetire,
                3 => ModalTypes.THandle,
                4 => ModalTypes.THandle,
                _ => ModalTypes.None
            };
            var mVm = MBoxBuilder.BuildParam(modalType, Ui.Lang);

            if (modalAction == 3)
                mVm = mVm with { ActionText2 = string.Empty };

            mVm.BodyParameters.Add(nameof(TModalRender.SelectedMember), selectedMember);
            mVm.BodyParameters.Add(nameof(TModalRender.CanDidateNo), 0);

            var result = await Ui.Modal.ShowAsync(mVm);

            if (result != ModalResult.Button1 && result != ModalResult.Button2)
                return;

            if (modalAction == 4 && result != ModalResult.Button1)
                modalAction = 3; // ha button 1-et nyomja meg akkor is kiruja a tagot ha meg tudná gyógyítani

            var success = await Api.ManageTeamAsync(
                new ManageTeamRequest
                {
                    ReqType = (ManageType)modalAction,
                    MemberNo = selectedMember,
                    CandidateId = 0
                });

            if (!success)
                return;

            await Ui.ReloadAsync();
            BuildBoxes();
        }
        private void UpdateBckClick()
        {
            if (Ui.Header.PageIndex == 2)
                Ui.Nav.NavigateTo("/home");
            else
                OnBoxClick(2);
            InvokeAsync(StateHasChanged);
        }
        private void OnSubHeaderClicked(int index) => _selectedPos = index;

        public void Dispose()
        {
            //teamModal?.Dispose();
            Ui.SubHeader.OnButtonClicked -= OnSubHeaderClicked;
            Ui.SubHeader.Hide();
            Ui.Header.OnBackBtnClicked -= UpdateBckClick;
            GC.SuppressFinalize(this);
        }

        private static int[] CheckEnable(bool[] bools, bool reference)
        {
            int[] result;
            var indexes = new List<int>();

            for (int i = 1; i < bools.Length; i++)
            {
                if (bools[i] == reference || !reference)
                    indexes.Add(i);
            }

            result = [.. indexes];
            return result;
        }
    }
}
/*


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
                    //ActionText2 = SelectedMember.SkillPoints>0 ? tModal.ActionText2 : string.Empty,
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