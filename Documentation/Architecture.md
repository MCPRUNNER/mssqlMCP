# SQL Server MCP Architecture

This document provides a comprehensive architectural overview of the SQL Server MCP (Model Context Protocol) Server, including component interactions, data flows, and implementation details.

## System Overview

The SQL Server MCP Server is a .NET-based application that implements the Model Context Protocol to provide SQL Server database connectivity for AI assistants like GitHub Copilot. The system enables retrieving metadata about database objects and executing SQL queries through a standardized protocol interface.

## Architecture Diagram

```mermaid
graph TD
    Client[VS Code / Copilot] <-->|MCP Protocol| Server[SQL Server MCP Server]
    Server --> Tools
    Server --> Services
    Server --> Models

    subgraph Tools
        ST[SqlServerTools]
        CMT[ConnectionManagerTool]
    end

    subgraph Services
        DMP[DatabaseMetadataProvider]
        CM[ConnectionManager]
        CR[ConnectionRepository]
        CSP[ConnectionStringProvider]
    end

    subgraph Models
        TI[TableInfo]
        CI[ColumnInfo]
        FKI[ForeignKeyInfo]
        CE[ConnectionEntry]
    end

    ST --> DMP
    ST --> CM
    ST --> CSP
    CMT --> CM
    CM --> CR
    CM --> CSP

    subgraph Databases
        SQLDB[(SQL Server DB)]
        SQLite[(SQLite for Connection Storage)]
    end

    DMP -->|Retrieves Metadata| SQLDB
    CM --> SQLite
    CR -->|CRUD Operations| SQLite
    CM -->|Opens Connections| SQLDB
```

## Component Descriptions

### Client Layer

- **VS Code & Copilot**: Connects to the MCP server using HTTP transport and issues commands to interact with SQL databases.

### Server Core

- **MCP Server**: Provides the Model Context Protocol implementation including HTTP transport and tool registration.
- **Dependency Injection Container**: Manages component lifecycles and dependencies.
- **Logging**: Comprehensive logging using Serilog for diagnostics and troubleshooting.

### Tools Layer

- **SqlServerTools**: Exposes MCP tool methods for SQL Server operations to clients.
- **ConnectionManagerTool**: Manages database connection strings and connection information.

### Services Layer

- **DatabaseMetadataProvider**: Core service that queries SQL Server system tables to retrieve metadata.
- **ConnectionManager**: Manages database connections and connection pooling.
- **ConnectionRepository**: Persists connection information to SQLite database.
- **ConnectionStringProvider**: Retrieves connection strings from configuration and repository.

### Models Layer

- **TableInfo**: Represents database objects (tables, views, procedures, functions).
- **ColumnInfo**: Represents columns and parameters.
- **ForeignKeyInfo**: Represents foreign key relationships.
- **ConnectionEntry**: Represents saved database connections.

### Persistence Layer

- **SQL Server**: Target databases that the server connects to.
- **SQLite**: Local storage for connection information.

## Client Request Flow

The following diagram illustrates the typical flow of a client request through the system:

```mermaid
journey
    title MCP Request Processing Flow
    section Client
        Send MCP Request: 5: Client
    section Server
        Receive HTTP Request: 5: MCP Server
        Parse JSON Payload: 5: MCP Server
        Identify Tool & Method: 5: MCP Server
        Dispatch to Tool: 5: MCP Server
    section Tool Processing
        Execute Tool Method: 5: SqlServerTools
        Call Service Layer: 5: SqlServerTools
    section Service Layer
        Process Request: 5: DatabaseMetadataProvider
        Connect to Database: 5: ConnectionManager
        Execute SQL Query: 5: DatabaseMetadataProvider
        Transform Results: 5: DatabaseMetadataProvider
    section Response
        Format Response: 5: SqlServerTools
        Return JSON Response: 5: MCP Server
        Client Receives Result: 5: Client
```

## Database Metadata Retrieval Flow

The following diagram shows the process of retrieving database metadata:

