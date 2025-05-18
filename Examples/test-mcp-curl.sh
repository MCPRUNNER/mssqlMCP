#!/bin/bash
# Example of using the SQL Server MCP with curl

# Define the API key - replace with your actual key from Set-Api-Key.ps1
API_KEY="your-api-key-here"

# Base URL for the MCP server
BASE_URL="http://localhost:3001"

# Function to call MCP methods
call_mcp() {
    local method=$1
    local params=$2
    
    echo "Calling $method..."
    curl -s -X POST $BASE_URL \
         -H "X-API-Key: $API_KEY" \
         -H "Content-Type: application/json" \
         -H "Accept: application/json" \
         -d "{\"jsonrpc\": \"2.0\", \"id\": \"$RANDOM\", \"method\": \"$method\", \"params\": $params}"
    echo ""
}

# Initialize the connection
call_mcp "initialize" "{\"connectionName\": \"DefaultConnection\"}"

# Get metadata about tables
call_mcp "getTableMetadata" "{\"connectionName\": \"DefaultConnection\", \"schema\": \"dbo\"}"

# Execute a simple query
call_mcp "executeQuery" "{\"connectionName\": \"DefaultConnection\", \"query\": \"SELECT name, database_id, create_date FROM sys.databases\"}"

# Get database objects including views
call_mcp "getDatabaseObjectsMetadata" "{\"connectionName\": \"DefaultConnection\", \"includeViews\": true}"

# List connections
call_mcp "connectionManager/list" "{}"

echo "Example completed successfully!"
