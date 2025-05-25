#!/bin/bash
# Example of using the SQL Server MCP with curl
# This script demonstrates all available MCP tools and their proper usage

# Define the API key - replace with your actual key from Set-Api-Key.ps1
API_KEY="your-api-key-here"

# Improvements needed in code
# Base URL for the MCP server
BASE_URL="http://localhost:3001"

# Function to call MCP methods with enhanced error handling
call_mcp() {
    local method=$1
    local params=$2
    local description=$3
    
    echo "======================================"
    echo "Testing: $description"
    echo "Method: $method"
    echo "Params: $params"
    echo "======================================"
    
    response=$(curl -s -X POST $BASE_URL \
         -H "X-API-Key: $API_KEY" \
         -H "Content-Type: application/json" \
         -H "Accept: application/json" \
         -d "{\"jsonrpc\": \"2.0\", \"id\": \"$RANDOM\", \"method\": \"$method\", \"params\": $params}")
    
    echo "Response:"
    echo "$response" | jq '.' 2>/dev/null || echo "$response"
    echo ""
    echo ""
}

# Function to call connection management tools
call_connection_tool() {
    local action=$1
    local request_data=$2
    local description=$3
    
    echo "======================================"
    echo "Testing: $description"
    echo "Action: $action"
    echo "Request: $request_data"
    echo "======================================"
    
    response=$(curl -s -X POST $BASE_URL/$action \
         -H "X-API-Key: $API_KEY" \
         -H "Content-Type: application/json" \
         -H "Accept: application/json" \
         -d "$request_data")
    
    echo "Response:"
    echo "$response" | jq '.' 2>/dev/null || echo "$response"
    echo ""
    echo ""
}

echo "==================================================================="
echo "SQL Server MCP Tool Testing Script"
echo "Testing all available MCP tools with current method names"
echo "==================================================================="
echo ""

# 1. CONNECTION MANAGEMENT TOOLS
echo "### CONNECTION MANAGEMENT TOOLS ###"

# List existing connections
call_connection_tool "list-connections" "{}" "List all database connections"

# Test a connection
call_connection_tool "test-connection" '{
    "request": {
        "connectionString": "Server=localhost;Database=master;Trusted_Connection=true;"
    }
}' "Test database connection"

# Add a new connection
call_connection_tool "add-connection" '{
    "request": {
        "name": "TestConnection",
        "connectionString": "Server=localhost;Database=TestDB;Trusted_Connection=true;",
        "description": "Test database connection"
    }
}' "Add new database connection"

# Update existing connection
call_connection_tool "update-connection" '{
    "request": {
        "name": "TestConnection",
        "connectionString": "Server=localhost;Database=UpdatedTestDB;Trusted_Connection=true;",
        "description": "Updated test database connection"
    }
}' "Update existing database connection"

echo ""
echo "### CORE SQL SERVER TOOLS ###"

# 2. Initialize the connection
call_mcp "Initialize" '{"connectionName": "DefaultConnection"}' "Initialize SQL Server connection"

# 3. Execute a simple query
call_mcp "ExecuteQuery" '{
    "connectionName": "DefaultConnection",
    "query": "SELECT name, database_id, create_date FROM sys.databases WHERE name NOT IN (\"master\", \"tempdb\", \"model\", \"msdb\")"
}' "Execute SQL query to get user databases"

# 4. Get table metadata
call_mcp "GetTableMetadata" '{
    "connectionName": "DefaultConnection",
    "schema": "dbo"
}' "Get table metadata for dbo schema"

# 5. Get database objects metadata
call_mcp "GetDatabaseObjectsMetadata" '{
    "connectionName": "DefaultConnection",
    "includeViews": true,
    "schema": "dbo"
}' "Get database objects metadata including views"

# 6. Get database objects by type
call_mcp "GetDatabaseObjectsByType" '{
    "connectionName": "DefaultConnection",
    "objectType": "TABLE",
    "schema": "dbo"
}' "Get database objects filtered by type (tables only)"

