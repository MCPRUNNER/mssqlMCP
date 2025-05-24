using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using mssqlMCP.Interfaces;
using mssqlMCP.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace mssqlMCP.Services;

public partial class DatabaseMetadataProvider : IDatabaseMetadataProvider
{
    /// <summary>
    /// Gets comprehensive Azure DevOps information from the database using the AnalyticsModel schema
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Azure DevOps information or null if error</returns>
    public async Task<AzureDevOpsInfo?> GetAzureDevOpsInfoAsync(CancellationToken cancellationToken = default)
    {
        var info = new AzureDevOpsInfo();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get projects from the main project table
            await GetAzureDevOpsProjectsAsync(connection, info, cancellationToken);

            // Get repositories from AnalyticsModel schema
            await GetAzureDevOpsRepositoriesAsync(connection, info, cancellationToken);

            // Get build information
            await GetAzureDevOpsBuildInfoAsync(connection, info, cancellationToken);

            // Get work item counts
            await GetAzureDevOpsWorkItemCountAsync(connection, info, cancellationToken);

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Azure DevOps information");
            return null;
        }
    }
    private async Task GetAzureDevOpsProjectsAsync(SqlConnection connection, AzureDevOpsInfo info, CancellationToken cancellationToken)
    {
        try
        {
            // Query from AnalyticsModel.tbl_Project using actual column names
            var query = @"
                SELECT 
                    ProjectId,
                    ProjectName,
                    ProcessName,
                    ProcessDescription,
                    IsDeleted,
                    AnalyticsCreatedDate,
                    AnalyticsUpdatedDate
                FROM AnalyticsModel.tbl_Project 
                WHERE IsDeleted = 0
                ORDER BY ProjectName";

            using var cmd = new SqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var projectIdObj = reader["ProjectId"];
                Guid projectId = Guid.Empty;
                if (projectIdObj != DBNull.Value && Guid.TryParse(projectIdObj.ToString(), out var parsedProjectId))
                    projectId = parsedProjectId;

                var project = new AzureDevOpsProject
                {
                    ProjectId = projectId,
                    Name = reader["ProjectName"]?.ToString() ?? string.Empty,
                    Description = reader["ProcessDescription"]?.ToString() ?? string.Empty,
                    State = reader["ProcessName"]?.ToString() ?? string.Empty,
                    CreatedDate = reader["AnalyticsCreatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["AnalyticsCreatedDate"]) : DateTime.MinValue,
                    LastUpdatedDate = reader["AnalyticsUpdatedDate"] != DBNull.Value ? Convert.ToDateTime(reader["AnalyticsUpdatedDate"]) : DateTime.MinValue
                };
                info.Projects.Add(project);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve Azure DevOps projects");
        }
    }
    private async Task GetAzureDevOpsRepositoriesAsync(SqlConnection connection, AzureDevOpsInfo info, CancellationToken cancellationToken)
    {
        try
        {
            // Use dbo.tbl_GitRepository which has actual data, and join with projects for context
            var query = @"
                SELECT 
                    r.RepositoryId,
                    r.Name,
                    r.ContainerId,
                    r.CreationDate,
                    r.State,
                    r.IsHidden,
                    r.CompressedSize,
                    r.LastMetadataUpdate,
                    COALESCE(p.ProjectId, p2.ProjectId) as ProjectId,
                    COALESCE(p.ProjectName, p2.ProjectName) as ProjectName
                FROM dbo.tbl_GitRepository r 
                LEFT JOIN AnalyticsModel.tbl_Project p ON r.Name = p.ProjectName OR r.Name LIKE p.ProjectName + '%'
                LEFT JOIN AnalyticsModel.tbl_Project p2 ON r.Name LIKE '%' + p2.ProjectName + '%'
                WHERE r.State = 3 AND r.IsHidden = 0
                ORDER BY r.Name";

            using var cmd = new SqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var repoIdObj = reader["RepositoryId"];
                Guid repoId = Guid.Empty;
                if (repoIdObj != DBNull.Value && Guid.TryParse(repoIdObj.ToString(), out var parsedRepoId))
                    repoId = parsedRepoId;

                var projectIdObj = reader["ProjectId"];
                Guid projectId = Guid.Empty;
                if (projectIdObj != DBNull.Value && Guid.TryParse(projectIdObj.ToString(), out var parsedProjectId))
                    projectId = parsedProjectId;

                var repo = new AzureDevOpsRepository
                {
                    RepositoryId = repoId,
                    Name = reader["Name"]?.ToString() ?? string.Empty,
                    ProjectId = projectId,
                    ProjectName = reader["ProjectName"]?.ToString() ?? string.Empty,
                    DefaultBranch = "main", // Default value since not available in this table
                    Size = reader["CompressedSize"] != DBNull.Value ? Convert.ToInt64(reader["CompressedSize"]) : 0,
                    IsDisabled = reader["IsHidden"] != DBNull.Value && Convert.ToBoolean(reader["IsHidden"]),
                    BranchCount = 0, // Would need separate query
                    CommitCount = 0  // Would need separate query
                };
                info.Repositories.Add(repo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve Azure DevOps repositories");
        }
    }

    private async Task GetAzureDevOpsBuildInfoAsync(SqlConnection connection, AzureDevOpsInfo info, CancellationToken cancellationToken)
    {
        try
        {
            // Get build pipeline count from AnalyticsModel.tbl_BuildPipeline
            var query = @"
                SELECT 
                    COUNT(*) as BuildPipelineCount
                FROM AnalyticsModel.tbl_BuildPipeline 
                WHERE IsDeleted = 0";

            using var cmd = new SqlCommand(query, connection);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            info.BuildDefinitionCount = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;

            // Note: Azure DevOps Server may not have a direct equivalent to classic "Release Definitions"
            // in the analytics model, so we'll set this to 0 for now
            info.ReleaseDefinitionCount = 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve Azure DevOps build information");
        }
    }

    private async Task GetAzureDevOpsWorkItemCountAsync(SqlConnection connection, AzureDevOpsInfo info, CancellationToken cancellationToken)
    {
        try
        {
            var query = @"
                SELECT 
                    COUNT(*) as WorkItemCount
                FROM AnalyticsModel.tbl_WorkItem 
                WHERE IsDeleted = 0";

            using var cmd = new SqlCommand(query, connection);
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            info.WorkItemCount = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not retrieve Azure DevOps work item count");
        }
    }
}
