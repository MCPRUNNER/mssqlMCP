# SQL Server MCP (Model Context Protocol) Server

This is a Model Context Protocol (MCP) server that connects to SQL Server databases, designed to be used by Visual Studio Code as a Copilot Agent.

## Overview

This project implements an MCP server for SQL Server database connectivity, enabling VS Code and Copilot to interact with SQL databases via the Model Context Protocol.

Features include:

- SQL query execution
- Database metadata retrieval (tables, columns, primary/foreign keys)
- Echo capabilities for testing
- Async/await for all database operations
- Robust logging with Serilog
- Clean architecture with separation of concerns
- Dependency injection for testable components
- Strongly-typed models for database metadata
- **User-friendly error handling for database connection issues**

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server instance (local or remote)
- Visual Studio Code with Copilot extension

## Project Architecture

This project follows a clean architecture approach with separation of concerns:

### Folders Structure

- **Models**: Contains entity models for database metadata (TableInfo, ColumnInfo, ForeignKeyInfo)
- **Interfaces**: Contains interfaces for services (IDatabaseMetadataProvider, IConnectionStringProvider, ISqlServerTools)
- **Services**: Contains service implementations (DatabaseMetadataProvider)
- **Configuration**: Contains configuration-related classes (ConnectionStringProvider)
- **Tools**: Contains MCP tool implementations (SqlServerTools)
- **Extensions**: Contains extension methods for service registration

### Key Components

- **DatabaseMetadataProvider**: Service for retrieving database schema information
- **ConnectionStringProvider**: Service for managing database connection strings
- **SqlServerTools**: MCP tools implementation for SQL Server operations
- **ServiceCollectionExtensions**: Extension methods for registering services with dependency injection

### Installation

1. Clone this repository
2. Navigate to the project directory
3. Restore NuGet packages:
   ```powershell
   dotnet restore
   ```
4. Build the project:
   ```powershell
   dotnet build
   ```
5. Update the connection strings in `appsettings.json` for your database environment

### Configuration

The application uses `appsettings.json` for configuration. Update the connection strings with your SQL Server details:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30;",
  "AdventureWorks": "Server=localhost;Database=AdventureWorks;Trusted_Connection=True;TrustServerCertificate=True;Connection Timeout=30;",
  "ExampleWithSqlAuth": "Server=myserver;Database=mydatabase;User Id=myuser;Password=mypassword;TrustServerCertificate=True;Connection Timeout=30;"
}
```

Connection string parameters:

- `Server`: SQL Server instance name (hostname or IP address)
- `Database`: Name of the database to connect to
- `Trusted_Connection=True`: Use Windows Authentication
- `User Id` and `Password`: SQL Server Authentication credentials
- `TrustServerCertificate=True`: Skip certificate validation
- `Connection Timeout=30`: Time in seconds to wait for connection to open

### Running the Server

Run the server with:

```powershell
dotnet run
```

The server will start and listen on `http://localhost:3001` by default.

## Using as a Copilot Agent

To use this server with VS Code Copilot, set up `mcp.json`:

```json
{
  "name": "sqlserver-mcp",
  "version": "1.0.0",
  "description": "SQL Server MCP Server for VSCode",
  "endpoint": "http://localhost:3001",
  "authToken": "",
  "tools": ["Initialize", "Echo", "F1Echo", "ExecuteQuery", "GetTableMetadata"]
}
```

## Error Handling

The server implements robust error handling for common SQL Server issues:

### Connection Issues

- Authentication failures (error codes 4060, 18456, 18452)
- Server connectivity problems (error codes 2, 53)
- Database not found errors (error code 4064)
- Query timeouts and cancellations

### Friendly Error Messages

All database errors are caught and converted to user-friendly error messages in JSON format:

```json
{ "error": "Cannot access database or connection. Authentication failed." }
```

### Troubleshooting Connection Issues

If you encounter connection problems:

1. **Authentication failures**: Verify your username, password, or Windows authentication permissions
2. **Server not found**: Check server name and network connectivity
3. **Database not found**: Verify the database name exists on the server
4. **Timeouts**: Ensure the database server is running and not overloaded

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.
