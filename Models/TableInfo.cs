using System.Collections.Generic;

namespace mssqlMCP.Models;

/// <summary>
/// Represents database table or view metadata information
/// </summary>
public class TableInfo
{
    /// <summary>
    /// Database schema name
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// Table or view name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of object (BASE TABLE, VIEW, PROCEDURE, FUNCTION, etc.)
    /// </summary>
    public string ObjectType { get; set; } = "BASE TABLE";

    /// <summary>
    /// SQL definition for views, procedures, and functions
    /// </summary>
    public string? Definition
    {
        get; set;
    }

    /// <summary>
    /// Additional properties for specific object types
    /// </summary>
    public Dictionary<string, string>? Properties
    {
        get; set;
    }

    /// <summary>
    /// Collection of columns in the table or view
    /// </summary>
    public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();

    /// <summary>
    /// List of primary key column names
    /// </summary>
    public List<string> PrimaryKeys { get; set; } = new List<string>();    /// <summary>
                                                                           /// Collection of foreign key relationships
                                                                           /// </summary>
    public List<ForeignKeyInfo> ForeignKeys { get; set; } = new List<ForeignKeyInfo>();
}
