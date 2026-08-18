namespace KvizCommando.Client.Models.ViewModels.Ui
{
    /// <summary>
    /// Az alsó fejléc egy gombjának feliratát, állapotát és műveleti azonosítóját tartalmazza.
    /// </summary>
    /// <param name="Text">A gombon megjelenő szöveg.</param>
    /// <param name="Enable">Jelzi, hogy a gomb használható-e.</param>
    /// <param name="Visible">Jelzi, hogy a gomb látható-e.</param>
    /// <param name="ClickId">A kattintási művelet azonosítója.</param>
    /// <param name="ToolTip">A gomb magyarázó szövege.</param>
    /// <param name="Icon">A gomb ikonazonosítója.</param>
    public sealed record SubHeaderVm(
        string Text = "",
        bool Enable = true,
        bool Visible = true,
        int ClickId = 0,
        string ToolTip = "",
        string Icon = "");

}
