using System.Globalization;
using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Help.TeamHelpRules;

public static class MemberHelpRules
{
    /// <summary>
    /// Összeállítja a súgószöveg dinamikus helyettesítési értékeit.
    /// </summary>
    public static IReadOnlyDictionary<string, string> BuildTokens(
        AppState appStates)
    {
        var tokens = new Dictionary<string, string>();
        var numberCulture = CultureInfo.GetCultureInfo(appStates.Culture);

        foreach (var level in TeamRules.MemberLevels)
        {
            var number = level.ToString("00");
            var rankClass = TeamRules.GetMemberRankClass(level);

            tokens[$"MEMBER_LEVEL_{number}"] =
                RankNameTable.Data[level].PublicLevel!;
            tokens[$"MEMBER_CLASS_{number}"] =
                RankNameLocalizer.GetClass(rankClass, appStates.Culture);
            tokens[$"MEMBER_RANK_{number}"] =
                RankNameLocalizer.GetName(level, appStates.Culture);
            tokens[$"MEMBER_NEXT_XP_{number}"] =
                RankRewards.List[level].NextLevelMember
                    .ToString("N0", numberCulture);
        }

        return tokens;
    }
}
