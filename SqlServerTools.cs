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

                // Create a cancellation token source with a reasonable timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                try
                {
                    await connection.OpenAsync(cts.Token);
                    _logger.LogInformation("Successfully connected to SQL Server database");
                    await connection.CloseAsync();
                    return $"Successfully connected to SQL Server database using connection: {connectionName}";
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Connection attempt timed out or was canceled");
                    throw new TimeoutException("Connection attempt timed out. Check your database server.");
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
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

            // Create a cancellation token source with a reasonable timeout for database operations
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

            try
            {
                var connectionString = GetConnectionString(connectionName);
                using var connection = new SqlConnection(connectionString);

                try
                {
                    await connection.OpenAsync(cts.Token);

                    using var command = new SqlCommand(query, connection);
                    command.CommandTimeout = 60; // Set command timeout in seconds

                    using var reader = await command.ExecuteReaderAsync(cts.Token);

                    var dataTable = new DataTable();
                    dataTable.Load(reader);

                    return ConvertDataTableToJson(dataTable);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Query execution was canceled or timed out");
                    throw new TimeoutException("The SQL query execution timed out or was canceled.");
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, $"SQL error executing query: {query}");
                throw;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException || ex is TimeoutException))
            {
                _logger.LogError(ex, $"Error executing query: {query}");
                throw;
            }
        }
        [McpServerTool, Description("Gets detailed metadata about the database tables, columns, primary keys and foreign keys.")]
        public static async Task<string> GetTableMetadata(string connectionName = "DefaultConnection", string? schema = null)
        {
            _logger.LogInformation("Getting table metadata for connection: {ConnectionName}, schema: {Schema}", connectionName, schema ?? "all schemas");
            // Create a cancellation token source with a reasonable timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120)); // 2 minutes timeout for metadata

            try
            {
                var connectionString = GetConnectionString(connectionName);
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
                var metadataProvider = new DatabaseMetadataProvider(connectionString, logger); try
                {
                    var databaseSchema = await metadataProvider.GetDatabaseSchemaAsync(cts.Token, schema);
                    return JsonSerializer.Serialize(databaseSchema, new JsonSerializerOptions { WriteIndented = true });
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Metadata retrieval was canceled or timed out");
                    throw new TimeoutException("The metadata retrieval timed out or was canceled.");
                }
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError(sqlEx, "SQL error getting table metadata");
                throw;
            }
            catch (Exception ex) when (!(ex is OperationCanceledException || ex is TimeoutException))
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
