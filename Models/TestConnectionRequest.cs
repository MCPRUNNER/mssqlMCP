namespace mssqlMCP.Models;

/// <summary>
/// Request model for testing a database connection
/// </summary>
public class TestConnectionRequest
{

    /// <summary>
    /// Connection string to test
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}