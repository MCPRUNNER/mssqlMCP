using Microsoft.Extensions.Configuration;
using mssqlMCP.Interfaces;

namespace mssqlMCP.Configuration
{
    /// <summary>
    /// Provides access to database connection strings from configuration
    /// </summary>
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the ConnectionStringProvider
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        public ConnectionStringProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets a database connection string by name
        /// </summary>
        /// <param name="connectionName">Name of the connection string, null or empty for default</param>
        /// <returns>The connection string</returns>
        public string GetConnectionString(string? connectionName = null)
        {
            var connStr = connectionName != null && !string.IsNullOrEmpty(connectionName)
                ? _configuration.GetConnectionString(connectionName) ?? _configuration.GetConnectionString("DefaultConnection")
                : _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connStr))
            {
                throw new InvalidOperationException("Connection string not found");
            }

            return connStr;
        }
    }
}
