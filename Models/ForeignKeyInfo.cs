namespace mssqlMCP.Models;

/// <summary>
/// Represents a foreign key relationship metadata
/// </summary>
public class ForeignKeyInfo
{
    /// <summary>
    /// Foreign key constraint name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Column name in the referring table
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// Schema name of the referenced table
    /// </summary>
    public string ReferencedSchema { get; set; } = string.Empty;

    /// <summary>
    /// Name of the referenced table
    /// </summary>
    public string ReferencedTable { get; set; } = string.Empty;

    /// <summary>
    /// Column name in the referenced table
    /// </summary>
    public string ReferencedColumn { get; set; } = string.Empty;
}
