using KvizCommando.Client.Helpers;
using KvizCommando.Client.Layout;
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
        private bool[] TCharmask => AppStates.Team!.CharCatMask[1..9];
        private bool[] TCanHireMask => AppStates.Team!.TeamInfo.AbleToHireMask[1..9];
        private TeamExtendedInfo TInfo => AppStates.Team!.TeamInfo;
        private TeamRootBoxInfo TRootBoxInfo => AppStates.Team!.RootBoxInfo;

        private CandidateDto[] TCandidates => AppStates.Team!.Candidates[1..9];


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
                    selectedPos = Array.FindIndex(TCharmask, x => x) + 1;
                    list = THelpers.SubHeaderResolver(TCharmask, TCharmask, new string[8], Culture);
                    break;
                case 203:
                    boxOrder = TBoxBuilder.SubRecruit;
                    headerTitle = _boxes[TBoxKeyContent.Recruit.ToString()].Header;
                    selectedPos = Math.Max(Array.FindIndex(TInfo.AbleToHireMask, x => x), 0);
                    bool[] invertedMask = [.. TCharmask.Select(x => !x)];
                    var toolTips = THelpers.RecruitToolTipResolver(TCharmask, TCandidates, Ui.Lang);
                    list = THelpers.SubHeaderResolver(invertedMask, TCanHireMask, toolTips, Culture);
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

            await Ui.ReloadAsync(ReqStates.Team);
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

            await Ui.ReloadAsync(ReqStates.All);
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
                     THelpers.SubHeaderResolver(TCharmask, TCharmask, new string[8], Culture)
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

            var reqState = modalAction == 2 || modalAction == 3 ? ReqStates.All : ReqStates.Team;
            await Ui.ReloadAsync(reqState);
            BuildBoxes();
        }

        public void Dispose()
        {
            //teamModal?.Dispose();
            Ui.SubHeader.OnButtonClicked -= OnSubHeaderClicked;
            Ui.SubHeader.Hide();
            Ui.Header.OnBackBtnClicked -= UpdateBckClick;
            GC.SuppressFinalize(this);
        }

        private void UpdateBckClick()
        {
            BuildBoxes();
            if (Ui.Header.PageIndex == 2)
                Ui.Nav.NavigateTo("/home");
            else
                OnBoxClick(2);
            InvokeAsync(StateHasChanged);
        }
        private void OnSubHeaderClicked(int index) => _selectedPos = index;

    }
}