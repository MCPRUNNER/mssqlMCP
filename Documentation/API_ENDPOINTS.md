# SQL Server MCP API Endpoints

This document provides comprehensive examples for using the SQL Server MCP (Model Context Protocol) API endpoints via JSON-RPC 2.0.

## Base URL and Authentication

**Base URL**: `http://localhost:3001/mcp`  
**Protocol**: JSON-RPC 2.0 over HTTP POST

### Headers

All requests should include the following headers:

```http
Content-Type: application/json
Accept: application/json, text/event-stream
```

### Authentication

The server supports two methods of authentication:

1. **Bearer Token**:

   ```http
   Authorization: Bearer <your-api-key>
   ```

2. **X-API-Key Header**:
   ```http
   X-API-Key: <your-api-key>
   ```

Authentication configuration:

- Master API Key: Environment variable `MSSQL_MCP_API_KEY` or `appsettings.json` under `ApiSecurity.ApiKey`
- User API Keys: Stored in SQLite database (apikeys.db)
- Encryption Master Key: Environment variable `MSSQL_MCP_KEY` for encrypting stored API keys
- Authentication is optional if no API key is configured

The system supports multi-key authentication:

- Master key has full system access, including managing other API keys
- User keys are stored in an encrypted database and can be revoked/expired
- All API requests are authenticated against both the master key and stored keys

## Tools/List Endpoint

Retrieve all available tools from the MCP server.

### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/list",
  "params": {}
}
```

### Response

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "tools": [
      {
        "name": "ExecuteQuery",
        "description": "Executes a SQL query and returns the results as JSON.",
        "inputSchema": {
          "type": "object",
          "properties": {
            "connectionName": {
              "type": "string",
              "default": "DefaultConnection"
            },
            "query": {
              "type": "string"
            }
          },
          "required": ["query"]
        }
      }
      // ... other tools
    ]
  }
}
```

## Tools/Call Endpoint

Call a specific tool with its parameters.

### Request Format

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": "unique-request-id",
  "method": "tools/call",
  "params": {
    "name": "ToolName",
    "arguments": {
      // tool-specific arguments
    }
  }
}
```

### Response Format

```json
{
  "jsonrpc": "2.0",
  "id": "unique-request-id",
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Human-readable response"
      }
    ],
    "structuredContent": {
      // Tool-specific structured response
    },
    "isError": false
  }
}
```

## Direct Tool Invocation Examples

Below are examples for directly invoking each available tool using their method names.

### 1. Initialize

Initializes a SQL Server connection.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "Initialize",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "success": true,
    "message": "Connection initialized successfully"
  }
}
```

### 2. ExecuteQuery

Executes a SQL query and returns the results.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "ExecuteQuery",
  "params": {
    "connectionName": "DefaultConnection",
    "query": "SELECT TOP 10 * FROM sys.databases"
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {
    "columns": ["name", "database_id", "create_date"],
    "rows": [
      ["master", 1, "2023-01-01T00:00:00"],
      ["tempdb", 2, "2023-01-01T00:00:00"],
      ["model", 3, "2023-01-01T00:00:00"],
      ["msdb", 4, "2023-01-01T00:00:00"]
    ]
  }
}
```

### 3. GetTableMetadata

Retrieves table metadata including columns, primary keys, and foreign keys.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "GetTableMetadata",
    "arguments": {
      "connectionName": "DefaultConnection",
      "schema": "dbo"
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "tables": [
      {
        "name": "Customers",
        "schema": "dbo",
        "columns": [
          {
            "name": "CustomerId",
            "dataType": "int",
            "isNullable": false,
            "isPrimaryKey": true
          },
          {
            "name": "Name",
            "dataType": "nvarchar",
            "isNullable": false,
            "isPrimaryKey": false
          }
        ],
        "primaryKeys": ["CustomerId"],
        "foreignKeys": []
      }
    ]
  }
}
```

### 4. ListConnections

Lists all available database connections.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "ListConnections",
  "params": {}
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "result": {
    "connections": [
      {
        "name": "DefaultConnection",
        "description": "Default SQL Server connection"
      },
      {
        "name": "ProductionDB",
        "description": "Production database connection"
      }
    ]
  }
}
```

### 5. AddConnection

Adds a new database connection.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "AddConnection",
  "params": {
    "request": {
      "name": "NewConnection",
      "connectionString": "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;",
      "description": "My new database connection"
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "result": {
    "success": true,
    "message": "Connection added successfully"
  }
}
```

### 6. RemoveConnection

