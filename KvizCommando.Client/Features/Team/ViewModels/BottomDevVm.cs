namespace KvizCommando.Client.Features.Team.ViewModels
{
    public sealed class BottomDevVm
    {
        public List<BottomDevRow> Rows { get; set; } = [];
        public int[] UsedPoints { get; set; } = [0, 0, 0, 0];
        public int AvailableDevPoints { get; set; }
        public string HeaderText { get; set; } = string.Empty;
        public string ResetButtonText { get; set; } = string.Empty;
    }
    /// <summary>
    /// Egy fejleszthető csapattulajdonság aktuális értékét és módosíthatóságát írja le.
    /// </summary>
    /// <param name="CategoryName">A fejleszthető tulajdonság neve.</param>
    /// <param name="LevelDisplay">A hozzá tartozó szint megjelenítési alakja.</param>
    /// <param name="ValueDisplay">Az aktuális érték megjelenítési alakja.</param>
    /// <param name="HasChanged">Jelzi, hogy a felhasználó módosította-e az értéket.</param>
    /// <param name="CanDevelop">Jelzi, hogy az érték tovább növelhető-e.</param>
    /// <param name="StatusText">A módosíthatóságot magyarázó felületi szöveg.</param>
    public sealed record BottomDevRow(
      string CategoryName,
      string LevelDisplay,
      string ValueDisplay,
      bool HasChanged,
      bool CanDevelop,
      string StatusText);



}
