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

## Getting Started

### Prerequisites

- .NET 9.0 SDK

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

## Connection Management

This project includes a robust connection management system that allows you to:

1. **Store and manage multiple database connections** using SQLite as a persistent storage backend
2. **Add, update, and remove connections** programmatically or through the MCP interface
3. **Test connection strings** before saving them
4. **Use connections across different tools** with a unified interface

### Connection Storage

Connections are stored in two places:

1. **SQLite database**: Located in the `Data/connections.db` file, providing persistent storage
2. **JSON file**: Located in the `Data/connections.json` file, providing a human-readable backup

### Connection Management Tools

The project exposes connection management features through:

1. **ConnectionManager class**: For use within the application
2. **ConnectionManagerTool**: MCP tool for client applications to manage connections

### Using Connection Management

#### Starting with Sample Connections

Run the included script to start the server with sample connections:

```
./start-mcp-with-connections.ps1
```

#### Managing Connections through MCP

Use the following MCP commands to manage connections:

- **List connections**:

  ```
  connectionManager/list
  ```

- **Add a connection**:

  ```
  connectionManager/add
  Params: {
    "Name": "MyConnection",
    "ConnectionString": "Server=myserver;Database=mydb;Trusted_Connection=True;",
    "Description": "Optional description"
  }
  ```

- **Update a connection**:

  ```
  connectionManager/update
  Params: {
    "Name": "MyConnection",
    "ConnectionString": "Updated connection string",
    "Description": "Updated description"
  }
  ```

- **Remove a connection**:

  ```
  connectionManager/remove
  Params: {
    "Name": "MyConnection"
  }
  ```

- **Test a connection string**:
  ```
  connectionManager/test
  Params: {
    "ConnectionString": "Server=myserver;Database=mydb;Trusted_Connection=True;"
  }
  ```

#### Testing Connection Management

Use the included test script to verify connection management functionality:

```
./test-connection-manager.ps1
```

This script demonstrates the full lifecycle of connection management including adding, testing, updating, and removing connections.

## Database Metadata Features

This MCP server provides comprehensive metadata retrieval functionality for SQL Server databases, allowing Copilot to understand and work with your database schemas effectively.

### Table Metadata

You can retrieve detailed information about database tables using the `GetTableMetadata` tool:

```
#GetTableMetadata connectionName="YourConnection" schema="dbo"
```

This provides complete table metadata including:

- Table names and schemas
- Column details (name, type, nullability, constraints)
- Primary keys
- Foreign key relationships

### View Metadata (New!)

As of May 2025, the MCP server now supports retrieving metadata from SQL Server views in addition to tables. This allows Copilot to understand the structure of views and use them in queries.

You can retrieve metadata for both tables and views using the new `GetDatabaseObjectsMetadata` tool:

```
#GetDatabaseObjectsMetadata connectionName="YourConnection" schema="dbo" includeViews=true
```

You can also filter by specific object types using the objectType parameter:

```
#GetDatabaseObjectsMetadata connectionName="YourConnection" schema="dbo" objectType="VIEW"
```

Valid objectType values are:

- "TABLE" or "TABLES" - Returns only base tables
- "VIEW" or "VIEWS" - Returns only views
- "ALL" (default) - Returns all database objects

The view metadata includes:

- View names and schemas
- Column details
- SQL definition of the view
- Relationships to base tables (where applicable)

By setting `includeViews=false`, you can retrieve only table metadata, similar to the original `GetTableMetadata` tool.

### Example Usage

Here's an example of retrieving both tables and views from a database:

```
User: Show me all the database objects in my PROTO database, including views

Copilot: I'll retrieve the metadata for all database objects in the PROTO database, including both tables and views.

[Tool used: GetDatabaseObjectsMetadata with connectionName="PROTO" includeViews=true]

Results:
The PROTO database contains 18 tables and 3 views across multiple schemas:

Tables:
1. dbo.cross_db_databases - Database information tracking
2. dbo.cross_db_objects - Object metadata storage
// ...existing tables...

Views:
1. dbo.vw_AllReferences - Consolidated view of all cross-database references
2. dbo.vw_DatabaseObjects - Unified view of all database objects
3. Reporting.vw_ETLEvents - Combined view of ETL events from multiple sources
```

A testing script is available in `test-view-metadata.ps1` that demonstrates how to use the new view metadata functionality.

## Using with VS Code Copilot Agent

1. Configure your Copilot Agent to use this MCP server by referencing the `mcp.json` file
2. Initialize the connection by calling the `Initialize` method
3. Execute SQL queries using the `ExecuteQuery` method
4. Retrieve database metadata with the `GetTableMetadata` method

### Copilot Chat Examples

#### Using GetTableMetadata in Copilot Chat

You can ask Copilot to retrieve schema information about database tables:

