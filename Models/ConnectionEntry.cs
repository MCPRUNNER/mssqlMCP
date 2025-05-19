using System;

namespace mssqlMCP.Models;

/// <summary>
/// Represents a database connection entry stored in SQLite
/// </summary>
public class ConnectionEntry
{
    /// <summary>
    /// Unique name/identifier for the connection
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The connection string (will be encrypted when stored)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Type of database server (e.g., "SqlServer", "PostgreSQL", etc.)
    /// </summary>
    public string ServerType { get; set; } = "SqlServer";

    /// <summary>
    /// Optional description for the connection
    /// </summary>
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// When this connection was last used
    /// </summary>
    public DateTime? LastUsed
    {
        get; set;
    }

    /// <summary>
    /// When this connection was created
    /// </summary>
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
