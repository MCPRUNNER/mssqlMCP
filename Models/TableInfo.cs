using System.Collections.Generic;

namespace mssqlMCP.Models
{
    /// <summary>
    /// Represents database table metadata information
    /// </summary>
    public class TableInfo
    {
        /// <summary>
        /// Database schema name
        /// </summary>
        public string Schema { get; set; } = string.Empty;

        /// <summary>
        /// Table name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Collection of columns in the table
        /// </summary>
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();

        /// <summary>
        /// List of primary key column names
        /// </summary>
        public List<string> PrimaryKeys { get; set; } = new List<string>();

        /// <summary>
        /// Collection of foreign key relationships
        /// </summary>
        public List<ForeignKeyInfo> ForeignKeys { get; set; } = new List<ForeignKeyInfo>();
    }
}
