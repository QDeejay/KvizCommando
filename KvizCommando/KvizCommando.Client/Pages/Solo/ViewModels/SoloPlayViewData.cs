

namespace KvizCommando.Client.Pages.Solo.ViewModels
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
        public int Level { get; init; }
        public string OrientationName { get; init; } = string.Empty;
        public string PictureCode { get; init; } = string.Empty;
        public string ImageSrc { get; init; } = string.Empty;
    }

    public sealed class SoloGameViewData
    {
        public string Title { get; init; } = string.Empty;
        public int Points { get; init; }
        public int CurrentQuestion { get; init; }
        public int TotalQuestions { get; init; }
        public int TotalSeconds { get; init; }
        public int RemainingSeconds { get; init; }
    }

    public sealed class SoloPanelViewData
    {
        public SoloPanelMode Mode { get; init; }
        public IReadOnlyList<SoloDisplayLine> DisplayLines { get; init; } = [];
        public IReadOnlyList<string> Answers { get; init; } = [];
        public IReadOnlyList<SoloQuestionState> Progress { get; init; } = [];
        public int SelectedAnswerIndex { get; init; } = -1;
        public bool? CurrentAnswerResult { get; init; }
        public bool AnswerEnabled { get; init; }
        public bool ShowSkip { get; init; }
    }
    public sealed class SoloDisplayLine
    {
        public string Text { get; init; } = string.Empty;
        public string ResourceKey { get; init; } = string.Empty;
        public string Value { get; init; } = string.Empty;
        public bool Emphasized { get; init; }
    }

}
