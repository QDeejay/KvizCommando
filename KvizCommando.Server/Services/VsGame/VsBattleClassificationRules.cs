using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Server.Services.VsGame;

public static class VsBattleClassificationRules
{
    public const int REQUIRED_BATTLE_READY_CHARACTERS = 3;
    public const int REQUIRED_CREDIT_BALANCE = 50;
    public const int UNRANKED_MEMBER_MINIMUM_TEAM_RANK = 28;

    // Első, központi szabálytábla. A konkrét minimumok és létszámok
    // a játékszabály véglegesítésekor kizárólag itt módosítandók.
    public static readonly IReadOnlyList<VsBattleClassificationDto> List =
    [
        new()
        {
            ClassificationId = 1,
            Stake = 50,
            MinimumTeamRank = 1,
            RequiredPartySize = 3,
            MemberMinimumRankClass = 1,
            MemberMaximumRankClass = 3,
            RequiredMembersInRankClassRange = 2
        },
        new()
        {
            ClassificationId = 2,
            Stake = 100,
            MinimumTeamRank = 7,
            RequiredPartySize = 3,
            MemberMinimumRankClass = 3,
            MemberMaximumRankClass = 5,
            RequiredMembersInRankClassRange = 3
        },
        new()
        {
            ClassificationId = 3,
            Stake = 200,
            MinimumTeamRank = 13,
            RequiredPartySize = 4,
            MemberMinimumRankClass = 5,
            MemberMaximumRankClass = 7,
            RequiredMembersInRankClassRange = 4
        },
        new()
        {
            ClassificationId = 4,
            Stake = 500,
            MinimumTeamRank = 19,
            RequiredPartySize = 5,
            MemberMinimumRankClass = 7,
            MemberMaximumRankClass = 7,
            RequiredMembersInRankClassRange = 4
        },
        new()
        {
            ClassificationId = 5,
            Stake = 1000,
            MinimumTeamRank = 28,
            RequiredPartySize = 6,
            MemberMinimumRankClass = 0,
            MemberMaximumRankClass = 7,
            RequiredMembersInRankClassRange = 6
        }
    ];

    /// <summary>
    /// Visszaadja azokat a rangsorolt osztályokat, amelyekbe a csapat beléphet.
    /// </summary>
    /// <param name="teamRank">A csapat aktuális rangja.</param>
    /// <param name="memberRanks">A kiválasztott csapattagok rangjai.</param>
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

    /// <summary>
    /// Jelzi, hogy a megadott csapatlétszám támogatott-e.
    /// </summary>
    /// <param name="partySize">A csapatba választott tagok száma.</param>
    public static bool IsSupportedPartySize(int partySize) =>
        List.Any(rule => rule.RequiredPartySize == partySize);

    /// <summary>
    /// Jelzi, hogy a karakter kiválasztható-e a rangsorolt csapatba.
    /// </summary>
    /// <param name="teamRank">A csapat aktuális rangja.</param>
    /// <param name="energyPoints">A csapat rendelkezésre álló energiapontjai.</param>
    /// <param name="memberRank">A csapattag aktuális rangja.</param>
    /// <param name="memberXp">A csapattag aktuális tapasztalati pontja.</param>
    public static bool CanSelectMember(
        int teamRank,
        int energyPoints,
        int memberRank,
        int memberXp) =>
        energyPoints > 0 &&
        !IsAwaitingRetirement(memberRank, memberXp) &&
        (memberRank > 0 ||
         teamRank >= UNRANKED_MEMBER_MINIMUM_TEAM_RANK);

    /// <summary>
    /// Jelzi, hogy a karakter nyugdíjazásra vár-e.
    /// </summary>
    /// <param name="memberRank">A csapattag aktuális rangja.</param>
    /// <param name="memberXp">A csapattag aktuális tapasztalati pontja.</param>
    public static bool IsAwaitingRetirement(
        int memberRank,
        int memberXp) =>
        memberRank == TeamRules.LAST_MEMBER_LEVEL &&
        memberXp >= RankRewards.List[TeamRules.LAST_MEMBER_LEVEL].NextLevelMember;

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

    /// <summary>
    /// Visszaadja a ranghoz tartozó rendfokozati osztályt.
    /// </summary>
    /// <param name="rank">A feloldandó rang értéke.</param>
    public static int ResolveRankClass(int rank) =>
        rank == 0 ? 0 : (rank - 1) / 3 + 1;
}
