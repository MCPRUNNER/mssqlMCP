using System.ComponentModel;

namespace mssqlMCP.Interfaces
{
    /// <summary>
    /// Interface for SQL Server MCP tools functionality
    /// </summary>
    public interface ISqlServerTools
    {
        /// <summary>
        /// Initialize the SQL Server connection
        /// </summary>
        /// <param name="connectionName">The name of the connection string to use</param>
        /// <returns>Confirmation message</returns>
        Task<string> Initialize(string connectionName = "DefaultConnection");

        /// <summary>
        /// Echoes the message back to the client
        /// </summary>
        /// <param name="message">The message to echo</param>
        /// <returns>The echoed message</returns>
        string Echo(string message);

        /// <summary>
        /// Echoes the message back to the client. F1 version.
        /// </summary>
        /// <param name="message">The message to echo</param>
        /// <returns>The echoed message</returns>
        string F1Echo(string message);

        /// <summary>
        /// Executes a SQL query and returns the results as JSON
        /// </summary>
        /// <param name="query">The SQL query to execute</param>
        /// <param name="connectionName">The name of the connection string to use</param>
        /// <returns>JSON representation of query results</returns>
        Task<string> ExecuteQuery(string query, string connectionName = "DefaultConnection");

        /// <summary>
        /// Gets detailed metadata about database tables, columns, primary keys and foreign keys
        /// </summary>
        /// <param name="connectionName">The name of the connection string to use</param>
        /// <param name="schema">Optional schema filter</param>
        /// <returns>JSON representation of database metadata</returns>
        Task<string> GetTableMetadata(string connectionName = "DefaultConnection", string? schema = null);
    }
}
