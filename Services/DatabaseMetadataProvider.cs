using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using mssqlMCP.Interfaces;
using mssqlMCP.Models;
using System.Data;

namespace mssqlMCP.Services;
/// <summary>
/// Service that provides metadata information about SQL Server databases
/// </summary>
public partial class DatabaseMetadataProvider : IDatabaseMetadataProvider
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseMetadataProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the DatabaseMetadataProvider with connection string and logger
    /// </summary>
    /// <param name="connectionString">Database connection string</param>
    /// <param name="logger">Logger for the provider</param>
    public DatabaseMetadataProvider(string connectionString, ILogger<DatabaseMetadataProvider> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Helper method to create SQL command with timeout
    /// </summary>
    private SqlCommand CreateCommandWithTimeout(string commandText, SqlConnection connection)
    {
        var command = new SqlCommand(commandText, connection);
        command.CommandTimeout = 30; // 30 second timeout
        return command;
    }

    /// <summary>
    /// Gets database schema metadata asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="schema">Optional filter for specific schema, null returns all schemas</param>
    /// <returns>List of table metadata information</returns>
    public async Task<List<TableInfo>> GetDatabaseSchemaAsync(CancellationToken cancellationToken = default, string? schema = null)
    {
        _logger.LogInformation("Retrieving database schema" + (schema != null ? $" for schema '{schema}'" : " for all schemas"));

        var tables = new List<TableInfo>(); try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get base tables
            await GetTablesAsync(connection, tables, schema, cancellationToken);

            // Get views
            await GetViewsAsync(connection, tables, schema, cancellationToken);

            // Get stored procedures
            await GetStoredProceduresAsync(connection, tables, schema, cancellationToken);

            // Get functions
            await GetFunctionsAsync(connection, tables, schema, cancellationToken);

            return tables;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation to retrieve database schema was canceled");
            throw; // Let the calling code handle cancellation
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error retrieving database schema");

            // Add specific error handling for known SQL error codes
            if (ex.Number == 208) // Invalid object name
            {
                _logger.LogWarning("Attempted to query a non-existent table or view");
            }
            else if (ex.Number == 4060 || ex.Number == 18456) // Login failures
            {
                _logger.LogWarning("Authentication or access denied error");
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving database schema");
            throw;
        }
    }

    /// <summary>
    /// Retrieves base table metadata
    /// </summary>
    private async Task GetTablesAsync(SqlConnection connection, List<TableInfo> tables, string? schema, CancellationToken cancellationToken = default)
    {
        var tableQuery = @"
                SELECT 
                    t.TABLE_CATALOG,
                    t.TABLE_SCHEMA,
                    t.TABLE_NAME,
                    t.TABLE_TYPE
                FROM 
                    INFORMATION_SCHEMA.TABLES t
                WHERE 
                    t.TABLE_TYPE = 'BASE TABLE'
                    " + (schema != null ? "AND t.TABLE_SCHEMA = @SchemaName" : "") + @"
                ORDER BY 
                    t.TABLE_SCHEMA, t.TABLE_NAME";

        using var tableCommand = CreateCommandWithTimeout(tableQuery, connection);
        if (schema != null)
        {
            tableCommand.Parameters.AddWithValue("@SchemaName", schema);
        }
        using var tableReader = await tableCommand.ExecuteReaderAsync(cancellationToken);

        var tableNames = new List<(string Schema, string Name)>();

        while (await tableReader.ReadAsync(cancellationToken))
        {
            var schemaName = tableReader["TABLE_SCHEMA"].ToString() ?? string.Empty;
            var name = tableReader["TABLE_NAME"].ToString() ?? string.Empty;
            tableNames.Add((schemaName, name));
        }
        await tableReader.CloseAsync();

        foreach (var table in tableNames)
        {
            var tableInfo = new TableInfo
            {
                Schema = table.Schema,
                Name = table.Name,
                ObjectType = "BASE TABLE",
                Columns = new List<ColumnInfo>(),
                PrimaryKeys = new List<string>(),
                ForeignKeys = new List<ForeignKeyInfo>()
            };

            // Get columns
            await GetColumnsAsync(connection, tableInfo, cancellationToken);

            // Get primary keys
            await GetPrimaryKeysAsync(connection, tableInfo, cancellationToken);

            // Get foreign keys
            await GetForeignKeysAsync(connection, tableInfo, cancellationToken);

            tables.Add(tableInfo);
        }
    }

    /// <summary>
    /// Retrieves view metadata
    /// </summary>
    private async Task GetViewsAsync(SqlConnection connection, List<TableInfo> tables, string? schema, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving view metadata" + (schema != null ? $" for schema '{schema}'" : " for all schemas"));

        var viewQuery = @"
                SELECT 
                    v.TABLE_CATALOG,
                    v.TABLE_SCHEMA,
                    v.TABLE_NAME,
                    v.VIEW_DEFINITION
                FROM 
                    INFORMATION_SCHEMA.VIEWS v
                WHERE 
                    1=1
                    " + (schema != null ? "AND v.TABLE_SCHEMA = @SchemaName" : "") + @"
                ORDER BY 
                    v.TABLE_SCHEMA, v.TABLE_NAME";

        using var viewCommand = CreateCommandWithTimeout(viewQuery, connection);
        if (schema != null)
        {
            viewCommand.Parameters.AddWithValue("@SchemaName", schema);
        }
        using var viewReader = await viewCommand.ExecuteReaderAsync(cancellationToken);

        var viewNames = new List<(string Schema, string Name)>();

        while (await viewReader.ReadAsync(cancellationToken))
        {
            var schemaName = viewReader["TABLE_SCHEMA"].ToString() ?? string.Empty;
            var name = viewReader["TABLE_NAME"].ToString() ?? string.Empty;
            viewNames.Add((schemaName, name));
        }
        await viewReader.CloseAsync();

        foreach (var view in viewNames)
        {
            _logger.LogDebug("Processing view: {Schema}.{Name}", view.Schema, view.Name);
            var viewInfo = new TableInfo
            {
                Schema = view.Schema,
                Name = view.Name,
                ObjectType = "VIEW",
                Columns = new List<ColumnInfo>(),
                PrimaryKeys = new List<string>(),
                ForeignKeys = new List<ForeignKeyInfo>()
            };

            // Get columns for the view
            await GetColumnsAsync(connection, viewInfo, cancellationToken);

            // Views don't have their own primary and foreign keys in the traditional sense
            // but we can try to identify columns that are keys in the base tables

            // For views, we can also retrieve the view definition/query
            await GetViewDefinitionAsync(connection, viewInfo, cancellationToken);

            tables.Add(viewInfo);
        }
    }

    /// <summary>
    /// Retrieves the SQL definition of a view
    /// </summary>
    private async Task GetViewDefinitionAsync(SqlConnection connection, TableInfo viewInfo, CancellationToken cancellationToken = default)
    {
        var definitionQuery = @"
                SELECT 
                    v.VIEW_DEFINITION
                FROM 
                    INFORMATION_SCHEMA.VIEWS v
                WHERE 
                    v.TABLE_SCHEMA = @Schema
                    AND v.TABLE_NAME = @ViewName";

        using var command = CreateCommandWithTimeout(definitionQuery, connection);
        command.Parameters.AddWithValue("@Schema", viewInfo.Schema);
        command.Parameters.AddWithValue("@ViewName", viewInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken); if (await reader.ReadAsync(cancellationToken))
        {
            var definition = reader["VIEW_DEFINITION"] != DBNull.Value
                ? reader["VIEW_DEFINITION"].ToString()
                : null;
            if (!string.IsNullOrEmpty(definition))
            {
                // Store the view definition in the TableInfo object
                viewInfo.Definition = definition;

                _logger.LogDebug("View definition for {Schema}.{Name}: {Definition}",
                    viewInfo.Schema, viewInfo.Name, definition);
            }
        }
    }

    /// <summary>
    /// Retrieves column metadata for a specific table
    /// </summary>
    private async Task GetColumnsAsync(SqlConnection connection, TableInfo tableInfo, CancellationToken cancellationToken = default)
    {
        var columnQuery = @"
                SELECT 
                    c.COLUMN_NAME,
                    c.DATA_TYPE,
                    c.IS_NULLABLE,
                    c.CHARACTER_MAXIMUM_LENGTH,
                    c.NUMERIC_PRECISION,
                    c.NUMERIC_SCALE,
                    c.COLUMN_DEFAULT
                FROM 
                    INFORMATION_SCHEMA.COLUMNS c
                WHERE 
                    c.TABLE_SCHEMA = @Schema
                    AND c.TABLE_NAME = @TableName
                ORDER BY 
                    c.ORDINAL_POSITION";

        using var command = CreateCommandWithTimeout(columnQuery, connection);
        command.Parameters.AddWithValue("@Schema", tableInfo.Schema);
        command.Parameters.AddWithValue("@TableName", tableInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var columnInfo = new ColumnInfo
            {
                Name = reader["COLUMN_NAME"].ToString() ?? string.Empty,
                DataType = reader["DATA_TYPE"].ToString() ?? string.Empty,
                IsNullable = reader["IS_NULLABLE"].ToString() == "YES",
                MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]) : null,
                Precision = reader["NUMERIC_PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_PRECISION"]) : null,
                Scale = reader["NUMERIC_SCALE"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_SCALE"]) : null,
                DefaultValue = reader["COLUMN_DEFAULT"] != DBNull.Value ? reader["COLUMN_DEFAULT"].ToString() : null
            };

            tableInfo.Columns.Add(columnInfo);
        }
    }

    /// <summary>
    /// Retrieves primary key metadata for a specific table
    /// </summary>
    private async Task GetPrimaryKeysAsync(SqlConnection connection, TableInfo tableInfo, CancellationToken cancellationToken = default)
    {
        var pkQuery = @"
                SELECT 
                    c.COLUMN_NAME
                FROM 
                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN 
                    INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE c
                    ON c.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                WHERE 
                    tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    AND tc.TABLE_SCHEMA = @Schema
                    AND tc.TABLE_NAME = @TableName";

        using var command = CreateCommandWithTimeout(pkQuery, connection);
        command.Parameters.AddWithValue("@Schema", tableInfo.Schema);
        command.Parameters.AddWithValue("@TableName", tableInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var columnName = reader["COLUMN_NAME"].ToString() ?? string.Empty;
            tableInfo.PrimaryKeys.Add(columnName);

            // Update the column info to mark it as a primary key
            var column = tableInfo.Columns.FirstOrDefault(c => c.Name == columnName);
            if (column != null)
            {
                column.IsPrimaryKey = true;
            }
        }
    }

    /// <summary>
    /// Retrieves foreign key metadata for a specific table
    /// </summary>
    private async Task GetForeignKeysAsync(SqlConnection connection, TableInfo tableInfo, CancellationToken cancellationToken = default)
    {
        var fkQuery = @"
                SELECT 
                    fk.name AS FK_NAME,
                    OBJECT_SCHEMA_NAME(fk.parent_object_id) AS SCHEMA_NAME,
                    OBJECT_NAME(fk.parent_object_id) AS TABLE_NAME,
                    c1.name AS COLUMN_NAME,
                    OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS REFERENCED_SCHEMA_NAME,
                    OBJECT_NAME(fk.referenced_object_id) AS REFERENCED_TABLE_NAME,
                    c2.name AS REFERENCED_COLUMN_NAME
                FROM 
                    sys.foreign_keys fk
                INNER JOIN 
                    sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
                INNER JOIN 
                    sys.columns c1 ON fkc.parent_column_id = c1.column_id AND fkc.parent_object_id = c1.object_id
                INNER JOIN 
                    sys.columns c2 ON fkc.referenced_column_id = c2.column_id AND fkc.referenced_object_id = c2.object_id
                WHERE 
                    OBJECT_SCHEMA_NAME(fk.parent_object_id) = @Schema
                    AND OBJECT_NAME(fk.parent_object_id) = @TableName";

        using var command = CreateCommandWithTimeout(fkQuery, connection);
        command.Parameters.AddWithValue("@Schema", tableInfo.Schema);
        command.Parameters.AddWithValue("@TableName", tableInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var fkInfo = new ForeignKeyInfo
            {
                Name = reader["FK_NAME"].ToString() ?? string.Empty,
                Column = reader["COLUMN_NAME"].ToString() ?? string.Empty,
                ReferencedSchema = reader["REFERENCED_SCHEMA_NAME"].ToString() ?? string.Empty,
                ReferencedTable = reader["REFERENCED_TABLE_NAME"].ToString() ?? string.Empty,
                ReferencedColumn = reader["REFERENCED_COLUMN_NAME"].ToString() ?? string.Empty
            };

            tableInfo.ForeignKeys.Add(fkInfo);

            // Update the column info to mark it as a foreign key
            var column = tableInfo.Columns.FirstOrDefault(c => c.Name == fkInfo.Column);
            if (column != null)
            {
                column.IsForeignKey = true;
                column.ForeignKeyReference = $"{fkInfo.ReferencedSchema}.{fkInfo.ReferencedTable}.{fkInfo.ReferencedColumn}";
            }
        }
    }

    /// <summary>
    /// Retrieves stored procedure metadata
    /// </summary>
    private async Task GetStoredProceduresAsync(SqlConnection connection, List<TableInfo> tables, string? schema, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving stored procedure metadata" + (schema != null ? $" for schema '{schema}'" : " for all schemas"));

        var procQuery = @"
                SELECT 
                    ROUTINE_CATALOG,
                    ROUTINE_SCHEMA,
                    ROUTINE_NAME,
                    ROUTINE_TYPE,
                    ROUTINE_DEFINITION,
                    CREATED,
                    LAST_ALTERED
                FROM 
                    INFORMATION_SCHEMA.ROUTINES
                WHERE 
                    ROUTINE_TYPE = 'PROCEDURE'
                    " + (schema != null ? "AND ROUTINE_SCHEMA = @SchemaName" : "") + @"
                ORDER BY 
                    ROUTINE_SCHEMA, ROUTINE_NAME";

        using var procCommand = CreateCommandWithTimeout(procQuery, connection);
        if (schema != null)
        {
            procCommand.Parameters.AddWithValue("@SchemaName", schema);
        }
        using var procReader = await procCommand.ExecuteReaderAsync(cancellationToken);

        var procNames = new List<(string Schema, string Name)>();

        while (await procReader.ReadAsync(cancellationToken))
        {
            var schemaName = procReader["ROUTINE_SCHEMA"].ToString() ?? string.Empty;
            var name = procReader["ROUTINE_NAME"].ToString() ?? string.Empty;
            procNames.Add((schemaName, name));
        }
        await procReader.CloseAsync();

        foreach (var proc in procNames)
        {
            _logger.LogDebug("Processing stored procedure: {Schema}.{Name}", proc.Schema, proc.Name);

            var procInfo = new TableInfo
            {
                Schema = proc.Schema,
                Name = proc.Name,
                ObjectType = "PROCEDURE",
                Columns = new List<ColumnInfo>(),
                PrimaryKeys = new List<string>(),
                ForeignKeys = new List<ForeignKeyInfo>()
            };

            // Get the procedure definition
            await GetProcedureDefinitionAsync(connection, procInfo, cancellationToken);

            // Get the procedure parameters
            await GetProcedureParametersAsync(connection, procInfo, cancellationToken);

            tables.Add(procInfo);
        }
    }

    /// <summary>
    /// Retrieves the SQL definition of a stored procedure
    /// </summary>
    private async Task GetProcedureDefinitionAsync(SqlConnection connection, TableInfo procInfo, CancellationToken cancellationToken = default)
    {
        var definitionQuery = @"
                SELECT 
                    r.ROUTINE_DEFINITION
                FROM 
                    INFORMATION_SCHEMA.ROUTINES r
                WHERE 
                    r.ROUTINE_SCHEMA = @Schema
                    AND r.ROUTINE_NAME = @ProcName
                    AND r.ROUTINE_TYPE = 'PROCEDURE'";

        using var command = CreateCommandWithTimeout(definitionQuery, connection);
        command.Parameters.AddWithValue("@Schema", procInfo.Schema);
        command.Parameters.AddWithValue("@ProcName", procInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var definition = reader["ROUTINE_DEFINITION"] != DBNull.Value
                ? reader["ROUTINE_DEFINITION"].ToString()
                : null;

            if (!string.IsNullOrEmpty(definition))
            {
                // If the definition is NULL or an empty string, the procedure might be using encrypted WITH ENCRYPTION
                // or is a system procedure that doesn't expose its definition
                procInfo.Definition = definition;

                _logger.LogDebug("Stored procedure definition for {Schema}.{Name}: {Definition}",
                    procInfo.Schema, procInfo.Name, definition);
            }
            else
            {
                // For procedures where INFORMATION_SCHEMA.ROUTINES.ROUTINE_DEFINITION is NULL,
                // try using sys.sql_modules
                await GetProcedureDefinitionFromSysModulesAsync(connection, procInfo, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Retrieves the SQL definition from sys.sql_modules when INFORMATION_SCHEMA.ROUTINES.ROUTINE_DEFINITION is NULL
    /// </summary>
    private async Task GetProcedureDefinitionFromSysModulesAsync(SqlConnection connection, TableInfo procInfo, CancellationToken cancellationToken = default)
    {
        var sysModulesQuery = @"
                SELECT 
                    m.definition
                FROM 
                    sys.sql_modules m
                INNER JOIN 
                    sys.procedures p ON m.object_id = p.object_id
                INNER JOIN 
                    sys.schemas s ON p.schema_id = s.schema_id
                WHERE 
                    s.name = @Schema
                    AND p.name = @ProcName";

        using var command = CreateCommandWithTimeout(sysModulesQuery, connection);
        command.Parameters.AddWithValue("@Schema", procInfo.Schema);
        command.Parameters.AddWithValue("@ProcName", procInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var definition = reader["definition"] != DBNull.Value
                ? reader["definition"].ToString()
                : null;

            if (!string.IsNullOrEmpty(definition))
            {
                procInfo.Definition = definition;

                _logger.LogDebug("Stored procedure definition (from sys.sql_modules) for {Schema}.{Name}: {Definition}",
                    procInfo.Schema, procInfo.Name, definition);
            }
            else
            {
                _logger.LogWarning("Unable to retrieve definition for stored procedure {Schema}.{Name}. It may be encrypted.",
                    procInfo.Schema, procInfo.Name);
            }
        }
    }

    /// <summary>
    /// Retrieves the parameters of a stored procedure
    /// </summary>
    private async Task GetProcedureParametersAsync(SqlConnection connection, TableInfo procInfo, CancellationToken cancellationToken = default)
    {
        var parametersQuery = @"
                SELECT 
                    p.PARAMETER_NAME,
                    p.DATA_TYPE,
                    p.PARAMETER_MODE,
                    p.CHARACTER_MAXIMUM_LENGTH,
                    p.NUMERIC_PRECISION,
                    p.NUMERIC_SCALE,
                    p.IS_RESULT
                FROM 
                    INFORMATION_SCHEMA.PARAMETERS p
                WHERE 
                    p.SPECIFIC_SCHEMA = @Schema
                    AND p.SPECIFIC_NAME = @ProcName
                ORDER BY 
                    p.ORDINAL_POSITION";

        using var command = CreateCommandWithTimeout(parametersQuery, connection);
        command.Parameters.AddWithValue("@Schema", procInfo.Schema);
        command.Parameters.AddWithValue("@ProcName", procInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            // Store parameters as "columns" for consistency
            var paramName = reader["PARAMETER_NAME"].ToString() ?? string.Empty;
            var dataType = reader["DATA_TYPE"].ToString() ?? string.Empty;
            var paramMode = reader["PARAMETER_MODE"].ToString() ?? string.Empty;

            var columnInfo = new ColumnInfo
            {
                Name = paramName,
                DataType = dataType,
                IsNullable = true, // Most parameters allow NULL by default
                MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]) : null,
                Precision = reader["NUMERIC_PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_PRECISION"]) : null,
                Scale = reader["NUMERIC_SCALE"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_SCALE"]) : null,
                // Add a property to indicate this is a parameter and its direction
                Description = $"Parameter, Direction: {paramMode}"
            };

            procInfo.Columns.Add(columnInfo);
        }
    }

    /// <summary>
    /// Retrieves SQL Server Agent job metadata from msdb
    /// </summary>
    public async Task<List<SqlServerAgentJobInfo>> GetSqlServerAgentJobsAsync(CancellationToken cancellationToken = default)
    {
        var jobs = new List<SqlServerAgentJobInfo>();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            var query = @"
                SELECT 
                    job.job_id,
                    job.name,
                    job.enabled,
                    job.description,
                    SUSER_SNAME(job.owner_sid) AS owner,
                    job.date_created,
                    job.date_modified,
                    cat.name AS category
                FROM msdb.dbo.sysjobs job
                LEFT JOIN msdb.dbo.syscategories cat ON job.category_id = cat.category_id
                ORDER BY job.name
            ";
            using var command = CreateCommandWithTimeout(query, connection);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                jobs.Add(new SqlServerAgentJobInfo
                {
                    JobId = reader.GetGuid(reader.GetOrdinal("job_id")),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Enabled = reader["enabled"] != DBNull.Value && Convert.ToInt32(reader["enabled"]) == 1,
                    Description = reader["description"]?.ToString() ?? string.Empty,
                    Owner = reader["owner"]?.ToString() ?? string.Empty,
                    DateCreated = reader["date_created"] != DBNull.Value ? Convert.ToDateTime(reader["date_created"]) : DateTime.MinValue,
                    DateModified = reader["date_modified"] != DBNull.Value ? Convert.ToDateTime(reader["date_modified"]) : DateTime.MinValue,
                    Category = reader["category"]?.ToString() ?? string.Empty
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SQL Server Agent jobs");
        }
        return jobs;
    }

    /// <summary>
    /// Gets detailed information about a specific SQL Server Agent job, including steps, schedules, and execution history
    /// </summary>
    /// <param name="jobName">Name of the job to retrieve details for</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>SqlServerAgentJobInfo object with detailed information about the job</returns>
    public async Task<SqlServerAgentJobInfo?> GetSqlServerAgentJobDetailsAsync(string jobName, CancellationToken cancellationToken = default)
    {
        SqlServerAgentJobInfo? job = null;
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get job metadata
            var jobQuery = @"
                SELECT job.job_id, job.name, job.enabled, job.description, SUSER_SNAME(job.owner_sid) AS owner,
                       job.date_created, job.date_modified, cat.name AS category
                FROM msdb.dbo.sysjobs job
                LEFT JOIN msdb.dbo.syscategories cat ON job.category_id = cat.category_id
                WHERE job.name = @JobName
            ";
            using (var jobCmd = CreateCommandWithTimeout(jobQuery, connection))
            {
                jobCmd.Parameters.AddWithValue("@JobName", jobName);
                using var reader = await jobCmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    job = new SqlServerAgentJobInfo
                    {
                        JobId = reader.GetGuid(reader.GetOrdinal("job_id")),
                        Name = reader["name"]?.ToString() ?? string.Empty,
                        Enabled = reader["enabled"] != DBNull.Value && Convert.ToInt32(reader["enabled"]) == 1,
                        Description = reader["description"]?.ToString() ?? string.Empty,
                        Owner = reader["owner"]?.ToString() ?? string.Empty,
                        DateCreated = reader["date_created"] != DBNull.Value ? Convert.ToDateTime(reader["date_created"]) : DateTime.MinValue,
                        DateModified = reader["date_modified"] != DBNull.Value ? Convert.ToDateTime(reader["date_modified"]) : DateTime.MinValue,
                        Category = reader["category"]?.ToString() ?? string.Empty
                    };
                }
            }
            if (job == null) return null;

            // Get job steps
            var stepsQuery = @"
                SELECT step_id, step_name, subsystem, command, database_name, on_success_action, on_fail_action
                FROM msdb.dbo.sysjobsteps WHERE job_id = @JobId ORDER BY step_id
            ";
            using (var stepsCmd = CreateCommandWithTimeout(stepsQuery, connection))
            {
                stepsCmd.Parameters.AddWithValue("@JobId", job.JobId);
                using var reader = await stepsCmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    job.Steps.Add(new SqlServerAgentJobStepInfo
                    {
                        StepId = reader.GetInt32(reader.GetOrdinal("step_id")),
                        StepName = reader["step_name"]?.ToString() ?? string.Empty,
                        Subsystem = reader["subsystem"]?.ToString() ?? string.Empty,
                        Command = reader["command"]?.ToString() ?? string.Empty,
                        DatabaseName = reader["database_name"]?.ToString() ?? string.Empty,
                        OnSuccessAction = reader["on_success_action"] != DBNull.Value ? Convert.ToInt32(reader["on_success_action"]) : 0,
                        OnFailAction = reader["on_fail_action"] != DBNull.Value ? Convert.ToInt32(reader["on_fail_action"]) : 0
                    });
                }
            }

            // Get schedules
            var schedulesQuery = @"
                SELECT s.name AS schedule_name, s.enabled, s.freq_type, s.active_start_time, s.active_end_time
                FROM msdb.dbo.sysjobschedules js
                JOIN msdb.dbo.sysschedules s ON js.schedule_id = s.schedule_id
                WHERE js.job_id = @JobId
            ";
            using (var schedCmd = CreateCommandWithTimeout(schedulesQuery, connection))
            {
                schedCmd.Parameters.AddWithValue("@JobId", job.JobId);
                using var reader = await schedCmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    job.Schedules.Add(new SqlServerAgentJobScheduleInfo
                    {
                        ScheduleName = reader["schedule_name"]?.ToString() ?? string.Empty,
                        Enabled = reader["enabled"] != DBNull.Value && Convert.ToInt32(reader["enabled"]) == 1,
                        FrequencyType = reader["freq_type"] != DBNull.Value ? Convert.ToInt32(reader["freq_type"]) : 0,
                        ActiveStartTime = reader["active_start_time"] != DBNull.Value ? Convert.ToInt32(reader["active_start_time"]) : 0,
                        ActiveEndTime = reader["active_end_time"] != DBNull.Value ? Convert.ToInt32(reader["active_end_time"]) : 0
                    });
                }
            }

            // Get execution history (last 10 runs)
            var historyQuery = @"
                SELECT TOP 10 instance_id, run_date, run_time, run_status, run_duration, message
                FROM msdb.dbo.sysjobhistory
                WHERE job_id = @JobId
                ORDER BY instance_id DESC
            ";
            using (var histCmd = CreateCommandWithTimeout(historyQuery, connection))
            {
                histCmd.Parameters.AddWithValue("@JobId", job.JobId);
                using var reader = await histCmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    job.History.Add(new SqlServerAgentJobHistoryInfo
                    {
                        InstanceId = reader["instance_id"] != DBNull.Value ? Convert.ToInt32(reader["instance_id"]) : 0,
                        RunDate = reader["run_date"] != DBNull.Value ? Convert.ToInt32(reader["run_date"]) : 0,
                        RunTime = reader["run_time"] != DBNull.Value ? Convert.ToInt32(reader["run_time"]) : 0,
                        RunStatus = reader["run_status"] != DBNull.Value ? Convert.ToInt32(reader["run_status"]) : 0,
                        RunDuration = reader["run_duration"] != DBNull.Value ? Convert.ToInt32(reader["run_duration"]) : 0,
                        Message = reader["message"]?.ToString() ?? string.Empty
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SQL Server Agent job details for {JobName}", jobName);
        }
        return job;
    }

    /// <summary>
    /// Gets SSIS catalog information including Project Deployment and Package Deployment models
    /// </summary>
    // This method is implemented in DatabaseMetadataProvider.Ssis.cs partial class
}
