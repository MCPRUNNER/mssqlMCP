# Multi-Key API Authentication System

This document explains the multi-key authentication system implemented in mssqlMCP.

## Overview

The multi-key authentication system allows the mssqlMCP server to authenticate requests using both a master API key and multiple user-specific API keys stored in a SQLite database. This provides more robust security, allows for API key rotation, and enables fine-grained access control for different clients or integrations.

**November 2025 Enhancement**: The system now supports **connection-level restrictions**, allowing user API keys to be limited to specific database connections for enhanced security and multi-tenant support.

## Architecture

![Multi-Key Authentication Architecture](architecture_diagram.svg)

### Components

1. **Master API Key**:

   - Environment variable: `MSSQL_MCP_API_KEY`
   - Allows full administrative access, including API key management

2. **Encryption Master Key**:

   - Environment variable: `MSSQL_MCP_KEY`
   - Used to encrypt API keys stored in the SQLite database

3. **SQLite Database**:

   - Stores encrypted API keys and usage logs
   - Located in the same directory as the connection database

4. **ApiKeyAuthMiddleware**:

   - Validates requests against both the master API key and stored keys
   - Enforces connection-level restrictions for user keys
   - Provides role-based authorization for different key types
   - Logs API key usage and security events
   - Handles case-insensitive connection name validation

5. **API Key Manager**:
   - Handles API key creation, validation, and revocation
   - Records usage statistics and enforces security policies

## API Keys Structure

Each API key includes the following attributes:

- **Id**: Unique identifier
- **Name**: Descriptive name for the key
- **Key**: The encrypted API key value
- **UserId**: User or service associated with the key
- **CreatedAt**: Creation timestamp
- **ExpirationDate** (optional): Date when the key becomes invalid
- **LastUsed**: Timestamp of last use
- **IsActive**: Whether the key is currently active
- **KeyType**: Type of key (user, service, admin, etc.)
- **Description**: Optional description of the key's purpose
- **AllowedConnectionNames** (NEW): JSON array of database connection names this key can access

## Connection-Level Security (November 2025 Enhancement)

The multi-key authentication system now supports **connection-level restrictions** for enhanced security:

### How Connection Restrictions Work

- **User API Keys** can be restricted to specific database connections
- **Admin and Master Keys** have unrestricted access to all connections
- Restrictions are enforced at the middleware level before request processing
- Connection names are validated case-insensitively for flexibility

### Connection Restriction Scenarios

1. **Unrestricted Keys**: If `AllowedConnectionNames` is null, empty, or contains an empty array `[]`, the key can access all connections
2. **Restricted Keys**: If `AllowedConnectionNames` contains specific connection names, the key is limited to those connections only
3. **No Connection Parameter**: Requests without a `connectionName` parameter are allowed to proceed (for general operations)

### Security Benefits

- **Principle of Least Privilege**: Keys can be limited to only the databases they need to access
- **Multi-tenant Support**: Different API keys can access different database environments
- **Environment Isolation**: Production, staging, and development databases can be isolated by key
- **Granular Access Control**: Fine-grained control over database access per API key

## Authentication Flow

1. Client includes the API key in the request using one of two methods:

   - `Authorization: Bearer <api-key>` header
   - `X-API-Key: <api-key>` header

2. ApiKeyAuthMiddleware intercepts the request and:

   - Compares the provided key against the master key
   - If not a match, checks against the stored keys in the SQLite database
   - Validates that the key is active and not expired
   - **NEW**: Enforces connection-level restrictions for user keys
   - Logs the API usage for monitoring

3. **Connection Restriction Validation** (for user keys):

   - Extracts `connectionName` parameter from request body
   - Checks if the connection name is in the key's `AllowedConnectionNames` list
   - Returns 403 Forbidden if the connection is not allowed
   - Allows request to proceed if no connection restrictions apply

4. If authentication and authorization succeed, the request is processed; otherwise, a 401 or 403 error is returned.

### Key Type Authorization Levels

- **Master Key**: Full access to all endpoints and all database connections
- **Admin Keys**: Access to all endpoints including management operations and all database connections
- **User Keys**: Limited to data access endpoints (see User-Accessible Endpoints above) and restricted database connections (if configured)

