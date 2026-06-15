using Microsoft.AspNetCore.Components;

namespace KvizCommando.Client;

public partial class App : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
       
        Console.WriteLine($" [{this}]has been started");
        await Task.Delay(5);
    }
}

