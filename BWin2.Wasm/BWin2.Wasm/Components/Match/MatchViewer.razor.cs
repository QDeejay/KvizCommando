using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;

namespace BWin2.Wasm.Components.Match;

public partial class MatchViewer : IDisposable
{
    private static readonly string[] QBasicColors =
    [
        "#05070a", "#14213d", "#1b4332", "#0b525b",
        "#7f1d1d", "#581c87", "#854d0e", "#cbd5e1",
        "#475569", "#2563eb", "#22c55e", "#06b6d4",
        "#ef4444", "#d946ef", "#facc15", "#ffffff"
    ];

    [Inject] private IMatchPresentation Match { get; set; } = default!;

    private string ClockText => Match.Phase switch
    {
        MatchPresentationPhase.Introduction => "PRE-MATCH",
        MatchPresentationPhase.Penalties => "PEN",
        MatchPresentationPhase.Finished => "FT",
        _ => $"{Match.Minute}'"
    };

    private string CommentaryStyle =>
        !Match.CommentaryIsColorized
            ? string.Empty
            : $"color:{Color(Match.CommentaryForeground)};" +
              $"background:{Color(Match.CommentaryBackground)};";

    protected override void OnInitialized()
    {
        Match.Changed += OnMatchChanged;
    }

    private void Continue() => Match.Continue();

    private void OnMatchChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private static string Color(int number) =>
        QBasicColors[Math.Clamp(number, 0, QBasicColors.Length - 1)];

    public void Dispose()
    {
        Match.Changed -= OnMatchChanged;
    }
}
