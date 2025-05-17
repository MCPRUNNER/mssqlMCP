namespace mssqlMCP.Models;

public class ConnectionInfo
{
    public string Name { get; set; } = string.Empty;
    public string? Description
    {
        get; set;
    }
    public string ServerType { get; set; } = string.Empty;
    public DateTime? LastUsed
    {
        get; set;
    }
    public DateTime CreatedOn
    {
        get; set;
    }
}