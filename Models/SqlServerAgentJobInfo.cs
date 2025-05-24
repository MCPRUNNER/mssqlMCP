namespace mssqlMCP.Models;

public class SqlServerAgentJobInfo
{
    public Guid JobId
    {
        get; set;
    }
    public string Name { get; set; } = string.Empty;
    public bool Enabled
    {
        get; set;
    }
    public string Description { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime DateCreated
    {
        get; set;
    }
    public DateTime DateModified
    {
        get; set;
    }
    public string Category { get; set; } = string.Empty;
    public List<SqlServerAgentJobStepInfo> Steps { get; set; } = new();
    public List<SqlServerAgentJobScheduleInfo> Schedules { get; set; } = new();
    public List<SqlServerAgentJobHistoryInfo> History { get; set; } = new();
}
