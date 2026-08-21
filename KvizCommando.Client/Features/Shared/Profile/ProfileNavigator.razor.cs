using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Utilities;
using KvizCommando.Shared.Contracts.Profile;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Shared.Profile;

public partial class ProfileNavigator : KcComponentBase
{
    private enum ProfileSection { Team, Account }
    [Inject] private IProfileClientService ProfileClient { get; set; } = default!;

    [Parameter] public AppState AppStates { get; set; } = default!;
    [Parameter] public EventCallback OnProfileChanged { get; set; }

    private TeamProfileDto? _profile;
    private string _teamNameDraft = string.Empty;
    private string _checkedTeamName = string.Empty;
    private TeamNameCheckState _teamNameState = TeamNameCheckState.NotChecked;
    private int _avatarDraft = ProfileRules.DEFAULT_AVATAR_NO;
    private bool _isOpen;
    private bool _isLoading;
    private bool _isTeamNameBusy;
    private bool _isAvatarBusy;
    private ProfileSection _activeSection = ProfileSection.Team;

    private bool CanEditTeamName =>
        _profile is not null &&
        ProfileRules.CanChangeTeamName(
            _profile.RankEnum,
            _profile.TeamNameChangedUtc,
            DateTime.UtcNow);

    private bool HasTeamNameChanged =>
        _profile is not null &&
        !string.Equals(
            _teamNameDraft.Trim(),
            _profile.TeamName.Trim(),
            StringComparison.OrdinalIgnoreCase);

    private bool CanCheckTeamName =>
        CanEditTeamName &&
        !_isTeamNameBusy &&
        HasTeamNameChanged;

    private bool CanSaveTeamName =>
        CanEditTeamName &&
        !_isTeamNameBusy &&
        HasTeamNameChanged &&
        _teamNameState == TeamNameCheckState.Available &&
        string.Equals(
            _teamNameDraft.Trim(),
            _checkedTeamName,
            StringComparison.Ordinal) &&
        !string.IsNullOrWhiteSpace(_checkedTeamName);

    private bool CanEditAvatar =>
        _profile is not null &&
        ProfileRules.CanChangeAvatar(_profile.RankEnum);

    private bool CanSaveAvatar =>
        CanEditAvatar &&
        !_isAvatarBusy &&
        _avatarDraft != ProfileRules.GetAvatarNumber(
            _profile!.CaptainAvatar);

    private string AvatarImageSrc =>
        $"images/avatars/avatar-{_avatarDraft:D2}.webp";

    private string TeamNameRuleText
    {
        get
        {
            if (_profile is null)
                return string.Empty;

            if (_profile.RankEnum < _profile.TeamNameRequiredRank)
            {
                return Ui.Lang["profile.TeamName.RequiredRank"]
                    .FormatSafe(GetPublicLevel(_profile.TeamNameRequiredRank));
            }

            if (_profile.NextTeamNameChangeUtc is DateTime nextChangeUtc &&
                nextChangeUtc > DateTime.UtcNow)
            {
                return Ui.Lang["profile.TeamName.NextChange"]
                    .FormatSafe(nextChangeUtc.ToLocalTime().ToString("yyyy.MM.dd. HH:mm"));
            }

            return Ui.Lang["profile.TeamName.ChangeAvailable"];
        }
    }

    private string AvatarRuleText =>
        _profile is null
            ? string.Empty
            : Ui.Lang["profile.Avatar.RequiredRank"]
                .FormatSafe(GetPublicLevel(_profile.AvatarRequiredRank));

    private string TeamNameStatusSymbol => _teamNameState switch
    {
        TeamNameCheckState.Available => "✓",
        TeamNameCheckState.NotChecked => "○",
        _ => "✕"
    };

    private string TeamNameStatusClass => _teamNameState switch
    {
        TeamNameCheckState.Available => "available",
        TeamNameCheckState.NotChecked => "neutral",
        _ => "invalid"
    };

    private string TeamNameStatusText =>
        Ui.Lang[$"profile.TeamName.State.{_teamNameState}"];

    public async Task ShowAsync()
    {
        _isOpen = true;
        _activeSection = ProfileSection.Team;
        _isLoading = true;
        _profile = null;
        ResetTeamNameCheck();
        StateHasChanged();

        await Ui.Lang.LoadModuleAsync(AppStates.Culture, "profile");
        var response = await ProfileClient.GetAsync();

        if (response.State == ProfileRequestState.Success &&
            response.Profile is not null)
        {
            ApplyProfile(response.Profile);
        }

        _isLoading = false;
        StateHasChanged();
    }

