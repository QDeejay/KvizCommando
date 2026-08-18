namespace KvizCommando.Shared.Models.Rules
{
    public sealed record TeamHelpRule(
    int HelpId,
    int RankRuleIndex,
    int ModifierIndex);

    public enum MemberVitalityState
    {
        Critical,
        Low,
        Medium,
        High,
        Full
    }

    public static class TeamRules
    {
        public const int HIRED_CHAR_STARTLEVEL = 1;
        public const int FIRST_MEMBER_LEVEL = 0;
        public const int LAST_MEMBER_LEVEL = 21;
        public const int MEMBER_RANKS_PER_CLASS = 3;
        public const int MEMBER_BASE_VITALITY = 36;
        public const int MEMBER_VITALITY_PER_LEVEL = 3;

        public const int PROMOTION_TEAM_DEV_POINT_COST = 1;
        public const int HEAL_CHARACTER_DEV_POINT_COST = 1;
        public const int SOLO_HEALING_COOLDOWN_HOURS = 24;
        public const int FIRE_RECRUIT_DELAY_DAYS = 1;
        public const int HELP_LEVEL_TEAM_DEV_POINT_COST = 1;
        public const int RETIRE_REWARD_RANK = 22;

        public const int LAST_XP_LEVEL = 21;
        public const int LAST_PROGRESS_LEVEL = 29;
        public const int FIRST_HELP_LEVEL = 1;

        public const int FIFTY_FIFTY_HELP_ID = 101;
        public const int GUESS_RANGE_HELP_ID = 102;
        public const int TIME_FREEZE_HELP_ID = 103;
        public const int AI_SUGGESTION_HELP_ID = 104;

        // RankRuleIndex: a RankConstants startLevels/maxLevels tömbjeiben
        // a 12-15. hely a négy csapatsegítség szabálya.
        // ModifierIndex: a ModifierTableRow.Modifier tömbjében
        // a 8-11. hely ugyanennek a négy segítségnek az értéke.
        public static readonly TeamHelpRule[] HelpRules =
        [
            new(FIFTY_FIFTY_HELP_ID, 12, 8),
        new(GUESS_RANGE_HELP_ID, 13, 9),
        new(TIME_FREEZE_HELP_ID, 14, 10),
        new(AI_SUGGESTION_HELP_ID, 15, 11)
        ];

        public static readonly int[] MemberLevels = Enumerable
            .Range(
                FIRST_MEMBER_LEVEL,
                LAST_MEMBER_LEVEL - FIRST_MEMBER_LEVEL + 1)
            .ToArray();

        /// <summary>
        /// Visszaadja a megadott csapatsegítség szabályait.
        /// </summary>
        public static TeamHelpRule GetHelp(int helpId) =>
            HelpRules.First(rule => rule.HelpId == helpId);

        /// <summary>
        /// Visszaadja a karakterszinthez tartozó rendfokozati osztályt.
        /// </summary>
        public static int GetMemberRankClass(int memberLevel) =>
            memberLevel == FIRST_MEMBER_LEVEL
                ? 0
                : (memberLevel - 1) / MEMBER_RANKS_PER_CLASS + 1;

        /// <summary>
        /// Jelzi, hogy az előléptetés rendfokozati osztályt is vált-e.
        /// </summary>
        public static bool IsRankClassChangingPromotion(int currentLevel) =>
            GetMemberRankClass(currentLevel) !=
            GetMemberRankClass(currentLevel + 1);

        /// <summary>
        /// Kiszámítja a karakter számára jóváírható tapasztalatot.
        /// </summary>
        public static int GetCreditableMemberExperience(
            int earnedXp,
            int level,
            int currentXp)
        {
            var availableXp = Math.Max(
                RankRewards.List[level].NextLevelMember - currentXp,
                0);

            return Math.Min(
                Math.Max(earnedXp, 0),
                availableXp);
        }

        /// <summary>
        /// Jelzi, hogy a karakter rendelkezik-e felhasználható életerővel.
        /// </summary>
        public static bool HasVitality(int vitality) =>
            vitality > 0;

        /// <summary>
        /// Visszaadja a karakterszinthez tartozó maximális életerőt.
        /// </summary>
        public static int GetMemberMaxVitality(int level) =>
            MEMBER_BASE_VITALITY +
            level * MEMBER_VITALITY_PER_LEVEL;

        /// <summary>
        /// Kiszámítja a karakter életerejének százalékos értékét.
        /// </summary>
        public static int GetMemberVitalityPercent(
            int vitality,
            int level) =>
            Math.Clamp(
                vitality * 100 / GetMemberMaxVitality(level),
                0,
                100);

        /// <summary>
        /// Visszaadja a karakter életerőállapotát.
        /// </summary>
        public static MemberVitalityState GetMemberVitalityState(
            int vitality,
            int level) =>
            GetMemberVitalityPercent(vitality, level) switch
            {
                >= 100 => MemberVitalityState.Full,
                >= 80 => MemberVitalityState.High,
                >= 50 => MemberVitalityState.Medium,
                >= 20 => MemberVitalityState.Low,
                _ => MemberVitalityState.Critical
            };

        /// <summary>
        /// Jelzi, hogy elindítható-e a gyógyítást adó egyéni játék.
        /// </summary>
        public static bool CanStartSoloHealingGame(
            int vitality,
            int developmentPoints,
            DateTime? nextHealingGameUtc,
            DateTime utcNow) =>
            !HasVitality(vitality) &&
            developmentPoints < HEAL_CHARACTER_DEV_POINT_COST &&
            (!nextHealingGameUtc.HasValue ||
             nextHealingGameUtc.Value <= utcNow);

        /// <summary>
        /// Visszaadja a következő gyógyító egyéni játék legkorábbi időpontját.
        /// </summary>
        public static DateTime GetNextSoloHealingGameUtc(
            DateTime completedUtc) =>
            completedUtc.AddHours(SOLO_HEALING_COOLDOWN_HOURS);
    }

}
