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
    /// <summary>
    /// Retrieves SQL function metadata
    /// </summary>
    private async Task GetFunctionsAsync(SqlConnection connection, List<TableInfo> tables, string? schema, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving function metadata" + (schema != null ? $" for schema '{schema}'" : " for all schemas"));

        var functionQuery = @"
                SELECT 
                    ROUTINE_CATALOG,
                    ROUTINE_SCHEMA,
                    ROUTINE_NAME,
                    ROUTINE_TYPE,
                    DATA_TYPE AS RETURN_TYPE,
                    ROUTINE_DEFINITION,
                    CREATED,
                    LAST_ALTERED
                FROM 
                    INFORMATION_SCHEMA.ROUTINES
                WHERE 
                    ROUTINE_TYPE = 'FUNCTION'
                    " + (schema != null ? "AND ROUTINE_SCHEMA = @SchemaName" : "") + @"
                ORDER BY 
                    ROUTINE_SCHEMA, ROUTINE_NAME";

        using var functionCommand = CreateCommandWithTimeout(functionQuery, connection);
        if (schema != null)
        {
            functionCommand.Parameters.AddWithValue("@SchemaName", schema);
        }
        using var functionReader = await functionCommand.ExecuteReaderAsync(cancellationToken);

        var functionNames = new List<(string Schema, string Name, string ReturnType)>();

        while (await functionReader.ReadAsync(cancellationToken))
        {
            var schemaName = functionReader["ROUTINE_SCHEMA"].ToString() ?? string.Empty;
            var name = functionReader["ROUTINE_NAME"].ToString() ?? string.Empty;
            var returnType = functionReader["RETURN_TYPE"].ToString() ?? string.Empty;
            functionNames.Add((schemaName, name, returnType));
        }
        await functionReader.CloseAsync();

        foreach (var func in functionNames)
        {
            _logger.LogDebug("Processing function: {Schema}.{Name}", func.Schema, func.Name);

            var functionInfo = new TableInfo
            {
                Schema = func.Schema,
                Name = func.Name,
                ObjectType = "FUNCTION",
                Columns = new List<ColumnInfo>(),
                PrimaryKeys = new List<string>(),
                ForeignKeys = new List<ForeignKeyInfo>()
            };

            // Add return type information as a property
            functionInfo.Properties = new Dictionary<string, string>
            {
                { "ReturnType", func.ReturnType }
            };

            // Get the function definition
            await GetFunctionDefinitionAsync(connection, functionInfo, cancellationToken);

            // Get the function parameters
            await GetFunctionParametersAsync(connection, functionInfo, cancellationToken);

            tables.Add(functionInfo);
        }
    }

    /// <summary>
    /// Retrieves the SQL definition of a function
    /// </summary>
    private async Task GetFunctionDefinitionAsync(SqlConnection connection, TableInfo functionInfo, CancellationToken cancellationToken = default)
    {
        var definitionQuery = @"
                SELECT 
                    r.ROUTINE_DEFINITION
                FROM 
                    INFORMATION_SCHEMA.ROUTINES r
                WHERE 
                    r.ROUTINE_SCHEMA = @Schema
                    AND r.ROUTINE_NAME = @FunctionName
                    AND r.ROUTINE_TYPE = 'FUNCTION'";

        using var command = CreateCommandWithTimeout(definitionQuery, connection);
        command.Parameters.AddWithValue("@Schema", functionInfo.Schema);
        command.Parameters.AddWithValue("@FunctionName", functionInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var definition = reader["ROUTINE_DEFINITION"] != DBNull.Value
                ? reader["ROUTINE_DEFINITION"].ToString()
                : null;

            if (!string.IsNullOrEmpty(definition))
            {
                // Store the function definition in the TableInfo object
                functionInfo.Definition = definition;

                _logger.LogDebug("Function definition for {Schema}.{Name}: {Definition}",
                    functionInfo.Schema, functionInfo.Name, definition);
            }
            else
            {
                // For functions where INFORMATION_SCHEMA.ROUTINES.ROUTINE_DEFINITION is NULL,
                // try using sys.sql_modules
                await GetFunctionDefinitionFromSysModulesAsync(connection, functionInfo, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Retrieves the SQL definition from sys.sql_modules when INFORMATION_SCHEMA.ROUTINES.ROUTINE_DEFINITION is NULL
    /// </summary>
    private async Task GetFunctionDefinitionFromSysModulesAsync(SqlConnection connection, TableInfo functionInfo, CancellationToken cancellationToken = default)
    {
        var sysModulesQuery = @"
                SELECT 
                    m.definition
                FROM 
                    sys.sql_modules m
                INNER JOIN 
                    sys.objects o ON m.object_id = o.object_id
                INNER JOIN 
                    sys.schemas s ON o.schema_id = s.schema_id
                WHERE 
                    s.name = @Schema
                    AND o.name = @FunctionName
                    AND (o.type IN ('FN', 'IF', 'TF'))"; // FN = Scalar, IF = Inline Table, TF = Table-valued

        using var command = CreateCommandWithTimeout(sysModulesQuery, connection);
        command.Parameters.AddWithValue("@Schema", functionInfo.Schema);
        command.Parameters.AddWithValue("@FunctionName", functionInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var definition = reader["definition"] != DBNull.Value
                ? reader["definition"].ToString()
                : null;

            if (!string.IsNullOrEmpty(definition))
            {
                functionInfo.Definition = definition;

                _logger.LogDebug("Function definition (from sys.sql_modules) for {Schema}.{Name}: {Definition}",
                    functionInfo.Schema, functionInfo.Name, definition);
            }
            else
            {
                _logger.LogWarning("Unable to retrieve definition for function {Schema}.{Name}. It may be encrypted.",
                    functionInfo.Schema, functionInfo.Name);
            }
        }
    }

    /// <summary>
    /// Retrieves the parameters of a function
    /// </summary>
    private async Task GetFunctionParametersAsync(SqlConnection connection, TableInfo functionInfo, CancellationToken cancellationToken = default)
    {
        var parametersQuery = @"
                SELECT 
                    p.PARAMETER_NAME,
                    p.DATA_TYPE,
                    p.PARAMETER_MODE,
                    p.CHARACTER_MAXIMUM_LENGTH,
                    p.NUMERIC_PRECISION,
                    p.NUMERIC_SCALE
                FROM 
                    INFORMATION_SCHEMA.PARAMETERS p
                WHERE 
                    p.SPECIFIC_SCHEMA = @Schema
                    AND p.SPECIFIC_NAME = @FunctionName
                ORDER BY 
                    p.ORDINAL_POSITION";

        using var command = CreateCommandWithTimeout(parametersQuery, connection);
        command.Parameters.AddWithValue("@Schema", functionInfo.Schema);
        command.Parameters.AddWithValue("@FunctionName", functionInfo.Name);

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
                IsNullable = true, // Most parameters allow NULL by default unless specified otherwise
                MaxLength = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value ? Convert.ToInt32(reader["CHARACTER_MAXIMUM_LENGTH"]) : null,
                Precision = reader["NUMERIC_PRECISION"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_PRECISION"]) : null,
                Scale = reader["NUMERIC_SCALE"] != DBNull.Value ? Convert.ToInt32(reader["NUMERIC_SCALE"]) : null,
                // Add a property to indicate this is a parameter and its direction
                Description = $"Parameter, Direction: {paramMode}"
            };

            functionInfo.Columns.Add(columnInfo);
        }
    }

    /// <summary>
    /// Gets the function type (Scalar, Table-valued, etc.)
    /// </summary>
    private async Task GetFunctionTypeAsync(SqlConnection connection, TableInfo functionInfo, CancellationToken cancellationToken = default)
    {
        var typeQuery = @"
                SELECT 
                    CASE 
                        WHEN o.type = 'FN' THEN 'Scalar Function'
                        WHEN o.type = 'IF' THEN 'Inline Table-valued Function'
                        WHEN o.type = 'TF' THEN 'Table-valued Function'
                        ELSE 'Unknown Function Type'
                    END AS FunctionType
                FROM 
                    sys.objects o
                INNER JOIN 
                    sys.schemas s ON o.schema_id = s.schema_id
                WHERE 
                    s.name = @Schema
                    AND o.name = @FunctionName
                    AND o.type IN ('FN', 'IF', 'TF')";

        using var command = CreateCommandWithTimeout(typeQuery, connection);
        command.Parameters.AddWithValue("@Schema", functionInfo.Schema);
        command.Parameters.AddWithValue("@FunctionName", functionInfo.Name);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var functionType = reader["FunctionType"].ToString() ?? "Unknown Function Type";

            // Add function type to properties dictionary
            if (functionInfo.Properties == null)
            {
                functionInfo.Properties = new Dictionary<string, string>();
            }

            functionInfo.Properties["FunctionType"] = functionType;
        }
    }
}
