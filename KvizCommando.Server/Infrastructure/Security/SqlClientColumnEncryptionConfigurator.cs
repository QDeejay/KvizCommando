using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace KvizCommando.Server.Infrastructure.Security
{
    /// <summary>
    /// Helper az Always Encrypted bekapcsolásához SqlClient-ben.
    /// </summary>
    public static class SqlClientColumnEncryptionConfigurator
    {
        /// <summary>
        /// Visszaad egy connection stringet, amelyben engedélyezve van a Column Encryption Setting=Enabled.
        /// Ha már be van állítva, változatlanul adja vissza.
        /// </summary>
        public static string WithAlwaysEncrypted(string connectionString)
        {
            var csb = new SqlConnectionStringBuilder(connectionString);
            if (!csb.ContainsKey("Column Encryption Setting"))
            {
                csb["Column Encryption Setting"] = "Enabled";
            }
            else
            {
                csb["Column Encryption Setting"] = "Enabled";
            }
            return csb.ToString();
        }
    }
}
