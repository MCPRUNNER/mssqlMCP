using System;

namespace mssqlMCP.Models;

/// <summary>
/// Request model for updating an existing database connection
/// </summary>
public class UpdateConnectionRequest
{
    /// <summary>
    /// Name of the connection to update (required)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// New connection string for the database (required)
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Optional new description for the connection
    /// </summary>
    public string? Description
    {
        get; set;
    }
}
