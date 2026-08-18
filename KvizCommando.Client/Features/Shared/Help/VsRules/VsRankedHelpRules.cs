using System.Globalization;
using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Help.VsRules;

/// <summary>
/// A rangsorolt súgó változó értékeit a betöltött képernyőadatokból
/// és a közös meccsszabályokból állítja elő.
/// </summary>
public static class VsRankedHelpRules
{
    /// <summary>
    /// Összeállítja a súgószöveg dinamikus helyettesítési értékeit.
    /// </summary>
    /// <param name="appStates">A súgószöveg változóinak feloldásához használt alkalmazásállapot.</param>
    public static IReadOnlyDictionary<string, string> BuildTokens(
        AppState appStates)
    {
        var data = appStates.VsGame!;
        var culture = appStates.Culture;
        var numberCulture = CultureInfo.GetCultureInfo(culture);
        var tokens = new Dictionary<string, string>
        {
            ["VS_ENTRY_READY_COUNT"] =
                data.RootBoxInfo.RequiredBattleReadyCharacterCount
                    .ToString(numberCulture),
            ["VS_PREPARATION_SECONDS"] =
                VsRankedMatchRules.PREPARATION_SECONDS
                    .ToString(numberCulture),
            ["VS_GUESS_SECONDS"] =
                VsRankedMatchRules.GUESS_SECONDS
                    .ToString(numberCulture),
            ["VS_QUESTION_SECONDS"] =
                VsRankedMatchRules.QUESTION_SECONDS
                    .ToString(numberCulture),
            ["VS_POINT_UNIT"] =
                VsRankedMatchRules.POINT_UNIT
                    .ToString(numberCulture),
            ["VS_CAPTAIN_MULTIPLIER"] =
                VsRankedMatchRules.CAPTAIN_MULTIPLIER
                    .ToString(numberCulture),
            ["VS_CAPTAIN_POINT_UNIT"] =
                (VsRankedMatchRules.POINT_UNIT *
                 VsRankedMatchRules.CAPTAIN_MULTIPLIER)
                    .ToString(numberCulture),
            ["VS_WRONG_ANSWER_TIME_PENALTY_SECONDS"] =
                VsRankedMatchRules
                    .TIME_FREEZE_WRONG_ANSWER_PENALTY_SECONDS
                    .ToString(numberCulture),
            ["VS_WINNER_COMPENSATION_MIN"] =
                VsRankedMatchRules.WINNER_COMPENSATION_MIN
                    .ToString("0.0", numberCulture),
            ["VS_WINNER_COMPENSATION_MAX"] =
                VsRankedMatchRules.WINNER_COMPENSATION_MAX
                    .ToString("0.0", numberCulture)
        };

        foreach (var classification in
                 data.RankedBattlefields.Classifications)
        {
            var number = classification.ClassificationId.ToString("00");
            var minimumClass = RankNameLocalizer.GetClass(
                classification.MemberMinimumRankClass,
                culture);
            var maximumClass = RankNameLocalizer.GetClass(
                classification.MemberMaximumRankClass,
                culture);

            tokens[$"VS_CLASSIFICATION_{number}_STAKE"] =
                classification.Stake.ToString("N0", numberCulture);
            tokens[$"VS_CLASSIFICATION_{number}_TEAM_LEVEL"] =
                RankNameTable.Data[classification.MinimumTeamRank]
                    .PublicLevel!;
            tokens[$"VS_CLASSIFICATION_{number}_TEAM_SIZE"] =
                classification.RequiredPartySize
                    .ToString(numberCulture);
            tokens[$"VS_CLASSIFICATION_{number}_RANK_CLASS_RANGE"] =
                classification.MemberMinimumRankClass ==
                classification.MemberMaximumRankClass
                    ? $"{classification.MemberMinimumRankClass} – " +
                      minimumClass
                    : $"{classification.MemberMinimumRankClass}–" +
                      $"{classification.MemberMaximumRankClass} – " +
                      $"{minimumClass}–{maximumClass}";
            tokens[$"VS_CLASSIFICATION_{number}_REQUIRED_MEMBERS"] =
                classification.RequiredMembersInRankClassRange
                    .ToString(numberCulture);
        }

        return tokens;
    }
}
