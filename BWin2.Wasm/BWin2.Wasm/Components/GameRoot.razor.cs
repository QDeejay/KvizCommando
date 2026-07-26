using BWin2.Wasm.State;
using Microsoft.AspNetCore.Components;

namespace BWin2.Wasm.Components;

public partial class GameRoot : IDisposable
{
    [Inject] private IGameSession Game { get; set; } = default!;

    [Inject] private IMatchPresentation Match { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        Game.Changed += HandleChanged;
        Match.Changed += HandleChanged;
        await Game.InitializeAsync();
    }

    private void HandleChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        Game.Changed -= HandleChanged;
        Match.Changed -= HandleChanged;
    }
}
