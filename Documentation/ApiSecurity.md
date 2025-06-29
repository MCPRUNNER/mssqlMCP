### 7. Create Security Documentation

````markdown
# API Security

## Overview

The SQL Server MCP server implements API key authentication to secure the JSON-RPC API endpoint. This document describes how the authentication works and how to use it securely.

## API Key Authentication

### How It Works

1. Each API request must include an API key in the HTTP header.
2. The server validates this key against a configured value.
3. Requests with missing or invalid keys are rejected with appropriate error messages.
4. Valid requests proceed to normal processing.

### Implementation Details

- **Authentication Method**: Bearer token authentication using the `Authorization` header
- **Token Format**: `Bearer <your-api-key>` (without the `Bearer` prefix in configuration)
- **Key Storage**: The API key is stored as an environment variable (`MSSQL_MCP_API_KEY`) or in the application configuration
- **Key Generation**: Keys are generated using cryptographically secure random methods
- **Error Handling**: Proper HTTP status codes (401 for missing/malformed token, 403 for invalid token)

## Setting Up API Key Authentication

### Using the Setup Script

The easiest way to set up API key authentication is to use the provided script:

```powershell
./Scripts/Set-Api-Key.ps1
```
````
