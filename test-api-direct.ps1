#!/usr/bin/env pwsh

Write-Host "Testing Direct API Endpoints" -ForegroundColor Green
Write-Host "=========================="

# Base URL for the API
$baseUrl = "http://localhost:3001"

# Test the health check endpoint
Write-Host "`nTesting API health..." -ForegroundColor Cyan
try {
    $healthResponse = Invoke-RestMethod -Uri "$baseUrl/api/test" -Method Get -ContentType "application/json"
    Write-Host "Health check successful: $healthResponse" -ForegroundColor Green
}
catch {
    Write-Host "Error checking API health: $_" -ForegroundColor Red
    exit 1
}

# Test the tables endpoint without schema filter
Write-Host "`nTesting tables endpoint (all schemas)..." -ForegroundColor Cyan
try {
    $allTablesResponse = Invoke-RestMethod -Uri "$baseUrl/api/tables" -Method Get -ContentType "application/json"
    $schemas = $allTablesResponse | Select-Object -Property Schema -Unique | ForEach-Object { $_.Schema } | Sort-Object
    
    Write-Host "Found $($allTablesResponse.Count) tables in all schemas" -ForegroundColor Green
    Write-Host "Available schemas: $($schemas -join ", ")" -ForegroundColor Green
    
    # Display a few sample tables
    if ($allTablesResponse.Count -gt 0) {
        $sampleTables = $allTablesResponse | Select-Object -First 5
        Write-Host "`nSample tables:" -ForegroundColor Cyan
        $sampleTables | ForEach-Object { Write-Host "  - $($_.Schema).$($_.Name)" -ForegroundColor Yellow }
    }
}
catch {
    Write-Host "Error getting all tables: $_" -ForegroundColor Red
}

# Test the tables endpoint with schema filter
if ($schemas -and $schemas.Count -gt 0) {
    $testSchema = if ($schemas -contains "dbo") { "dbo" } else { $schemas[0] }
    
    Write-Host "`nTesting tables endpoint with schema filter ('$testSchema')..." -ForegroundColor Cyan
    try {
        $schemaTablesResponse = Invoke-RestMethod -Uri "$baseUrl/api/tables?schema=$testSchema" -Method Get -ContentType "application/json"
        
        Write-Host "Found $($schemaTablesResponse.Count) tables in '$testSchema' schema" -ForegroundColor Green
        
        # Check if the schema filter is working properly
        $wrongSchemaCount = ($schemaTablesResponse | Where-Object { $_.Schema -ne $testSchema }).Count
        if ($wrongSchemaCount -gt 0) {
            Write-Host "WARNING: Found $wrongSchemaCount tables with incorrect schema!" -ForegroundColor Red
        } else {
            Write-Host "Schema filter is working correctly!" -ForegroundColor Green
        }
        
        # Display a few sample tables
        if ($schemaTablesResponse.Count -gt 0) {
            $schemaSampleTables = $schemaTablesResponse | Select-Object -First 5
            Write-Host "`nSample tables in '$testSchema' schema:" -ForegroundColor Cyan
            $schemaSampleTables | ForEach-Object { Write-Host "  - $($_.Name)" -ForegroundColor Yellow }
        }
    }
    catch {
        Write-Host "Error getting tables for schema '$testSchema': $_" -ForegroundColor Red
    }
}

Write-Host "`nAPI testing completed successfully!" -ForegroundColor Green
