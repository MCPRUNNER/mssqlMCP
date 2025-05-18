# Test MCP with PowerShell
# Example script to test the SQL Server MCP server with PowerShell

# Define the API key - replace with your actual key from Set-Api-Key.ps1
$apiKey = "your-api-key-here"

# Base URL for the MCP server
$baseUrl = "http://localhost:3001"

# Function to call MCP methods
function Invoke-McpMethod {
    param (
        [string]$Method,
        [hashtable]$Params = @{}
    )
    
    Write-Host "Calling $Method..." -ForegroundColor Cyan
    
    $body = @{
        jsonrpc = "2.0"
        id      = [guid]::NewGuid().ToString()
        method  = $Method
        params  = $Params
    } | ConvertTo-Json -Depth 10
    
    $headers = @{
        "Content-Type" = "application/json"
        "Accept"       = "application/json"
        "X-API-Key"    = $apiKey
    }
    
    try {
        $response = Invoke-RestMethod -Uri $baseUrl -Method Post -Body $body -Headers $headers
        return $response
    }
    catch {
        Write-Host "Error calling MCP API: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseText = $reader.ReadToEnd()
            Write-Host "Response content: $responseText" -ForegroundColor Red
        }
        throw
    }
}

# Test the connection and API
try {
    # Initialize the connection
    $initResult = Invoke-McpMethod -Method "initialize" -Params @{
        connectionName = "DefaultConnection"
    }
    Write-Host "Initialization result: $($initResult.result | ConvertTo-Json -Depth 3)" -ForegroundColor Green
    
    # Get table metadata
    $metadataResult = Invoke-McpMethod -Method "getTableMetadata" -Params @{
        connectionName = "DefaultConnection"
        schema         = "dbo"
    }
    Write-Host "Found $($metadataResult.result.tables.Count) tables in the dbo schema" -ForegroundColor Green
    
    # Execute a simple query
    $queryResult = Invoke-McpMethod -Method "executeQuery" -Params @{
        connectionName = "DefaultConnection"
        query          = "SELECT name, database_id, create_date FROM sys.databases"
    }
    Write-Host "Query returned $($queryResult.result.Count) rows" -ForegroundColor Green
    Write-Host ($queryResult.result | ConvertTo-Json -Depth 2)
    
    # List connections
    $connections = Invoke-McpMethod -Method "connectionManager/list" -Params @{}
    Write-Host "Available connections:" -ForegroundColor Green
    Write-Host ($connections.result.connections | ConvertTo-Json -Depth 2)
    
    Write-Host "All tests completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Test failed: $($_.Exception.Message)" -ForegroundColor Red
}
