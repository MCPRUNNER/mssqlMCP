namespace mssqlMCP.Models;

public class TestConnectionResponse
{
    public bool Success
    {
        get; set;
    }
    public string Message { get; set; } = string.Empty;
}