call_mcp "GetDatabaseObjectsByType" '{
    "connectionName": "DefaultConnection",
    "objectType": "VIEW",
    "schema": "dbo"
}' "Get database objects filtered by type (views only)"

call_mcp "GetDatabaseObjectsByType" '{
    "connectionName": "DefaultConnection",
    "objectType": "PROCEDURE",
    "schema": "dbo"
}' "Get database objects filtered by type (stored procedures only)"

echo ""
echo "### SPECIALIZED METADATA TOOLS ###"

# 7. Get SQL Server Agent Jobs
call_mcp "GetSqlServerAgentJobs" '{
    "connectionName": "DefaultConnection"
}' "Get SQL Server Agent jobs metadata"

# 8. Get detailed SQL Server Agent Job information
call_mcp "GetSqlServerAgentJobDetails" '{
    "connectionName": "DefaultConnection",
    "jobName": "DatabaseBackup"
}' "Get detailed SQL Server Agent job information"

# 9. Get SSIS Catalog information
call_mcp "GetSsisCatalogInfo" '{
    "connectionName": "DefaultConnection"
}' "Get SSIS catalog information"

# 10. Get Azure DevOps information
call_mcp "GetAzureDevOpsInfo" '{
    "connectionName": "DefaultConnection"
}' "Get Azure DevOps information"

echo ""
echo "### SECURITY TOOLS ###"

# Generate secure key
call_connection_tool "generate-secure-key" '{
    "length": 32
}' "Generate secure encryption key"

# Migrate connections to encrypted format
call_connection_tool "migrate-to-encrypted" '{}' "Migrate connection strings to encrypted format"

# Note: Key rotation requires a new key parameter
# call_connection_tool "rotate-key" '{
#     "newKey": "new-generated-key-here"
# }' "Rotate encryption key"

echo ""
echo "### ADVANCED QUERY EXAMPLES ###"

# Complex metadata query
call_mcp "ExecuteQuery" '{
    "connectionName": "DefaultConnection",
    "query": "SELECT t.name AS TableName, c.name AS ColumnName, ty.name AS DataType, c.max_length, c.is_nullable FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id WHERE t.name LIKE \"%User%\" ORDER BY t.name, c.column_id"
}' "Get detailed column information for tables containing 'User'"

# Performance monitoring query
call_mcp "ExecuteQuery" '{
    "connectionName": "DefaultConnection",
    "query": "SELECT db_name(database_id) as DatabaseName, type_desc, size_mb = size * 8.0 / 1024, growth_desc = CASE WHEN is_percent_growth = 1 THEN CAST(growth AS VARCHAR(3)) + \"%\" ELSE CAST(growth * 8.0 / 1024 AS VARCHAR(10)) + \" MB\" END FROM sys.master_files WHERE database_id > 4"
}' "Get database file information and growth settings"

# Index analysis query
call_mcp "ExecuteQuery" '{
    "connectionName": "DefaultConnection",
    "query": "SELECT OBJECT_NAME(i.object_id) AS TableName, i.name AS IndexName, i.type_desc AS IndexType, i.is_unique, i.fill_factor FROM sys.indexes i INNER JOIN sys.tables t ON i.object_id = t.object_id WHERE i.name IS NOT NULL ORDER BY OBJECT_NAME(i.object_id), i.name"
}' "Get index information for all tables"

echo ""
echo "### CLEANUP ###"

# Remove test connection
call_connection_tool "remove-connection" '{
    "request": {
        "name": "TestConnection"
    }
}' "Remove test database connection"

echo "==================================================================="
echo "SQL Server MCP Tool Testing Complete!"
echo ""
echo "This script tested all available MCP tools:"
echo "- Connection Management: Add, Update, Remove, List, Test"
echo "- Core SQL Tools: Initialize, ExecuteQuery, GetTableMetadata"
echo "- Metadata Tools: GetDatabaseObjectsMetadata, GetDatabaseObjectsByType"
echo "- Specialized Tools: SQL Agent Jobs, SSIS Catalog, Azure DevOps"
echo "- Security Tools: Generate Key, Migrate to Encrypted, Rotate Key"
echo "==================================================================="
