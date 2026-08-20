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

    private string Text(string key) => Ui.Lang[$"profile.{key}"];

    private bool CanEditTeamName =>
        _profile is not null &&
        ProfileRules.CanChangeTeamName(
            _profile.RankEnum,
            _profile.TeamNameChangedUtc,
            DateTime.UtcNow);

    private bool CanSaveTeamName =>
        CanEditTeamName &&
        !_isTeamNameBusy &&
        _teamNameState == TeamNameCheckState.Available &&
        string.Equals(
            _teamNameDraft,
            _checkedTeamName,
            StringComparison.Ordinal) &&
        !string.Equals(
            _teamNameDraft,
            _profile!.TeamName,
            StringComparison.Ordinal);

    private bool CanEditAvatar =>
        _profile is not null &&
        ProfileRules.CanChangeAvatar(_profile.RankEnum);

    private bool CanSaveAvatar =>
        CanEditAvatar &&
        !_isAvatarBusy &&
        !string.Equals(
            _avatarDraft.ToString(),
            _profile!.CaptainAvatar,
            StringComparison.Ordinal);

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
                return Text("TeamName.RequiredRank")
                    .FormatSafe(GetPublicLevel(_profile.TeamNameRequiredRank));
            }

            if (_profile.NextTeamNameChangeUtc is DateTime nextChangeUtc &&
                nextChangeUtc > DateTime.UtcNow)
            {
                return Text("TeamName.NextChange")
                    .FormatSafe(nextChangeUtc.ToLocalTime().ToString("yyyy.MM.dd. HH:mm"));
            }

            return Text("TeamName.ChangeAvailable");
        }
    }

    private string AvatarRuleText =>
        _profile is not null &&
        _profile.RankEnum < _profile.AvatarRequiredRank
            ? Text("Avatar.RequiredRank")
                .FormatSafe(GetPublicLevel(_profile.AvatarRequiredRank))
            : Text("Avatar.ChangeAvailable");

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
        Text($"TeamName.State.{_teamNameState}");

    public async Task ShowAsync()
    {
        _isOpen = true;
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
        if (!CanEditTeamName)
            return;

        _isTeamNameBusy = true;
        var response = await ProfileClient.CheckTeamNameAsync(_teamNameDraft);
        _isTeamNameBusy = false;

        if (response.State is ProfileRequestState.SessionMismatch or
            ProfileRequestState.NotFound or
            ProfileRequestState.ServerError)
        {
            Ui.Toast.Error(Text("Error.Request"));
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
            Ui.Toast.Success(Text("TeamName.SaveSuccess"));
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

        Ui.Toast.Error(Text($"SaveState.{response.State}"));
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
            Ui.Toast.Success(Text("Avatar.SaveSuccess"));
            await OnProfileChanged.InvokeAsync();
            return;
        }

        if (response.Profile is not null)
            ApplyProfile(response.Profile);

        Ui.Toast.Error(Text($"SaveState.{response.State}"));
    }

    private void PreviousAvatar()
    {
        if (!CanEditAvatar || _profile is null)
            return;

        _avatarDraft = _avatarDraft <= ProfileRules.DEFAULT_AVATAR_NO
            ? _profile.AvatarCount - 1
            : _avatarDraft - 1;
    }

    private void NextAvatar()
    {
        if (!CanEditAvatar || _profile is null)
            return;

        _avatarDraft = _avatarDraft >= _profile.AvatarCount - 1
            ? ProfileRules.DEFAULT_AVATAR_NO
            : _avatarDraft + 1;
    }

    private void ApplyProfile(TeamProfileDto profile)
    {
        _profile = profile;
        _teamNameDraft = profile.TeamName;
        _checkedTeamName = string.Empty;
        _teamNameState = TeamNameCheckState.NotChecked;

        _avatarDraft = ProfileRules.TryGetAvatarNumber(
            profile.CaptainAvatar,
            out var avatarNumber)
                ? avatarNumber
                : ProfileRules.DEFAULT_AVATAR_NO;
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
