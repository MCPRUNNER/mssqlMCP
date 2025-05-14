#!/usr/bin/env pwsh

Write-Host "Testing MCP Server API with curl" -ForegroundColor Green
Write-Host "=============================="

# Base endpoint
$uri = "http://localhost:3001"

# 1. Test initialization
Write-Host "`nTesting initialization..." -ForegroundColor Cyan
$initBody = @{
    jsonrpc = "2.0"
    method  = "initialize"
    params  = @{
        connectionName = "DefaultConnection"
    }
    id      = 1
} | ConvertTo-Json

Write-Host "Request body: $initBody"

$initHeaders = @{
    "Content-Type" = "application/json"
    "Accept"       = "application/json"
}

$initResponse = Invoke-WebRequest -Uri $uri -Method Post -Body $initBody -Headers $initHeaders -UseBasicParsing
Write-Host "Response Status: $($initResponse.StatusCode)" -ForegroundColor Green
Write-Host "Response: $($initResponse.Content)"

# 2. Test getting all tables (no schema filter)
Write-Host "`nTesting getTableMetadata (all schemas)..." -ForegroundColor Cyan
$allTablesBody = @{
    jsonrpc = "2.0"
    method  = "getTableMetadata"
    params  = @{
        connectionName = "DefaultConnection"
    }
    id      = 2
} | ConvertTo-Json

$allTablesResponse = Invoke-WebRequest -Uri $uri -Method Post -Body $allTablesBody -Headers $initHeaders -UseBasicParsing
Write-Host "Response Status: $($allTablesResponse.StatusCode)" -ForegroundColor Green

$allTables = ($allTablesResponse.Content | ConvertFrom-Json).result | ConvertFrom-Json
$schemas = $allTables | Select-Object -ExpandProperty Schema -Unique | Sort-Object
Write-Host "Found schemas: $($schemas -join ", ")" -ForegroundColor Yellow

# 3. Test getting tables with schema filter
$testSchema = if ($schemas -contains "dbo") { "dbo" } else { $schemas[0] }
Write-Host "`nTesting getTableMetadata with schema filter ('$testSchema')..." -ForegroundColor Cyan
$schemaTablesBody = @{
    jsonrpc = "2.0"
    method  = "getTableMetadata"
    params  = @{
        connectionName = "DefaultConnection"
        schema = $testSchema
    }
    id      = 3
} | ConvertTo-Json

$schemaTablesResponse = Invoke-WebRequest -Uri $uri -Method Post -Body $schemaTablesBody -Headers $initHeaders -UseBasicParsing
Write-Host "Response Status: $($schemaTablesResponse.StatusCode)" -ForegroundColor Green

$schemaTables = ($schemaTablesResponse.Content | ConvertFrom-Json).result | ConvertFrom-Json
Write-Host "Found $($schemaTables.Count) tables in '$testSchema' schema" -ForegroundColor Yellow
Write-Host "Sample tables: $($schemaTables | Select-Object -First 5 -ExpandProperty Name | ForEach-Object { $_ } | Join-String -Separator ", ")" -ForegroundColor Yellow

Write-Host "`nTest completed successfully!" -ForegroundColor Green
