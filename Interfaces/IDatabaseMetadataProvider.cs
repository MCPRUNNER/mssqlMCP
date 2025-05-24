using mssqlMCP.Models;

namespace mssqlMCP.Interfaces;

/// <summary>
/// Interface for retrieving database metadata from SQL Server
/// </summary>
public interface IDatabaseMetadataProvider
{
    /// <summary>
    /// Gets database schema metadata asynchronously
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <param name="schema">Optional filter for specific schema, null returns all schemas</param>
    /// <returns>List of table metadata information</returns>
    Task<List<TableInfo>> GetDatabaseSchemaAsync(CancellationToken cancellationToken = default, string? schema = null);

    /// <summary>
    /// Gets SQL Server Agent job metadata from msdb
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>List of SQL Server Agent job metadata</returns>
    Task<List<SqlServerAgentJobInfo>> GetSqlServerAgentJobsAsync(CancellationToken cancellationToken = default);
}
