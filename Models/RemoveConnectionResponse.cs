using System;

namespace mssqlMCP.Models;

/// <summary>
/// Response model for removing a database connection
/// </summary>
public class RemoveConnectionResponse
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