```
User: Show me all the tables in my PROTO database

Copilot: I'll retrieve the metadata for all tables in the PROTO database.

[Tool used: GetTableMetadata with connectionName="PROTO"]

Results:
The PROTO database contains 18 tables across multiple schemas:

1. dbo.cross_db_databases - Database information tracking
2. dbo.cross_db_objects - Object metadata storage
3. dbo.cross_db_reference - Cross-database reference information
4. dbo.cross_db_server_databases - Server and database connections
5. dbo.dtsx_info - SSIS package information
6. dbo.T - Simple table with various data types
7. ETL.AnsibleEvents - Ansible event tracking
8. ETL.KafkaEvents - Kafka event tracking
9. Holler.ApiConfigurations - API connection settings
10. Holler.DatasetConfigurations - Dataset query definitions
11. Holler.ExecutionResults - API execution results
12. Holler.ExecutionRuns - API execution tracking
13. SSIS.DTSX_Attributes - SSIS attribute information
14. SSIS.DTSX_Elements - SSIS element data
15. SSIS.DTSX_Mapper - SSIS mapping information
16. SSIS.DTSX_Variables - SSIS variable tracking
17. SSIS.VarConnect - Connection variables
18. temp.Department - Department information
```

#### Using ExecuteQuery in Copilot Chat

You can ask Copilot to run SQL queries against your database:

```
User: How many events are in the ETL.KafkaEvents table?

Copilot: I'll run a query to count the events in the ETL.KafkaEvents table.

[Tool used: ExecuteQuery with connectionName="PROTO"]
Query: SELECT COUNT(*) AS EventCount FROM ETL.KafkaEvents

Results:
[
  {
    "EventCount": 143
  }
]

There are 143 events stored in the ETL.KafkaEvents table.

User: Show me the most recent 5 events

Copilot: I'll retrieve the 5 most recent events from the KafkaEvents table.

[Tool used: ExecuteQuery with connectionName="PROTO"]
Query: SELECT TOP 5 Id, Topic, [Key], [Timestamp], Partition, Offset FROM ETL.KafkaEvents ORDER BY [Timestamp] DESC

Results:
[
  {
    "Id": 143,
    "Topic": "inventory-changes",
    "Key": "product-5429",
    "Timestamp": "2025-05-13T14:23:45.000Z",
    "Partition": 2,
    "Offset": 8796
  },
  {
    "Id": 142,
    "Topic": "inventory-changes",
    "Key": "product-1853",
    "Timestamp": "2025-05-13T14:22:18.000Z",
    "Partition": 1,
    "Offset": 7654
  },
  ...
]
```

### Advanced Copilot Chat Examples

#### Exploring Database Relationships

```
User: Show me the relationships between tables in the Holler schema

Copilot: I'll get the foreign key relationships in the Holler schema for you.

[Tool used: GetTableMetadata with connectionName="PROTO", schema="Holler"]

Results:
I found the following relationships in the Holler schema:

1. ExecutionResults.ExecutionId → ExecutionRuns.ExecutionId
2. ExecutionRuns.ApiConfigurationId → ApiConfigurations.Id
3. ExecutionRuns.DatasetConfigurationId → DatasetConfigurations.Id

This shows that:
- ExecutionResults are linked to ExecutionRuns
- Each ExecutionRun is associated with an ApiConfiguration
- Each ExecutionRun may optionally be associated with a DatasetConfiguration
```

#### Advanced SQL Queries

```
User: Find the top 3 most used Topics in the KafkaEvents table with their event counts

Copilot: I'll query the KafkaEvents table to find the most frequently used Topics.

[Tool used: ExecuteQuery with connectionName="PROTO"]
Query:
SELECT TOP 3
    Topic,
    COUNT(*) AS EventCount
FROM ETL.KafkaEvents
GROUP BY Topic
ORDER BY EventCount DESC

Results:
[
  {
    "Topic": "inventory-changes",
    "EventCount": 57
  },
  {
    "Topic": "user-activity",
    "EventCount": 42
  },
  {
    "Topic": "order-processing",
    "EventCount": 28
  }
]

The top 3 most used Topics in the KafkaEvents table are:
1. "inventory-changes" with 57 events
2. "user-activity" with 42 events
3. "order-processing" with 28 events
```

#### Querying Across Tables

