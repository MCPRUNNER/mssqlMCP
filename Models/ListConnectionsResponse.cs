namespace mssqlMCP.Models;
public class ListConnectionsResponse
{
    public bool Success
    {
        get; set;
    }
    public List<ConnectionInfo> Connections { get; set; } = new List<ConnectionInfo>();
    public string? ErrorMessage
    {
        get; set;
    }
}