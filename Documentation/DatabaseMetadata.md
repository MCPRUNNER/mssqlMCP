# SQL Server Database Metadata Retrieval

This document provides comprehensive information about the database metadata retrieval capabilities in the SQL Server MCP server, including core database objects and specialized metadata providers.

## Overview

The MCP server provides extensive metadata retrieval for SQL Server database objects and specialized systems, allowing you to explore and analyze database schemas, SQL Server Agent jobs, SSIS packages, and Azure DevOps analytics. This functionality is essential for database development, documentation, automation, and integration with tools like Copilot.

## Available MCP Tools

The SQL Server MCP server provides 9 comprehensive tools organized into categories:

### Core SQL Server Tools

1. **Initialize** - Initialize the SQL Server connection
2. **ExecuteQuery** - Execute SQL queries and return results as JSON
3. **GetTableMetadata** - Get detailed metadata about database tables only
4. **GetDatabaseObjectsMetadata** - Get metadata for all database objects with view filtering
5. **GetDatabaseObjectsByType** - Get metadata filtered by specific object types

### Specialized Metadata Tools

6. **GetSqlServerAgentJobs** - Get SQL Server Agent job metadata
7. **GetSqlServerAgentJobDetails** - Get detailed job information including steps, schedules, and history
8. **GetSsisCatalogInfo** - Get SSIS catalog information for Project and Package deployment models
9. **GetAzureDevOpsInfo** - Get Azure DevOps analytics including projects, repositories, builds, and work items

### Connection Management & Security Tools

- **ConnectionManagerTool** - Manage database connections (Add, Update, Remove, Test, List)
- **SecurityTool** - Handle encryption key rotation and security operations

## Supported Database Objects

The core metadata system supports four main types of database objects:

1. **Tables** - Retrieve information about tables, columns, primary keys, and foreign keys
2. **Views** - Retrieve information about views, their columns, and SQL definitions
3. **Stored Procedures** - Retrieve information about procedures, parameters, and SQL definitions
4. **Functions** - Retrieve information about SQL functions, parameters, return types, and SQL definitions (with type classification)

## Using Core Metadata Commands

### Get All Database Objects

To retrieve metadata for all database objects (tables, views, procedures, and functions):

```
#GetDatabaseObjectsMetadata connectionName="YourConnection"
```

### Get All Database Objects by Type

To retrieve metadata filtered by specific object types:

```
#GetDatabaseObjectsByType connectionName="YourConnection" objectType="ALL"
```

### Get Tables Only

To retrieve metadata for tables only:

```
#GetTableMetadata connectionName="YourConnection"
```

Or using the type-specific tool:

```
#GetDatabaseObjectsByType connectionName="YourConnection" objectType="TABLE"
```

### Get Views Only

To retrieve metadata for views only:

```
#GetDatabaseObjectsByType connectionName="YourConnection" objectType="VIEW"
```

### Get Stored Procedures Only

To retrieve metadata for stored procedures only:

```
#GetDatabaseObjectsByType connectionName="YourConnection" objectType="PROCEDURE"
```

### Get Functions Only

To retrieve metadata for SQL functions only:

```
#GetDatabaseObjectsByType connectionName="YourConnection" objectType="FUNCTION"
```

### Filter by Schema

To filter objects by schema:

```
#GetDatabaseObjectsByType connectionName="YourConnection" schema="dbo" objectType="ALL"
```

## Using Specialized Metadata Commands

### SQL Server Agent Jobs

Get all SQL Server Agent jobs:

```
#GetSqlServerAgentJobs connectionName="YourConnection"
```

Get detailed information about a specific job:

```
#GetSqlServerAgentJobDetails jobName="YourJobName" connectionName="YourConnection"
```

### SSIS Catalog Information

Get SSIS catalog metadata including folders, projects, and packages:

```
#GetSsisCatalogInfo connectionName="YourConnection"
```

