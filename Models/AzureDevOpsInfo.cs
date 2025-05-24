using System;
using System.Collections.Generic;

namespace mssqlMCP.Models;

public class AzureDevOpsInfo
{
    public List<AzureDevOpsProject> Projects { get; set; } = new();
    public List<AzureDevOpsRepository> Repositories { get; set; } = new();
    public int BuildDefinitionCount { get; set; }
    public int ReleaseDefinitionCount { get; set; }
    public int WorkItemCount { get; set; }
    public List<AzureDevOpsBuildDefinition> BuildDefinitions { get; set; } = new();
}

public class AzureDevOpsProject
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int Revision { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
    public List<AzureDevOpsBuildDefinition> BuildDefinitions { get; set; } = new();
}

public class AzureDevOpsRepository
{
    public Guid RepositoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public long Size { get; set; }
    public int BranchCount { get; set; }
    public int CommitCount { get; set; }
    public string DefaultBranch { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }
}

public class AzureDevOpsBuildDefinition
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string QueueStatus { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastBuildDate { get; set; }
    public int TotalBuilds { get; set; }
    public int SuccessfulBuilds { get; set; }
    public int FailedBuilds { get; set; }
}
