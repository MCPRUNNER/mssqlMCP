using System;

namespace mssqlMCP.Models;

/// <summary>
/// Request model for removing a database connection
/// </summary>
public class RemoveConnectionRequest
{
    /// <summary>
    /// Name of the connection to remove (required)
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
