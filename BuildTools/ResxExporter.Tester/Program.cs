using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResxExporter;


namespace ResxExporter.Tester
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Running ResxExporter ===");

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine("[INFO] Current base dir: " + baseDir);

            // Beállítjuk a forrás- és célmappákat relatív útvonal alapján
            string resxSource = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\KvizCommando\KvizCommando.Server\Resources\localization"));
            string jsonTarget = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\..\KvizCommando\KvizCommando.Server\wwwroot\localization"));

            Console.WriteLine("[INFO] Resx folder: " + resxSource);
            Console.WriteLine("[INFO] Json output: " + jsonTarget);

            bool success = ResxExporterLogic.ProcessResx(
                resxSource,
                jsonTarget,
                s => Console.WriteLine("[INFO] " + s),
                s => Console.WriteLine("[WARN] " + s),
                s => Console.WriteLine("[ERROR] " + s)
            );

            Console.WriteLine($"=== ResxExporter finished. Success: {success} ===");
            Console.ReadLine();
        }
    }
}
