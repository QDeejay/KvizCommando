using KvizCommando.Shared.Models.Dtos;

namespace KvizCommando.Server.Services.VsGame;

public static class VsBattleClassificationRules
{
    public const int RequiredBattleReadyCharacters = 3;
    public const int RequiredCreditBalance = 50;
    public const int UnrankedMemberMinimumTeamRank = 28;

    // Első, központi szabálytábla. A konkrét minimumok és létszámok
    // a játékszabály véglegesítésekor kizárólag itt módosítandók.
    public static readonly IReadOnlyList<VsBattleClassificationDto> List =
    [
        new()
        {
            ClassificationId = 1,
            MinimumTeamRank = 1,
            RequiredPartySize = 3,
            MemberMinimumRankClass = 1,
            MemberMaximumRankClass = 3,
            RequiredMembersInRankClassRange = 3
        },
        new()
        {
            ClassificationId = 2,
            MinimumTeamRank = 7,
            RequiredPartySize = 3,
            MemberMinimumRankClass = 3,
            MemberMaximumRankClass = 5,
            RequiredMembersInRankClassRange = 3
        },
        new()
        {
            ClassificationId = 3,
            MinimumTeamRank = 13,
            RequiredPartySize = 4,
            MemberMinimumRankClass = 5,
            MemberMaximumRankClass = 7,
            RequiredMembersInRankClassRange = 4
        },
        new()
        {
            ClassificationId = 4,
            MinimumTeamRank = 19,
            RequiredPartySize = 4,
            MemberMinimumRankClass = 7,
            MemberMaximumRankClass = 10,
            RequiredMembersInRankClassRange = 4
        },
        new()
        {
            ClassificationId = 5,
            MinimumTeamRank = 28,
            RequiredPartySize = 5,
            MemberMinimumRankClass = 10,
            MemberMaximumRankClass = 10,
            RequiredMembersInRankClassRange = 5
        }
    ];

    public static int[] GetEligibleClassificationIds(
        int teamRank,
        IReadOnlyCollection<int> memberRanks)
    {
        if (memberRanks.Count == 0)
            return [];

        return
        [
            .. List
                .Where(rule =>
                    IsEligible(rule, teamRank, memberRanks))
                .Select(rule => rule.ClassificationId)
        ];
    }

    public static bool IsSupportedPartySize(int partySize) =>
        List.Any(rule => rule.RequiredPartySize == partySize);

    public static bool CanSelectMember(
        int teamRank,
        int energyPoints,
        int memberRank) =>
        energyPoints > 0 &&
        (memberRank > 0 ||
         teamRank >= UnrankedMemberMinimumTeamRank);

    private static bool IsEligible(
        VsBattleClassificationDto rule,
        int teamRank,
        IReadOnlyCollection<int> memberRanks)
    {
        if (teamRank < rule.MinimumTeamRank ||
            memberRanks.Count != rule.RequiredPartySize)
        {
            return false;
        }

        var membersInRange = memberRanks.Count(rank =>
        {
            var rankClass = ResolveRankClass(rank);
            return rankClass >= rule.MemberMinimumRankClass &&
                   rankClass <= rule.MemberMaximumRankClass;
        });

        return membersInRange >=
               rule.RequiredMembersInRankClassRange;
    }

    public static int ResolveRankClass(int rank) =>
        rank == 0 ? 0 : (rank - 1) / 3 + 1;
}
