using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client;

public partial class App : ComponentBase
{
    protected override void OnInitialized()
    {
        Console.WriteLine($" [{this}]has been started");
    }
}