```mermaid
journey
    title Database Metadata Retrieval Process
    section Client Request
        Client Requests Metadata: 5: Client
    section Initial Processing
        Identify Target Connection: 5: SqlServerTools
        Validate Connection: 5: ConnectionManager
    section Metadata Collection
        Connect to Database: 5: DatabaseMetadataProvider
        Retrieve Tables: 5: DatabaseMetadataProvider
        Retrieve Views: 5: DatabaseMetadataProvider
        Retrieve Procedures: 5: DatabaseMetadataProvider
        Retrieve Functions: 5: DatabaseMetadataProvider
    section Detailed Information
        Get Columns: 5: DatabaseMetadataProvider
        Get Primary Keys: 5: DatabaseMetadataProvider
        Get Foreign Keys: 5: DatabaseMetadataProvider
        Get Definitions: 5: DatabaseMetadataProvider
    section Response
        Assemble Metadata Objects: 5: DatabaseMetadataProvider
        Serialize to JSON: 5: SqlServerTools
        Return to Client: 5: MCP Server
```

## Connection Management Flow

The following diagram illustrates how database connections are managed:

```mermaid
journey
    title Connection Management Flow
    section Initial Setup
        Load Connections at Startup: 5: ConnectionManager
    section Add Connection
        Receive Add Request: 5: ConnectionManagerTool
        Validate Connection String: 5: ConnectionManager
        Test Connection: 5: ConnectionManager
        Save to Repository: 5: ConnectionRepository
    section Use Connection
        Retrieve Connection: 5: ConnectionManager
        Open Connection: 5: ConnectionManager
        Use Connection: 5: SqlServerTools
        Return Connection to Pool: 5: ConnectionManager
    section Manage Connections
        List Connections: 5: ConnectionManagerTool
        Update Connection: 5: ConnectionManagerTool
        Remove Connection: 5: ConnectionManagerTool
```

## SQL Query Execution Flow

This diagram shows the process of executing SQL queries:

```mermaid
journey
    title SQL Query Execution Flow
    section Request Preparation
        Client Sends Query: 5: Client
        Receive Query Request: 5: SqlServerTools
    section Execution
        Get Connection: 5: ConnectionManager
        Create Command: 5: SqlServerTools
        Execute Query: 5: SqlServerTools
        Process Result Set: 5: SqlServerTools
    section Response
        Convert to JSON: 5: SqlServerTools
        Format Response: 5: SqlServerTools
        Return Results: 5: MCP Server
```

## Key Technical Features

### Comprehensive Metadata

The system collects detailed metadata about:

- Tables with columns, primary keys, and foreign keys
- Views with column information and SQL definitions
- Stored procedures with parameters and SQL code
- SQL functions with parameters, return types and SQL code

### Connection Management

- Persisted connections in SQLite database
- Connection string security
- Connection testing and validation
- Connection pooling for performance

### Robust Error Handling

- Detailed error logging
- Client-friendly error messages
- Timeout handling
- SQL error analysis

### Performance Considerations

- Asynchronous operations throughout
- Connection pooling
- Timeout controls
- Cancellation support

## Extension Points

The architecture supports the following extension points:

1. **Additional Database Providers**

   - The system can be extended to support other databases by implementing new tool and provider classes

2. **Enhanced Metadata**

   - Additional metadata can be collected by extending the models and provider methods

3. **Additional MCP Tools**

   - New tool classes can be registered to extend functionality

4. **Authentication Mechanisms**
   - Security can be enhanced with additional authentication providers

## Security Considerations

1. **Connection String Security**

   - Connection strings are stored in a local SQLite database
   - Sensitive information should be protected

2. **SQL Injection Prevention**

   - Parameterized queries are used throughout the codebase
   - User input validation

3. **Error Information**
   - Error details are logged but sanitized before returning to clients

## Conclusion

The SQL Server MCP Server provides a robust implementation of the Model Context Protocol for SQL Server databases. With its clean architecture, comprehensive metadata support, and connection management capabilities, it enables AI assistants like Copilot to effectively explore and interact with SQL Server databases.
