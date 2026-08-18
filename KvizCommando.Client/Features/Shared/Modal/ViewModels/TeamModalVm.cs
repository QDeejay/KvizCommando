namespace KvizCommando.Client.Models.ViewModels
{
    public abstract class TeamModalVm
    {
        public List<ModalRow> Rows { get; set; } = new();
        public InfoBlock Info { get; set; } = default!;
        public string Infotext1 { get; set; } = string.Empty;

    }
    public sealed class ModalTeamPromoteVm : TeamModalVm
    {
        public int StartOfHelps { get; set; } = 0;
        public string Unlocks { get; set; } = string.Empty;
        public string UnlocksLevel { get; set; } = string.Empty;
        public string UnlocksOrg { get; set; } = string.Empty;

        public string UnlockHelps { get; set; } = string.Empty;
        public string UnlockExtras { get; set; } = string.Empty;
    }
    /// <summary>
    /// Egy csapatművelet modális ablakában megjelenő összehasonlító sort ír le.
    /// </summary>
    /// <param name="CategoryName">A sor felirata.</param>
    /// <param name="ValueDisplay">Az aktuális érték megjelenítési alakja.</param>
    /// <param name="separator">Az aktuális és a módosított érték közötti elválasztó.</param>
    /// <param name="ValueChangeDisplay">A módosítás utáni érték megjelenítési alakja.</param>
    /// <param name="color">A változás kiemeléséhez használt szín.</param>
    public sealed record ModalRow(
        string CategoryName,
        string ValueDisplay,
        string separator,
        string ValueChangeDisplay,
        string color
        );
    /// <summary>
    /// A csapattag részletes adatait előre lokalizált címke–érték párokban adja át a modális nézetnek.
    /// </summary>
    /// <param name="Name">A név mező felirata.</param>
    /// <param name="NameValue">A csapattag megjelenített neve.</param>
    /// <param name="Color">A csapattag kiemelési színe.</param>
    /// <param name="Rank">A rang mező felirata.</param>
    /// <param name="RankValue">A rang megjelenített értéke.</param>
    /// <param name="Level">A szint mező felirata.</param>
    /// <param name="LevelValue">A szint megjelenített értéke.</param>
    /// <param name="Orient1">Az első beállítottság felirata.</param>
    /// <param name="Orient2">A második beállítottság felirata.</param>
    /// <param name="Orient1Value">Az első beállítottság megjelenített értéke.</param>
    /// <param name="Orient2Value">A második beállítottság megjelenített értéke.</param>
    /// <param name="Devpoints">A fejlesztési pontok mezőjének felirata.</param>
    /// <param name="DevPointsValue">A rendelkezésre álló fejlesztési pontok megjelenített értéke.</param>
    /// <param name="AddedDevPoints">A művelet által hozzáadott fejlesztési pontok megjelenített értéke.</param>
    public sealed record InfoBlock(
        string Name,
        string NameValue,
        string Color,
        string Rank,
        string RankValue,
        string Level,
        string LevelValue,
        string Orient1,
        string Orient2,
        string Orient1Value,
        string Orient2Value,
        string Devpoints,
        string DevPointsValue,
        string AddedDevPoints
        );
    public sealed class ModalHireVm : TeamModalVm
    {
        public string Labelpros { get; set; } = string.Empty;
        public string Labelcons { get; set; } = string.Empty;
    }
    public class ModalRetireVm : TeamModalVm
    {
        public string Unlocks { get; set; } = string.Empty;
        public string UnlocksLevel { get; set; } = string.Empty;
        public string UnlocksRank { get; set; } = string.Empty;
        public string RankClass { get; set; } = string.Empty;
        public bool RankClassChanged { get; set; } = false;
    }
    public sealed class ModalPromoteVm : ModalRetireVm
    {

        public string UnlockMaxLevels1 { get; set; } = string.Empty;
        public string UnlockMaxLevels2 { get; set; } = string.Empty;
    }
    public sealed class ModalHandleVm : TeamModalVm
    {

        public string Infotext2 { get; set; } = string.Empty;
        public string Infotext3 { get; set; } = string.Empty;
        public string Infotext4 { get; set; } = string.Empty;
    }
    public static class ModalConstants
    {
        public static readonly int[] HireVal =
        {
           0, 0, 4,0, 1, 5
        };

    }
}
