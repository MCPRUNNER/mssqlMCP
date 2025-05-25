# Test MCP with PowerShell
# Comprehensive example script to test all SQL Server MCP tools with PowerShell
# This script demonstrates all available MCP tools and their proper usage

# Define the API key - replace with your actual key from Set-Api-Key.ps1
$apiKey = "pTUDn0e/FsqjCmyuZ4y76/tm8q3ISTC9NqMn4aM7fd4="
$connectionName = "DefaultConnection"
# Base URL for the MCP server
$baseUrl = "http://localhost:3001"

# Improvements needed in code
# Function to call MCP methods
function Invoke-McpMethod {
    param (
        [string]$Method,
        [hashtable]$Params = @{},
        [string]$Description = ""
    )
    
    Write-Host "======================================" -ForegroundColor Yellow
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "Method: $Method" -ForegroundColor Cyan
    Write-Host "Params: $($Params | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor Gray
    Write-Host "======================================" -ForegroundColor Yellow
    
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
        Write-Host "✅ Success!" -ForegroundColor Green
        if ($response.result) {
            Write-Host "Result:" -ForegroundColor Green
            Write-Host ($response.result | ConvertTo-Json -Depth 3) -ForegroundColor White
        }
        Write-Host ""
        return $response
    }
    catch {
        Write-Host "❌ Error calling MCP API: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseText = $reader.ReadToEnd()
            Write-Host "Response content: $responseText" -ForegroundColor Red
        }
        Write-Host ""
        throw
    }
}

# Function to call connection management endpoints
function Invoke-ConnectionMethod {
    param (
        [string]$Action,
        [hashtable]$RequestData = @{},
        [string]$Description = ""
    )
    
    Write-Host "======================================" -ForegroundColor Yellow
    Write-Host "Testing: $Description" -ForegroundColor Yellow
    Write-Host "Action: $Action" -ForegroundColor Cyan
    Write-Host "Request: $($RequestData | ConvertTo-Json -Depth 3 -Compress)" -ForegroundColor Gray
    Write-Host "======================================" -ForegroundColor Yellow
    
    $headers = @{
        "Content-Type" = "application/json"
        "Accept"       = "application/json"
        "X-API-Key"    = $apiKey
    }
    
    $uri = "$baseUrl/$Action"
    $body = $RequestData | ConvertTo-Json -Depth 10
    
    try {
        $response = Invoke-RestMethod -Uri $uri -Method Post -Body $body -Headers $headers
        Write-Host "✅ Success!" -ForegroundColor Green
        Write-Host "Result:" -ForegroundColor Green
        Write-Host ($response | ConvertTo-Json -Depth 3) -ForegroundColor White
        Write-Host ""
        return $response
    }
    catch {
        Write-Host "❌ Error calling Connection API: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseText = $reader.ReadToEnd()
            Write-Host "Response content: $responseText" -ForegroundColor Red
        }
        Write-Host ""
        throw
    }
}

# Main testing script
Write-Host "===================================================================" -ForegroundColor Magenta
Write-Host "SQL Server MCP Tool Testing Script - PowerShell Edition" -ForegroundColor Magenta
Write-Host "Testing all available MCP tools with current method names" -ForegroundColor Magenta
Write-Host "===================================================================" -ForegroundColor Magenta
Write-Host ""

