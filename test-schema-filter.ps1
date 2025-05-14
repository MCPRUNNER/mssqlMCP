#!/usr/bin/env pwsh
# PowerShell script to test the GetTableMetadata function with schema filtering

$uri = "http://localhost:3001"
$headers = @{
    'Content-Type' = 'application/json'
    'Accept'       = 'application/json'
}

Write-Host "Testing GetTableMetadata without schema filter (all schemas)..." -ForegroundColor Green
$allSchemasBody = @{
    jsonrpc = "2.0"
    method  = "getTableMetadata"
    params  = @{
        connectionName = "DefaultConnection"
    }
    id      = 1
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $allSchemasBody
    Write-Host "Response: Number of tables: $($response.result | ConvertFrom-Json | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor Cyan
    
    # Show schemas in the response
    $schemas = ($response.result | ConvertFrom-Json) | Select-Object -ExpandProperty Schema -Unique
    Write-Host "Schemas found: $($schemas -join ', ')" -ForegroundColor Cyan
}
catch {
    Write-Host "Error calling GetTableMetadata: $_" -ForegroundColor Red
}

Write-Host "`nTesting GetTableMetadata with 'dbo' schema filter..." -ForegroundColor Green
$dboSchemaBody = @{
    jsonrpc = "2.0"
    method  = "getTableMetadata"
    params  = @{
        connectionName = "DefaultConnection"
        schema         = "dbo"
    }
    id      = 2
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Headers $headers -Body $dboSchemaBody
    $tables = $response.result | ConvertFrom-Json
    Write-Host "Response: Number of dbo schema tables: $($tables | Measure-Object | Select-Object -ExpandProperty Count)" -ForegroundColor Cyan
    Write-Host "Table names: $($tables | Select-Object -ExpandProperty Name | Sort-Object | ForEach-Object { "$($_.ToLower())" } | Select-Object -First 5 | Join-String -Separator ', ')" -ForegroundColor Cyan
}
catch {
    Write-Host "Error calling GetTableMetadata with schema filter: $_" -ForegroundColor Red
}

Write-Host "`nTest completed." -ForegroundColor Green
