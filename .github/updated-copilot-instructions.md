```instructions
<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# mssqlMCP Copilot Instructions

This is a Model Context Protocol (MCP) Server project that connects to SQL Server databases, and is designed to be used by VS Code as a Copilot Agent.

## Resources & References

- https://modelcontextprotocol.io/llms-full.txt
- https://github.com/modelcontextprotocol/csharp-sdk

## Project Overview

This project:

- Implements the Model Context Protocol for SQL Server database connectivity
- Uses stdio for communication with VS Code
- Allows VS Code and Copilot to connect to SQL databases via MCP
- Supports fetching extensive database metadata (tables, views, stored procedures, functions)
- Retrieves schema information, column details, primary/foreign keys, and object definitions
- Uses C# (.NET) for the implementation
- Supports logging, encryption, and connection management

## Key Components

- **DatabaseMetadataProvider**: Core service that retrieves database schema information
  - Handles tables, views, stored procedures, and functions
  - Gets column definitions, primary keys, foreign keys
  - Retrieves SQL object definitions (for views, procedures, functions)

- **SqlServerTools**: MCP tools implementation for SQL Server operations
  - Provides endpoints for metadata retrieval and query execution
  - Has methods like GetDatabaseObjectsMetadata, GetTableMetadata, ExecuteQuery

- **ConnectionManager**: Manages database connections and their encryption
  - Stores connection strings securely
  - Handles connection testing and validation

- **SecurityTool**: Handles encryption aspects of the MCP server

## Configuration

- MCP server configured in `.vscode/mcp.json`
- Connection strings managed in `appsettings.json` and a SQLite database
- Supports logging via Serilog (configured in `appsettings.json`)

## Development Guidelines

- Use Microsoft.Data.SqlClient instead of System.Data.SqlClient
- Use ModelContextProtocol namespaces and attributes for all tools
- Use async/await for all database calls
- Implement proper exception handling and logging
- Always provide descriptive tool names and parameters
- Structure new features as MCP tools with proper attributes
- Add detailed XML documentation to all public methods
- Properly handle timeouts and large result sets
- When adding timestamp support, leverage existing database metadata queries
- Follow existing patterns for error handling and response formatting

## VS Code Integration

- Make sure to test with VS Code's Copilot Agent features
- Ensure tool descriptions are clear and useful for end users
- When prompting for terminal commands, use a semicolon to separate commands
```
