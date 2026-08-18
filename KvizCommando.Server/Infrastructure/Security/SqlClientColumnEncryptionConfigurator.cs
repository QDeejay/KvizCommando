using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace KvizCommando.Server.Infrastructure.Security
{
    /// <summary>
    /// Az SQL Server Always Encrypted kapcsolati beállítását kezelő segédosztály.
    /// </summary>
    public static class SqlClientColumnEncryptionConfigurator
    {
        /// <summary>
        /// Engedélyezi a Column Encryption Setting értéket a kapcsolati karakterláncban.
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
