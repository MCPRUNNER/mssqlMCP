using Microsoft.Data.SqlClient;
using System.Data;

namespace mssqlMCP
{
    public class DatabaseMetadataProvider
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseMetadataProvider> _logger;

        public DatabaseMetadataProvider(string connectionString, ILogger<DatabaseMetadataProvider> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        // Helper method to create SQL command with timeout
        private SqlCommand CreateCommandWithTimeout(string commandText, SqlConnection connection)
        {
            var command = new SqlCommand(commandText, connection);
            command.CommandTimeout = 30; // 30 second timeout
            return command;
        }
        public async Task<List<TableInfo>> GetDatabaseSchemaAsync(CancellationToken cancellationToken = default, string? schema = null)
        {
            _logger.LogInformation("Retrieving database schema" + (schema != null ? $" for schema '{schema}'" : " for all schemas"));

            var tables = new List<TableInfo>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken); var tableQuery = @"
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

                var tableNames = new List<(string Schema, string Name)>(); while (await tableReader.ReadAsync(cancellationToken))
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
                        Columns = new List<ColumnInfo>(),
                        PrimaryKeys = new List<string>(),
                        ForeignKeys = new List<ForeignKeyInfo>()
                    };                    // Get columns
                    await GetColumnsAsync(connection, tableInfo, cancellationToken);

                    // Get primary keys
                    await GetPrimaryKeysAsync(connection, tableInfo, cancellationToken);

                    // Get foreign keys
                    await GetForeignKeysAsync(connection, tableInfo, cancellationToken);

                    tables.Add(tableInfo);
                }

                return tables;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Database schema retrieval operation was canceled");
                throw; // Let the calling code handle cancellation
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving database schema");
                throw;
            }
        }
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
                    c.ORDINAL_POSITION"; using var command = CreateCommandWithTimeout(columnQuery, connection);
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
                    AND tc.TABLE_NAME = @TableName"; using var command = CreateCommandWithTimeout(pkQuery, connection);
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
                    AND OBJECT_NAME(fk.parent_object_id) = @TableName"; using var command = CreateCommandWithTimeout(fkQuery, connection);
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
    }
    public class TableInfo
    {
        public string Schema { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
        public List<string> PrimaryKeys { get; set; } = new List<string>();
        public List<ForeignKeyInfo> ForeignKeys { get; set; } = new List<ForeignKeyInfo>();
    }

    public class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DataType { get; set; } = string.Empty;
        public bool IsNullable
        {
            get; set;
        }
        public int? MaxLength
        {
            get; set;
        }
        public int? Precision
        {
            get; set;
        }
        public int? Scale
        {
            get; set;
        }
        public string? DefaultValue
        {
            get; set;
        }
        public bool IsPrimaryKey
        {
            get; set;
        }
        public bool IsForeignKey
        {
            get; set;
        }
        public string? ForeignKeyReference
        {
            get; set;
        }
    }

    public class ForeignKeyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Column { get; set; } = string.Empty;
        public string ReferencedSchema { get; set; } = string.Empty;
        public string ReferencedTable { get; set; } = string.Empty;
        public string ReferencedColumn { get; set; } = string.Empty;
    }
}
