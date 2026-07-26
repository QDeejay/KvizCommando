using BWin2.Wasm.Domain;

namespace BWin2.Wasm.Services;

internal interface ICommentaryScriptService
{
    IReadOnlyList<CommentaryPart> BuildMatchCommentary(
        GameState state,
        Fixture fixture,
        int scoringSide,
        int scorerNumber,
        int commentCode);

    IReadOnlyList<CommentaryPart> BuildPenaltyCommentary(
        GameState state,
        Fixture fixture,
        int scoringSide,
        int kickIndex,
        string script);
}
