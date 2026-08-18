using KvizCommando.Client.Data;
using KvizCommando.Client.Helpers;
using KvizCommando.Client.Services.ClientCache;
using KvizCommando.Client.Services.Visual.UiService.Language;
using KvizCommando.Shared.Models.Dtos;
using KvizCommando.Shared.Models.Rules;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Solo.Components;

public partial class SoloOrientationStatus
{
    [Inject] private ILanguageService Lang { get; set; } = default!;

    [CascadingParameter]
    private AppState AppStates { get; set; } = default!;

    [Parameter]
    public int CharacterPosition { get; set; }

    private TeamMemberDto? Member =>
        AppStates.Team!.TeamMembers![CharacterPosition];

    private string PublicLevel =>
        Member is null
            ? "-----"
            : RankNameTable.Data[Member.Level].PublicLevel ?? string.Empty;

    private string OrientationShort =>
        OrientationLocalizer.GetOrientShort(
            CharacterPosition,
            AppStates.Culture);

    private int VitalityPercent =>
        Member is null
            ? 0
            : TeamRules.GetMemberVitalityPercent(
                Member.EnergyPoints,
                Member.Level);

    private string VitalityStateCssClass =>
        Member is null
            ? string.Empty
            : TeamRules.GetMemberVitalityState(
                    Member.EnergyPoints,
                    Member.Level)
                .ToString()
                .ToLowerInvariant();

    private string VitalityStyle =>
        Member is null
            ? string.Empty
            : $"--vitality-percent: {VitalityPercent}%";

    private bool IsHealingGame =>
        Member is not null &&
        TeamRules.CanStartSoloHealingGame(
            Member.EnergyPoints,
            Member.SkillPoints,
            Member.NextHealingGameUtc,
            DateTime.UtcNow);

    private string HealingTooltip
    {
        get
        {
            if (Member is null || TeamRules.HasVitality(Member.EnergyPoints))
                return string.Empty;

            if (Member.SkillPoints >= TeamRules.HEAL_CHARACTER_DEV_POINT_COST)
                return Lang["solo.Tooltip.Orientation.HealingReload"]
                    .FormatSafe(OrientationShort);

            if (IsHealingGame)
                return Lang["solo.Tooltip.Orientation.HealingAvailable"]
                    .FormatSafe(OrientationShort);

            var remaining = Member.NextHealingGameUtc!.Value - DateTime.UtcNow;
            var remainingMinutes = Math.Max(
                (int)Math.Ceiling(remaining.TotalMinutes),
                0);

            return Lang["solo.Tooltip.Orientation.HealingCooldown"]
                .FormatSafe(
                    OrientationShort,
                    remainingMinutes / 60,
                    remainingMinutes % 60);
        }
    }

    private bool HasHealingTooltip =>
        Member is not null &&
        !TeamRules.HasVitality(Member.EnergyPoints);

    private string HealingTooltipCssClass =>
        HasHealingTooltip ? "has-tooltip" : string.Empty;

    private bool IsExperienceGame =>
        Member is not null &&
        SoloGameRules.CanEarnMemberExperience(Member.Level);

    private bool HasMaxedExperience =>
        IsExperienceGame &&
        Member!.Xp >= Member.NextXp;

    private string ExperienceCssClass =>
        HasMaxedExperience
            ? "maxed"
            : string.Empty;

    private string ExperienceTooltip =>
        Lang[HasMaxedExperience
            ? "solo.Tooltip.Orientation.XpMaxed"
            : "solo.Tooltip.Orientation.XpAvailable"];

    private bool HasMaxedScoreDevelopmentPoints =>
        Member is not null &&
        SoloGameRules.HasMaxedScoreDevelopmentPoints(
            Member.SoloBestScore,
            Member.Level);

    private string MaxedCssClass =>
        HasMaxedScoreDevelopmentPoints
            ? "maxed"
            : string.Empty;

    private string MaximumScoreCssClass =>
        Member is not null &&
        Member.SoloBestScore >= SoloGameRules.GetMaximumScore(Member.Level)
            ? "maximum-score"
            : string.Empty;

    private string HealingCssClass =>
        IsHealingGame
            ? "healing-game"
            : string.Empty;
}
