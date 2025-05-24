using System.ComponentModel;

namespace mssqlMCP.Interfaces;

/// <summary>
/// Interface for SQL Server MCP tools functionality
/// </summary>
public interface ISqlServerTools
{
    /// <summary>
    /// Initialize the SQL Server connection
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>Confirmation message</returns>
    Task<string> Initialize(string connectionName = "DefaultConnection");



    /// <summary>
    /// Executes a SQL query and returns the results as JSON
    /// </summary>
    /// <param name="query">The SQL query to execute</param>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>JSON representation of query results</returns>
    Task<string> ExecuteQuery(string query, string connectionName = "DefaultConnection");

    /// <summary>
    /// Gets detailed metadata about database tables, columns, primary keys and foreign keys
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <param name="schema">Optional schema filter</param>
    /// <returns>JSON representation of database metadata</returns>
    Task<string> GetTableMetadata(string connectionName = "DefaultConnection", string? schema = null);
    /// <summary>
    /// Gets detailed metadata about database objects including tables and views
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <param name="schema">Optional schema filter</param>
    /// <param name="includeViews">Whether to include views in the results</param>
    /// <returns>JSON representation of database metadata</returns>
    Task<string> GetDatabaseObjectsMetadata(string connectionName = "DefaultConnection", string? schema = null, bool includeViews = true);
    /// <summary>
    /// Gets detailed metadata about specific database object types (tables, views, procedures, or functions)
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <param name="schema">Optional schema filter</param>
    /// <param name="objectType">Object type filter: "TABLE", "VIEW", "PROCEDURE", "FUNCTION", or "ALL"</param>
    /// <returns>JSON representation of database metadata</returns>
    Task<string> GetDatabaseObjectsByType(string connectionName = "DefaultConnection", string? schema = null, string objectType = "ALL");
    /// <summary>
    /// Gets SQL Server Agent job metadata from msdb
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>JSON representation of SQL Server Agent jobs</returns>
    Task<string> GetSqlServerAgentJobs(string connectionName = "DefaultConnection");
    /// <summary>
    /// Gets detailed information (steps, schedules, history) for a specific SQL Server Agent job
    /// </summary>
    /// <param name="jobName">The name of the job</param>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>JSON representation of job info with steps, schedules, and history</returns>
    Task<string> GetSqlServerAgentJobDetails(string jobName, string connectionName = "DefaultConnection");
    /// <summary>
    /// Gets SSIS catalog information including Project Deployment and Package Deployment models
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>JSON representation of SSIS catalog information</returns>
    Task<string> GetSsisCatalogInfo(string connectionName = "DefaultConnection");

    /// <summary>
    /// Gets Azure DevOps information including projects, repositories, builds, and work items
    /// </summary>
    /// <param name="connectionName">The name of the connection string to use</param>
    /// <returns>JSON representation of Azure DevOps information</returns>
    Task<string> GetAzureDevOpsInfo(string connectionName = "DefaultConnection");
}