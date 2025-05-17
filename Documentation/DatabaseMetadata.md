# SQL Server Database Metadata Retrieval

This document provides detailed information about the database metadata retrieval capabilities in the SQL Server MCP server.

## Overview

The MCP server provides comprehensive metadata retrieval for SQL Server database objects, allowing you to explore and analyze database schemas. This functionality is essential for database development, documentation, and integration with tools like Copilot.

## Supported Database Objects

The metadata system supports three main types of database objects:

1. **Tables** - Retrieve information about tables, columns, primary keys, and foreign keys
2. **Views** - Retrieve information about views, their columns, and SQL definitions
3. **Stored Procedures** - Retrieve information about procedures, parameters, and SQL definitions

## Using Metadata Commands

### Get All Database Objects

To retrieve metadata for all database objects (tables, views, and procedures):

```
#GetDatabaseObjectsMetadata connectionName="YourConnection"
```

### Get Tables Only

To retrieve metadata for tables only:

```
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=TABLE
```

### Get Views Only

To retrieve metadata for views only:

```
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=VIEW
```

### Get Stored Procedures Only

To retrieve metadata for stored procedures only:

```
#GetDatabaseObjectsMetadata connectionName="YourConnection" objectType=PROCEDURE
```

### Filter by Schema

To filter objects by schema:

```
#GetDatabaseObjectsMetadata connectionName="YourConnection" schema="dbo" objectType=ALL
```

## Metadata Structure

### Table Metadata

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
- **PrimaryKeys** - List of primary key column names
- **ForeignKeys** - List of foreign key relationships with details:
  - Name (constraint name)
  - Column (local column name)
  - ReferencedSchema
  - ReferencedTable
  - ReferencedColumn

### View Metadata

For views, the metadata includes:

- **Schema** - The database schema name
- **Name** - The view name
- **ObjectType** - Always "VIEW" for views
- **Definition** - The SQL query that defines the view
- **Columns** - List of columns with their properties similar to tables

### Stored Procedure Metadata

For stored procedures, the metadata includes:

- **Schema** - The database schema name
- **Name** - The procedure name
- **ObjectType** - Always "PROCEDURE" for stored procedures
- **Definition** - The SQL code that defines the procedure (when not encrypted)
- **Columns** - List of parameters with their properties:
  - Name
  - DataType
  - Description - Contains the parameter direction (IN, OUT, INOUT)
  - MaxLength, Precision, Scale (where applicable)

## Implementation Details

The metadata retrieval system uses SQL Server system catalog views:

- `INFORMATION_SCHEMA.TABLES` for table metadata
- `INFORMATION_SCHEMA.VIEWS` for view metadata
- `INFORMATION_SCHEMA.ROUTINES` for stored procedure metadata
- `INFORMATION_SCHEMA.COLUMNS` for column metadata
- `INFORMATION_SCHEMA.PARAMETERS` for stored procedure parameters
- `sys.foreign_keys` and related tables for foreign key relationships
- `sys.sql_modules` for additional procedure definitions

## Example Test Scripts

Three test scripts are provided to demonstrate metadata retrieval:

1. **test-view-metadata.ps1** - Demonstrates view metadata retrieval
2. **test-stored-procedures.ps1** - Demonstrates stored procedure metadata retrieval
3. **test-connection-manager.ps1** - Demonstrates connection management

Run these scripts to see the metadata retrieval in action.

## Using Metadata with Copilot

When integrated with Copilot, this metadata allows the AI to:

1. Understand your database schema
2. Generate accurate SQL queries
3. Explain relationships between tables
4. Suggest schema improvements
5. Create documentation for your database

The comprehensive metadata makes Copilot much more effective when working with your databases.