## Management Endpoints

The following MCP endpoints are available for API key management:

- **mssql_create_key**: Create a new API key for a user or service
- **mssql_list_user_keys**: List API keys for a specific user
- **mssql_list_all_keys**: List all API keys in the system (admin only)
- **mssql_revoke_key**: Revoke an API key (mark as inactive)
- **mssql_delete_key**: Permanently delete an API key
- **mssql_get_key_usage_logs**: Get usage logs for a specific API key
- **mssql_get_user_usage_logs**: Get API usage logs for a specific user

### User-Accessible Endpoints

User API keys (non-admin) can access the following endpoints:

**General Operations:**

- **mssql_initialize_connection**: Initialize the SQL Server connection
- **mssql_list_connections**: List all available database connections

**Database Query and Metadata:**

- **mssql_execute_query**: Execute SQL queries and return results as JSON
- **mssql_get_table_metadata**: Get detailed metadata about database tables, columns, primary keys and foreign keys
- **mssql_get_database_objects_metadata**: Get detailed metadata about database objects including tables and views
- **mssql_get_database_objects_by_type**: Get detailed metadata about specific database object types

**SQL Server Features:**

- **mssql_get_agent_jobs**: Get SQL Server Agent job metadata from msdb
- **mssql_get_agent_job_details**: Get detailed information for a specific SQL Server Agent job
- **mssql_get_ssis_catalog_info**: Get SSIS catalog information including deployment models
- **mssql_get_azure_devops_info**: Get Azure DevOps information including projects, repositories, builds, and work items

**Protocol Operations:**

- **notifications/initialized**: MCP protocol initialization notifications
- **tools/list**: List available MCP tools

### Admin-Only Endpoints

The following endpoints require admin or master API key access:

**Connection Management:**

- **mssql_add_connection**: Add a new database connection
- **mssql_update_connection**: Update an existing database connection
- **mssql_remove_connection**: Remove a database connection
- **mssql_test_connection**: Test a database connection

**Security Management:**

- **mssql_create_key**: Create new API keys
- **mssql_list_user_keys**: List API keys for users
- **mssql_list_all_keys**: List all API keys in the system
- **mssql_revoke_key**: Revoke API keys
- **mssql_delete_key**: Delete API keys
- **mssql_get_key_usage_logs**: Get API key usage logs
- **mssql_get_user_usage_logs**: Get user usage logs

**Encryption and Security:**

- **mssql_generate_key**: Generate secure random keys for encryption
- **mssql_rotate_key**: Rotate encryption key for connection strings
- **mssql_migrate_connections**: Migrate unencrypted connection strings to encrypted format

These endpoints follow the same JSON-RPC 2.0 format as other MCP endpoints. See the API_ENDPOINTS.md document for detailed usage examples.

**Note**: The API key management tools use the `mssql_` prefix naming convention, distinguishing them from the general database operation tools.

### API Key Management Tool Naming Convention

All API key management tools follow the `mssql_` prefix convention:

| Function            | Tool Name                   | Description                                |
| ------------------- | --------------------------- | ------------------------------------------ |
| Create API Key      | `mssql_create_key`          | Create a new API key for a user or service |
| List User Keys      | `mssql_list_user_keys`      | List API keys for a specific user          |
| List All Keys       | `mssql_list_all_keys`       | List all API keys (admin only)             |
| Revoke Key          | `mssql_revoke_key`          | Revoke an API key                          |
| Delete Key          | `mssql_delete_key`          | Delete an API key                          |
| Get Key Usage Logs  | `mssql_get_key_usage_logs`  | Get usage logs for a specific API key      |
| Get User Usage Logs | `mssql_get_user_usage_logs` | Get usage logs for a user                  |

This naming convention helps distinguish API key management operations from general database operations and provides consistency across the mssqlMCP toolset.

**Important**: The middleware authorization still references the method names without the `mssql_` prefix for internal processing, but all external tool calls must use the `mssql_` prefixed names.

## Security Considerations

