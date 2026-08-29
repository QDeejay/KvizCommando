using System.Text;
using DatabaseMigrationTool;

Console.OutputEncoding = Encoding.UTF8;
Console.Title = "KvizCommando Database Migration Tool";

MigrationWorkflow workflow;

try
{
    workflow = MigrationWorkflow.Create();
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    Console.WriteLine();
    Console.WriteLine("Nyomj Entert a kilépéshez.");
    Console.ReadLine();
    return;
}

while (true)
{
    Console.WriteLine();
    Console.WriteLine("KVIZ COMMANDO DATABASE TOOL");
    Console.WriteLine();
    Console.WriteLine("[1] Development migration");
    Console.WriteLine("[2] Generate production scripts");
    Console.WriteLine("[3] Generate + upload production scripts");
    Console.WriteLine("[0] Exit");
    Console.WriteLine();
    Console.Write("Selection: ");

    var selection = Console.ReadLine()?.Trim();
    Console.WriteLine();

    try
    {
        switch (selection)
        {
            case "1":
                await workflow.RunDevelopmentMigrationAsync();
                break;

            case "2":
                await workflow.GenerateProductionScriptsAsync(upload: false);
                break;

            case "3":
                await workflow.GenerateProductionScriptsAsync(upload: true);
                break;

            case "0":
                return;

            default:
                Console.WriteLine("Ismeretlen menüpont.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine();
        Console.Error.WriteLine("UNEXPECTED ERROR");
        Console.Error.WriteLine(ex);
        Console.Error.WriteLine("A folyamat leállt, további adatbázis-művelet nem futott.");
    }
}
