using System;

namespace mssqlMCP.Models;

public class AzureDevOpsWorkItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string WorkItemType { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? ChangedDate { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string AreaPath { get; set; } = string.Empty;
    public string IterationPath { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Severity { get; set; } = string.Empty;
}
