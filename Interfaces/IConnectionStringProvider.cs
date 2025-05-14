namespace mssqlMCP.Interfaces
{
    /// <summary>
    /// Interface for retrieving connection strings
    /// </summary>
    public interface IConnectionStringProvider
    {
        /// <summary>
        /// Gets a database connection string by name
        /// </summary>
        /// <param name="connectionName">Name of the connection string, null or empty for default</param>
        /// <returns>The connection string</returns>
        string GetConnectionString(string? connectionName = null);
    }
}
