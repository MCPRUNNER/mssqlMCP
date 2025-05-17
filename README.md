# SQL Server MCP (Model Context Protocol) Server

This is a Model Context Protocol (MCP) server that connects to SQL Server databases, designed to be used by Visual Studio Code as a Copilot Agent.

## Overview

This project implements an MCP server for SQL Server database connectivity, enabling VS Code and Copilot to interact with SQL databases via the Model Context Protocol.

Features include:

- SQL query execution
- Database metadata retrieval (tables, views, stored procedures, functions)
- Detailed schema information including primary/foreign keys
- Connection string encryption with AES-256
- Key rotation and security management
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

### API Endpoint

The MCP server exposes a JSON-RPC API endpoint at:

```
http://localhost:3001/
```

All JSON-RPC requests should be sent to this endpoint as HTTP POST requests with the appropriate method and parameters.

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
3. **Connection Security Tools**: For encrypting and managing connection string security including key rotation and encryption status verification

### Using Connection Management

#### Starting with Sample Connections

Run the included script to start the server with sample connections:

```powershell
./Scripts/start-mcp-with-connections.ps1
```

#### Starting with Encryption Enabled

For enhanced security, use the encryption-enabled starter script:

```powershell
./Scripts/Start-MCP-Encrypted.ps1
```

This script automatically generates a cryptographically secure random key using System.Security.Cryptography.RandomNumberGenerator, sets it as an environment variable, and starts the server with encryption enabled. You can also provide your own key:

```powershell
$env:MSSQL_MCP_KEY = "your-secure-key"
./Scripts/Start-MCP-Encrypted.ps1
```

For production environments, you should store the key securely and set the environment variable externally using a secrets management solution.

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
./Scripts/test-connection-manager.ps1
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

### Stored Procedure Metadata (New!)

The MCP server now also supports retrieving metadata from SQL Server stored procedures, including procedure definitions and parameters. This allows Copilot to understand and work with stored procedures in your database.

You can retrieve stored procedure metadata using the `GetDatabaseObjectsMetadata` tool with the `objectType` parameter:

```
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=PROCEDURE
```

This provides detailed procedure metadata including:

- Procedure names and schemas
- Parameter details (name, type, direction)
- SQL definition (when not encrypted)

### Filtering Database Objects

You can filter database objects by type using the `objectType` parameter:

```
# Get only tables
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=TABLE

# Get only views
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=VIEW

# Get only stored procedures
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=PROCEDURE

# Get all database objects
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=ALL
```

You can also filter by schema:

```
# Get objects from a specific schema
#GetDatabaseObjectsMetadata connectionName="YourConnection" schema="dbo" objectType=ALL
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
User: Show me all the database objects in my AdventureWorks2022 database, including views

Copilot: I'll retrieve the metadata for all database objects in the AdventureWorks2022 database, including both tables and views.

[Tool used: GetDatabaseObjectsMetadata with connectionName="AdventureWorks2022" includeViews=true]

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

## Using with VS Code Copilot Agent

1. Configure your Copilot Agent to use this MCP server by referencing the `mcp.json` file
2. Initialize the connection by calling the `Initialize` method
3. Execute SQL queries using the `ExecuteQuery` method
4. Retrieve database metadata with the `GetTableMetadata` method

### Copilot Chat Examples

#### Using GetTableMetadata in Copilot Chat

You can ask Copilot to retrieve schema information about database tables:

```
User: Show me all the tables in my AdventureWorks2022 database

Copilot: I'll retrieve the metadata for all tables in the AdventureWorks2022 database.

[Tool used: GetTableMetadata with connectionName="AdventureWorks2022"]

Results:
The AdventureWorks2022 database contains 68 tables across multiple schemas:

1. Person.Person - Customer and employee information
2. Production.Product - Product information
3. Sales.SalesOrderHeader - Sales order headers
4. Sales.SalesOrderDetail - Sales order line items
5. Production.ProductInventory - Product inventory levels
6. HumanResources.Employee - Employee information
7. Purchasing.PurchaseOrderHeader - Purchase order headers
8. Purchasing.PurchaseOrderDetail - Purchase order details
9. Sales.Customer - Customer information
10. Production.WorkOrder - Manufacturing work orders
11. Production.BillOfMaterials - Product component structure
12. Production.ProductReview - Customer reviews of products
13. Sales.SalesTerritory - Sales territory regions
14. Sales.SalesPerson - Sales staff information
15. HumanResources.Department - Company departments
16. HumanResources.EmployeeDepartmentHistory - Employee department assignments
17. Person.Address - Customer and employee addresses
18. Person.StateProvince - State and province information
```

#### Using ExecuteQuery in Copilot Chat

You can ask Copilot to run SQL queries against your database:

```
User: How many products are in the AdventureWorks2022 database?

Copilot: I'll run a query to count the products in the AdventureWorks2022 database.

[Tool used: ExecuteQuery with connectionName="AdventureWorks2022"]
Query: SELECT COUNT(*) AS ProductCount FROM Production.Product

