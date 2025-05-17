# Test script for stored procedure metadata
# This script demonstrates how to use the new functionality for retrieving 
# metadata about SQL Server stored procedures.

# Use a stored connection or provide a connection string
$connectionName = "PROTO"  # Change this to your connection

# First, list all available connections
Write-Host "Listing available connections..." -ForegroundColor Cyan
$mcpInput = "#ListConnections"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe

# Get all database objects metadata (tables, views, and procedures)
Write-Host "`nGetting all database objects metadata..." -ForegroundColor Cyan
$mcpInput = "#GetDatabaseObjectsMetadata connectionName=$connectionName"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "all-database-objects.json"
Write-Host "All database objects metadata saved to all-database-objects.json" -ForegroundColor Green

# Get only stored procedure metadata
Write-Host "`nGetting stored procedure metadata only..." -ForegroundColor Cyan
$mcpInput = "#GetDatabaseObjectsMetadata connectionName=$connectionName objectType=PROCEDURE"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "stored-procedures-metadata.json"
Write-Host "Stored procedure metadata saved to stored-procedures-metadata.json" -ForegroundColor Green

# Get system stored procedures from the master database (if you have a connection to master)
Write-Host "`nGetting system stored procedures from master database..." -ForegroundColor Cyan
$masterConnection = "P330_master"  # Change this to your master connection
$mcpInput = "#GetDatabaseObjectsMetadata connectionName=$masterConnection objectType=PROCEDURE"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "system-stored-procedures.json"
Write-Host "System stored procedures metadata saved to system-stored-procedures.json" -ForegroundColor Green

# Get objects from a specific schema
Write-Host "`nGetting stored procedures from dbo schema..." -ForegroundColor Cyan
$mcpInput = "#GetDatabaseObjectsMetadata connectionName=$connectionName schema=dbo objectType=PROCEDURE"
$mcpInput | .\bin\Debug\net9.0\mssqlMCP.exe | Out-File -FilePath "dbo-stored-procedures.json"
Write-Host "dbo schema stored procedures metadata saved to dbo-stored-procedures.json" -ForegroundColor Green

Write-Host "`nTest complete. Check the generated JSON files for results." -ForegroundColor Yellow
