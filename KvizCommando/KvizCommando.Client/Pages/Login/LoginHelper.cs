namespace KvizCommando.Client.Pages.Login
{
    public sealed class LoginHelper
    {
        internal static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        internal static string MaskEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return email ?? string.Empty;

            int atIndex = email.IndexOf('@');
            return atIndex <= 2 ? email : email[..2] + "*****" + email[atIndex..];
        }
    }
}