try {
    # 1. CONNECTION MANAGEMENT TOOLS
    Write-Host "### CONNECTION MANAGEMENT TOOLS ###" -ForegroundColor DarkCyan
    # List existing connections
    $listResult = Invoke-ConnectionMethod -Action "list-connections" -RequestData @{} -Description "List all database connections"
    Write-Host "Found $($listResult.connections.Count) existing connections" -ForegroundColor Green
    
    # Test a connection
    $testResult = Invoke-ConnectionMethod -Action "test-connection" -RequestData @{
        request = @{
            connectionString = "Server=localhost;Database=master;Trusted_Connection=true;"
        }
    } -Description "Test database connection"
    Write-Host "Connection test result: $($testResult.success)" -ForegroundColor Green
    
    # Add a new connection
    $addResult = Invoke-ConnectionMethod -Action "add-connection" -RequestData @{
        request = @{
            name             = "TestConnection"
            connectionString = "Server=localhost;Database=TestDB;Trusted_Connection=true;"
            description      = "Test database connection from PowerShell"
        }
    } -Description "Add new database connection"
    Write-Host "Connection added: $($addResult.success)" -ForegroundColor Green
    
    # Update existing connection
    $updateResult = Invoke-ConnectionMethod -Action "update-connection" -RequestData @{
        request = @{
            name             = "TestConnection"
            connectionString = "Server=localhost;Database=UpdatedTestDB;Trusted_Connection=true;"
            description      = "Updated test database connection from PowerShell"
        }
    } -Description "Update existing database connection"
    Write-Host "Connection updated: $($updateResult.success)" -ForegroundColor Green
    Write-Host ""
    Write-Host "### CORE SQL SERVER TOOLS ###" -ForegroundColor DarkCyan
    # 2. Initialize the connection
    $initResult = Invoke-McpMethod -Method "Initialize" -Params @{
        connectionName = "DefaultConnection"
    } -Description "Initialize SQL Server connection"
    Write-Host "Connection initialization completed successfully" -ForegroundColor Green
    if ($initResult.result) {
        Write-Host "Initialization successful" -ForegroundColor Green
    }
    
    # 3. Execute simple queries
    $queryResult1 = Invoke-McpMethod -Method "ExecuteQuery" -Params @{
        connectionName = $connectionName 
        query          = "SELECT name, database_id, create_date FROM sys.databases WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')"
    } -Description "Execute SQL query to get user databases"
    Write-Host "Query returned $($queryResult1.result.Count) user databases" -ForegroundColor Green
    
    # 4. Get table metadata
    $metadataResult = Invoke-McpMethod -Method "GetTableMetadata" -Params @{
        connectionName = $connectionName
        schema         = "dbo"
    } -Description "Get table metadata for dbo schema"
    
    if ($metadataResult.result.tables) {
        Write-Host "Found $($metadataResult.result.tables.Count) tables in the dbo schema" -ForegroundColor Green
    }
    # 5. Get database objects metadata
    $objectsResult = Invoke-McpMethod -Method "GetDatabaseObjectsMetadata" -Params @{
        connectionName = $connectionName 
        includeViews   = $true
        schema         = "dbo"
    } -Description "Get database objects metadata including views"
    Write-Host "Found $($objectsResult.result.Count) database objects" -ForegroundColor Green
    # 6. Get database objects by type
    $tablesResult = Invoke-McpMethod -Method "GetDatabaseObjectsByType" -Params @{
        connectionName = $connectionName 
        objectType     = "TABLE"
        schema         = "dbo"
    } -Description "Get database objects filtered by type (tables only)"
    Write-Host "Found $($tablesResult.result.Count) tables" -ForegroundColor Green
    
    $viewsResult = Invoke-McpMethod -Method "GetDatabaseObjectsByType" -Params @{
        connectionName = $connectionName 
        objectType     = "VIEW"
        schema         = "dbo"
    } -Description "Get database objects filtered by type (views only)"
    Write-Host "Found $($viewsResult.result.Count) views" -ForegroundColor Green
    
    $proceduresResult = Invoke-McpMethod -Method "GetDatabaseObjectsByType" -Params @{
        connectionName = $connectionName
        objectType     = "PROCEDURE"
        schema         = "dbo"
    } -Description "Get database objects filtered by type (stored procedures only)"
    Write-Host "Found $($proceduresResult.result.Count) procedures" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "### SPECIALIZED METADATA TOOLS ###" -ForegroundColor DarkCyan
    
    # 7. Get SQL Server Agent Jobs
    $agentJobsResult = Invoke-McpMethod -Method "GetSqlServerAgentJobs" -Params @{
        connectionName = "DefaultConnection"
    } -Description "Get SQL Server Agent jobs metadata"
    # 8. Get detailed SQL Server Agent Job information (if jobs exist)
    if ($agentJobsResult.result -and $agentJobsResult.result.Count -gt 0) {
        $firstJobName = $agentJobsResult.result[0].JobName
        $jobDetailsResult = Invoke-McpMethod -Method "GetSqlServerAgentJobDetails" -Params @{
            connectionName = $connectionName
            jobName        = $firstJobName
        } -Description "Get detailed SQL Server Agent job information for '$firstJobName'"
        Write-Host "Retrieved details for job: $firstJobName" -ForegroundColor Green
        if ($jobDetailsResult.result) {
            Write-Host "Job details retrieved successfully" -ForegroundColor Green
        }
    }
    else {
        Write-Host "No SQL Server Agent jobs found to get details for" -ForegroundColor Yellow
    }
    # 9. Get SSIS Catalog information
    $ssisResult = Invoke-McpMethod -Method "GetSsisCatalogInfo" -Params @{
        connectionName = $connectionName
    } -Description "Get SSIS catalog information"
    Write-Host "SSIS catalog info retrieved successfully" -ForegroundColor Green
    if ($ssisResult.result) {
        Write-Host "SSIS data processed" -ForegroundColor Green
    }
    
    # 10. Get Azure DevOps information
    $azureDevOpsResult = Invoke-McpMethod -Method "GetAzureDevOpsInfo" -Params @{
        connectionName = $connectionName
    } -Description "Get Azure DevOps information"
    Write-Host "Azure DevOps info retrieved successfully" -ForegroundColor Green
    if ($azureDevOpsResult.result) {
        Write-Host "Azure DevOps data processed" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "### SECURITY TOOLS ###" -ForegroundColor DarkCyan
    # Generate secure key
    $keyResult = Invoke-ConnectionMethod -Action "generate-secure-key" -RequestData @{
        length = 32
    } -Description "Generate secure encryption key"
    Write-Host "Generated secure key of length: $($keyResult.length)" -ForegroundColor Green
    # Migrate connections to encrypted format
    $migrateResult = Invoke-ConnectionMethod -Action "migrate-to-encrypted" -RequestData @{} -Description "Migrate connection strings to encrypted format"
    Write-Host "Migration to encrypted format completed" -ForegroundColor Green
    if ($migrateResult.success) {
        Write-Host "Migration was successful" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "### ADVANCED QUERY EXAMPLES ###" -ForegroundColor DarkCyan
    # Complex metadata query
    $complexQuery1 = Invoke-McpMethod -Method "ExecuteQuery" -Params @{
        connectionName = $connectionName 
        query          = @"
SELECT 
    t.name AS TableName, 
    c.name AS ColumnName, 
    ty.name AS DataType, 
    c.max_length, 
    c.is_nullable 
FROM sys.tables t 
INNER JOIN sys.columns c ON t.object_id = c.object_id 
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id 
WHERE t.name LIKE '%User%' 
ORDER BY t.name, c.column_id
"@
    } -Description "Get detailed column information for tables containing 'User'"
    Write-Host "Complex metadata query returned $($complexQuery1.result.Count) rows" -ForegroundColor Green
    # Performance monitoring query
    $performanceQuery = Invoke-McpMethod -Method "ExecuteQuery" -Params @{
        connectionName = $connectionName
        query          = @"
SELECT 
    db_name(database_id) as DatabaseName, 
    type_desc, 
    size_mb = size * 8.0 / 1024, 
    growth_desc = CASE 
        WHEN is_percent_growth = 1 THEN CAST(growth AS VARCHAR(3)) + '%' 
        ELSE CAST(growth * 8.0 / 1024 AS VARCHAR(10)) + ' MB' 
    END 
FROM sys.master_files 
WHERE database_id > 4
"@
    } -Description "Get database file information and growth settings"
    Write-Host "Performance query returned $($performanceQuery.result.Count) database files" -ForegroundColor Green
    # Index analysis query
    $indexQuery = Invoke-McpMethod -Method "ExecuteQuery" -Params @{
        connectionName = $connectionName
        query          = @"
SELECT 
    OBJECT_NAME(i.object_id) AS TableName, 
    i.name AS IndexName, 
    i.type_desc AS IndexType, 
    i.is_unique, 
    i.fill_factor 
FROM sys.indexes i 
INNER JOIN sys.tables t ON i.object_id = t.object_id 
WHERE i.name IS NOT NULL 
ORDER BY OBJECT_NAME(i.object_id), i.name
"@
    } -Description "Get index information for all tables"
    Write-Host "Index analysis query returned $($indexQuery.result.Count) indexes" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "### CLEANUP ###" -ForegroundColor DarkCyan
    # Remove test connection
    $removeResult = Invoke-ConnectionMethod -Action "remove-connection" -RequestData @{
        request = @{
            name = "TestConnection"
        }
    } -Description "Remove test database connection"
    Write-Host "Test connection removal completed: $($removeResult.success)" -ForegroundColor Green
    
    Write-Host "===================================================================" -ForegroundColor Magenta
    Write-Host "SQL Server MCP Tool Testing Complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "This script tested all available MCP tools:" -ForegroundColor Green
    Write-Host "- Connection Management: Add, Update, Remove, List, Test" -ForegroundColor Green
    Write-Host "- Core SQL Tools: Initialize, ExecuteQuery, GetTableMetadata" -ForegroundColor Green
    Write-Host "- Metadata Tools: GetDatabaseObjectsMetadata, GetDatabaseObjectsByType" -ForegroundColor Green
    Write-Host "- Specialized Tools: SQL Agent Jobs, SSIS Catalog, Azure DevOps" -ForegroundColor Green
    Write-Host "- Security Tools: Generate Key, Migrate to Encrypted" -ForegroundColor Green
    Write-Host "===================================================================" -ForegroundColor Magenta
}
catch {
    Write-Host "❌ Test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace:" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
}

# Summary statistics
Write-Host ""
Write-Host "### TEST SUMMARY ###" -ForegroundColor DarkYellow
Write-Host "Script execution completed at: $(Get-Date)" -ForegroundColor White
Write-Host "PowerShell version: $($PSVersionTable.PSVersion)" -ForegroundColor White
Write-Host "API endpoint: $baseUrl" -ForegroundColor White
