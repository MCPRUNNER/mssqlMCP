# Comprehensive SQL MCP Server Schema Filtering Test
# This script tests both direct API and MCP JSON-RPC endpoints

# Color theme for output
$Colors = @{
    Title = "Magenta"
    Section = "Cyan"
    Success = "Green" 
    Error = "Red"
    Warning = "Yellow"
    Info = "White"
}

# Output formatting function
function Write-Header {
    param (
        [string]$Text,
        [string]$Color = $Colors.Title
    )
    
    Write-Host "`n$("=" * 60)" -ForegroundColor $Color
    Write-Host "  $Text" -ForegroundColor $Color
    Write-Host "$("=" * 60)" -ForegroundColor $Color
}

function Write-Section {
    param (
        [string]$Text,
        [string]$Color = $Colors.Section
    )
    
    Write-Host "`n$Text" -ForegroundColor $Color
    Write-Host "$("-" * 40)" -ForegroundColor $Color
}

# Configuration
$baseUrl = "http://localhost:3001"
$mcp = @{
    Uri = $baseUrl
    Headers = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
    }
}

Write-Host "Testing MCP Server Schema Filtering" -ForegroundColor Green
Write-Host "=================================="

# Initialize connection
Write-Host "`nInitializing connection..." -ForegroundColor Cyan
$initBody = @{
    jsonrpc = "2.0"
    method  = "initialize"
    params  = @{
        connectionName = "DefaultConnection"
    }
    id      = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $initBody
    Write-Host "Connection initialized successfully: $($response.result)" -ForegroundColor Green
}
catch {
    Write-Host "Failed to initialize connection: $_" -ForegroundColor Red
    exit 1
}

# Test GetTableMetadata - All schemas
Write-Host "`nGetting all tables from all schemas..." -ForegroundColor Cyan
$allTablesBody = @{
    jsonrpc = "2.0"
    method  = "getTableMetadata"
    params  = @{
        connectionName = "DefaultConnection"
    }
    id      = 2
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $allTablesBody
    $tables = $response.result | ConvertFrom-Json
    
    # Extract unique schemas
    $schemas = $tables | Select-Object -ExpandProperty Schema -Unique | Sort-Object
    
    Write-Host "Found $($tables.Count) tables across $($schemas.Count) schemas" -ForegroundColor Green
    Write-Host "Schemas: $($schemas -join ', ')" -ForegroundColor Green
    
    # Display sample of tables
    Write-Host "`nSample tables from all schemas:" -ForegroundColor Cyan
    $tables | Select-Object Schema, Name -First 5 | Format-Table
}
catch {
    Write-Host "Error getting all tables: $_" -ForegroundColor Red
}

# Test GetTableMetadata - specific schema
$testSchema = "dbo" # Can be changed to any schema found above
Write-Host "`nGetting tables from '$testSchema' schema only..." -ForegroundColor Cyan
$schemaTablesBody = @{
    jsonrpc = "2.0"
    method  = "getTableMetadata"
    params  = @{
        connectionName = "DefaultConnection"
        schema = $testSchema
    }
    id      = 3
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $schemaTablesBody
    $tables = $response.result | ConvertFrom-Json
    
    Write-Host "Found $($tables.Count) tables in '$testSchema' schema" -ForegroundColor Green
    
    # Display all tables in this schema
    Write-Host "`nTables in '$testSchema' schema:" -ForegroundColor Cyan
    $tables | Select-Object Name | Format-Table
}
catch {
    Write-Host "Error getting tables for schema '$testSchema': $_" -ForegroundColor Red
}

Write-Host "`nSchema filtering test completed" -ForegroundColor Green
