using System.ComponentModel;
using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using mssqlMCP.Interfaces;
using mssqlMCP.Services;

namespace mssqlMCP.Tools;

/// <summary>
/// Provides MCP tools for SQL Server integration
/// </summary>
[McpServerToolType]
public class SqlServerTools : ISqlServerTools
{
    private readonly ILogger<SqlServerTools> _logger;
    private readonly IConnectionStringProvider _connectionStringProvider;
    private readonly IConnectionManager _connectionManager;

    /// <summary>
    /// Initializes a new instance of the SqlServerTools class
    /// </summary>
    /// <param name="logger">Logger for the tools</param>
    /// <param name="connectionStringProvider">Legacy provider for connection strings</param>
    /// <param name="connectionManager">Manager for database connections</param>
    public SqlServerTools(
        ILogger<SqlServerTools> logger,
        IConnectionStringProvider connectionStringProvider,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _connectionStringProvider = connectionStringProvider;
        _connectionManager = connectionManager;
    }        /// <summary>
             /// Initialize the SQL Server connection
             /// </summary>
    [McpServerTool, Description("Initialize the SQL Server connection.")]
    public async Task<string> Initialize(string connectionName = "DefaultConnection")
    {
        try
        {
            // Create a cancellation token source with a reasonable timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            try
            {
                // Use the connection manager to get a connection
                using var connection = await _connectionManager.GetConnectionAsync(connectionName);
                _logger.LogInformation("Successfully connected to SQL Server database");
                await connection.CloseAsync();
                return $"Successfully connected to SQL Server database using connection: {connectionName}";
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Connection attempt timed out or was canceled");
                return "Connection attempt timed out. Check if your database server is running and accessible.";
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error initializing connection");

            // Check for login failure errors (error numbers 4060, 18456, 18452, etc.)
            if (sqlEx.Number == 4060 || sqlEx.Number == 18456 || sqlEx.Number == 18452)
            {
                return $"Login failed: Unable to access database with connection '{connectionName}'. Please check your credentials and permissions.";
            }

            return $"Database error occurred while connecting to '{connectionName}'.";
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is SqlException))
        {
            _logger.LogError(ex, "Error initializing SQL Server connection");
            return $"An unexpected error occurred while connecting to the database.";
        }
    }


