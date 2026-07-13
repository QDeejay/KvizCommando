namespace KvizCommando.Client.Models.ViewModels
{
    public sealed record SubHeaderVm(
        string Text = "",
        bool Enable = true,
        bool Visible = true,
        int ClickId = 0,
        string Icon = "");

}
