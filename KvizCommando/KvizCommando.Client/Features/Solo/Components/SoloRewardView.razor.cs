using KvizCommando.Client.Data;
using KvizCommando.Client.Features.Solo.ViewModels;
using KvizCommando.Client.Services.Visual.UiService.Language;
using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client.Features.Solo.Components;

public partial class SoloRewardView
{
    [Inject] private ILanguageService Lang { get; set; } = default!;

    [Parameter, EditorRequired]
    public SoloRewardViewData Data { get; set; } = default!;

    private string NewTeamLevelCode =>
        RankNameTable.Data[Data.NewTeamLevel].PublicLevel ??
        string.Empty;

    private static string Signed(int value) =>
        $"{(value > 0 ? "+" : string.Empty)}{value}";
}

/**
 * ÚJ FÁJL: a Solo jutalomnézet paraméterét és formázási segédeit
 * tartalmazza. A tényleges új csapatszintet a szerver eredményéből
 * oldja fel nyilvános szintkódra.
 */
