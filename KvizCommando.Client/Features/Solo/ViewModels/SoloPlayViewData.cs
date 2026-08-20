

using KvizCommando.Shared.Models.Enums.VsGame;

namespace KvizCommando.Client.Features.Solo.ViewModels
{
    public sealed class SoloPlayViewData
    {
        public SoloPlayerViewData Player { get; init; } = new();
        public SoloGameViewData Game { get; init; } = new();
        public SoloPanelViewData Panel { get; init; } = new();
    }

    public sealed class SoloPlayerViewData
    {
        public string Name { get; init; } = string.Empty;
        public string RankName { get; init; } = string.Empty;
        public string Level { get; init; } = string.Empty;
        public string OrientationName { get; init; } = string.Empty;
        public string PictureCode { get; init; } = string.Empty;
        public string CaptainAvatar { get; init; } = string.Empty;
        public string ImageSrc { get; init; } = string.Empty;
        public int? SoloBestScore { get; set; }
    }

    public sealed class SoloGameViewData
    {
        public string Title { get; init; } = string.Empty;
        public int Points { get; init; }
        public int CurrentQuestion { get; init; }
        public int TotalQuestions { get; init; }
        public int TotalSeconds { get; init; }
        public int RemainingSeconds { get; init; }
        public int ResponseTimeMilliseconds { get; init; }
        public VsConnectionQuality ConnectionQuality { get; init; }
        public bool IsConnectionActive { get; init; }
        public bool IsExperienceGame { get; init; }
        public bool IsHealing { get; init; }
        public bool IsHealingCompleted { get; init; }
    }

    public sealed class SoloPanelViewData
    {
        public SoloPanelMode Mode { get; init; }
        public SoloRewardViewData? Reward { get; init; }
        public IReadOnlyList<SoloDisplayLine> DisplayLines { get; init; } = [];
        public IReadOnlyList<string> Answers { get; init; } = [];
        public IReadOnlyList<SoloQuestionState> Progress { get; init; } = [];
        public int SelectedAnswerIndex { get; init; } = -1;
        public bool? CurrentAnswerResult { get; init; }
        public bool AnswerEnabled { get; init; }
    }

    public sealed class SoloRewardViewData
    {
        public int Answered { get; init; }
        public int TotalQuestions { get; init; }
        public int Correct { get; init; }
        public string Time { get; init; } = string.Empty;
        public int TotalPoints { get; init; }
        public bool IsNewHighScore { get; init; }
        public int TeamXp { get; init; }
        public int TeamDevPoints { get; init; }
        public int MemberXp { get; init; }
        public bool IsExperienceGame { get; init; }
        public bool IsMemberXpCapped { get; init; }
        public int MemberDevPoints { get; init; }
        public int NewTeamLevel { get; init; }
        public bool HealingPointAwarded { get; init; }
    }
    public sealed class SoloDisplayLine
    {
        public string Text { get; init; } = string.Empty;
        public string ResourceKey { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public bool Emphasized { get; init; }
    }

}
