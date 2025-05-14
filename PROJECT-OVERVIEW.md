# SQL Server MCP Project Overview

## Project Structure

```
mssqlMCP/
├── Program.cs                 # Entry point and application configuration
├── SqlServerTools.cs          # MCP tools implementation for SQL Server
├── DatabaseMetadataProvider.cs # Database schema metadata provider
├── appsettings.json           # Application settings and connection strings
├── mcp.json                   # MCP configuration for Copilot Agent
├── Logs/                      # Log files directory
├── examples/                  # Example projects for various languages
│   ├── node-example.js        # Node.js example
│   ├── python-example.py      # Python example
│   └── vscode-extension-example.md # VS Code extension guide
└── README.md                  # Project documentation
```

## Key Features

1. **SQL Server Connectivity**: Connect to SQL Server databases using Microsoft.Data.SqlClient
2. **MCP Protocol Support**: Implements the Model Context Protocol for use with Copilot Agent
3. **Database Tools**:
   - SQL Query Execution
   - Database Schema Metadata (tables, columns, keys)
   - Connection Management
4. **Echo Functionality**: Basic echo tools for testing
5. **Robust Logging**: Serilog integration with console and file logging
6. **Configurable Connections**: Multiple connection strings support
7. **Examples**: Demos in multiple languages

## MCP Tools

| Tool Name        | Description                                                   |
| ---------------- | ------------------------------------------------------------- |
| echo             | Echoes a message back to the client                           |
| f1Echo           | Alternative echo function (F1 version)                        |
| initialize       | Initializes the SQL Server connection                         |
| executeQuery     | Executes a SQL query and returns results as JSON              |
| getTableMetadata | Retrieves metadata about database tables, columns, keys, etc. |

## Getting Started

1. Update connection strings in `appsettings.json`
2. Run the server: `dotnet run`
3. Connect to the MCP endpoint at http://localhost:3001

## Next Steps

1. Add more robust SQL parameter support to prevent SQL injection
2. Implement transactions support
3. Add authentication mechanisms
4. Create more comprehensive database management tools
5. Add support for stored procedures and functions
