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

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- SQL Server instance (local or remote)
- Visual Studio Code with Copilot extension

### Installation

1. Clone this repository
2. Navigate to the project directory
3. Restore NuGet packages:
   ```
   dotnet restore
   ```
4. Build the project:
   ```
   dotnet build
   ```
5. Update connection strings in `appsettings.json` to match your SQL Server environment

### Running the Server

Start the MCP server with:

```
dotnet run
```

The server will start on http://localhost:3001 by default.

## Configuration

Connection strings are stored in `appsettings.json`. The default configuration includes:

- `DefaultConnection`: Points to the master database
- `AdventureWorks`: Points to the AdventureWorks sample database

You can modify or add new connection strings as needed.

## Using with VS Code Copilot Agent

1. Configure your Copilot Agent to use this MCP server by referencing the `mcp.json` file
2. Initialize the connection by calling the `Initialize` method
3. Execute SQL queries using the `ExecuteQuery` method
4. Retrieve database metadata with the `GetTableMetadata` method

## Available Tools

- `echo`: Echoes a message back to the client
- `f1Echo`: Alternative echo function (F1 version)
- `initialize`: Initializes the SQL Server connection
- `executeQuery`: Executes a SQL query and returns results as JSON
- `getTableMetadata`: Retrieves metadata about database tables, columns, keys, etc. You can filter by schema or get all schemas.

### Schema Filtering

The `getTableMetadata` tool supports schema filtering, which allows you to retrieve metadata for tables in a specific schema:

#### Usage Examples

```csharp
// Get all database metadata (all schemas)
var metadata = await GetTableMetadata();

// Get metadata for a specific connection (all schemas)
var awMetadata = await GetTableMetadata("AdventureWorks");

// Get metadata for tables in a specific schema
var dboSchemaMetadata = await GetTableMetadata("DefaultConnection", "dbo");

// Get metadata for a specific schema in a specific database
var awSalesSchema = await GetTableMetadata("AdventureWorks", "Sales");
```

This feature is particularly useful when working with large databases that have many schemas, allowing you to focus on just the relevant parts of the database structure.

## Example Usage

### Initializing Connection

```csharp
// Initialize the default connection
var result = await Initialize();

// Or specify a specific connection
var adventureWorksResult = await Initialize("AdventureWorks");
```

### Executing SQL Queries

```csharp
// Basic SELECT query
var users = await ExecuteQuery("SELECT TOP 10 * FROM Users");

// Query with parameters (handle SQL injection carefully)
var productQuery = "SELECT * FROM Products WHERE Category = 'Electronics' AND Price < 500";
var products = await ExecuteQuery(productQuery);

// Query with specific connection
var salesData = await ExecuteQuery("SELECT * FROM Sales.SalesOrderHeader", "AdventureWorks");
```

### Getting Database Metadata

```csharp
// Get all database metadata (all schemas)
var metadata = await GetTableMetadata();

// Get metadata for a specific connection (all schemas)
var awMetadata = await GetTableMetadata("AdventureWorks");

// Get metadata for tables in a specific schema
var dboSchemaMetadata = await GetTableMetadata("DefaultConnection", "dbo");

// Get metadata for a specific schema in a specific database
var awSalesSchema = await GetTableMetadata("AdventureWorks", "Sales");
```

### Common SQL Query Examples

```sql
-- Get all tables in the database
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME

-- Get column information for a specific table
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'YourTableName'
ORDER BY ORDINAL_POSITION

-- Get primary key information
SELECT TC.TABLE_SCHEMA, TC.TABLE_NAME, KCU.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KCU
    ON TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME
WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
ORDER BY TC.TABLE_SCHEMA, TC.TABLE_NAME
```

## Logs

Logs are stored in the `Logs` directory with daily rolling files. The logging configuration can be customized in the `appsettings.json` file under the `Serilog` section.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
