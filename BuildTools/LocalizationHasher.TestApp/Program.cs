using System;
using LocalizationHasher;

namespace LocalizationHasher.TestApp
{
    internal class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("=== Localization Manifest Hasher Test ===");

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: LocalizationHasher.TestApp <input-root>");
                return 1;
            }

            string inputRoot = args[0];

            bool success = LocalizationHashLogic.ProcessRoot(
                inputRoot,
                msg => Console.WriteLine("[INFO] " + msg),
                warn => Console.WriteLine("[WARN] " + warn),
                err => Console.WriteLine("[ERROR] " + err)
            );

            Console.WriteLine(success
                ? "✅ Hashing completed successfully."
                : "❌ Hashing failed.");

            return success ? 0 : 1;
        }
    }
}
