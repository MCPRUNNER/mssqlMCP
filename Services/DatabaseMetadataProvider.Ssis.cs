// filepath: c:\Users\U00001\source\repos\MCP\mssqlMCP\Services\DatabaseMetadataProvider.Ssis.cs
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace mssqlMCP.Services;

/// <summary>
/// Service that provides metadata information about SQL Server databases - SSIS related methods
/// </summary>
public partial class DatabaseMetadataProvider
{
    /// <summary>
    /// Safely converts a database value to DateTime
    /// </summary>
    private DateTime SafeGetDateTime(object? value)
    {
        if (value == null || value == DBNull.Value)
            return DateTime.MinValue;

        if (value is DateTime dateTime)
            return dateTime;

        if (value is DateTimeOffset dateTimeOffset)
            return dateTimeOffset.DateTime;

        try
        {
            return DateTime.Parse(value.ToString() ?? string.Empty);
        }
        catch
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    /// Gets SSIS catalog information including Project Deployment and Package Deployment models
    /// </summary>
    public async Task<SsisCatalogInfo?> GetSsisCatalogInfoAsync(CancellationToken cancellationToken = default)
    {
        var catalogInfo = new SsisCatalogInfo();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get SSISDB catalog info
            var catalogQuery = @"
                SELECT 
                    CAST(SERVERPROPERTY('ServerName') AS NVARCHAR(128)) AS server_name,
                    'SSISDB' AS catalog_name,
                    GETDATE() AS created_time";

            try
            {
                // First check if SSISDB exists
                var checkDbQuery = @"SELECT database_id FROM sys.databases WHERE name = 'SSISDB'";
                using var checkCmd = CreateCommandWithTimeout(checkDbQuery, connection);
                var dbExists = await checkCmd.ExecuteScalarAsync(cancellationToken);

                if (dbExists == null)
                {
                    _logger.LogWarning("SSISDB catalog does not exist on this server.");
                    return null;
                }

                using var catalogCmd = CreateCommandWithTimeout(catalogQuery, connection);
                using var reader = await catalogCmd.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    catalogInfo.ServerName = reader["server_name"]?.ToString() ?? string.Empty;
                    catalogInfo.CatalogName = reader["catalog_name"]?.ToString() ?? string.Empty;
                    catalogInfo.CreatedDate = SafeGetDateTime(reader["created_time"]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SSISDB catalog may not exist on this server.");
                return null;
            }

            // Get SSIS folders
            var foldersQuery = @"
                SELECT 
                    name, 
                    created_time
                FROM [SSISDB].[catalog].[folders]";

            try
            {
                using var foldersCmd = CreateCommandWithTimeout(foldersQuery, connection);
                using var reader = await foldersCmd.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    var folder = new SsisFolderInfo
                    {
                        Name = reader["name"]?.ToString() ?? string.Empty,
                        CreatedDate = SafeGetDateTime(reader["created_time"])
                    };
                    catalogInfo.Folders.Add(folder);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve SSIS folders.");
                // Continue with what we have
            }

            // For each folder, get projects
            foreach (var folder in catalogInfo.Folders)
            {
                try
                {
                    var projectsQuery = @"
                        SELECT 
                            p.name, 
                            p.description,
                            p.created_time,
                            p.last_deployed_time,                            'Project' as deployment_model -- All projects in SSISDB use project deployment model
                        FROM [SSISDB].[catalog].[projects] p
                        INNER JOIN [SSISDB].[catalog].[folders] f ON p.folder_id = f.folder_id
                        WHERE f.name = @FolderName";

                    using var projectsCmd = CreateCommandWithTimeout(projectsQuery, connection);
                    projectsCmd.Parameters.AddWithValue("@FolderName", folder.Name);
                    using var reader = await projectsCmd.ExecuteReaderAsync(cancellationToken);
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        var project = new SsisProjectInfo
                        {
                            Name = reader["name"]?.ToString() ?? string.Empty,
                            Description = reader["description"]?.ToString() ?? string.Empty,
                            CreatedDate = SafeGetDateTime(reader["created_time"]),
                            LastDeployedDate = SafeGetDateTime(reader["last_deployed_time"]),
                            DeploymentModel = reader["deployment_model"]?.ToString() ?? string.Empty
                        };
                        folder.Projects.Add(project);

                        if (project.DeploymentModel == "Project")
                        {
                            catalogInfo.ProjectDeploymentCount++;
                        }
                        else
                        {
                            catalogInfo.PackageDeploymentCount++;
                        }                        // Get packages for each project
                        try
                        {                            // Use a separate connection for nested operations to avoid DataReader conflicts
                            using var packageConnection = new SqlConnection(_connectionString);
                            await packageConnection.OpenAsync(cancellationToken);

                            // Query with correct column names from INFORMATION_SCHEMA
                            var packagesQuery = @"
                                SELECT 
                                    pkg.name,
                                    pkg.description,
                                    p.created_time,
                                    pkg.last_validation_time
                                FROM [SSISDB].[catalog].[packages] pkg
                                INNER JOIN [SSISDB].[catalog].[projects] p ON pkg.project_id = p.project_id
                                INNER JOIN [SSISDB].[catalog].[folders] f ON p.folder_id = f.folder_id
                                WHERE f.name = @FolderName AND p.name = @ProjectName";

                            using var packagesCmd = CreateCommandWithTimeout(packagesQuery, packageConnection);
                            packagesCmd.Parameters.AddWithValue("@FolderName", folder.Name);
                            packagesCmd.Parameters.AddWithValue("@ProjectName", project.Name);
                            using var pkgReader = await packagesCmd.ExecuteReaderAsync(cancellationToken);
                            while (await pkgReader.ReadAsync(cancellationToken))
                            {
                                var package = new SsisPackageInfo
                                {
                                    Name = pkgReader["name"]?.ToString() ?? string.Empty,
                                    Description = pkgReader["description"]?.ToString() ?? string.Empty,
                                    CreatedDate = SafeGetDateTime(pkgReader["created_time"]),
                                    LastModifiedDate = SafeGetDateTime(pkgReader["last_validation_time"])
                                };
                                project.Packages.Add(package);
                                catalogInfo.PackageCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not retrieve packages for project {ProjectName} in folder {FolderName}. Parameters used: FolderName={FolderName}, ProjectName={ProjectName}",
                                project.Name, folder.Name, folder.Name, project.Name);
                            // Continue with what we have
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not retrieve projects for folder {FolderName}", folder.Name);
                    // Continue with what we have
                }

                catalogInfo.ProjectCount += folder.Projects.Count;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SSIS catalog information");
            return null;
        }
        return catalogInfo;
    }
}