    private void Close()
    {
        _isOpen = false;
        _isLoading = false;
        _isTeamNameBusy = false;
        _isAvatarBusy = false;
        _profile = null;
        ResetTeamNameCheck();
    }

    private void OnTeamNameInput(ChangeEventArgs args)
    {
        _teamNameDraft = args.Value?.ToString() ?? string.Empty;
        ResetTeamNameCheck();
    }

    private async Task CheckTeamNameAsync()
    {
        if (!CanCheckTeamName)
            return;

        _isTeamNameBusy = true;
        var response = await ProfileClient.CheckTeamNameAsync(_teamNameDraft);
        _isTeamNameBusy = false;

        if (response.State is ProfileRequestState.SessionMismatch or
            ProfileRequestState.NotFound or
            ProfileRequestState.ServerError)
        {
            Ui.Toast.Error(Ui.Lang["profile.Error.Request"]);
            ResetTeamNameCheck();
            return;
        }

        _teamNameDraft = response.CheckedTeamName;
        _checkedTeamName = response.CheckedTeamName;
        _teamNameState = response.TeamNameState;
    }

    private async Task SaveTeamNameAsync()
    {
        if (!CanSaveTeamName)
            return;

        _isTeamNameBusy = true;
        var response = await ProfileClient.SaveTeamNameAsync(_teamNameDraft);
        _isTeamNameBusy = false;

        if (response.State == ProfileRequestState.Success &&
            response.Profile is not null)
        {
            ApplyProfile(response.Profile);
            Ui.Toast.Success(Ui.Lang["profile.TeamName.SaveSuccess"]);
            await OnProfileChanged.InvokeAsync();
            return;
        }

        if (response.State == ProfileRequestState.TeamNameRejected)
        {
            _teamNameState = response.TeamNameState;
            _checkedTeamName = _teamNameDraft;
        }
        else if (response.Profile is not null)
        {
            ApplyProfile(response.Profile);
        }

        Ui.Toast.Error(Ui.Lang[$"profile.SaveState.{response.State}"]);
    }

    private async Task SaveAvatarAsync()
    {
        if (!CanSaveAvatar)
            return;

        _isAvatarBusy = true;
        var response = await ProfileClient.SaveAvatarAsync(
            _avatarDraft.ToString());
        _isAvatarBusy = false;

        if (response.State == ProfileRequestState.Success &&
            response.Profile is not null)
        {
            ApplyProfile(response.Profile);
            Ui.Toast.Success(Ui.Lang["profile.Avatar.SaveSuccess"]);
            await OnProfileChanged.InvokeAsync();
            return;
        }

        if (response.Profile is not null)
            ApplyProfile(response.Profile);

        Ui.Toast.Error(Ui.Lang[$"profile.SaveState.{response.State}"]);
    }

    private void PreviousAvatar()
    {
        if (!CanEditAvatar || _profile is null)
            return;

        _avatarDraft = _avatarDraft <= ProfileRules.MIN_AVATAR_NO
            ? ProfileRules.MAX_AVATAR_NO
            : _avatarDraft - 1;
    }

    private void NextAvatar()
    {
        if (!CanEditAvatar || _profile is null)
            return;

        _avatarDraft = _avatarDraft >= ProfileRules.MAX_AVATAR_NO
            ? ProfileRules.MIN_AVATAR_NO
            : _avatarDraft + 1;
    }

    private void ApplyProfile(TeamProfileDto profile)
    {
        _profile = profile;
        _teamNameDraft = profile.TeamName;
        _checkedTeamName = string.Empty;
        _teamNameState = TeamNameCheckState.NotChecked;

        _avatarDraft = ProfileRules.GetAvatarNumber(
            profile.CaptainAvatar);
    }

    private void ResetTeamNameCheck()
    {
        _checkedTeamName = string.Empty;
        _teamNameState = TeamNameCheckState.NotChecked;
    }

    private static string GetPublicLevel(int rankEnum) =>
        rankEnum >= 0 && rankEnum < RankNameTable.Data.Count
            ? RankNameTable.Data[rankEnum].PublicLevel
            : RankNameTable.Data[0].PublicLevel;
}
