using System;

namespace mssqlMCP.Models;

/// <summary>
/// Response model for updating an existing database connection
/// </summary>
public class UpdateConnectionResponse
{
    /// <summary>
    /// Indicates if the operation was successful
    /// </summary>
    public bool Success
    {
        get; set;
    }

    /// <summary>
    /// Message describing the result of the operation
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
