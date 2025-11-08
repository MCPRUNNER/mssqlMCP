# SQL Server MCP (Model Context Protocol) Server

This is a Model Context Protocol (MCP) server that connects to one or more SQL Server databases; designed to be used by Visual Studio Code as a Copilot Agent.

## 🚨 Recent Updates (November 2025)

**Version 1.0.9.5** - Documentation Standardization & Tool Naming Consistency

**Important**: This version includes comprehensive documentation updates and tool naming standardization. All MCP tools now use consistent `mssql_` prefixed names. Key improvements include:

- **Standardized Tool Names**: All tools now use `mssql_` prefix (e.g., `mssql_execute_query`, `mssql_get_table_metadata`)
- **Updated Documentation**: All documentation files synchronized with actual implementation
- **Enhanced Examples**: C# client examples and console applications updated with correct tool names
- **Test Suite Updates**: All tests updated and passing with new tool naming conventions
- **API Reference**: Complete API documentation refresh with accurate tool specifications

**Migration Note**: While tool functionality remains unchanged, documentation and examples have been updated for consistency. See the [RELEASE_NOTES.md](./RELEASE_NOTES.md) for complete details.

---

## 🚨 Security Updates (November 2025)

**Important**: This project received significant security enhancements in November 2025. Please review the [Security Update Documentation](./Documentation/MERGE_UPDATE_NOVEMBER_2025.md) for:

- Enhanced multi-tier API key authentication system
- Role-based access control (master, admin, user keys)
- Connection-level security restrictions
- Encrypted API key storage
- Comprehensive test coverage for security features