### Azure DevOps Analytics

Get Azure DevOps information from the warehouse database:

```
#GetAzureDevOpsInfo connectionName="YourConnection"
```

## Metadata Structure

### Core Database Object Metadata

#### Table Metadata

For tables, the metadata includes:

- **Schema** - The database schema name
- **Name** - The table name
- **ObjectType** - Always "BASE TABLE" for tables
- **Columns** - List of columns with their properties:
  - Name
  - DataType
  - IsNullable
  - IsPrimaryKey
  - IsForeignKey
  - MaxLength (for string types)
  - Precision and Scale (for numeric types)
  - DefaultValue
  - ForeignKeyReference (if applicable)
- **PrimaryKeys** - List of primary key column names
- **ForeignKeys** - List of foreign key relationships with details:
  - Name (constraint name)
  - Column (local column name)
  - ReferencedSchema
  - ReferencedTable
  - ReferencedColumn

#### View Metadata

For views, the metadata includes:

- **Schema** - The database schema name
- **Name** - The view name
- **ObjectType** - Always "VIEW" for views
- **Definition** - The SQL query that defines the view
- **Columns** - List of columns with their properties similar to tables (no keys since views don't have their own constraints)

#### Stored Procedure Metadata

For stored procedures, the metadata includes:

- **Schema** - The database schema name
- **Name** - The procedure name
- **ObjectType** - Always "PROCEDURE" for stored procedures
- **Definition** - The SQL code that defines the procedure (retrieved from `sys.sql_modules` when not encrypted)
- **Columns** - List of parameters with their properties:
  - Name
  - DataType
  - Description - Contains the parameter direction (IN, OUT, INOUT)
  - MaxLength, Precision, Scale (where applicable)

#### Function Metadata

For SQL functions, the metadata includes:

- **Schema** - The database schema name
- **Name** - The function name
- **ObjectType** - Always "FUNCTION" for SQL functions
- **Definition** - The SQL code that defines the function (retrieved from `sys.sql_modules` when not encrypted)
- **Properties** - Additional function properties:
  - ReturnType - The data type returned by the function
  - FunctionType - The type classification:
    - "Scalar Function" - Returns a single value
    - "Inline Table-valued Function" - Returns a table through a single SELECT statement
    - "Table-valued Function" - Returns a table through multiple statements
- **Columns** - List of parameters with their properties:
  - Name
  - DataType
  - Description - Contains the parameter direction
  - MaxLength, Precision, Scale (where applicable)

### Specialized Metadata Structures

#### SQL Server Agent Job Metadata

SQL Server Agent jobs include:

- **JobId** - Unique identifier
- **Name** - Job name
- **Enabled** - Whether the job is enabled
- **Description** - Job description
- **Owner** - Job owner
- **DateCreated** and **DateModified** - Timestamps
- **Category** - Job category
- **Steps** - List of job steps with:
  - Step details, commands, and configurations
- **Schedules** - List of job schedules
- **History** - Recent execution history

#### SSIS Catalog Metadata

SSIS catalog information includes:

- **Folders** - SSIS catalog folders with:
  - Name and creation date
  - **Projects** - List of SSIS projects with:
    - Name, deployment model (Project/Package)
    - Description and deployment dates
    - **Packages** - List of packages with parameters
    - **Environments** - Associated environments
- **Parameters** - Package and project parameters with data types and default values

#### Azure DevOps Analytics

Azure DevOps information includes:

- **Projects** - List of projects with IDs, names, descriptions, and states
- **Repositories** - Repository information including:
  - Repository ID, name, and project association
  - Size, branch count, commit count
  - Default branch and repository URL
- **BuildDefinitions** - Build definition metadata
- **Counts** - Summary counts for builds, releases, and work items

## Implementation Details

The metadata retrieval system is implemented using a comprehensive approach with multiple data sources:

### Core Database Metadata System

The core system uses SQL Server system catalog views and tables:

- **INFORMATION_SCHEMA.TABLES** - For table and view basic metadata
- **INFORMATION_SCHEMA.VIEWS** - For view-specific metadata and definitions
- **INFORMATION_SCHEMA.ROUTINES** - For stored procedure and function basic metadata
- **INFORMATION_SCHEMA.COLUMNS** - For detailed column metadata across all object types
- **INFORMATION_SCHEMA.PARAMETERS** - For stored procedure and function parameters
- **sys.foreign_keys** and related system tables - For foreign key relationships
- **sys.sql_modules** - For retrieving procedure and function definitions when `INFORMATION_SCHEMA.ROUTINES.ROUTINE_DEFINITION` is NULL (encrypted or large objects)
- **sys.objects** and **sys.types** - For additional object type information and function type classification

### Enhanced Features

#### Function Type Classification

The system automatically classifies SQL functions into three types:

- **Scalar Functions** - Return a single value
- **Inline Table-valued Functions** - Return a table through a single SELECT
- **Table-valued Functions** - Return a table through multiple statements

#### Fallback Definition Retrieval

When procedure or function definitions are not available through `INFORMATION_SCHEMA.ROUTINES` (due to encryption or size limitations), the system automatically falls back to querying `sys.sql_modules` for the complete definition.

#### Object Type Filtering

The `GetDatabaseObjectsByType` tool supports filtering by:

- **TABLE/TABLES** - Returns only base tables
- **VIEW/VIEWS** - Returns only views
- **PROCEDURE/PROC/PROCEDURES** - Returns only stored procedures
- **FUNCTION/FUNC/FUNCTIONS** - Returns only functions
- **ALL** - Returns all supported object types

### Specialized Metadata Providers

#### SQL Server Agent Provider

Retrieves metadata from the `msdb` system database:

- **sysjobs** - Job definitions and properties
- **sysjobsteps** - Job step details and commands
- **sysjobschedules** - Job scheduling information
- **sysjobhistory** - Job execution history

#### SSIS Catalog Provider

Accesses the SSISDB catalog database:

- **catalog.folders** - SSIS catalog folder structure
- **catalog.projects** - Project deployment model information
- **catalog.packages** - Package metadata and parameters
- **catalog.environments** - Environment configurations
- **catalog.object_parameters** - Project and package parameters

#### Azure DevOps Analytics Provider

Queries Azure DevOps Analytics warehouse databases:

- **Project and repository metadata**
- **Build and release definition information**
- **Work item counts and statistics**
- **Repository statistics** (size, branches, commits)

### Error Handling and Timeouts

The system includes comprehensive error handling:

- **Connection timeout management** - 120-second timeout for metadata operations
- **SQL error code handling** - Specific handling for authentication failures (4060, 18456, 18452)
- **Cancellation token support** - Proper cancellation handling for long-running operations
- **Encrypted object handling** - Graceful handling when definitions are encrypted or unavailable

### Performance Considerations

- **Schema filtering** - Optional schema parameter to limit scope and improve performance
- **Batched operations** - Efficient querying of related metadata in batches
- **Connection reuse** - Single connection per metadata operation to minimize overhead
- **Selective object type retrieval** - Filter by object type to reduce data transfer

## Example Usage Scripts

Several test scripts are provided to demonstrate metadata retrieval capabilities:

### Core Database Metadata Scripts

1. **test-view-metadata.ps1** - Demonstrates view metadata retrieval and definition access
2. **test-stored-procedures.ps1** - Demonstrates stored procedure metadata with parameter details
3. **test-function-metadata.ps1** - Demonstrates function metadata with type classification
4. **test-connection-manager.ps1** - Demonstrates secure connection management

### Specialized Metadata Scripts

5. **Test SQL Server Agent Jobs**:

   ```powershell
   # Get all SQL Server Agent jobs
   Invoke-RestMethod -Uri "http://localhost:5000/tools/GetSqlServerAgentJobs" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"DefaultConnection"}'

   # Get detailed job information
   Invoke-RestMethod -Uri "http://localhost:5000/tools/GetSqlServerAgentJobDetails" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"jobName":"YourJobName","connectionName":"DefaultConnection"}'
   ```

6. **Test SSIS Catalog Information**:

   ```powershell
   # Get SSIS catalog metadata
   Invoke-RestMethod -Uri "http://localhost:5000/tools/GetSsisCatalogInfo" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"DefaultConnection"}'
   ```

7. **Test Azure DevOps Analytics**:
   ```powershell
   # Get Azure DevOps warehouse information
   Invoke-RestMethod -Uri "http://localhost:5000/tools/GetAzureDevOpsInfo" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"AzureDevOpsConnection"}'
   ```

### Object Type Filtering Examples

```powershell
# Get only tables
Invoke-RestMethod -Uri "http://localhost:5000/tools/GetDatabaseObjectsByType" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"DefaultConnection","objectType":"TABLE"}'

# Get only views
Invoke-RestMethod -Uri "http://localhost:5000/tools/GetDatabaseObjectsByType" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"DefaultConnection","objectType":"VIEW"}'

# Get only stored procedures
Invoke-RestMethod -Uri "http://localhost:5000/tools/GetDatabaseObjectsByType" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"DefaultConnection","objectType":"PROCEDURE"}'

# Get only functions
Invoke-RestMethod -Uri "http://localhost:5000/tools/GetDatabaseObjectsByType" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"DefaultConnection","objectType":"FUNCTION"}'

# Get objects from specific schema
Invoke-RestMethod -Uri "http://localhost:5000/tools/GetDatabaseObjectsByType" -Method POST -Headers @{"Content-Type"="application/json"} -Body '{"connectionName":"DefaultConnection","schema":"dbo","objectType":"ALL"}'
```

Run these scripts to see the metadata retrieval in action with the latest capabilities.

## Using Metadata with Copilot

When integrated with Copilot, the comprehensive metadata system enables the AI to:

### Core Database Intelligence

1. **Understand your complete database schema** - Tables, views, procedures, and functions with full structural details
2. **Generate accurate SQL queries** - Using precise column names, data types, and relationships
3. **Explain relationships between tables** - Through foreign key analysis and cross-references
4. **Suggest schema improvements** - Based on structural analysis and best practices
5. **Create comprehensive documentation** - For your database objects and their relationships

### Advanced Database Operations

6. **Function type awareness** - Distinguish between scalar, inline table-valued, and multi-statement table-valued functions
7. **Parameter analysis** - Understand stored procedure and function parameters for accurate call generation
8. **View dependency analysis** - Understand view definitions and underlying table relationships

### Enterprise Integration

9. **SQL Server Agent automation** - Understand job structures, schedules, and dependencies for automation recommendations
10. **SSIS package analysis** - Analyze ETL processes, parameters, and deployment models
11. **Azure DevOps integration** - Connect database changes with build processes and repository management
12. **Cross-system analysis** - Understand relationships between database objects, SSIS packages, and DevOps processes

### Development Workflow Enhancement

13. **Intelligent code completion** - Accurate suggestions based on actual database structure
14. **Error prevention** - Catch schema mismatches and type conflicts before execution
15. **Best practice recommendations** - Suggest improvements based on actual database patterns
16. **Automated testing suggestions** - Recommend test scenarios based on procedure parameters and data types

### Security and Compliance

17. **Encrypted object handling** - Gracefully handle encrypted procedures and functions
18. **Connection security** - Understand connection patterns and security configurations
19. **Access pattern analysis** - Analyze database usage patterns through job and SSIS metadata

The comprehensive metadata makes Copilot significantly more effective when working with enterprise SQL Server environments, providing deep contextual understanding across the entire data platform ecosystem.