Results:
[
  {
    "ProductCount": 504
  }
]

There are 504 products in the AdventureWorks2022 database.

User: Show me the 5 most expensive products

Copilot: I'll retrieve the 5 most expensive products from the Product table.

[Tool used: ExecuteQuery with connectionName="AdventureWorks2022"]
Query: SELECT TOP 5 ProductID, Name, ProductNumber, ListPrice, Color FROM Production.Product ORDER BY ListPrice DESC

Results:
[
  {
    "ProductID": 749,
    "Name": "Road-250 Red, 44",
    "ProductNumber": "BK-R93R-44",
    "ListPrice": 3578.27,
    "Color": "Red"
  },
  {
    "ProductID": 750,
    "Name": "Road-250 Red, 48",
    "ProductNumber": "BK-R93R-48",
    "ListPrice": 3578.27,
    "Color": "Red"
  },
  {
    "ProductID": 751,
    "Name": "Road-250 Red, 52",
    "ProductNumber": "BK-R93R-52",
    "ListPrice": 3578.27,
    "Color": "Red"
  },
  {
    "ProductID": 752,
    "Name": "Road-250 Red, 58",
    "ProductNumber": "BK-R93R-58",
    "ListPrice": 3578.27,
    "Color": "Red"
  },
  {
    "ProductID": 753,
    "Name": "Road-250 Red, 60",
    "ProductNumber": "BK-R93R-60",
    "ListPrice": 3578.27,
    "Color": "Red"
  }
]
```

### Advanced Copilot Chat Examples

#### Exploring Database Relationships

```
User: Show me the relationships between tables in the Sales schema

Copilot: I'll get the foreign key relationships in the Sales schema for you.

[Tool used: GetTableMetadata with connectionName="AdventureWorks2022", schema="Sales"]

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

[Tool used: ExecuteQuery with connectionName="AdventureWorks2022"]
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

[Tool used: ExecuteQuery with connectionName="AdventureWorks2022"]
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
  connectionName: "AdventureWorks2022",
  query: "SELECT TOP 5 * FROM Production.Product",
});
f1e_GetTableMetadata({ connectionName: "AdventureWorks2022", schema: "Sales" });
f1e_GetDatabaseObjectsMetadata({
  connectionName: "AdventureWorks2022",
  schema: "Person",
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
User: Run this query: SELECT FROM Production.Product WHERE ListPrice < 100

Copilot: I'll execute this query for you.

[Tool used: ExecuteQuery with connectionName="AdventureWorks2022"]
Query: SELECT FROM Production.Product WHERE ListPrice < 100

Error: Incorrect syntax near the keyword 'FROM'.
```

**Solution**: Check that your SQL query follows proper syntax. In this case, you need to specify columns after SELECT.

### Connection Name Mismatch

When the connection name doesn't match exactly what's in your configuration:

```
User: Get tables from AdventureWorks database

Copilot: I'll get the tables from the AdventureWorks database.

[Tool used: GetTableMetadata with connectionName="AdventureWorks"]

Error: Connection string 'AdventureWorks' was not found in the configuration.
```

**Solution**: Use the exact connection string name as defined in your configuration. In this case, use "AdventureWorks2022" instead of "AdventureWorks".

### Table or Schema Not Found

When querying non-existent objects:

```
User: Show me data from the Customers table

Copilot: I'll query the Customers table for you.

[Tool used: ExecuteQuery with connectionName="AdventureWorks2022"]
Query: SELECT TOP 10 * FROM Customers

Error: Invalid object name 'Customers'.
```

**Solution**: First use GetTableMetadata to see which tables are available, then query the correct table name with the proper schema prefix if needed, e.g., "Sales.Customer".

### Performance Tips

- Be specific about which schema you're interested in when using GetTableMetadata
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
$env:MSSQL_MCP_KEY = "your-strong-random-key"
dotnet run

# Option 2: Use the automated script that handles key generation and server startup
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

### Testing Security Features

To test the security features of the MCP server, you can use the provided test script:

```powershell
./Scripts/Test-Security-Features.ps1
```

This script will:

1. Add a test connection with encryption
2. List connections to verify encryption
3. Test migrating unencrypted connections to encrypted format
4. Optionally test key rotation (requires restarting the server)
5. Clean up the test connection

For more detailed testing, you can use the individual scripts:

```powershell
# Start the server with encryption enabled
./Scripts/Start-MCP-Encrypted.ps1

# Rotate the encryption key
./Scripts/Rotate-Encryption-Key.ps1

# Migrate unencrypted connections to encrypted format
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
./Scripts/Start-MCP-Encrypted.ps1
```

This script:

1. Generates a cryptographically secure random key if one is not provided
2. Sets the key as an environment variable for the current session
3. Displays the key for you to save securely
4. Starts the MCP server with encryption enabled

You can also provide your own key:

```powershell
$env:MSSQL_MCP_KEY = "your-secure-key"
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
