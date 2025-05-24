namespace mssqlMCP.Models;

public class SqlServerAgentJobStepInfo
{
    public int StepId
    {
        get; set;
    }
    public string StepName { get; set; } = string.Empty;
    public string Subsystem { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public int OnSuccessAction
    {
        get; set;
    }
    public int OnFailAction
    {
        get; set;
    }
}

public class SqlServerAgentJobScheduleInfo
{
    public string ScheduleName { get; set; } = string.Empty;
    public bool Enabled
    {
        get; set;
    }
    public int FrequencyType
    {
        get; set;
    }
    public int ActiveStartTime
    {
        get; set;
    }
    public int ActiveEndTime
    {
        get; set;
    }
}

public class SqlServerAgentJobHistoryInfo
{
    public int InstanceId
    {
        get; set;
    }
    public int RunDate
    {
        get; set;
    }
    public int RunTime
    {
        get; set;
    }
    public int RunStatus
    {
        get; set;
    }
    public int RunDuration
    {
        get; set;
    }
    public string Message { get; set; } = string.Empty;
}
