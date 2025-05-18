# SQL Server MCP Architecture

This document provides a comprehensive architectural overview of the SQL Server MCP (Model Context Protocol) Server, including component interactions, data flows, and implementation details.

## System Overview

The SQL Server MCP Server is a .NET-based application that implements the Model Context Protocol to provide SQL Server database connectivity for AI assistants like GitHub Copilot. The system enables retrieving metadata about database objects and executing SQL queries through a standardized protocol interface.

## Architecture Diagram

```mermaid
graph TD
    Client[VS Code / Copilot] <-->|MCP Protocol| Auth[API Security Layer]
    Auth <-->|Authentication| Server[SQL Server MCP Server]
    Server --> Tools
    Server --> Services
    Server --> Models

    subgraph Tools
        ST[SqlServerTools]
        CMT[ConnectionManagerTool]
        SCT[SecurityTool]
    end

    subgraph Services
        DMP[DatabaseMetadataProvider]
        CM[ConnectionManager]
        CR[ConnectionRepository]
        CSP[ConnectionStringProvider]
        ES[EncryptionService]
        KRS[KeyRotationService]
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
    SCT --> KRS
    SCT --> ES
    CM --> CR
    CM --> CSP
    KRS --> CR
    KRS --> ES
    CM --> ES

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

### API Security Layer

- **API Key Authentication**: Validates API keys in request headers to secure access to the MCP server.
- **ApiKeyAuthMiddleware**: Middleware component that enforces API key validation for each request.

### Server Core

- **MCP Server**: Provides the Model Context Protocol implementation including HTTP transport and tool registration.
- **Dependency Injection Container**: Manages component lifecycles and dependencies.
- **Logging**: Comprehensive logging using Serilog for diagnostics and troubleshooting.

### Tools Layer

- **SqlServerTools**: Exposes MCP tool methods for SQL Server operations to clients.
- **ConnectionManagerTool**: Manages database connection strings and connection information.
- **SecurityTool**: Provides security operations such as key rotation, connection encryption, and secure key generation.

### Services Layer

- **DatabaseMetadataProvider**: Core service that queries SQL Server system tables to retrieve metadata.
- **ConnectionManager**: Manages database connections and connection pooling.
- **ConnectionRepository**: Persists connection information to SQLite database.
- **ConnectionStringProvider**: Retrieves connection strings from configuration and repository.
- **EncryptionService**: Handles the encryption and decryption of sensitive connection data.
- **KeyRotationService**: Manages the rotation of encryption keys for improved security.

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
    section API Security
        Authenticate Request: 5: ApiKeyAuthMiddleware
        Validate API Key: 5: ApiKeyAuthMiddleware
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
    section API Security
        Validate API Key: 5: ApiKeyAuthMiddleware
    section Initial Processing
        Identify Target Connection: 5: SqlServerTools
        Validate Connection: 5: ConnectionManager
        Decrypt Connection String: 5: EncryptionService
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
        Decrypt Connection Strings: 5: EncryptionService
    section Add Connection
        Receive Add Request: 5: ConnectionManagerTool
        Validate API Key: 5: ApiKeyAuthMiddleware
        Validate Connection String: 5: ConnectionManager
        Encrypt Connection String: 5: EncryptionService
        Test Connection: 5: ConnectionManager
        Save to Repository: 5: ConnectionRepository
    section Use Connection
        Retrieve Connection: 5: ConnectionManager
        Decrypt Connection String: 5: EncryptionService
        Open Connection: 5: ConnectionManager
        Use Connection: 5: SqlServerTools
        Return Connection to Pool: 5: ConnectionManager
    section Manage Connections
        List Connections: 5: ConnectionManagerTool
        Update Connection: 5: ConnectionManagerTool
        Encrypt Updated Connection: 5: EncryptionService
        Remove Connection: 5: ConnectionManagerTool
```

## SQL Query Execution Flow

This diagram shows the process of executing SQL queries:

