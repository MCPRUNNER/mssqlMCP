using System.ComponentModel;
using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using ModelContextProtocol.Server;

namespace mssqlMCP
{
    [McpServerToolType]
    public static class SqlServerTools
    {
        private static readonly ILogger<Program> _logger = LoggerFactory.Create(builder =>
            builder.AddConsole()).CreateLogger<Program>();

        private static string GetConnectionString(string? connectionName = null)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var connStr = connectionName != null && !string.IsNullOrEmpty(connectionName)
                ? config.GetConnectionString(connectionName) ?? config.GetConnectionString("DefaultConnection")
                : config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connStr))
            {
                throw new InvalidOperationException("Connection string not found");
            }

            return connStr;
        }
        [McpServerTool, Description("Initialize the SQL Server connection.")]
        public static async Task<string> Initialize(string connectionName = "DefaultConnection")
        {
            try
            {
                var connectionString = GetConnectionString(connectionName);
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                _logger.LogInformation("Successfully connected to SQL Server database");
                await connection.CloseAsync();
                return $"Successfully connected to SQL Server database using connection: {connectionName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing SQL Server connection");
                throw;
            }
        }

        [McpServerTool, Description("Echoes the message back to the client.")]
        public static string Echo(string message)
        {
            _logger.LogInformation($"Echo received: {message}");
            return message;
        }

        [McpServerTool, Description("Echoes the message back to the client. F1 version.")]
        public static string F1Echo(string message)
        {
            _logger.LogInformation($"F1 Echo received: {message}");
            return message;
        }
        [McpServerTool, Description("Executes a SQL query and returns the results as JSON.")]
        public static async Task<string> ExecuteQuery(string query, string connectionName = "DefaultConnection")
        {
            _logger.LogInformation($"Executing query: {query}");
            try
            {
                var connectionString = GetConnectionString(connectionName);
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var dataTable = new DataTable();
                dataTable.Load(reader);

                return ConvertDataTableToJson(dataTable);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing query: {query}");
                throw;
            }
        }
        [McpServerTool, Description("Gets detailed metadata about the database tables, columns, primary keys and foreign keys.")]
        public static async Task<string> GetTableMetadata(string connectionName = "DefaultConnection")
        {
            try
            {
                var connectionString = GetConnectionString(connectionName);
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
                var metadataProvider = new DatabaseMetadataProvider(connectionString, logger);

                var databaseSchema = await metadataProvider.GetDatabaseSchemaAsync();

                return JsonSerializer.Serialize(databaseSchema, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table metadata");
                throw;
            }
        }
        private static string ConvertDataTableToJson(DataTable dataTable)
        {
            var rows = new List<Dictionary<string, object?>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var rowDict = new Dictionary<string, object?>();
                foreach (DataColumn column in dataTable.Columns)
                {
                    rowDict[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                }
                rows.Add(rowDict);
            }

            return JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
