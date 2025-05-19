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
}