```
User: Get the execution details and results for the most recent API call

Copilot: I'll retrieve the most recent API execution along with its results.

[Tool used: ExecuteQuery with connectionName="PROTO"]
Query:
SELECT TOP 1
    er.ExecutionId,
    er.RunTimestamp,
    ac.Name AS ApiName,
    dc.Name AS DatasetName,
    er.ParallelCount,
    COUNT(res.ResultId) AS ResultCount,
    MIN(res.StartTimeUtc) AS FirstCallTime,
    MAX(res.EndTimeUtc) AS LastCallTime,
    SUM(CASE WHEN res.StatusCode BETWEEN 200 AND 299 THEN 1 ELSE 0 END) AS SuccessfulCalls,
    SUM(CASE WHEN res.StatusCode NOT BETWEEN 200 AND 299 THEN 1 ELSE 0 END) AS FailedCalls
FROM Holler.ExecutionRuns er
JOIN Holler.ApiConfigurations ac ON er.ApiConfigurationId = ac.Id
LEFT JOIN Holler.DatasetConfigurations dc ON er.DatasetConfigurationId = dc.Id
LEFT JOIN Holler.ExecutionResults res ON er.ExecutionId = res.ExecutionId
GROUP BY er.ExecutionId, er.RunTimestamp, ac.Name, dc.Name, er.ParallelCount
ORDER BY er.RunTimestamp DESC

Results:
[
  {
    "ExecutionId": "8f7e6d5c-4b3a-2c1d-9e8f-7a6b5c4d3e2f",
    "RunTimestamp": "2025-05-14T08:45:12.340Z",
    "ApiName": "InventoryService",
    "DatasetName": "ProductUpdates",
    "ParallelCount": 5,
    "ResultCount": 8,
    "FirstCallTime": "2025-05-14T08:45:13.450Z",
    "LastCallTime": "2025-05-14T08:45:19.230Z",
    "SuccessfulCalls": 7,
    "FailedCalls": 1
  }
]
```

These examples demonstrate how Copilot can help you explore relationships between tables, analyze data patterns, and perform complex queries across multiple tables in your SQL Server databases.

## Available Tools

- `echo`: Echoes a message back to the client
- `f1Echo`: Alternative echo function (F1 version)
- `initialize`: Initializes the SQL Server connection
- `executeQuery`: Executes a SQL query and returns results as JSON
- `getTableMetadata`: Retrieves metadata about database tables, columns, keys, etc. You can filter by schema or get all schemas.
- `getDatabaseObjectsMetadata`: Retrieves metadata about both tables and views, including schemas, columns, and relationships.

### Copilot Tool Usage

When used with GitHub Copilot in Visual Studio Code, the tools are available as functions that can be invoked in chat:

```javascript
// Tool invocation pattern
f1e_Initialize({ connectionName: "DefaultConnection" });
f1e_ExecuteQuery({
  connectionName: "PROTO",
  query: "SELECT TOP 5 * FROM dbo.T",
});
f1e_GetTableMetadata({ connectionName: "PROTO", schema: "dbo" });
f1e_GetDatabaseObjectsMetadata({
  connectionName: "PROTO",
  schema: "dbo",
  includeViews: true,
});
```

You'll typically interact with these tools by asking questions in natural language, and Copilot will handle the function calls automatically.

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

## Troubleshooting Copilot Tools

When using the MCP tools with GitHub Copilot, you might encounter some common issues:

### Connection Issues

If Copilot cannot connect to your database:

```
User: Show me all tables in the AdventureWorksLT database

Copilot: I'll try to retrieve the metadata from the AdventureWorksLT database.

[Tool used: GetTableMetadata with connectionName="AdventureWorksLT"]

Error: Connection string 'AdventureWorksLT' was not found in the configuration.
```

**Solution**: Make sure the connection string is properly defined in your `appsettings.json` file.

### Query Syntax Errors

If your SQL query has syntax errors:

```
User: Run this query: SELECT FROM Products WHERE Price < 100

Copilot: I'll execute this query for you.

[Tool used: ExecuteQuery with connectionName="PROTO"]
Query: SELECT FROM Products WHERE Price < 100

Error: Incorrect syntax near the keyword 'FROM'.
```

**Solution**: Check that your SQL query follows proper syntax. In this case, you need to specify columns after SELECT.

### Connection Name Mismatch

When the connection name doesn't match exactly what's in your configuration:

```
User: Get tables from PROTO database

Copilot: I'll get the tables from the PROTO database.

[Tool used: GetTableMetadata with connectionName="PROTO"]

Error: Connection string 'PROTO' was not found in the configuration.
```

**Solution**: Use the exact connection string name as defined in your configuration. In this case, use "PROTO" instead of "PROTO".

### Table or Schema Not Found

When querying non-existent objects:

```
User: Show me data from the Customers table

Copilot: I'll query the Customers table for you.

[Tool used: ExecuteQuery with connectionName="PROTO"]
Query: SELECT TOP 10 * FROM Customers

Error: Invalid object name 'Customers'.
```

**Solution**: First use GetTableMetadata to see which tables are available, then query the correct table name with the proper schema prefix if needed, e.g., "dbo.Customers".

### Performance Tips

- Be specific about which schema you're interested in when using GetTableMetadata
- Limit the number of rows returned in queries with TOP or LIMIT clauses
- Consider adding WHERE clauses to filter data and improve query performance
- For large databases, query only the columns you need instead of using SELECT \*

## Logs

Logs are stored in the `Logs` directory with daily rolling files. The logging configuration can be customized in the `appsettings.json` file under the `Serilog` section.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
