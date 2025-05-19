namespace mssqlMCP.Interfaces;

/// <summary>
/// Interface for retrieving and managing connection strings
/// </summary>
public interface IConnectionStringProvider
{
    /// <summary>
    /// Gets a database connection string by name
    /// </summary>
    /// <param name="connectionName">Name of the connection string, null or empty for default</param>
    /// <returns>The connection string</returns>
    string GetConnectionString(string? connectionName = null);

    /// <summary>
    /// Gets a database connection string by name asynchronously
    /// </summary>
    /// <param name="connectionName">Name of the connection string, null or empty for default</param>
    /// <returns>The connection string</returns>
    Task<string> GetConnectionStringAsync(string? connectionName = null);

    /// <summary>
    /// Saves a connection string
    /// </summary>
    /// <param name="name">Name of the connection</param>
    /// <param name="connectionString">Connection string to save</param>
    /// <returns>Task representing the operation</returns>
    Task SaveConnectionStringAsync(string name, string connectionString);

    /// <summary>
    /// Removes a connection string
    /// </summary>
    /// <param name="name">Name of the connection to remove</param>
    /// <returns>Task representing the operation</returns>
    Task RemoveConnectionStringAsync(string name);

    /// <summary>
    /// Gets all available connection names
    /// </summary>
    /// <returns>Collection of connection names</returns>
    Task<IEnumerable<string>> GetConnectionNamesAsync();
}
