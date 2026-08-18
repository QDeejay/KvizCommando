using System.Globalization;
using KvizCommando.Client.Data;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Shared.Models;
using KvizCommando.Shared.Models.Rules;

namespace KvizCommando.Client.Features.Shared.Help.TeamHelpRules;

public static class TeamOverviewHelpRules
{
    public static IReadOnlyDictionary<string, string> BuildTokens(
        AppState appStates)
    {
        var currentTeamLevel = appStates.Team!.TeamInfo.Level;
        var numberCulture = CultureInfo.GetCultureInfo(appStates.Culture);
        var tokens = new Dictionary<string, string>();

        foreach (var reward in RankRewards.List)
        {
            var number = reward.RowIndex.ToString("00");

            tokens[$"TEAM_LEVEL_{number}"] =
                RankNameTable.Data[reward.RowIndex].PublicLevel!;
            tokens[$"TEAM_BONUS_{number}"] =
                reward.WinBonus.ToString(numberCulture);
            tokens[$"TEAM_MAX_MEMBERS_{number}"] =
                reward.MaxCharacters.ToString(numberCulture);
            tokens[$"TEAM_WINNER_COMPENSATION_{number}"] =
                reward.RowIndex == 0
                    ? "N/A"
                    : "×" + VsRankedMatchRules
                        .GetWinnerCompensation(reward.RowIndex)
                        .ToString("0.0", numberCulture);

            if (reward.RowIndex <= TeamRules.LAST_XP_LEVEL)
            {
                tokens[$"TEAM_NEXT_XP_{number}"] =
                    reward.NextLevelTeam.ToString("N0", numberCulture);
            }
            else if (reward.RowIndex <= TeamRules.LAST_PROGRESS_LEVEL)
            {
                tokens[$"TEAM_PROGRESS_CLASS_{number}"] =
                    GetProgressClass(currentTeamLevel, reward.RowIndex);
            }
        }

        AddHelpTokens(
            tokens,
            "FIFTY_FIFTY",
            TeamRules.GetHelp(TeamRules.FIFTY_FIFTY_HELP_ID),
            numberCulture);
        AddHelpTokens(
            tokens,
            "GUESS_RANGE",
            TeamRules.GetHelp(TeamRules.GUESS_RANGE_HELP_ID),
            numberCulture,
            includeValues: true);
        AddHelpTokens(
            tokens,
            "TIME_FREEZE",
            TeamRules.GetHelp(TeamRules.TIME_FREEZE_HELP_ID),
            numberCulture);
        AddHelpTokens(
            tokens,
            "AI_SUGGESTION",
            TeamRules.GetHelp(TeamRules.AI_SUGGESTION_HELP_ID),
            numberCulture,
            includeValues: true);

        tokens["TEAM_PROMOTION_COST"] =
            TeamRules.PROMOTION_TEAM_DEV_POINT_COST.ToString(numberCulture);
        tokens["TEAM_HEAL_COST"] =
            TeamRules.HEAL_CHARACTER_DEV_POINT_COST.ToString(numberCulture);
        tokens["TEAM_FIRE_RECRUIT_DELAY_DAYS"] =
            TeamRules.FIRE_RECRUIT_DELAY_DAYS.ToString(numberCulture);
        tokens["TEAM_HELP_LEVEL_COST"] =
            TeamRules.HELP_LEVEL_TEAM_DEV_POINT_COST.ToString(numberCulture);
        tokens["TEAM_RETIRE_REWARD"] =
            RankRewards.List[TeamRules.RETIRE_REWARD_RANK]
                .DevPointToStore
                .ToString(numberCulture);

        return tokens;
    }

    private static void AddHelpTokens(
        Dictionary<string, string> tokens,
        string name,
        TeamHelpRule helpRule,
        CultureInfo numberCulture,
        bool includeValues = false)
    {
        var unlockLevel = RankRewards.List
            .First(reward => reward.HelpRewardNo == helpRule.HelpId)
            .RowIndex;
        var maxLevel = RankConstants.maxLevels[helpRule.RankRuleIndex];

        tokens[$"TEAM_HELP_{name}_UNLOCK_LEVEL"] =
            RankNameTable.Data[unlockLevel].PublicLevel!;
        tokens[$"TEAM_HELP_{name}_MAX_LEVEL"] =
            maxLevel.ToString(numberCulture);

        if (!includeValues)
            return;

        tokens[$"TEAM_HELP_{name}_START_VALUE"] = FormatModifier(
            ModifierTable.Data[TeamRules.FIRST_HELP_LEVEL]
                .Modifier[helpRule.ModifierIndex]);
        tokens[$"TEAM_HELP_{name}_MAX_VALUE"] = FormatModifier(
            ModifierTable.Data[maxLevel]
                .Modifier[helpRule.ModifierIndex]);
    }

    private static string GetProgressClass(
        int currentTeamLevel,
        int rowLevel)
    {
        if (currentTeamLevel < rowLevel)
            return "is-secret";

        return currentTeamLevel == rowLevel
            ? "is-unlocked"
            : "is-completed";
    }

    private static string FormatModifier(double? value) =>
        value!.Value.ToString("0.#", CultureInfo.InvariantCulture);
}