Removes an existing database connection.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "method": "RemoveConnection",
  "params": {
    "request": {
      "name": "ConnectionToRemove"
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "result": {
    "success": true,
    "message": "Connection removed successfully"
  }
}
```

### 7. UpdateConnection

Updates an existing database connection.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "method": "UpdateConnection",
  "params": {
    "request": {
      "name": "ExistingConnection",
      "connectionString": "Server=newserver;Database=newdb;User Id=newuser;Password=newpassword;",
      "description": "Updated connection"
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "result": {
    "success": true,
    "message": "Connection updated successfully"
  }
}
```

### 8. TestConnection

Tests a database connection string.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "TestConnection",
  "params": {
    "request": {
      "connectionString": "Server=myserver;Database=mydb;User Id=myuser;Password=mypassword;"
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "result": {
    "success": true,
    "message": "Connection test successful",
    "serverVersion": "Microsoft SQL Server 2019"
  }
}
```

### 9. GetDatabaseObjectsMetadata

Gets detailed metadata about database objects including tables and views.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "method": "tools/call",
  "params": {
    "name": "GetDatabaseObjectsMetadata",
    "arguments": {
      "connectionName": "myDB_ConnectionName",
      "schema": "dbo",
      "includeViews": true
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "result": {
    "objects": [
      {
        "name": "Customers",
        "schema": "dbo",
        "type": "TABLE"
      },
      {
        "name": "ActiveCustomers",
        "schema": "dbo",
        "type": "VIEW"
      }
    ]
  }
}
```

### 10. GetDatabaseObjectsByType

Gets detailed metadata about specific database object types.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "method": "GetDatabaseObjectsByType",
  "params": {
    "connectionName": "DefaultConnection",
    "schema": "dbo",
    "objectType": "TABLE"
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "result": {
    "objects": [
      {
        "name": "Customers",
        "schema": "dbo",
        "type": "TABLE",
        "createDate": "2023-01-01T00:00:00"
      },
      {
        "name": "Orders",
        "schema": "dbo",
        "type": "TABLE",
        "createDate": "2023-01-01T00:00:00"
      }
    ]
  }
}
```

### 11. GetSqlServerAgentJobs

Gets SQL Server Agent job metadata.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "method": "GetSqlServerAgentJobs",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "result": {
    "jobs": [
      {
        "name": "DatabaseBackup",
        "description": "Daily backup job",
        "enabled": true,
        "category": "Database Maintenance",
        "lastRunOutcome": "Succeeded",
        "lastRunDate": "2025-06-28T01:00:00"
      }
    ]
  }
}
```

### 12. GetSqlServerAgentJobDetails

Gets detailed information for a specific SQL Server Agent job.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "method": "GetSqlServerAgentJobDetails",
  "params": {
    "connectionName": "DefaultConnection",
    "jobName": "DatabaseBackup"
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "result": {
    "job": {
      "name": "DatabaseBackup",
      "description": "Daily backup job",
      "enabled": true,
      "category": "Database Maintenance",
      "steps": [
        {
          "stepId": 1,
          "stepName": "Backup System Databases",
          "command": "BACKUP DATABASE master TO DISK='C:\\Backups\\master.bak'",
          "subsystem": "TSQL"
        }
      ],
      "schedules": [
        {
          "name": "Daily at 1am",
          "enabled": true,
          "frequencyType": "Daily",
          "frequencyInterval": 1,
          "startTime": "01:00:00"
        }
      ],
      "history": [
        {
          "runDate": "2025-06-28T01:00:00",
          "outcome": "Succeeded",
          "duration": "00:05:32",
          "message": "The job succeeded."
        }
      ]
    }
  }
}
```

### 13. GetSsisCatalogInfo

Gets SSIS catalog information.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 13,
  "method": "GetSsisCatalogInfo",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 13,
  "result": {
    "folders": [
      {
        "name": "ETL",
        "projects": [
          {
            "name": "DataWarehouseETL",
            "packages": [
              {
                "name": "LoadDimCustomer.dtsx",
                "description": "Loads customer dimension"
              }
            ]
          }
        ]
      }
    ]
  }
}
```

### 14. GetAzureDevOpsInfo

Gets Azure DevOps information including projects and repositories.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 14,
  "method": "GetAzureDevOpsInfo",
  "params": {
    "connectionName": "DefaultConnection"
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 14,
  "result": {
    "projects": [
      {
        "name": "DatabaseProject",
        "repositories": [
          {
            "name": "SqlScripts",
            "url": "https://dev.azure.com/organization/DatabaseProject/_git/SqlScripts"
          }
        ],
        "builds": [
          {
            "id": 12345,
            "name": "Database CI Build",
            "status": "succeeded",
            "lastRunDate": "2025-06-27T10:30:00"
          }
        ],
        "workItems": [
          {
            "id": 67890,
            "title": "Update customer schema",
            "state": "Active",
            "type": "User Story"
          }
        ]
      }
    ]
  }
}
```

### 15. GenerateSecureKey

Generates a secure random key for connection string encryption.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 15,
  "method": "GenerateSecureKey",
  "params": {
    "length": 32
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 15,
  "result": {
    "key": "Uew8Ap2aiZoh5Wae/XiaNX2PVHXpnC6kPVX0Tcow4FA="
  }
}
```

### 16. RotateKey

Rotates the encryption key for connection strings.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 16,
  "method": "RotateKey",
  "params": {
    "newKey": "Uew8Ap2aiZoh5Wae/XiaNX2PVHXpnC6kPVX0Tcow4FA="
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 16,
  "result": {
    "success": true,
    "message": "Encryption key rotated successfully. All connection strings re-encrypted."
  }
}
```

### 17. MigrateConnectionsToEncrypted

Migrates unencrypted connection strings to encrypted format.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 17,
  "method": "MigrateConnectionsToEncrypted",
  "params": {}
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 17,
  "result": {
    "success": true,
    "message": "Successfully migrated 3 connections from unencrypted to encrypted format."
  }
}
```

## API Key Management Endpoints

The following endpoints enable management of API keys in the system. All these operations require authentication with the master API key.

### Create API Key

Create a new API key for a user or service.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Authorization: Bearer <master-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "CreateApiKey",
    "arguments": {
      "name": "My Service API Key",
      "userId": "service-123",
      "keyType": "service",
      "expirationDate": "2026-06-29T00:00:00Z",
      "allowedConnectionNames": ["conn1", "conn2"]
    }
  }
}
```

Alternative direct method format:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "CreateApiKey",
  "params": {
    "request": {
      "name": "My Service API Key",
      "userId": "service-123",
      "keyType": "service",
      "expirationDate": "2026-06-29T00:00:00Z",
      "allowedConnectionNames": ["conn1", "conn2"]
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "id": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
    "name": "My Service API Key",
    "key": "ak_1a2b3c4d5e6f7g8h9i0j...",
    "userId": "service-123",
    "createdAt": "2025-06-29T14:32:11.123456Z",
    "expirationDate": "2026-06-29T00:00:00Z",
    "lastUsed": null,
    "isActive": true,
    "keyType": "service",
    "description": "",
    "allowedConnectionNames": ["conn1", "conn2"]
  }
}
```