```mermaid
journey
    title SQL Query Execution Flow
    section Request Preparation
        Client Sends Query: 5: Client
        Validate API Key: 5: ApiKeyAuthMiddleware
        Receive Query Request: 5: SqlServerTools
    section Connection
        Get Connection Info: 5: ConnectionManager
        Decrypt Connection String: 5: EncryptionService
        Create Connection: 5: ConnectionManager
    section Execution
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
- Connection string encryption with AES
- Connection testing and validation
- Connection pooling for performance
- Support for key rotation to enhance security

### API Security

- API key authentication via HTTP headers
- Configurable header name (default: X-API-Key)
- Flexible key storage (environment variable or configuration)
- Proper HTTP status codes for authentication failures (401, 403)
- Supports disabling authentication when no key is configured

### Robust Error Handling

- Detailed error logging
- Client-friendly error messages
- Timeout handling
- SQL error analysis
- Proper HTTP status codes for different error types

### Performance Considerations

- Asynchronous operations throughout
- Connection pooling
- Timeout controls
- Cancellation support
- Content type negotiation

## Security Features

### API Key Authentication

```mermaid
sequenceDiagram
    participant Client as Client
    participant Middleware as ApiKeyAuthMiddleware
    participant Server as MCP Server

    Client->>Middleware: HTTP Request with X-API-Key header
    alt No API Key Configured
        Middleware->>Server: Pass through (auth disabled)
    else API Key Configured
        alt Missing API Key
            Middleware-->>Client: 401 Unauthorized
        else Invalid API Key
            Middleware-->>Client: 403 Forbidden
        else Valid API Key
            Middleware->>Server: Pass request to server
            Server->>Server: Process request
            Server-->>Client: Response
        end
    end
```

### Connection String Encryption

```mermaid
sequenceDiagram
    participant App as Application
    participant ES as EncryptionService
    participant DB as Connection Repository

    App->>ES: Request to encrypt connection string
    ES->>ES: Get key from environment/config
    ES->>ES: Generate initialization vector
    ES->>ES: Apply AES encryption
    ES->>App: Return encrypted connection string + IV
    App->>DB: Store encrypted connection

    App->>DB: Request connection
    DB->>App: Return encrypted connection
    App->>ES: Request to decrypt connection string
    ES->>ES: Retrieve key
    ES->>ES: Extract IV
    ES->>ES: Apply AES decryption
    ES->>App: Return decrypted connection string
```

### Key Rotation

```mermaid
sequenceDiagram
    participant Tool as SecurityTool
    participant KRS as KeyRotationService
    participant ES as EncryptionService
    participant DB as ConnectionRepository

    Tool->>KRS: Request key rotation
    KRS->>DB: Get all connections
    loop For each connection
        KRS->>ES: Decrypt with old key
        KRS->>ES: Encrypt with new key
        KRS->>DB: Update connection
    end
    KRS->>Tool: Return success
```

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
   - The existing API key authentication can be extended with more sophisticated mechanisms

5. **Additional Security Features**
   - New encryption algorithms can be implemented through the IEncryptionService interface
   - Advanced key management solutions can be integrated

## Security Considerations

1. **Connection String Security**

   - Connection strings are stored encrypted in a local SQLite database
   - AES encryption is used to protect sensitive information
   - Encryption keys can be rotated periodically for enhanced security

2. **API Access Control**

   - API key authentication protects the MCP endpoint
   - Keys can be stored in environment variables for secure deployment
   - Script-based key management simplifies administration

3. **SQL Injection Prevention**

   - Parameterized queries are used throughout the codebase
   - User input validation
   - Proper error handling and logging

4. **Error Information**
   - Error details are logged but sanitized before returning to clients
   - HTTP status codes provide appropriate error categorization
   - Authentication failures are properly handled with 401/403 responses

## Configuration

The system is configured through several mechanisms:

1. **appsettings.json**

   - General application settings
   - Logging configuration
   - Base connection strings
   - API security settings

2. **Environment Variables**

   - `MSSQL_MCP_KEY` - Encryption key for connection strings
   - `MSSQL_MCP_API_KEY` - API key for authentication

3. **SQLite Database**

   - Stores encrypted connection information
   - Maintains connection metadata

4. **PowerShell Scripts**
   - Utility scripts for managing security features
   - Key management and rotation
   - Security assessment tools

## Conclusion

The SQL Server MCP Server provides a robust implementation of the Model Context Protocol for SQL Server databases. With its clean architecture, comprehensive metadata support, connection management capabilities, and security features like API key authentication and connection string encryption, it enables AI assistants like Copilot to effectively and securely explore and interact with SQL Server databases.

The layered architecture allows for separation of concerns, making the codebase maintainable and extensible. Security considerations are addressed at multiple levels, from API authentication to connection string encryption and SQL injection prevention, ensuring that database access is both flexible and secure.
