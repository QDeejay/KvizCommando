using System.Text;
using Terminal.Gui;

namespace KvizCommando.Admin;

internal static class Program
{
    public static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            var settings = AdminSettings.Resolve();
            using var database = new AdminDatabase(settings);
            database.TestConnections();

            Application.Init();
            try
            {
                Application.Top.Add(new AdminMainWindow(
                    database,
                    settings.IsProduction,
                    settings.AuditOutputRoot));
                Application.Run();
            }
            finally
            {
                Application.Shutdown();
            }

            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("KVIZ COMMANDO ADMIN - INDÍTÁSI HIBA");
            Console.Error.WriteLine(exception.Message);
            return 1;
        }
    }
}