> **Note**: The `key` value is only returned once when the API key is created. Store it securely, as it cannot be retrieved later.

### List API Keys for a User

List all API keys for a specific user.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Authorization: Bearer <master-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/call",
  "params": {
    "name": "ListUserApiKeys",
    "arguments": {
      "userId": "service-123"
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": [
    {
      "id": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
      "name": "My Service API Key",
      "key": null,
      "userId": "service-123",
      "createdAt": "2025-06-29T14:32:11.123456Z",
      "expirationDate": "2026-06-29T00:00:00Z",
      "lastUsed": "2025-06-29T14:35:42.654321Z",
      "isActive": true,
      "keyType": "service",
      "description": "",
      "allowedConnectionNames": null
    }
  ]
}
```

### List All API Keys (Admin Only)

List all API keys in the system.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Authorization: Bearer <master-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "ListAllApiKeys",
    "arguments": {}
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": [
    {
      "id": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
      "name": "My Service API Key",
      "userId": "service-123",
      "createdAt": "2025-06-29T14:32:11.123456Z",
      "expirationDate": "2026-06-29T00:00:00Z",
      "lastUsed": "2025-06-29T14:35:42.654321Z",
      "isActive": true,
      "keyType": "service",
      "allowedConnectionNames": null
    },
    {
      "id": "9a8b7c6d-5e4f-3g2h-1i0j-klm123456789",
      "name": "User Dashboard Key",
      "userId": "user-456",
      "createdAt": "2025-06-25T09:12:33.123456Z",
      "expirationDate": null,
      "lastUsed": "2025-06-29T10:15:22.654321Z",
      "isActive": true,
      "keyType": "user",
      "allowedConnectionNames": null
    }
  ]
}
```

### Revoke API Key

