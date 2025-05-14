#!/usr/bin/env pwsh

Write-Host "Testing MCP Server Schema Filtering" -ForegroundColor Green
Write-Host "================================="

# Base URL for the MCP server
$uri = "http://localhost:3001"

# Test the initialize endpoint
Write-Host "`nTesting initialize endpoint..." -ForegroundColor Cyan
$initRequest = @{
    jsonrpc = "2.0"
    id = 1
    method = "initialize"
    params = @{
        connectionName = "DefaultConnection"
    }
} | ConvertTo-Json

try {
    $initResponse = Invoke-RestMethod -Uri $uri -Method Post -Body $initRequest -ContentType "application/json"
    Write-Host "Connection initialized successfully: $($initResponse.result)" -ForegroundColor Green
}
catch {
    Write-Host "Error initializing connection: $_" -ForegroundColor Red
    Write-Host "StatusCode: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    exit 1
}

# Test getTableMetadata without schema filter
Write-Host "`nTesting getTableMetadata (all schemas)..." -ForegroundColor Cyan
$allTablesRequest = @{
    jsonrpc = "2.0"
    id = 2
    method = "getTableMetadata"
    params = @{
        connectionName = "DefaultConnection"
    }
} | ConvertTo-Json

try {
    $allTablesResponse = Invoke-RestMethod -Uri $uri -Method Post -Body $allTablesRequest -ContentType "application/json"
    if ($allTablesResponse.result) {
        # Convert the string result to a proper object
        $tables = $allTablesResponse.result | ConvertFrom-Json
        $schemas = $tables | Select-Object -Property Schema -Unique | ForEach-Object { $_.Schema } | Sort-Object
        
        Write-Host "Found $($tables.Count) tables in all schemas" -ForegroundColor Green
        Write-Host "Available schemas: $($schemas -join ", ")" -ForegroundColor Green
        
        # Check if we have any tables to sample
        if ($tables.Count -gt 0) {
            $sampleTables = $tables | Select-Object -First 5
            Write-Host "Sample tables from all schemas:" -ForegroundColor Cyan
            $sampleTables | ForEach-Object { Write-Host "  - $($_.Schema).$($_.Name)" -ForegroundColor Yellow }
        }
    }
    else {
        Write-Host "No tables found or empty result" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Error getting all tables: $_" -ForegroundColor Red
    Write-Host "StatusCode: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

# Test getTableMetadata with schema filter if we have found schemas
if ($schemas -and $schemas.Count -gt 0) {
    $testSchema = if ($schemas -contains "dbo") { "dbo" } else { $schemas[0] }
    
    Write-Host "`nTesting getTableMetadata with schema filter ('$testSchema')..." -ForegroundColor Cyan
    $schemaTablesRequest = @{
        jsonrpc = "2.0"
        id = 3
        method = "getTableMetadata"
        params = @{
            connectionName = "DefaultConnection"
            schema = $testSchema
        }
    } | ConvertTo-Json

    try {
        $schemaTablesResponse = Invoke-RestMethod -Uri $uri -Method Post -Body $schemaTablesRequest -ContentType "application/json"
        if ($schemaTablesResponse.result) {
            # Convert the string result to a proper object
            $schemaTables = $schemaTablesResponse.result | ConvertFrom-Json
            
            Write-Host "Found $($schemaTables.Count) tables in '$testSchema' schema" -ForegroundColor Green
            
            # Check if we have any tables to sample
            if ($schemaTables.Count -gt 0) {
                $schemaSampleTables = $schemaTables | Select-Object -First 5
                Write-Host "Tables in '$testSchema' schema:" -ForegroundColor Cyan
                $schemaSampleTables | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Yellow }
            }
        }
        else {
            Write-Host "No tables found in schema '$testSchema' or empty result" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Error getting tables for schema '$testSchema': $_" -ForegroundColor Red
        Write-Host "StatusCode: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    }
}

Write-Host "`nSchema filtering test completed" -ForegroundColor Green