**Migration Required**: Existing installations must configure authentication. See the [Migration Guide](./Documentation/MERGE_UPDATE_NOVEMBER_2025.md#migration-guide) for details.

## Overview

This project implements an MCP server for SQL Server database connectivity, enabling VS Code and Copilot to interact with SQL databases via the Model Context Protocol.

Features include:

- SQL query execution
- Database metadata retrieval (tables, views, stored procedures, functions)
- Detailed schema information including primary/foreign keys
- Multi-key API authentication system with master and user-specific keys
- Connection string encryption with AES-256
- Key rotation and security management
- API key usage tracking and analytics
- Async/await for all database operations
- Robust logging with Serilog
- Clean architecture with separation of concerns
- Dependency injection for testable components
- Strongly-typed models for database metadata

## How This Works

See [How This Works](./Documentation/HowItWorks.md) for an overview of communication between Copilot, the LLM and this MSSQL Model Context Protocol Server.

## API Endpoints

This server provides three types of MCP endpoints:

1. **Standard MCP Endpoints**: Accessible via the ModelContextProtocol.AspNetCore library
2. **JSON-RPC 2.0 Endpoints**: Following the MCP JSON-RPC 2.0 protocol
   - `POST /mcp`: Handles `tools/list` and `tools/call` methods
3. **Direct REST API Endpoints**: For simpler HTTP access to specific database operations

See the following documentation:

- [MCP JSON-RPC Documentation](./Documentation/McpJsonRpc.md) for details on the JSON-RPC endpoints
- [Multi-Key Authentication System](./Documentation/MultiKeyAuthentication.md) for details on the authentication system
- [Using MCP with C#](./Documentation/UsingMcpWithCSharp.md) for C# client examples
- [Testing MCP JSON-RPC Endpoints with PowerShell](./Documentation/McpJsonRpcPowershellTesting.md) for comprehensive PowerShell testing

## Getting Started

### Prerequisites

- .NET 9.0 SDK

### Runtime Variables

Set mandatory variables prior to starting MCP Server.
| Variable Name | Mandatory | Default Value | Recommended Values (Expected Format/Type) | Description |
| :-----------------------------------| :------------------------| :--------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| `MSSQL_MCP_KEY` | Yes | The `Start-MCP-Encrypted.ps1` will generate a secure random key if this variable is unset. To generate a secure encryption key you can use `Generate-MCP-Key.ps1`, to change the current key use `Rotate-Encryption-Key.ps1` | A strong, cryptographically secure random string (e.g., 32 bytes, Base64 encoded). | The master encryption key used for AES-256 encryption of connection strings stored in the `connections.db` SQLite database. |
| `MSSQL_MCP_API_KEY` | Yes | This is the master API key (Authorization Bearer token). If not set, API key authentication might be disabled or fall back to appsettings.json if configured there. The `Set-Api-Key.ps1` script generates one. | A strong, cryptographically secure random string. | The master API key required for administrative access to the MCP server and for creating additional user API keys. User API keys are stored in the SQLite database and managed through the API. |
| `MSSQL_MCP_DATA` | No | Data (A Data subdirectory in the application's root directory) | A valid file system path to a directory. | Overrides the default directory location for storing application data, most notably the `connections.db` SQLite database file. |

### TL;DR The Quick Setup Doc

[QUICK_INSTALL](./Documentation/QUICK_INSTALL.md)

## Project Architecture

This project follows a clean architecture approach with separation of concerns:

See the [full architecural documentation](./Documentation/Architecture.md) for diagrams and detailed process flow.

### Folder Structure

- **Models/**: Entity models for database metadata (TableInfo, ColumnInfo, ForeignKeyInfo, etc.)
- **Interfaces/**: Service interfaces (IDatabaseMetadataProvider, IConnectionStringProvider, ISqlServerTools)
- **Services/**: Service implementations (DatabaseMetadataProvider, ConnectionManager, ConnectionRepository)
- **Configuration/**: Configuration-related classes (ConnectionStringProvider)
- **Tools/**: MCP tool implementations (SqlServerTools, ConnectionManagerTool, SecurityTool)
- **Extensions/**: Extension methods for service registration (ServiceCollectionExtensions, ApiSecurityExtensions)
- **Middleware/**: Middleware components (ApiKeyAuthMiddleware)
- **Scripts/**: Utility and management scripts (Start-MCP-Encrypted.ps1, Rotate-Encryption-Key.ps1, Migrate-To-Encrypted.ps1, Set-Api-Key.ps1, Assess-Connection-Security.ps1, Test-Connection.ps1, Test-Security-Features.ps1, Verify-Encryption-Status.ps1, mcp.json)
- **Documentation/**: Architecture, security, usage, and API documentation
- **Examples/**: Example scripts and usage (initialize-mcp.js, test-mcp-curl.sh, test-mcp-powershell.ps1)
- **Logs/**: Log files (daily rolling logs)
- **Tests/**: Test code
- **Data/**: SQLite database for connection storage (connections.db)
- **appsettings.json / appsettings.Development.json**: Application configuration
- **Dockerfile**: Containerization support
- **mssqlMCP.sln / mssqlMCP.csproj**: .NET solution and project files
- **CopilotAgent.md / Overview.md**: Additional documentation

### Key Components

- **DatabaseMetadataProvider**: Service for retrieving database schema information
- **ConnectionStringProvider**: Service for managing database connection strings
- **ConnectionManager**: Manages connection storage and retrieval
- **ApiKeyManager**: Manages API key lifecycle and validation
- **ApiKeyRepository**: Stores and retrieves encrypted API keys
- **SqlServerTools**: MCP tools implementation for SQL Server operations
- **ConnectionManagerTool**: MCP tool for managing connections
- **SecurityTool**: MCP tool for security operations (encryption, key rotation, etc.)
- **ApiKeyManagementTool**: MCP tool for managing API keys
- **ServiceCollectionExtensions**: Extension methods for registering services with dependency injection
- **ApiKeyAuthMiddleware**: Middleware for multi-key API authentication
- SQL Server instance (local or remote)
- Visual Studio Code with Copilot extension

### Docker Image

To run via a pre-built docker image download and run via hub.docker.com [mcprunner/mssqlmcp](https://hub.docker.com/r/mcprunner/mssqlmcp)

### Installation and Setup

See [QUICK_INSTALL](./Documentation/QUICK_INSTALL.md)

## Connection Management

This project includes a robust connection management system that allows you to:

1. **Store and manage multiple database connections** using SQLite as a persistent storage backend
2. **Add, update, and remove connections** programmatically or through the MCP interface
3. **Test connection strings** before saving them
4. **Use connections across different tools** with a unified interface

### CORS Configuration

The MCP server includes built-in CORS support that can be configured through `appsettings.json`. This is particularly useful when integrating with web-based tools like Open WebUI.

CORS settings can be configured in the `Cors` section of `appsettings.json`:

```json
{
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "http://localhost:8080"],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH"],
    "AllowedHeaders": [
      "Content-Type",
      "Authorization",
      "X-API-Key",
      "X-Requested-With",
      "Accept",
      "Origin",
      "Access-Control-Request-Method",
      "Access-Control-Request-Headers"
    ],
    "AllowCredentials": true,
    "ExposedHeaders": ["Content-Disposition"]
  }
}
```

- **AllowedOrigins**: List of origins that can access the API
- **AllowedMethods**: HTTP methods that are allowed
- **AllowedHeaders**: HTTP headers that can be used
- **AllowCredentials**: Whether to allow credentials (cookies, authorization headers)
- **ExposedHeaders**: Headers that browsers are allowed to access

For production environments, make sure to:

1. Replace localhost URLs with your actual domain
2. Only include necessary origins, methods, and headers
3. Consider setting AllowCredentials to false if cross-origin credentials aren't needed

### Connection Storage

Connections are stored in a Sqlite database:

- **SQLite database**: Located in the `Data/connections.db` file, providing persistent storage
- **SQLite database override location**: Override the Data directory location with Environment Variable `MSSQL_MCP_DATA`

### Connection Management Tools

The project exposes connection management features through:

1. **ConnectionManager class**: For use within the application
2. **ConnectionManagerTool**: MCP tool for client applications to manage connections
3. **Connection Security Tools**: For encrypting and managing connection string security including key rotation and encryption status verification

### Using Connection Management

#### Starting with Encryption Enabled

See [QUICK_INSTALL](./Documentation/QUICK_INSTALL.md)

This script automatically generates a cryptographically secure random key using System.Security.Cryptography.RandomNumberGenerator, sets it as an environment variable, and starts the server with encryption enabled. You can also provide your own key:

```powershell
   $env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
   $env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
./Scripts/Start-MCP-Encrypted.ps1
```

For production environments, you should store the key securely and set the environment variable externally using a secrets management solution.

#### Testing Connection Management

Use the included test script to verify connection management functionality:

```
./Scripts/test-connection-manager.ps1
```

This script demonstrates the full lifecycle of connection management including adding, testing, updating, and removing connections.

## Database Metadata Features

This MCP server provides comprehensive metadata retrieval functionality for SQL Server databases, allowing Copilot to understand and work with your database schemas effectively.

### Table Metadata

You can retrieve detailed information about database tables using the `mssql_get_table_metadata` tool:

```
#mssql_get_table_metadata connectionName="YourConnection" schema="dbo"
```

This provides complete table metadata including:

- Table names and schemas
- Column details (name, type, nullability, constraints)
- Primary keys
- Foreign key relationships

### View Metadata (New!)

As of May 2025, the MCP server now supports retrieving metadata from SQL Server views in addition to tables. This allows Copilot to understand the structure of views and use them in queries.

You can retrieve metadata for both tables and views using the new `mssql_get_database_objects_metadata` tool:

```
#mssql_get_database_objects_metadata connectionName="YourConnection" schema="dbo" includeViews=true
```

### Stored Procedure Metadata (New!)

The MCP server now also supports retrieving metadata from SQL Server stored procedures, including procedure definitions and parameters. This allows Copilot to understand and work with stored procedures in your database.

You can retrieve stored procedure metadata using the `mssql_get_database_objects_metadata` tool with the `objectType` parameter:

```
#mssql_get_database_objects_metadata connectionName="YourConnection" objectType=PROCEDURE
```

This provides detailed procedure metadata including:

- Procedure names and schemas
- Parameter details (name, type, direction)
- SQL definition (when not encrypted)

### Filtering Database Objects

You can filter database objects by type using the `objectType` parameter:

```
# Get only tables
#mssql_get_database_objects_metadata connectionName="YourConnection" objectType=TABLE

# Get only views
#mssql_get_database_objects_metadata connectionName="YourConnection" objectType=VIEW

# Get only stored procedures
#mssql_get_database_objects_metadata connectionName="YourConnection" objectType=PROCEDURE

# Get all database objects
#mssql_get_database_objects_metadata connectionName="YourConnection" objectType=ALL
```

You can also filter by schema:

```
# Get objects from a specific schema
#mssql_get_database_objects_metadata connectionName="YourConnection" schema="dbo" objectType=ALL
```

You can also filter by specific object types using the objectType parameter:

```
#mssql_get_database_objects_metadata connectionName="YourConnection" schema="dbo" objectType="VIEW"
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

By setting `includeViews=false`, you can retrieve only table metadata, similar to the original `mssql_get_table_metadata` tool.

### Example Usage

Here's an example of retrieving both tables and views from a database:

```
User: Show me all the database objects in my AdventureWorks2022 database, including views

Copilot: I'll retrieve the metadata for all database objects in the AdventureWorks2022 database, including both tables and views.

[Tool used: mssql_get_database_objects_metadata with connectionName="AdventureWorks2022" includeViews=true]

Results:
The AdventureWorks2022 database contains 68 tables and 20 views across multiple schemas:

Tables:
1. Person.Person - Information about customers, employees, and other individuals
2. Production.Product - Products sold by the company
// ...existing tables...

Views:
1. HumanResources.vEmployee - Employee information combining various related tables
2. Sales.vSalesPerson - Sales person information with their sales data
3. Person.vAdditionalContactInfo - Additional contact information for individuals
```

A testing script is available in `Scripts/test-view-metadata.ps1` that demonstrates how to use the new view metadata functionality.

## Using with GitHub Copilot

This MCP server is designed to work seamlessly with GitHub Copilot in VS Code, allowing you to interact with SQL Server databases using natural language.

### Setting Up as a Copilot Agent

1. Start the MCP server using the provided script:

   ```powershell
   # Start with encryption and API security enabled (recommended)
   $env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
   $env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
   ./Scripts/Start-MCP-Encrypted.ps1
   ```

2. Configure Copilot to use this MCP server:

   - Copy the [Scripts\mcp.json](/Scripts/mcp.json) file to your VS Code workspace's `.vscode` folder
   - When prompted, enter the API key that was generated during server startup

3. Start asking Copilot about your SQL databases!

### Example Copilot Interactions

You can ask Copilot questions like:

- "What tables are in my AdventureWorks database?"
- "Show me the schema of the Sales.Customer table"
- "Write a query to find the top 5 customers by total sales"
- "What are the foreign key relationships in the Sales schema?"

Behind the scenes, Copilot will use the MCP tools to:

1. Initialize connection to the database
2. Retrieve metadata about tables and schemas
3. Execute SQL queries based on your questions
4. Format and present the results

For detailed instructions, see the [Copilot Agent Guide](./CopilotAgent.md).

### MCP Tools for Copilot

The following MCP tools are available for Copilot:

1. **mssql_initialize_connection**: Set up the database connection
2. **mssql_execute_query**: Run SQL queries against your database
3. **mssql_get_table_metadata**: Get metadata about database tables
4. **mssql_get_database_objects_metadata**: Get metadata about tables, views, and stored procedures
5. **mssql_get_database_objects_by_type**: Filter database objects by type
6. **mssql_list_connections**: Manage database connections
7. **mssql_add_connection**: Add new database connections
8. **mssql_update_connection**: Update existing connections
9. **mssql_remove_connection**: Remove connections
10. **mssql_test_connection**: Test connection strings
11. **mssql_get_agent_jobs**: Get SQL Server Agent job information
12. **mssql_get_agent_job_details**: Get detailed job information
13. **mssql_get_ssis_catalog_info**: Get SSIS catalog information
14. **mssql_get_azure_devops_info**: Get Azure DevOps analytics
15. **mssql_create_key**: Create API keys for users
16. **mssql_generate_key**: Generate encryption keys
17. **mssql_migrate_connections**: Migrate to encrypted storage
18. **mssql_rotate_key**: Rotate encryption keys

See the [full documentation](./Documentation/EXAMPLE_USAGE.md) for examples and detailed usage information.

### Advanced Copilot Chat Examples

#### Exploring Database Relationships

```
User: Show me the relationships between tables in the Sales schema

Copilot: I'll get the foreign key relationships in the Sales schema for you.

[Tool used: mssql_get_table_metadata with connectionName="AdventureWorks2022", schema="Sales"]

Results:
I found the following relationships in the Sales schema:

1. SalesOrderDetail.SalesOrderID → SalesOrderHeader.SalesOrderID
2. SalesOrderDetail.ProductID → Production.Product.ProductID
3. SalesOrderHeader.CustomerID → Customer.CustomerID
4. SalesOrderHeader.SalesPersonID → SalesPerson.BusinessEntityID
5. SalesOrderHeader.TerritoryID → SalesTerritory.TerritoryID
6. SalesPerson.TerritoryID → SalesTerritory.TerritoryID

This shows that:
- SalesOrderDetail records are linked to SalesOrderHeader records
- SalesOrderDetail also references products from the Production schema
- SalesOrderHeader connects to Customer, SalesPerson, and SalesTerritory
- SalesPerson is associated with a specific SalesTerritory
```

#### Advanced SQL Queries

```
User: Find the top 3 product categories by sales amount in the AdventureWorks2022 database

Copilot: I'll query the AdventureWorks2022 database to find the top product categories by sales.

[Tool used: mssql_execute_query with connectionName="AdventureWorks2022"]
Query:
SELECT TOP 3
    PC.Name AS CategoryName,
    SUM(SOD.LineTotal) AS SalesAmount
FROM Production.ProductCategory PC
JOIN Production.ProductSubcategory PSC ON PC.ProductCategoryID = PSC.ProductCategoryID
JOIN Production.Product P ON PSC.ProductSubcategoryID = P.ProductSubcategoryID
JOIN Sales.SalesOrderDetail SOD ON P.ProductID = SOD.ProductID
GROUP BY PC.Name
ORDER BY SalesAmount DESC

Results:
[
  {
    "CategoryName": "Bikes",
    "SalesAmount": 28318144.9706
  },
  {
    "CategoryName": "Components",
    "SalesAmount": 1319863.9132
  },
  {
    "CategoryName": "Clothing",
    "SalesAmount": 340253.1788
  }
]

The top 3 product categories by sales in the AdventureWorks2022 database are:
1. "Bikes" with $28,318,144.97 in sales
2. "Components" with $1,319,863.91 in sales
3. "Clothing" with $340,253.18 in sales
```

#### Querying Across Tables

```
User: Get the sales summary for the top 3 sales territories in the most recent year

Copilot: I'll retrieve the sales summary for the top territories from the AdventureWorks2022 database.

[Tool used: mssql_execute_query with connectionName="AdventureWorks2022"]
Query:
WITH RecentYear AS (
    SELECT MAX(YEAR(OrderDate)) AS MostRecentYear
    FROM Sales.SalesOrderHeader
)
SELECT TOP 3
    ST.Name AS Territory,
    ST.CountryRegionCode AS Country,
    SUM(SOH.TotalDue) AS TotalSales,
    COUNT(DISTINCT SOH.CustomerID) AS CustomerCount,
    COUNT(SOH.SalesOrderID) AS OrderCount,
    MIN(SOH.OrderDate) AS FirstOrder,
    MAX(SOH.OrderDate) AS LastOrder
FROM Sales.SalesTerritory ST
JOIN Sales.SalesOrderHeader SOH ON ST.TerritoryID = SOH.TerritoryID
JOIN RecentYear RY ON YEAR(SOH.OrderDate) = RY.MostRecentYear
GROUP BY ST.Name, ST.CountryRegionCode
ORDER BY TotalSales DESC

Results:
[
  {
    "Territory": "North America",
    "Country": "US",
    "TotalSales": 3519153.4118,
    "CustomerCount": 487,
    "OrderCount": 1573,
    "FirstOrder": "2014-01-01T00:00:00.000Z",
    "LastOrder": "2014-12-31T00:00:00.000Z"
  },
  {
    "Territory": "Europe",
    "Country": "GB",
    "TotalSales": 2897231.9182,
    "CustomerCount": 378,
    "OrderCount": 1041,
    "FirstOrder": "2014-01-01T00:00:00.000Z",
    "LastOrder": "2014-12-31T00:00:00.000Z"
  },
  {
    "Territory": "Pacific",
    "Country": "AU",
    "TotalSales": 1457240.8527,
    "CustomerCount": 192,
    "OrderCount": 531,
    "FirstOrder": "2014-01-01T00:00:00.000Z",
    "LastOrder": "2014-12-30T00:00:00.000Z"
  }
]
```

These examples demonstrate how Copilot can help you explore relationships between tables, analyze data patterns, and perform complex queries across multiple tables in your SQL Server databases.

## Available Tools

- `mssql_initialize_connection`: Initializes the SQL Server connection
- `mssql_execute_query`: Executes a SQL query and returns results as JSON
- `mssql_get_table_metadata`: Retrieves metadata about database tables, columns, keys, etc. You can filter by schema or get all schemas.
- `mssql_get_database_objects_metadata`: Retrieves metadata about tables, views, stored procedures and functions, including schemas, columns, and relationships.

### Copilot Tool Usage

When used with GitHub Copilot in Visual Studio Code, the tools are available as functions that can be invoked in chat:

```javascript
// Tool invocation pattern
f1e_mssql_initialize_connection({ connectionName: "DefaultConnection" });
f1e_mssql_execute_query({
  connectionName: "AdventureWorks2022",
  query: "SELECT TOP 5 * FROM Production.Product",
});
f1e_mssql_get_table_metadata({
  connectionName: "AdventureWorks2022",
  schema: "Sales",
});
f1e_mssql_get_database_objects_metadata({
  connectionName: "AdventureWorks2022",
  schema: "Person",
  includeViews: true,
});
```

You'll typically interact with these tools by asking questions in natural language, and Copilot will handle the function calls automatically.

### Schema Filtering

The `mssql_get_table_metadata` tool supports schema filtering, which allows you to retrieve metadata for tables in a specific schema:

#### Usage Examples

```csharp
// Get all database metadata (all schemas)
var metadata = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "DefaultConnection" });

// Get metadata for a specific connection (all schemas)
var awMetadata = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "AdventureWorks" });

// Get metadata for tables in a specific schema
var dboSchemaMetadata = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "DefaultConnection", schema = "dbo" });

// Get metadata for a specific schema in a specific database
var awSalesSchema = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "AdventureWorks", schema = "Sales" });
```

This feature is particularly useful when working with large databases that have many schemas, allowing you to focus on just the relevant parts of the database structure.

## Example Usage

### Initializing Connection

```csharp
// Initialize the default connection
var result = await CallToolAsync("mssql_initialize_connection", new { connectionName = "DefaultConnection" });

// Or specify a specific connection
var adventureWorksResult = await CallToolAsync("mssql_initialize_connection", new { connectionName = "AdventureWorks" });
```

### Executing SQL Queries

```csharp
// Basic SELECT query
var users = await CallToolAsync("mssql_execute_query", new { query = "SELECT TOP 10 * FROM Users", connectionName = "DefaultConnection" });

// Query with parameters (handle SQL injection carefully)
var productQuery = "SELECT * FROM Products WHERE Category = 'Electronics' AND Price < 500";
var products = await CallToolAsync("mssql_execute_query", new { query = productQuery, connectionName = "DefaultConnection" });

// Query with specific connection
var salesData = await CallToolAsync("mssql_execute_query", new { query = "SELECT * FROM Sales.SalesOrderHeader", connectionName = "AdventureWorks" });
```

### Getting Database Metadata

```csharp
// Get all database metadata (all schemas)
var metadata = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "DefaultConnection" });

// Get metadata for a specific connection (all schemas)
var awMetadata = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "AdventureWorks" });

// Get metadata for tables in a specific schema
var dboSchemaMetadata = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "DefaultConnection", schema = "dbo" });

// Get metadata for a specific schema in a specific database
var awSalesSchema = await CallToolAsync("mssql_get_table_metadata", new { connectionName = "AdventureWorks", schema = "Sales" });
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

[Tool used: mssql_get_table_metadata with connectionName="AdventureWorksLT"]

Error: Connection string 'AdventureWorksLT' was not found in the configuration.
```

**Solution**: Make sure the connection string is properly defined via `mssql_list_connections`.

### Query Syntax Errors

If your SQL query has syntax errors:

```
User: Run this query: SELECT FROM Production.Product WHERE ListPrice < 100

Copilot: I'll execute this query for you.

[Tool used: mssql_execute_query with connectionName="AdventureWorks2022"]
Query: SELECT FROM Production.Product WHERE ListPrice < 100

Error: Incorrect syntax near the keyword 'FROM'.
```

**Solution**: Check that your SQL query follows proper syntax. In this case, you need to specify columns after SELECT.

### Connection Name Mismatch

When the connection name doesn't match exactly what's in your configuration:

```
User: Get tables from AdventureWorks database

Copilot: I'll get the tables from the AdventureWorks database.

[Tool used: mssql_get_table_metadata with connectionName="AdventureWorks"]

Error: Connection string 'AdventureWorks' was not found in the configuration.
```

**Solution**: Use the exact connection string name as defined in your configuration. In this case, use "AdventureWorks2022" instead of "AdventureWorks".

### Table or Schema Not Found

When querying non-existent objects:

```
User: Show me data from the Customers table

Copilot: I'll query the Customers table for you.

[Tool used: mssql_execute_query with connectionName="AdventureWorks2022"]
Query: SELECT TOP 10 * FROM Customers

Error: Invalid object name 'Customers'.
```

**Solution**: First use mssql_get_table_metadata to see which tables are available, then query the correct table name with the proper schema prefix if needed, e.g., "Sales.Customer".

### Performance Tips

- Be specific about which schema you're interested in when using mssql_get_table_metadata
- Limit the number of rows returned in queries with TOP or LIMIT clauses
- Consider adding WHERE clauses to filter data and improve query performance
- For large databases, query only the columns you need instead of using SELECT \*

## Security

### Connection String Encryption

To enhance security, connection strings are encrypted with AES-256 encryption before being stored in the SQLite database. The encryption key is derived from the environment variable `MSSQL_MCP_KEY`.

To enable secure connection string encryption:

1. Set the `MSSQL_MCP_KEY` environment variable to a strong random value, or
2. Use the provided `Start-MCP-Encrypted.ps1` script which will generate a cryptographically secure random key for you and start the server with encryption enabled.

```powershell
# Option 1: Set the encryption key manually (should be a strong random value)
$env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
$env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
dotnet run

# Option 2: Use the automated script that handles key generation and server startup
$env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
$env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
./Scripts/Start-MCP-Encrypted.ps1
```

The `Scripts/Start-MCP-Encrypted.ps1` script:

1. Checks if the encryption key is already set
2. Generates a cryptographically secure random key (using System.Security.Cryptography.RandomNumberGenerator) if none exists
3. Sets the environment variable for the current session
4. Displays the key (securely store this for later use)
5. Starts the MCP server with encryption enabled

If the `MSSQL_MCP_KEY` environment variable is not set and you don't use the script, the server will still function but will use a default insecure key. This is not recommended for production use. For production environments, consider using a secure secrets management solution to store and retrieve your encryption key.

### Security Best Practices

1. Always use encrypted connections when possible (e.g., use `Encrypt=True` in your connection strings)
2. Use separate SQL accounts with minimal permissions for different applications
3. Regularly update the `MSSQL_MCP_KEY` environment variable to rotate encryption keys
4. Do not store the encryption key in plaintext files or source code
5. Consider using a secrets manager for the encryption key in production environments

### Starting

For more detailed testing, you can use the individual scripts:

```powershell
# Start the server with encryption enabled
$env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
$env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
./Scripts/Start-MCP-Encrypted.ps1

# Rotate the encryption key
./Scripts/Rotate-Encryption-Key.ps1

# Migrate unencrypted connections to encrypted format
$env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
$env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
./Scripts/Migrate-To-Encrypted.ps1
```

## Logs

Logs are stored in the `Logs` directory with daily rolling files. The logging configuration can be customized in the `appsettings.json` file under the `Serilog` section.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Security Features

The SQL Server MCP server includes robust security features to protect sensitive information such as connection strings.

### Connection String Encryption

All connection strings stored in the SQLite database are encrypted using AES-256 encryption with the following security measures:

- **AES-256 Encryption**: Industry-standard encryption algorithm
- **Environment Variable Key**: The encryption key is derived from the `MSSQL_MCP_KEY` environment variable
- **Unique IV Per Connection**: Each connection string uses a unique Initialization Vector
- **PBKDF2 Key Derivation**: Key is derived with 10,000 iterations for enhanced security

### Starting with Encryption Enabled

To run the server with encryption enabled, use the provided script:

```powershell
$env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
$env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
./Scripts/Start-MCP-Encrypted.ps1
```

This script:

1. Generates a cryptographically secure random key if one is not provided
2. Sets the key as an environment variable for the current session
3. Displays the key for you to save securely
4. Starts the MCP server with encryption enabled

You can also provide your own key:

```powershell
$env:MSSQL_MCP_KEY = "ReplaceWithMyKeyForTheConnectionEncryption"
$env:MSSQL_MCP_API_KEY = "ReplaceWithMyApiKeyForClientAccess"
./Scripts/Start-MCP-Encrypted.ps1
```

For production environments, you should store the key securely and set the environment variable externally using a secrets management solution.

### Key Rotation

The server supports rotating the encryption key to comply with security best practices. To rotate the key:

```powershell
./Scripts/Rotate-Encryption-Key.ps1
```

This script:

1. Generates a new random encryption key (or you can provide your own)
2. Re-encrypts all connection strings using the new key
3. Displays the new key for you to save

After running the key rotation script, you must restart the server with the new key.

### Migrating Unencrypted Connections

To migrate existing unencrypted connection strings to encrypted format:

```powershell
./Scripts/Migrate-To-Encrypted.ps1
```

This script will encrypt any unencrypted connection strings in the database.

### MCP Security Commands

The following MCP commands are available for security operations:

```
# Rotate the encryption key
#security.rotateKey newKey="your-new-key"

# Migrate unencrypted connections to encrypted format
#security.migrateConnectionsToEncrypted

# Generate a secure random key for encryption
#security.generateSecureKey length=32

# Verify encryption status of connections
#security.verifyEncryptionStatus

# Assess connection security
#security.assessConnectionSecurity
```

Each of these commands connects to the functionality in the `SecurityTool.cs` class, which implements the MCP server tools for security operations. These commands follow the standard MCP command syntax with the # prefix.

### Connection Validation and Security Assessment

The SQL Server MCP server includes enhanced security features for validating connections and assessing security status:

#### Connection Validation

When rotating keys or encrypting connections, the system:

- Validates input connections before processing
- Verifies encryption round-trip to ensure data integrity
- Tracks and reports any failures during the process
- Provides detailed logs of operations

#### Security Assessment

Use the included security assessment script to evaluate your connection security:

```powershell
./Scripts/Assess-Connection-Security.ps1
```

This script:

- Analyzes all connections to identify encrypted vs unencrypted connections
- Reports the encryption status of each connection
- Checks if the encryption key is properly set
- Offers to generate a new secure key if needed
- Provides guidance on securing your connections

#### Enhanced Testing

For comprehensive security testing, use:

```powershell
./Test-Security-Features.ps1
```

This enhanced testing script:

- Tests connection creation with encryption
- Verifies connections work after encryption
- Tests key rotation with validation
- Includes connection testing after key rotation
- Uses proper error handling for API communication

For detailed security information, see the [Security Documentation](./Documentation/Security.md).

## API Security

### Multi-Key API Authentication System

The MCP server implements a robust multi-key API authentication system to secure API endpoints. This system supports both a master API key for administrative access and multiple user-specific API keys for regular access.

#### Key Authentication Features

- Master API key for administrative access to all endpoints
- User-specific API keys with optional expiration dates
- API key usage tracking and analytics
- API key revocation capabilities
- Encrypted storage of API keys in SQLite database
- Multiple key types (user, service, admin)

For detailed information about the multi-key authentication system, see [MultiKeyAuthentication.md](./Documentation/MultiKeyAuthentication.md).

#### Setting Up API Key Authentication

1. Generate and set a master API key using the provided script:

```powershell
./Scripts/Set-Api-Key.ps1
```

This script will:

- Generate a cryptographically secure random API key
- Set it as the environment variable `MSSQL_MCP_API_KEY` for the current session
- Display usage examples for making authenticated API calls

2. Additional user-specific API keys can be created using the mssql_create_key endpoint:

```powershell
# PowerShell example to create a new user API key
Invoke-RestMethod -Uri "http://localhost:3001/mcp" -Method Post `
  -Headers @{"Authorization" = "Bearer your-master-api-key"; "Content-Type" = "application/json"} `
  -Body '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "tools/call",
    "params": {
      "name": "mssql_create_key",
      "arguments": {
        "name": "User API Key",
        "userId": "user123",
        "keyType": "user",
        "expirationDate": "2026-06-29T00:00:00Z"
      }
    }
  }'
```

#### API Key Configuration

The master API key can be configured in different ways:

1. **Environment Variable**: Set `MSSQL_MCP_API_KEY` environment variable

   ```powershell
   $env:MSSQL_MCP_API_KEY = "your-secure-master-api-key"
   ```

2. **Application Settings**: Configure in `appsettings.json` under the ApiSecurity section:
   ```json
   "ApiSecurity": {
     "HeaderName": "X-API-Key",
     "ApiKey": "your-secure-master-api-key"
   }
   ```

User-specific API keys are managed through the API endpoints and stored securely in the SQLite database.

#### Making Authenticated Requests

When API key authentication is enabled, all HTTP requests to the server must include either the master API key or a valid user API key as a Bearer token in the Authorization header:

```powershell
# PowerShell example
Invoke-RestMethod -Uri "http://localhost:3001/" -Method Post `
  -Headers @{"Authorization" = "Bearer your-api-key"; "Content-Type" = "application/json"} `
  -Body '{"jsonrpc": "2.0", "id": 1, "method": "#TestConnection", "params": {"ConnectionName": "My_DBCONNECTION_Name"}}'

# curl example
curl -X POST http://localhost:3001/ -H "Authorization: Bearer your-api-key" -H "Content-Type: application/json" `
  -d '{"jsonrpc": "2.0", "id": 1, "method": "#TestConnection", "params": {"ConnectionName": "My_DBCONNECTION_Name"}}'
```

#### API Key Management

The MCP server provides endpoints for managing API keys:

1. **mssql_create_key**: Create a new API key for a specific user

   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "mssql_create_key",
       "arguments": {
         "name": "My Test API Key",
         "userId": "SomeUserId",
         "keyType": "user",
         "expirationDate": "2026-06-29T00:00:00Z"
       }
     }
   }
   ```

2. **mssql_list_user_keys**: List all API keys for a specific user

   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "mssql_list_user_keys",
       "arguments": {
         "userId": "SomeUserId"
       }
     }
   }
   ```

3. **mssql_list_all_keys**: List all API keys in the system (admin only)

   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "mssql_list_all_keys",
       "arguments": {}
     }
   }
   ```

4. **mssql_revoke_key**: Revoke an API key (mark as inactive)

   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "mssql_revoke_key",
       "arguments": {
         "request": {
           "id": "key-id-to-revoke"
         }
       }
     }
   }
   ```

5. **mssql_delete_key**: Permanently delete an API key

   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "mssql_delete_key",
       "arguments": {
         "id": "key-id-to-delete"
       }
     }
   }
   ```

6. **mssql_get_key_usage_logs**: Get usage logs for a specific API key

   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "mssql_get_key_usage_logs",
       "arguments": {
         "apiKeyId": "target-key-id",
         "limit": 100
       }
     }
   }
   ```

7. **mssql_get_user_usage_logs**: Get API usage logs for a specific user
   ```json
   {
     "jsonrpc": "2.0",
     "id": 1,
     "method": "tools/call",
     "params": {
       "name": "mssql_get_user_usage_logs",
       "arguments": {
         "userId": "target-user-id",
         "limit": 100
       }
     }
   }
   ```

#### Security Considerations

- Store API keys securely and never expose them in client-side code
- Use the master API key only for administrative tasks
- Create user-specific API keys with appropriate expiration dates for regular access
- Rotate API keys periodically for enhanced security
- Monitor API usage logs for suspicious activity
- Revoke compromised API keys immediately
- Use HTTPS in production environments when exposing the API
- Consider implementing additional security measures for high-security environments