Revoke an API key (mark as inactive).

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Authorization: Bearer <master-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "tools/call",
  "params": {
    "name": "RevokeApiKey",
    "arguments": {
      "request": {
        "id": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb"
      }
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "result": true
}
```

### Delete API Key

Permanently delete an API key.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Authorization: Bearer <master-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "tools/call",
  "params": {
    "name": "DeleteApiKey",
    "arguments": {
      "id": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb"
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "result": true
}
```

### Get API Key Usage Logs

Get usage logs for a specific API key.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Authorization: Bearer <master-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "method": "tools/call",
  "params": {
    "name": "GetApiKeyUsageLogs",
    "arguments": {
      "apiKeyId": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
      "limit": 100
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "result": [
    {
      "id": "log-123456",
      "apiKeyId": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
      "userId": "service-123",
      "timestamp": "2025-06-29T14:35:42.654321Z",
      "resource": "/mcp",
      "method": "POST",
      "ipAddress": "192.168.1.100",
      "userAgent": "curl/7.79.1"
    },
    {
      "id": "log-123457",
      "apiKeyId": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
      "userId": "service-123",
      "timestamp": "2025-06-29T14:36:15.123456Z",
      "resource": "/mcp",
      "method": "POST",
      "ipAddress": "192.168.1.100",
      "userAgent": "curl/7.79.1"
    }
  ]
}
```

### Get User Usage Logs

Get API usage logs for a specific user.

#### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Authorization: Bearer <master-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "method": "tools/call",
  "params": {
    "name": "GetUserUsageLogs",
    "arguments": {
      "userId": "service-123",
      "limit": 100
    }
  }
}
```

#### Response

```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "result": [
    {
      "id": "log-123456",
      "apiKeyId": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
      "userId": "service-123",
      "timestamp": "2025-06-29T14:35:42.654321Z",
      "resource": "/mcp",
      "method": "POST",
      "ipAddress": "192.168.1.100",
      "userAgent": "curl/7.79.1"
    },
    {
      "id": "log-123457",
      "apiKeyId": "75e6d5f3-c851-4c7a-a4b7-874269a31bbb",
      "userId": "service-123",
      "timestamp": "2025-06-29T14:36:15.123456Z",
      "resource": "/mcp",
      "method": "POST",
      "ipAddress": "192.168.1.100",
      "userAgent": "curl/7.79.1"
    }
  ]
}
```

## Copilot Agent Usage

When GitHub Copilot uses the MCP server, it sends requests to the server via the JSON-RPC format with the `tools/call` method. Here's an example of how Copilot would retrieve table metadata:

### Request

```http
POST http://localhost:3001/mcp
Content-Type: application/json
Accept: application/json, text/event-stream
Authorization: Bearer <your-api-key>
```

```json
{
  "jsonrpc": "2.0",
  "id": "copilot-request-1",
  "method": "tools/call",
  "params": {
    "name": "GetTableMetadata",
    "arguments": {
      "connectionName": "DefaultConnection",
      "schema": null
    }
  }
}
```

### Response

```json
{
  "jsonrpc": "2.0",
  "id": "copilot-request-1",
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Table metadata retrieved successfully"
      }
    ],
    "structuredContent": {
      "tables": [
        {
          "name": "Users",
          "schema": "dbo",
          "columns": [
            {
              "name": "UserId",
              "dataType": "int",
              "isNullable": false,
              "isPrimaryKey": true
            },
            { "name": "Username", "dataType": "nvarchar", "isNullable": false },
            { "name": "Email", "dataType": "nvarchar", "isNullable": false }
          ]
        }
      ]
    },
    "isError": false
  }
}
```

## Error Handling

### Standard JSON-RPC Error Format

```json
{
  "jsonrpc": "2.0",
  "id": "request-id",
  "error": {
    "code": -32602,
    "message": "Invalid parameters: connection string is required"
  }
}
```

### Common Error Codes

- **-32600**: Invalid Request - The JSON sent is not a valid Request object
- **-32601**: Method Not Found - The method does not exist / is not available
- **-32602**: Invalid Params - Invalid method parameter(s)
- **-32603**: Internal Error - Internal JSON-RPC error
- **-32700**: Parse Error - Invalid JSON was received by the server

### Tool-Specific Error (using tools/call)

```json
{
  "jsonrpc": "2.0",
  "id": "request-id",
  "result": {
    "content": [
      {
        "type": "text",
        "text": "Failed to execute ExecuteQuery: Invalid SQL syntax near 'SELET'"
      }
    ],
    "isError": true
  }
}
```

## Security Best Practices

1. Always use HTTPS in production environments
2. Rotate API keys regularly
3. Use encrypted connection strings
4. Implement proper error handling to prevent information leakage
5. Utilize connection pooling to manage database connections efficiently
6. Apply principle of least privilege for database accounts
7. Keep the MCP server updated with security patches
