using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ResxExporter
{
    public class ResxExporter : Microsoft.Build.Utilities.Task
    {
        [Required]
        public string SourceRoot { get; set; } = null!;

        [Required]
        public string OutputRoot { get; set; } = null!;

        /// <summary>
        /// Végrehajtja az MSBuild feladatot.
        /// </summary>
        public override bool Execute()
        {
            return ResxExporterLogic.ProcessResx(
                SourceRoot,
                OutputRoot,
                msg => Log.LogMessage(MessageImportance.High, msg),
                warn => Log.LogWarning(warn),
                err => Log.LogError(err)
            );
        }
    }
}
