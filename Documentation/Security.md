# SQL Server MCP Security Features

This document provides details about the security features implemented in the SQL Server MCP server.

## Connection String Encryption

Connection strings often contain sensitive information such as server names, database names, usernames, and passwords. To protect this information, the SQL Server MCP server implements AES-256 encryption for all connection strings stored in the SQLite database.

### Encryption Implementation

The encryption is implemented using the following components:

1. **EncryptionService** - A service that provides encryption and decryption capabilities using AES-256 encryption.
2. **Environment Variable Key** - The encryption key is derived from the `MSSQL_MCP_KEY` environment variable.
3. **Key Derivation** - The actual encryption key is derived using PBKDF2 (Rfc2898DeriveBytes) with 10,000 iterations.
4. **Per-Connection IV** - Each connection string uses a unique Initialization Vector (IV) to enhance security.

### How It Works

When a new connection is added or an existing one is updated:

1. The plain-text connection string is encrypted using AES-256 with a unique IV.
2. The IV is prepended to the encrypted data.
3. The result is Base64-encoded and prefixed with "ENC:" to indicate it's encrypted.
4. This encrypted string is stored in the SQLite database.

When a connection is retrieved:

1. The system checks if the connection string starts with "ENC:".
2. If it does, the "ENC:" prefix is removed, and the remaining Base64 string is decoded.
3. The IV is extracted from the first 16 bytes of the decoded data.
4. The data is decrypted using the same key and the extracted IV.
5. The plain-text connection string is returned for use.

### Enabling Encryption

To enable secure connection string encryption:

1. Set the `MSSQL_MCP_KEY` environment variable to a strong random value:

   ```powershell
   $env:MSSQL_MCP_KEY = "your-strong-random-key"
   ```

2. Use the provided script to start the server:
   ```powershell
   ./Start-MCP-Encrypted.ps1
   ```

### Fallback Mechanism

If the `MSSQL_MCP_KEY` environment variable is not set, the system will:

1. Log a warning that connection strings will not be securely encrypted.
2. Use a default insecure key to allow the system to continue functioning.
3. Continue to encrypt/decrypt connection strings, but with reduced security.

This fallback mechanism ensures the system remains operational but is not recommended for production use.

## Error Handling

The encryption system is designed to gracefully handle errors:

1. If encryption fails, the system will log the error and return the plain-text connection string.
2. If decryption fails (e.g., if the encryption key has changed), the system will log the error and return the encrypted string.

This approach ensures that a change in the encryption key won't completely break existing connections, allowing administrators time to update connection strings.

## Security Recommendations

For optimal security:

1. **Use a Strong Key**: The encryption key should be at least 32 characters long and include a mix of letters, numbers, and special characters.

2. **Rotate Keys Regularly**: Periodically change the encryption key. When changing the key, you'll need to:

   - Retrieve all connections using the old key
   - Re-encrypt them with the new key
   - Save the updated encrypted strings

3. **Protect the Environment Variable**: Ensure the environment variable is set securely and not visible in logs or process listings.

4. **Use Secure SQL Authentication**: In addition to encrypting connection strings, use SQL authentication methods that follow security best practices.

5. **Apply Principle of Least Privilege**: Ensure SQL accounts used in connection strings have only the permissions they need.

## Implementation Details

The encryption implementation uses:

- **AES-256**: A symmetric encryption algorithm widely considered secure for sensitive data.
- **PBKDF2 (Rfc2898DeriveBytes)**: A key derivation function that applies a pseudorandom function to the input key along with a salt value.
- **Unique IV Per Connection**: Each connection string gets its own Initialization Vector to prevent pattern analysis.
- **Safe Error Handling**: The system doesn't throw exceptions for encryption/decryption errors but logs them and continues operation.

## Enhanced Security Features

### Connection Validation

When rotating keys or encrypting connections, the system now:

1. **Validates Input Connections**: Checks for null or empty connection strings.
2. **Verifies Encryption Round-Trip**: Ensures the decrypted text matches the original after encryption.
3. **Reports Failures**: Tracks and logs any failures during the process.
4. **Provides Detailed Logs**: Includes counts of successful and failed operations.

### Security Assessment

A new script `Assess-Connection-Security.ps1` helps you evaluate the security of your connections:

```powershell
./Assess-Connection-Security.ps1
```

This script:

1. Analyzes all connections to identify encrypted vs unencrypted connections
2. Reports the encryption status of each connection
3. Checks if the encryption key is properly set
4. Offers to generate a new secure key if needed
5. Provides guidance on securing your connections

### Secure Key Generation

You can now generate cryptographically secure random keys:

#### Via PowerShell Script

```powershell
./Assess-Connection-Security.ps1
```

Then select the option to generate a new key when prompted.

#### Via MCP Command

```
#security.generateSecureKey length=32
```

This returns a Base64-encoded random key suitable for encryption.

## Key Rotation Feature

The SQL Server MCP server now includes a key rotation feature that allows you to change the encryption key without losing access to your encrypted connection strings.

### How Key Rotation Works

1. **Extract and Decrypt**: Uses the current encryption key to decrypt all connection strings
2. **Re-encrypt**: Encrypts the connection strings using the new key
3. **Store**: Saves the re-encrypted connection strings back to the database

### Using Key Rotation

#### Via PowerShell Script

The easiest way to rotate your encryption key is using the provided PowerShell script:

```powershell
./Rotate-Encryption-Key.ps1
```

This script will:

1. Generate a new random encryption key (or you can provide your own)
2. Use the existing key to decrypt connection strings
3. Re-encrypt them with the new key
4. Display the new key for you to save

You can also provide your own key:

```powershell
./Rotate-Encryption-Key.ps1 -NewKey "your-custom-key"
```

#### Via MCP Command

You can also rotate keys using an MCP command:

```
#security.rotateKey newKey="your-new-key"
```

### After Key Rotation

After rotating the key, you must:

1. Save the new key securely
2. Set the new key as the `MSSQL_MCP_KEY` environment variable
3. Restart the MCP server

## Migrating Unencrypted Connections

If you have existing unencrypted connection strings in your database, you can migrate them to encrypted format using:

```powershell
./Migrate-To-Encrypted.ps1
```

Or via MCP:

```
#security.migrateConnectionsToEncrypted
```

This will encrypt any unencrypted connection strings using the current encryption key.
