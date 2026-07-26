using Microsoft.AspNetCore.Components;

namespace BWin2.Wasm.Components.Match;

public partial class PenaltyRow
{
    [Parameter, EditorRequired]
    public string Name { get; set; } = string.Empty;

    [Parameter, EditorRequired]
    public IReadOnlyList<bool?> Marks { get; set; } = [];
}
