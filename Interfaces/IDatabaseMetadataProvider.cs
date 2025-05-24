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

    /// <summary>
    /// Gets detailed information (steps, schedules, history) for a specific SQL Server Agent job
    /// </summary>
    /// <param name="jobName">The name of the job</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Job info with steps, schedules, and history</returns>
    Task<SqlServerAgentJobInfo?> GetSqlServerAgentJobDetailsAsync(string jobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets SSIS catalog information including Project Deployment and Package Deployment models
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>SSIS catalog information including folders, projects, and packages</returns>
    Task<SsisCatalogInfo?> GetSsisCatalogInfoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets Azure DevOps information including projects, repositories, builds, and work items
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Azure DevOps information or null if error</returns>
    Task<AzureDevOpsInfo?> GetAzureDevOpsInfoAsync(CancellationToken cancellationToken = default);
}
