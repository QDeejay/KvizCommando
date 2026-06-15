
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace LocalizationHasher
{
    public class LocalizationHasher : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string InputRoot { get; set; } = string.Empty;
        public string OutputRoot { get; set; } = string.Empty;
        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.High, "[Hasher] Task started.");
            return LocalizationHashLogic.ProcessRoot(
                InputRoot, OutputRoot,
                msg => Log.LogMessage(MessageImportance.Normal, msg),
                warn => Log.LogWarning(warn),
                err => Log.LogError(err)
            );
        }
    }
}