1. **Master Key Protection**: The master API key should be stored securely and rotated periodically.

2. **Encryption**: All API keys are encrypted at rest in the SQLite database using the master encryption key.

3. **Key Expiration**: API keys can be configured with expiration dates for automatic invalidation.

4. **Usage Logging**: All API key usage is logged for audit and security monitoring.

5. **Revocation**: API keys can be immediately revoked if compromised.

6. **Connection-Level Security**: User API keys can be restricted to specific database connections for enhanced security.

7. **Role-Based Access**: Different key types have different access levels to endpoints and operations.

8. **Case-Insensitive Validation**: Connection name validation is case-insensitive to prevent access issues while maintaining security.

## Adding New API Keys

### Creating an API Key with Connection Restrictions

To create a new API key with database connection restrictions, use the mssql_create_key endpoint:

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
    "name": "mssql_create_key",
    "arguments": {
      "request": {
        "name": "Development Environment API Key",
        "userId": "developer@company.com",
        "keyType": "user",
        "expirationDate": "2026-06-29T00:00:00Z",
        "allowedConnectionNames": ["DevDatabase", "TestDatabase"]
      }
    }
  }
}
```

### Creating an Unrestricted User API Key

To create a user API key without connection restrictions:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "mssql_create_key",
    "arguments": {
      "request": {
        "name": "Unrestricted User API Key",
        "userId": "poweruser@company.com",
        "keyType": "user",
        "expirationDate": "2026-06-29T00:00:00Z",
        "allowedConnectionNames": []
      }
    }
  }
}
```

### Creating an Admin API Key

Admin keys automatically have access to all connections:

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "mssql_create_key",
    "arguments": {
      "request": {
        "name": "Admin API Key",
        "userId": "admin@company.com",
        "keyType": "admin",
        "expirationDate": "2026-12-31T00:00:00Z"
      }
    }
  }
}
```

The response will include the API key value, which should be stored securely, as it cannot be retrieved later.

### Additional API Key Management Examples

#### Listing User API Keys

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "mssql_list_user_keys",
    "arguments": {
      "userId": "developer@company.com"
    }
  }
}
```

#### Revoking an API Key

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "mssql_revoke_key",
    "arguments": {
      "request": {
        "id": "api-key-id-here"
      }
    }
  }
}
```

#### Getting API Key Usage Logs

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "mssql_get_key_usage_logs",
    "arguments": {
      "apiKeyId": "api-key-id-here",
      "limit": 50
    }
  }
}
```

## Connection Restriction Examples

### Valid Request with Allowed Connection

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "mssql_execute_query",
    "arguments": {
      "connectionName": "DevDatabase",
      "query": "SELECT * FROM Users"
    }
  }
}
```

### Forbidden Request with Disallowed Connection

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "tools/call",
  "params": {
    "name": "mssql_execute_query",
    "arguments": {
      "connectionName": "ProductionDatabase",
      "query": "SELECT * FROM Users"
    }
  }
}
```

**Response**: HTTP 403 Forbidden

```json
{
  "error": "Forbidden",
  "message": "Access to connection 'ProductionDatabase' is not allowed with the provided API key."
}
```

## Best Practices for Connection Restrictions

### Environment Isolation

- **Development Keys**: Restrict to development and test databases only
- **Production Keys**: Use admin keys or carefully controlled user keys for production access
- **Service Keys**: Limit service accounts to only the databases they need

### Connection Naming Strategy

- Use consistent, descriptive connection names (e.g., "Prod-CustomerDB", "Dev-AnalyticsDB")
- Implement naming conventions that reflect environment and purpose
- Document connection names and their purposes

### Key Management Strategies

- **Principle of Least Privilege**: Always start with the most restrictive access and expand as needed
- **Regular Audits**: Periodically review API key permissions and connection restrictions
- **Key Rotation**: Implement regular key rotation policies, especially for keys with broad access
- **Monitoring**: Monitor API key usage to detect unusual access patterns

### Multi-Tenant Scenarios

- Create separate API keys for each tenant with restrictions to their specific databases
- Use connection naming that includes tenant identifiers
- Implement automated key provisioning for new tenants
