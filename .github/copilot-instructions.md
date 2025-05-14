<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# mssqlMCP Copilot Instructions

This is a Model Context Protocol (MCP) Server project that connects to SQL Server databases, and is designed to be used by vscode as a Copilot Agent.

# You can find more info and examples at

- https://modelcontextprotocol.io/llms-full.txt
- https://github.com/modelcontextprotocol/csharp-sdk

This project:

- Implements the Model Context Protocol for SQL Server database connectivity
- uses stdio for communication
- Allows VS Code and Copilot to connect to SQL databases via MCP
- Supports fetching metadata
- Uses C# for the implementation
- To be used as a Copilot Agent MCP

# Additional Instructions

- Use current vscode workspace
- Use Microsoft.Data.SqlClient instead of System.Data.SqlClient
- Use ModelContextProtocol
- Use async/await for all database calls
- Use a logging framework for logging
- Use a configuration file for connection strings
- Add all changes to current workspace
- Use a .gitignore file to ignore build files
- Create example mcp.json for running with Copilot Agent in VS Code
- Make sure to include an initialize endpoint.
- Use best practices
- when prompting for terminal commands, use a semicolon to separate commands
