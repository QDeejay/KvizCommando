using KvizCommando.Server.Identity;

namespace KvizCommando.Server.Infrastructure.Email;

public interface IAccountNotificationSender
{
    /// <summary>Értesítést küld a felhasználónak a sikeres jelszómódosításról.</summary>
    /// <param name="user">Az értesítendő Identity-felhasználó.</param>
    /// <param name="ct">A művelet megszakítását jelző token.</param>
    Task SendPasswordChangedAsync(ApplicationUser user, CancellationToken ct = default);
}
