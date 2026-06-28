using KvizCommando.Client.Features.Question;
using KvizCommando.Client.Helpers;
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

namespace KvizCommando.Client.Pages.Team
{
    public partial class Team : KcComponentBase, IDisposable
    {
        //[Inject] private PageHeaderService Header{ get; set; } = default!;
      
        //[Inject] private ILanguageService Lang { get; set; } = default!;
        [Inject] private ITeamState _tState { get; set; } = default!;
        //[Inject] private IApiService Api { get; set; } = default!;

        private IGeneralInfo _generalInfo { get; set; } = default!;
        private TeamMemberDto SelectedMember { get; set; } = new TeamMemberDto();
        private string culture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        private KcModal? teamModal;
        private ModalPar tModal = new();
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
        private (bool[], string[]) tabHEnabled => BuildTabHEnabled();
        private bool picTabEnable => pictureCode != string.Empty;
        protected override async Task OnInitializedAsync()
        {
            
            await _tState.EnsureLoadedAsync();
            Ui.Header.SetTitle(Ui.Lang["mainlayout.Header.Team"],2);
            _generalInfo = _tState.TeamInfo ?? new ExtendedInfo();
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
                _generalInfo = _tState.TeamInfo;
            }
            else
            {
                if (_tState.Charmask[index])
                {
                    activeVTab = 0;
                    SelectedMember = _tState.TeamMembers[index];
                    _generalInfo = SelectedMember;
                    pictureCode = SelectedMember.MemberPictureCode;
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
                _generalInfo = activeHTab == 0 ? _tState.TeamInfo : _tState.TeamMembers[activeHTab];
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
            var spec = ModalSpecs.Specs[btnId];
            ModalOption = spec.Mode;
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
            
            await teamModal!.ShowAsync();
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
                _generalInfo = _tState.TeamInfo;
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
        public void Dispose()
        {
            teamModal?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
