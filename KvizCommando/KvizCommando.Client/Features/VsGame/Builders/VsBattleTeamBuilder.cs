using KvizCommando.Client.Features.VsGame.ViewModels;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Client.Features.VsGame.Builders;

public sealed class VsBattleTeamBuilder
{
    private static readonly string[] RomanNumbers =
        ["", "I.", "II.", "III.", "IV.", "V."];

    private readonly ILanguageService _lang;

    public VsBattleTeamBuilder(ILanguageService lang)
    {
        _lang = lang;
    }

    public VsBattleTeamVm Build(
        VsGameDtos data,
        IReadOnlyCollection<int> selectedSlots,
        bool isDirty,
        string culture)
    {
        var selection = data.RankedBattlefields.SavedSelection;
        var members = data.RankedBattlefields.BattleReadyMembers;

        var memberVms = members
            .Select(member => new VsBattleMemberVm
            {
                SlotNumber = member.SlotNumber,
                Name = member.Name,
                PictureCode = member.PictureCode,
                RankName = RankNameLocalizer.GetName(
                    member.Rank,
                    culture),
                IsSelected =
                    selectedSlots.Contains(member.SlotNumber)
            })
            .ToArray();

        var lamps = data.RankedBattlefields.Classifications
            .OrderBy(rule => rule.ClassificationId)
            .Select(rule => new VsClassificationLampVm
            {
                ClassificationId = rule.ClassificationId,
                Label = ResolveRomanNumber(rule.ClassificationId),
                IsActive = IsEligible(
                    rule,
                    data.RootBoxInfo.TeamRank,
                    members,
                    selectedSlots)
            })
            .ToArray();

        return new VsBattleTeamVm
        {
            Message = ResolveMessage(
                selection.SelectedSlotNumbers),
            Members = memberVms,
            ClassificationLamps = lamps,
            CanSave = isDirty && lamps.Any(lamp => lamp.IsActive)
        };
    }

    private string ResolveMessage(int[] savedSlots)
    {
        if (savedSlots.Length == 0)
            return _lang["vsgame.Manager.Message.Initial"];

        if (savedSlots.All(slot => slot == 0))
            return _lang["vsgame.Manager.Message.Invalidated"];

        return _lang["vsgame.Manager.Message.Saved"];
    }

    private static bool IsEligible(
        VsBattleClassificationDto rule,
        int teamRank,
        IReadOnlyCollection<VsBattleMemberDto> members,
        IReadOnlyCollection<int> selectedSlots)
    {
        if (teamRank < rule.MinimumTeamRank ||
            selectedSlots.Count != rule.RequiredPartySize)
        {
            return false;
        }

        var selectedMembers = members
            .Where(member =>
                selectedSlots.Contains(member.SlotNumber))
            .ToArray();

        if (selectedMembers.Length != selectedSlots.Count)
            return false;

        var membersInRange = selectedMembers.Count(member =>
            member.RankClass >= rule.MemberMinimumRankClass &&
            member.RankClass <= rule.MemberMaximumRankClass);

        return membersInRange >=
               rule.RequiredMembersInRankClassRange;
    }

    private static string ResolveRomanNumber(int id) =>
        id >= 1 && id < RomanNumbers.Length
            ? RomanNumbers[id]
            : id.ToString();
}