    /// <summary>
    /// Executes a SQL query and returns the results as JSON
    /// </summary>
    [McpServerTool, Description("Executes a SQL query and returns the results as JSON.")]
    public async Task<string> ExecuteQuery(string query, string connectionName = "DefaultConnection")
    {
        _logger.LogInformation($"Executing query: {query}");

        // Create a cancellation token source with a reasonable timeout for database operations
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));

        try
        {
            // Use the connection manager to get a connection
            using var connection = await _connectionManager.GetConnectionAsync(connectionName);

            try
            {
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
                return "{ \"error\": \"The SQL query execution timed out. Your query might be too complex or the database server is busy.\" }";
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, $"SQL error executing query: {query}");

            // Check for login failure errors (error numbers 4060, 18456, 18452, etc.)
            if (sqlEx.Number == 4060 || sqlEx.Number == 18456 || sqlEx.Number == 18452)
            {
                return "{ \"error\": \"Cannot access database or connection. Authentication failed.\" }";
            }

            return "{ \"error\": \"Database error occurred while executing query.\" }";
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is TimeoutException))
        {
            _logger.LogError(ex, $"Error executing query: {query}");
            return "{ \"error\": \"An unexpected error occurred while executing query.\" }";
        }
    }        /// <summary>
             /// Gets detailed metadata about database tables, columns, primary keys and foreign keys
             /// </summary>
    [McpServerTool, Description("Gets detailed metadata about the database tables, columns, primary keys and foreign keys.")]
    public async Task<string> GetTableMetadata(string connectionName = "DefaultConnection", string? schema = null)
    {
        _logger.LogInformation("Getting table metadata for connection: {ConnectionName}, schema: {Schema}", connectionName, schema ?? "all schemas");

        // Create a cancellation token source with a reasonable timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120)); // 2 minutes timeout for metadata

        try
        {
            // Use the connection manager to get connection details
            var connection = await _connectionManager.GetConnectionEntryAsync(connectionName);
            string connectionString;

            if (connection != null)
            {
                connectionString = connection.ConnectionString;
            }
            else
            {
                // Fall back to legacy provider if not found in SQLite
                connectionString = _connectionStringProvider.GetConnectionString(connectionName);
            }

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
            var metadataProvider = new DatabaseMetadataProvider(connectionString, logger);

            try
            {
                var databaseSchema = await metadataProvider.GetDatabaseSchemaAsync(cts.Token, schema);
                return JsonSerializer.Serialize(databaseSchema, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Metadata retrieval was canceled or timed out");
                return "{ \"error\": \"The metadata retrieval timed out. The database schema might be very large or the server is busy.\" }";
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error getting table metadata");

            // Check for login failure errors (error numbers 4060, 18456, 18452, etc.)
            if (sqlEx.Number == 4060 || sqlEx.Number == 18456 || sqlEx.Number == 18452)
            {
                // Return a simple message instead of throwing an exception
                return "{ \"error\": \"Cannot access database or connection. Authentication failed.\" }";
            }

            // For other SQL errors, provide generic message
            return "{ \"error\": \"Database error occurred while retrieving metadata.\" }";
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is TimeoutException))
        {
            _logger.LogError(ex, "Error getting table metadata");
            return "{ \"error\": \"An unexpected error occurred while retrieving table metadata.\" }";
        }
    }        /// <summary>
             /// Gets detailed metadata about database objects including tables and views
             /// </summary>
    [McpServerTool, Description("Gets detailed metadata about database objects including tables and views.")]
    public async Task<string> GetDatabaseObjectsMetadata(string connectionName = "DefaultConnection", string? schema = null, bool includeViews = true)
    {
        _logger.LogInformation("Getting database objects metadata for connection: {ConnectionName}, schema: {Schema}, includeViews: {IncludeViews}",
            connectionName, schema ?? "all schemas", includeViews);

        // Create a cancellation token source with a reasonable timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120)); // 2 minutes timeout for metadata

        try
        {
            // Use the connection manager to get connection details
            var connection = await _connectionManager.GetConnectionEntryAsync(connectionName);
            string connectionString;

            if (connection != null)
            {
                connectionString = connection.ConnectionString;
            }
            else
            {
                // Fall back to legacy provider if not found in SQLite
                connectionString = _connectionStringProvider.GetConnectionString(connectionName);
            }

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
            var metadataProvider = new DatabaseMetadataProvider(connectionString, logger); try
            {
                var databaseSchema = await metadataProvider.GetDatabaseSchemaAsync(cts.Token, schema);

                // Filter out views if requested
                if (!includeViews)
                {
                    databaseSchema = databaseSchema.Where(t => t.ObjectType == "BASE TABLE").ToList();
                }

                return JsonSerializer.Serialize(databaseSchema, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Metadata retrieval was canceled or timed out");
                return "{ \"error\": \"The metadata retrieval timed out. The database schema might be very large or the server is busy.\" }";
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error getting database objects metadata");

            // Check for login failure errors (error numbers 4060, 18456, 18452, etc.)
            if (sqlEx.Number == 4060 || sqlEx.Number == 18456 || sqlEx.Number == 18452)
            {
                // Return a simple message instead of throwing an exception
                return "{ \"error\": \"Cannot access database or connection. Authentication failed.\" }";
            }

            // For other SQL errors, provide generic message
            return "{ \"error\": \"Database error occurred while retrieving metadata.\" }";
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is TimeoutException))
        {
            _logger.LogError(ex, "Error getting database objects metadata");
            return "{ \"error\": \"An unexpected error occurred while retrieving database objects metadata.\" }";
        }
    }

    /// <summary>
    /// Gets detailed metadata about specific database object types (tables or views)
    /// </summary>
    [McpServerTool, Description("Gets detailed metadata about specific database object types.")]
    public async Task<string> GetDatabaseObjectsMetadata(string connectionName = "DefaultConnection", string? schema = null, string objectType = "ALL")
    {
        _logger.LogInformation("Getting database objects metadata for connection: {ConnectionName}, schema: {Schema}, objectType: {ObjectType}",
            connectionName, schema ?? "all schemas", objectType);

        // Create a cancellation token source with a reasonable timeout
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120)); // 2 minutes timeout for metadata

        try
        {
            // Use the connection manager to get connection details
            var connection = await _connectionManager.GetConnectionEntryAsync(connectionName);
            string connectionString;

            if (connection != null)
            {
                connectionString = connection.ConnectionString;
            }
            else
            {
                // Fall back to legacy provider if not found in SQLite
                connectionString = _connectionStringProvider.GetConnectionString(connectionName);
            }

            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
            var metadataProvider = new DatabaseMetadataProvider(connectionString, logger);

            try
            {
                var databaseSchema = await metadataProvider.GetDatabaseSchemaAsync(cts.Token, schema);

                // Filter by object type if specified                    if (!string.IsNullOrEmpty(objectType) && objectType.ToUpper() != "ALL")
                {
                    if (objectType.ToUpper() == "TABLE" || objectType.ToUpper() == "TABLES")
                    {
                        databaseSchema = databaseSchema.Where(t => t.ObjectType == "BASE TABLE").ToList();
                    }
                    else if (objectType.ToUpper() == "VIEW" || objectType.ToUpper() == "VIEWS")
                    {
                        databaseSchema = databaseSchema.Where(t => t.ObjectType == "VIEW").ToList();
                    }
                    else if (objectType.ToUpper() == "PROCEDURE" || objectType.ToUpper() == "PROC" || objectType.ToUpper() == "PROCEDURES")
                    {
                        databaseSchema = databaseSchema.Where(t => t.ObjectType == "PROCEDURE").ToList();
                    }
                    else if (objectType.ToUpper() == "FUNCTION" || objectType.ToUpper() == "FUNC" || objectType.ToUpper() == "FUNCTIONS")
                    {
                        databaseSchema = databaseSchema.Where(t => t.ObjectType == "FUNCTION").ToList();
                    }
                }

                return JsonSerializer.Serialize(databaseSchema, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Metadata retrieval was canceled or timed out");
                return "{ \"error\": \"The metadata retrieval timed out. The database schema might be very large or the server is busy.\" }";
            }
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error getting database objects metadata");

            // Check for login failure errors (error numbers 4060, 18456, 18452, etc.)
            if (sqlEx.Number == 4060 || sqlEx.Number == 18456 || sqlEx.Number == 18452)
            {
                // Return a simple message instead of throwing an exception
                return "{ \"error\": \"Cannot access database or connection. Authentication failed.\" }";
            }

            // For other SQL errors, provide generic message
            return "{ \"error\": \"Database error occurred while retrieving metadata.\" }";
        }
        catch (Exception ex) when (!(ex is OperationCanceledException || ex is TimeoutException))
        {
            _logger.LogError(ex, "Error getting database objects metadata");
            return "{ \"error\": \"An unexpected error occurred while retrieving database objects metadata.\" }";
        }
    }

    /// <summary>
    /// Gets SQL Server Agent job metadata from msdb
    /// </summary>
    [McpServerTool, Description("Gets SQL Server Agent job metadata (jobs, status, owner, etc.) from msdb.")]
    public async Task<string> GetSqlServerAgentJobs(string connectionName = "DefaultConnection")
    {
        _logger.LogInformation("Getting SQL Server Agent jobs for connection: {ConnectionName}", connectionName);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try
        {
            var connection = await _connectionManager.GetConnectionEntryAsync(connectionName);
            string connectionString;
            if (connection != null)
            {
                connectionString = connection.ConnectionString;
            }
            else
            {
                connectionString = _connectionStringProvider.GetConnectionString(connectionName);
            }
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
            var metadataProvider = new DatabaseMetadataProvider(connectionString, logger);
            var jobs = await metadataProvider.GetSqlServerAgentJobsAsync(cts.Token);
            return JsonSerializer.Serialize(jobs, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SQL Server Agent jobs");
            return "{ \"error\": \"An error occurred while retrieving SQL Server Agent jobs.\" }";
        }
    }

    /// <summary>
    /// Gets detailed information (steps, schedules, history) for a specific SQL Server Agent job
    /// </summary>
    [McpServerTool, Description("Gets detailed information (steps, schedules, history) for a specific SQL Server Agent job.")]
    public async Task<string> GetSqlServerAgentJobDetails(string jobName, string connectionName = "DefaultConnection")
    {
        _logger.LogInformation("Getting SQL Server Agent job details for job: {JobName} on connection: {ConnectionName}", jobName, connectionName);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        try
        {
            var connection = await _connectionManager.GetConnectionEntryAsync(connectionName);
            string connectionString;
            if (connection != null)
            {
                connectionString = connection.ConnectionString;
            }
            else
            {
                connectionString = _connectionStringProvider.GetConnectionString(connectionName);
            }
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
            var metadataProvider = new DatabaseMetadataProvider(connectionString, logger);
            var job = await metadataProvider.GetSqlServerAgentJobDetailsAsync(jobName, cts.Token);
            return JsonSerializer.Serialize(job, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SQL Server Agent job details for {JobName}", jobName);
            return "{ \"error\": \"An error occurred while retrieving SQL Server Agent job details.\" }";
        }
    }

    /// <summary>
    /// Gets SSIS catalog information including Project Deployment and Package Deployment models
    /// </summary>
    [McpServerTool, Description("Gets SSIS catalog information including Project Deployment and Package Deployment models.")]
    public async Task<string> GetSsisCatalogInfo(string connectionName = "DefaultConnection")
    {
        _logger.LogInformation("Getting SSIS catalog information for connection: {ConnectionName}", connectionName);
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        try
        {
            var connection = await _connectionManager.GetConnectionEntryAsync(connectionName);
            string connectionString;
            if (connection != null)
            {
                connectionString = connection.ConnectionString;
            }
            else
            {
                connectionString = _connectionStringProvider.GetConnectionString(connectionName);
            }
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
            var metadataProvider = new DatabaseMetadataProvider(connectionString, logger);
            var catalogInfo = await metadataProvider.GetSsisCatalogInfoAsync(cts.Token);
            return JsonSerializer.Serialize(catalogInfo, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SSIS catalog information");
            return "{ \"error\": \"An error occurred while retrieving SSIS catalog information. The catalog may not exist on this server or you may not have sufficient permissions.\" }";
        }
    }

    /// <summary>
    /// Converts a DataTable to JSON string
    /// </summary>
    /// <param name="dataTable">The DataTable to convert</param>
    /// <returns>JSON representation of the DataTable</returns>
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
