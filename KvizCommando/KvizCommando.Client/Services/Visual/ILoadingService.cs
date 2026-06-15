using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace KvizCommando.Client.Services.Visual
{
    public interface ILoadingService
    {
        Task Show();
        Task Hide();
        Task IsActive();
    }
}
