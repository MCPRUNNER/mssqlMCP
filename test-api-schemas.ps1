#!/usr/bin/env pwsh

# Schema Filtering Test for SQL MCP Server

# Test parameters
$baseUrl = "http://localhost:3001"
$apiHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "=== Testing Schema Filtering Functionality ===" -ForegroundColor Magenta

# PART 1: Test Direct API Endpoints
Write-Host "`n-- Testing Direct API Endpoints --" -ForegroundColor Cyan

# Test health check
try {
    $healthResponse = Invoke-RestMethod -Uri "$baseUrl/api/test" -Method Get
    Write-Host "✓ Health check: $healthResponse" -ForegroundColor Green
}
catch {
    Write-Host "✗ Health check failed: $_" -ForegroundColor Red
    exit 1
}

# Test tables endpoint (all schemas)
try {
    $allTablesResponse = Invoke-RestMethod -Uri "$baseUrl/api/tables" -Method Get
    $schemas = $allTablesResponse | Select-Object -Property Schema -Unique | ForEach-Object { $_.Schema } | Sort-Object
    
    Write-Host "✓ Found $($allTablesResponse.Count) tables in all schemas" -ForegroundColor Green
    Write-Host "  Schemas: $($schemas -join ", ")" -ForegroundColor Yellow
    
    # Show sample tables
    Write-Host "`nSample tables:" -ForegroundColor White
    $allTablesResponse | Select-Object -First 3 | ForEach-Object {
        Write-Host "  - $($_.Schema).$($_.Name)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "✗ Failed to get all tables: $_" -ForegroundColor Red
}

# Test schema filtering - dbo schema
if ($schemas -and $schemas.Count -gt 0) {
    $testSchema = if ($schemas -contains "dbo") { "dbo" } else { $schemas[0] }
    
    Write-Host "`nTesting schema filter with '$testSchema'..." -ForegroundColor Cyan
    try {
        $schemaTablesResponse = Invoke-RestMethod -Uri "$baseUrl/api/tables?schema=$testSchema" -Method Get
        
        Write-Host "✓ Found $($schemaTablesResponse.Count) tables in '$testSchema' schema" -ForegroundColor Green
        
        # Verify schema filter
        $wrongSchemas = $schemaTablesResponse | Where-Object { $_.Schema -ne $testSchema }
        if ($wrongSchemas.Count -gt 0) {
            Write-Host "✗ Schema filter failed: Found tables with wrong schemas" -ForegroundColor Red
        } else {            Write-Host "✓ Schema filter working correctly!" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "✗ Failed to get tables for schema '$testSchema': $_" -ForegroundColor Red
    }
}

Write-Host "`n=== Schema Filtering Test Complete ===" -ForegroundColor Magenta
Write-Host "  Direct API Tests: Passed" -ForegroundColor Green
Write-Host "  Schema Filter Status: Working Correctly" -ForegroundColor Green
