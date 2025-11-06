# Using SQL Server MCP with Copilot Agent in VS Code

This document provides instructions for setting up and using the SQL Server MCP server with GitHub Copilot in Visual Studio Code.

## Setup Instructions

### 1. Start the MCP Server

You can start the MCP server using the provided script:

```powershell
# Start with encryption enabled and set up API key authentication (recommended)
./Scripts/Start-MCP-Encrypted.ps1

```

### 2. Setup API Authentication

For secure access, set up API key authentication:

```powershell
# Generate and configure an API key
./Scripts/Set-Api-Key.ps1
```

This script will:

- Generate a cryptographically secure random API key
- Set it as an environment variable (MSSQL_MCP_API_KEY)
- Update the appsettings.json file
- Display usage examples

### 3. Configure VS Code

1. Create a `.vscode` folder in your project (if it doesn't exist)
2. Copy the `mcp.json` file from the root of this project to your `.vscode` folder
3. Update the file if needed (see the Configuration section below)

## Using with Copilot

Once the MCP server is running and VS Code is configured, you can use Copilot to interact with your SQL Server databases.

### Sample Queries

Try asking Copilot questions like:

- "Show me all the tables in my DefaultConnection database"
- "What columns are in the Users table?"
- "Create a query to find the top 5 most recent orders"
- "How many products are in each category?"

### Tool Invocation

Behind the scenes, Copilot will use MCP tools through JSON-RPC 2.0 requests using the `tools/call` method:

```json
// Initialize a connection
{
  "jsonrpc": "2.0",
  "id": "copilot-request-1",
  "method": "tools/call",
  "params": {
    "name": "mssql_initialize_connection",
    "arguments": {
      "connectionName": "DefaultConnection"
    }
  }
}

// Get table metadata
{
  "jsonrpc": "2.0",
  "id": "copilot-request-2",
  "method": "tools/call",
  "params": {
    "name": "mssql_get_table_metadata",
    "arguments": {
      "connectionName": "DefaultConnection",
      "schema": "dbo"
    }
  }
}

// Execute a query
{
  "jsonrpc": "2.0",
  "id": "copilot-request-3",
  "method": "tools/call",
  "params": {
    "name": "mssql_execute_query",
    "arguments": {
      "connectionName": "DefaultConnection",
      "query": "SELECT TOP 5 * FROM Orders ORDER BY OrderDate DESC"
    }
  }
}
```

### Response Format

When Copilot calls MCP tools, the server responds with a standardized JSON-RPC format:

```json
{
  "jsonrpc": "2.0",
  "id": "copilot-request-id",
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Human-readable result description"
      }
    ],
    "structuredContent": {
      // Structured data that Copilot can reason about
    },
    "isError": false
  }
}
```

This format includes both a human-readable text description and structured data that Copilot can analyze to provide you with meaningful insights about your database.

## Configuration

### mcp.json Configuration

The `mcp.json` file in the `.vscode` folder configures how VS Code connects to the MCP server:

```json
{
  "inputs": [
    {
      "id": "mssql-server-mcp-api-key",
      "type": "promptString",
      "description": "Enter your SQL Server MCP API Key",
      "password": true
    }
  ],
  "servers": {
    "sql-server-mcp": {
      "url": "http://localhost:3001",
      "headers": {
        "Authorization": "Bearer ${input:mssql-server-mcp-api-key}",
        "Content-Type": "application/json"
      }
    }
  }
}
```

### Available Tools

The SQL Server MCP server exposes the following tools to Copilot:

1. **mssql_initialize_connection**: Initialize a SQL Server connection
2. **mssql_execute_query**: Run SQL queries and return results
3. **mssql_get_table_metadata**: Get metadata about database tables and their relationships
4. **mssql_get_database_objects_metadata**: Get metadata about tables, views, and stored procedures
5. **mssql_list_connections**: List all saved database connections
6. **mssql_add_connection**: Add a new database connection
7. **mssql_update_connection**: Update an existing connection
8. **mssql_remove_connection**: Remove a connection
9. **mssql_rotate_key**: Rotate the encryption key for connection strings
10. **mssql_generate_key**: Generate a secure random key

## Troubleshooting

### Connection Issues

If you see an error like "Connection string not found":

1. Check if the connection exists using the mssql_list_connections tool
2. Try adding the connection using the mssql_add_connection tool
3. Verify that your MCP server is running

### Authentication Errors

If you see 401 or 403 errors:

1. Run `./Scripts/Set-Api-Key.ps1` to generate a new API key
2. Make sure the key is properly set in your environment
3. Restart the MCP server and VS Code

### JSON-RPC Errors

If you see JSON-RPC protocol errors:

1. **Invalid Request (-32600)**: Check that your request format follows JSON-RPC 2.0 specification
2. **Method Not Found (-32601)**: Verify that the method name is correct
3. **Invalid Parameters (-32602)**: Check that all required parameters are provided correctly
4. **Internal Error (-32603)**: This indicates a server-side error that requires investigation

### Example Errors and Solutions

| Error                            | Solution                                                        |
| -------------------------------- | --------------------------------------------------------------- |
| "Connection string not found"    | Add the connection using mssql_add_connection tool              |
| "401 Unauthorized"               | Configure API key using `Set-Api-Key.ps1`                       |
| "Table not found"                | Check table name and schema, use mssql_get_table_metadata first |
| "SQL syntax error"               | Fix the SQL query syntax                                        |
| "Method not found: InvalidTool"  | Check tool name, use tools/list to see available tools          |
| "Invalid parameters for Execute" | Check that all required parameters are provided                 |

## Example Workflow

Here's an example of how you might use SQL Server MCP with Copilot:

1. Start the MCP server: `./Scripts/Start-MCP-Encrypted.ps1`
2. Configure API key: `./Scripts/Set-Api-Key.ps1`
3. Add a connection using the Copilot Agent format:
   ```json
   {
     "jsonrpc": "2.0",
     "id": "request-1",
     "method": "tools/call",
     "params": {
       "name": "mssql_add_connection",
       "arguments": {
         "request": {
           "name": "AdventureWorks",
           "connectionString": "Server=myserver;Database=AdventureWorks;Trusted_Connection=True;",
           "description": "AdventureWorks database"
         }
       }
     }
   }
   ```
4. Ask Copilot: "Show me all tables in the AdventureWorks database"
5. Ask Copilot: "Write a query to find the top 5 customers by order amount"

Behind the scenes, Copilot will execute the appropriate JSON-RPC calls based on your natural language requests.

For more information, see the [full documentation](./Documentation/README.md).

## Security Best Practices

1. Always use API key authentication in production
2. Rotate encryption keys periodically using `Rotate-Encryption-Key.ps1`
3. Store API keys and encryption keys securely
4. Use HTTPS when exposing the API endpoint externally
5. Follow the principle of least privilege for database connections
