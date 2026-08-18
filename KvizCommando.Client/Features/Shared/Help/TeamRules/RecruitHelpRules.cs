using KvizCommando.Client.Features.Team;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;

namespace KvizCommando.Client.Features.Shared.Help.TeamHelpRules;

public static class RecruitHelpRules
{
    public static IReadOnlyDictionary<string, string> BuildTokens(
        AppState appStates)
    {
        var tokens = new Dictionary<string, string>();

        for (var orientationId = 1; orientationId <= 8; orientationId++)
        {
            var number = orientationId.ToString("00");
            var firstSecondary =
                TeamHelpers.RecruitResolver(orientationId, 1).Item2;
            var secondSecondary =
                TeamHelpers.RecruitResolver(orientationId, 2).Item2;

            tokens[$"RECRUIT_ORIENTATION_{number}_NAME"] =
                OrientationLocalizer.GetOrientation(
                    orientationId,
                    appStates.Culture);
            tokens[$"RECRUIT_ORIENTATION_{number}_CATEGORY_01"] =
                CategoryNameLocalizer.GetCategory(
                    orientationId,
                    appStates.Culture);
            tokens[$"RECRUIT_ORIENTATION_{number}_CATEGORY_02"] =
                CategoryNameLocalizer.GetCategory(
                    orientationId + 8,
                    appStates.Culture);
            tokens[$"RECRUIT_ORIENTATION_{number}_SECONDARY_01"] =
                OrientationLocalizer.GetOrientation(
                    firstSecondary,
                    appStates.Culture);
            tokens[$"RECRUIT_ORIENTATION_{number}_SECONDARY_02"] =
                OrientationLocalizer.GetOrientation(
                    secondSecondary,
                    appStates.Culture);
        }

        return tokens;
    }
}
