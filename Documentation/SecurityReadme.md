# Security Features

The SQL Server MCP server includes robust security features to protect sensitive information such as connection strings.

## Connection String Encryption

All connection strings stored in the SQLite database are encrypted using AES-256 encryption with the following security measures:

- **AES-256 Encryption**: Industry-standard encryption algorithm
- **Environment Variable Key**: The encryption key is derived from the `MSSQL_MCP_KEY` environment variable
- **Unique IV Per Connection**: Each connection string uses a unique Initialization Vector
- **PBKDF2 Key Derivation**: Key is derived with 10,000 iterations for enhanced security

### Starting with Encryption Enabled

To run the server with encryption enabled, use the provided script:

```powershell
$env:MSSQL_MCP_KEY = "yourEncryptionKey"
$env:MSSQL_MCP_API_KEY = "YourApiKey"
./Start-MCP-Encrypted.ps1
```

This script:

1. Generates a random encryption key if one is not provided
2. Sets the key as an environment variable for the current session
3. Displays the key for you to save securely
4. Starts the MCP server

For production environments, you should set the environment variable externally.

## Key Rotation

The server supports rotating the encryption key to comply with security best practices. To rotate the key:

```powershell
./Rotate-Encryption-Key.ps1
```

This script:

1. Generates a new random encryption key (or you can provide your own)
2. Re-encrypts all connection strings using the new key
3. Displays the new key for you to save

After running the key rotation script, you must restart the server with the new key.

## Migrating Unencrypted Connections

To migrate existing unencrypted connection strings to encrypted format:

```powershell
./Migrate-To-Encrypted.ps1
```

This script will encrypt any unencrypted connection strings in the database.

## Enhanced Security Features

### Connection Validation

When rotating keys or encrypting connections, the system:

- Validates input connections before processing
- Verifies encryption round-trip to ensure data integrity
- Tracks and reports any failures during the process
- Provides detailed logs of operations

### Security Assessment

Use the included security assessment scripts to evaluate your connection security:

```powershell
# Assess all connections for encryption status
./Assess-Connection-Security.ps1

# Comprehensive verification of encryption settings
./Verify-Encryption-Status.ps1
```

These scripts:

- Analyze all connections to identify encrypted vs unencrypted connections
- Report the encryption status of each connection
- Check if the encryption key is properly set
- Offer to generate a new secure key if needed
- Provide guidance on securing your connections

### Secure Key Generation

You can generate cryptographically secure random keys:

```powershell
# Via MCP command
#security.generateSecureKey length=32
```

### API Improvements

The server now includes improved API handling:

- Proper content type negotiation
- Better error handling for HTTP 406 errors
- Automatic header correction for client requests

## MCP Security Commands

The following MCP commands are available for security operations:

```
# Rotate the encryption key
#security.rotateKey newKey="your-new-key"

# Migrate unencrypted connections to encrypted format
#security.migrateConnectionsToEncrypted

# Generate a secure random key
#security.generateSecureKey length=32
```

For detailed security information, see the [Security Documentation](./Security.md).
