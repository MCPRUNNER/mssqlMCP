# Test script for database object metadata (tables and views)
# This script demonstrates how to use the new functionality for retrieving 
# metadata about database views in addition to tables.

# Use a stored connection or provide a connection string
$connectionName = "PROTO"  # Change this to your connection

# First, list all available connections
Write-Host "Listing available connections..." -ForegroundColor Cyan
$mcpInput = "#ListConnections"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe

# Get metadata for the specified connection including views
Write-Host "`nGetting database objects metadata including views..." -ForegroundColor Cyan
$mcpInput = "#GetDatabaseObjectsMetadata connectionName=$connectionName includeViews=true"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "database-objects-with-views.json"
Write-Host "Database objects metadata saved to database-objects-with-views.json" -ForegroundColor Green

# Get metadata for the specified connection, excluding views (tables only)
Write-Host "`nGetting database objects metadata, tables only..." -ForegroundColor Cyan
$mcpInput = "#GetDatabaseObjectsMetadata connectionName=$connectionName includeViews=false"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "database-objects-tables-only.json"
Write-Host "Tables-only metadata saved to database-objects-tables-only.json" -ForegroundColor Green

# Get only views using the object type filter
Write-Host "`nGetting view metadata using object type filter..." -ForegroundColor Cyan
$mcpInput = "#GetDatabaseObjectsMetadata connectionName=$connectionName objectType=VIEW"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "views-only-metadata.json"
Write-Host "Views-only metadata saved to views-only-metadata.json" -ForegroundColor Green

# For comparison, use the original method that only retrieves tables
Write-Host "`nGetting table metadata using original method..." -ForegroundColor Cyan
$mcpInput = "#GetTableMetadata connectionName=$connectionName"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "tables-metadata.json"
Write-Host "Table metadata saved to tables-metadata.json" -ForegroundColor Green

Write-Host "`nTest complete. Check the generated JSON files for results." -ForegroundColor Yellow
