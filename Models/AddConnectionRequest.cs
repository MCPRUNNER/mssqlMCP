using System;

namespace mssqlMCP.Models;

/// <summary>
/// Request model for adding a new database connection
/// </summary>
public class AddConnectionRequest
{
    /// <summary>
    /// The name of the connection
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The connection string for the database
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the connection
    /// </summary>
    public string? Description
    {
        get; set;
    }
}
