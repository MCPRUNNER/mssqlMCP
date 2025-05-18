# Using SQL Server MCP with Copilot Agent in VS Code

This document provides instructions for setting up and using the SQL Server MCP server with GitHub Copilot in Visual Studio Code.

## Setup Instructions

### 1. Start the MCP Server

You can start the MCP server using one of the provided scripts:

```powershell
# Start with encryption enabled (recommended)
./Scripts/Start-MCP-Encrypted.ps1

# OR with API security enabled
./Scripts/Start-MCP-Encrypted-Local.ps1
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

Behind the scenes, Copilot will use MCP tools to execute your requests:

```javascript
f1e_initialize({ connectionName: "DefaultConnection" });

f1e_getTableMetadata({
  connectionName: "DefaultConnection",
  schema: "dbo",
});

f1e_executeQuery({
  connectionName: "DefaultConnection",
  query: "SELECT TOP 5 * FROM Orders ORDER BY OrderDate DESC",
});
```

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
        "X-API-Key": "${input:mssql-server-mcp-api-key}"
      }
    }
  }
}
```

### Available Tools

The SQL Server MCP server exposes the following tools to Copilot:

1. **initialize**: Initialize a SQL Server connection
2. **executeQuery**: Run SQL queries and return results
3. **getTableMetadata**: Get metadata about database tables and their relationships
4. **getDatabaseObjectsMetadata**: Get metadata about tables, views, and stored procedures
5. **connectionManager/list**: List all saved database connections
6. **connectionManager/add**: Add a new database connection
7. **connectionManager/update**: Update an existing connection
8. **connectionManager/remove**: Remove a connection
9. **security/rotateKey**: Rotate the encryption key for connection strings
10. **security/generateSecureKey**: Generate a secure random key

## Troubleshooting

### Connection Issues

If you see an error like "Connection string not found":

1. Check if the connection exists using `connectionManager/list`
2. Try adding the connection using `connectionManager/add`
3. Verify that your MCP server is running

### Authentication Errors

If you see 401 or 403 errors:

1. Run `./Scripts/Set-Api-Key.ps1` to generate a new API key
2. Make sure the key is properly set in your environment
3. Restart the MCP server and VS Code

### Example Errors and Solutions

| Error                         | Solution                                                  |
| ----------------------------- | --------------------------------------------------------- |
| "Connection string not found" | Add the connection using `connectionManager/add`          |
| "401 Unauthorized"            | Configure API key using `Set-Api-Key.ps1`                 |
| "Table not found"             | Check table name and schema, use `getTableMetadata` first |
| "SQL syntax error"            | Fix the SQL query syntax                                  |

## Example Workflow

Here's an example of how you might use SQL Server MCP with Copilot:

1. Start the MCP server: `./Scripts/Start-MCP-Encrypted.ps1`
2. Configure API key: `./Scripts/Set-Api-Key.ps1`
3. Add a connection:
   ```
   connectionManager/add
   Params: {
     "Name": "AdventureWorks",
     "ConnectionString": "Server=myserver;Database=AdventureWorks;Trusted_Connection=True;",
     "Description": "AdventureWorks database"
   }
   ```
4. Ask Copilot: "Show me all tables in the AdventureWorks database"
5. Ask Copilot: "Write a query to find the top 5 customers by order amount"

For more information, see the [full documentation](./Documentation/README.md).

## Security Best Practices

1. Always use API key authentication in production
2. Rotate encryption keys periodically using `Rotate-Encryption-Key.ps1`
3. Store API keys and encryption keys securely
4. Use HTTPS when exposing the API endpoint externally
5. Follow the principle of least privilege for database connections
