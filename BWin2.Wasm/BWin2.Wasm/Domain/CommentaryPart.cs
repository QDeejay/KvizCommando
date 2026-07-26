namespace BWin2.Wasm.Domain;

internal sealed record CommentaryPart(
    string Text,
    int ForegroundColor,
    int BackgroundColor,
    bool Colorize = true);
