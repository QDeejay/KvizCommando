namespace KvizCommando.Client.Models.ViewModels.Ui
{
    public sealed record SubHeaderVm(
        string Text = "",
        bool Enable = true,
        bool Visible = true,
        int ClickId = 0,
        string ToolTip = "",
        string Icon = "");

}
