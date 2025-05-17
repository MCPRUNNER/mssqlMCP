namespace mssqlMCP.Models;

/// <summary>
/// Represents database column metadata information
/// </summary>
public class ColumnInfo
{
    /// <summary>
    /// Column name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SQL data type
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Whether the column accepts NULL values
    /// </summary>
    public bool IsNullable
    {
        get; set;
    }

    /// <summary>
    /// Maximum length for character data types
    /// </summary>
    public int? MaxLength
    {
        get; set;
    }

    /// <summary>
    /// Precision for numeric data types
    /// </summary>
    public int? Precision
    {
        get; set;
    }

    /// <summary>
    /// Scale for numeric data types
    /// </summary>
    public int? Scale
    {
        get; set;
    }

    /// <summary>
    /// Default value expression
    /// </summary>
    public string? DefaultValue
    {
        get; set;
    }

    /// <summary>
    /// Whether the column is part of the primary key
    /// </summary>
    public bool IsPrimaryKey
    {
        get; set;
    }

    /// <summary>
    /// Whether the column is part of a foreign key
    /// </summary>
    public bool IsForeignKey
    {
        get; set;
    }        /// <summary>
             /// Reference to the foreign key target (schema.table.column)
             /// </summary>
    public string? ForeignKeyReference
    {
        get; set;
    }        /// <summary>
             /// Additional description (used for stored procedure parameters, etc.)
             /// </summary>
    public string? Description
    {
        get; set;
    }
}
