using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace mssqlMCP.Services;

/// <summary>
/// Repository for managing API keys stored in SQLite
/// </summary>
public interface IApiKeyRepository
{
    Task<IEnumerable<ApiKey>> GetAllApiKeysAsync();
    Task<IEnumerable<ApiKey>> GetApiKeysForUserAsync(string userId);
    Task<ApiKey?> GetApiKeyByIdAsync(string id);
    Task<ApiKey?> GetApiKeyByValueAsync(string keyValue);
    Task<ApiKey> SaveApiKeyAsync(ApiKey apiKey);
    Task<bool> DeleteApiKeyAsync(string id);
    Task<bool> RevokeApiKeyAsync(string id);
    Task UpdateLastUsedAsync(string id);
    Task<IEnumerable<ApiKeyUsageLog>> GetApiKeyUsageLogsAsync(string apiKeyId, int limit = 100);
    Task<IEnumerable<ApiKeyUsageLog>> GetUserUsageLogsAsync(string userId, int limit = 100);
    Task LogApiKeyUsageAsync(ApiKeyUsageLog log);
    Task<bool> ValidateApiKeyAsync(string keyValue);
}

/// <summary>
/// SQLite implementation of the API key repository
/// </summary>
public class SqliteApiKeyRepository : IApiKeyRepository, IDisposable
{
    private readonly ILogger<SqliteApiKeyRepository> _logger;
    private readonly string _connectionString;
    private readonly IEncryptionService _encryptionService;
    private bool _initialized = false;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

    public SqliteApiKeyRepository(ILogger<SqliteApiKeyRepository> logger, IEncryptionService encryptionService)
    {
        _logger = logger;
        _encryptionService = encryptionService;
        var dataDirectory = Environment.GetEnvironmentVariable("MSSQL_MCP_DATA");

        if (!string.IsNullOrEmpty(dataDirectory))
        {
            // Use the specified data directory
            dataDirectory = Path.GetFullPath(dataDirectory);
            // Create data directory if it doesn't exist
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }
        else
        {
            // Use the default data directory
            var appDataDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "mssqlMCP");

            // Create default directory if it doesn't exist
            if (!Directory.Exists(appDataDirectory))
            {
                Directory.CreateDirectory(appDataDirectory);
            }

            dataDirectory = appDataDirectory;
        }

        var dbPath = Path.Combine(dataDirectory, "apikeys.db");
        _connectionString = $"Data Source={dbPath}";
        _logger.LogInformation($"API Key database path: {dbPath}");
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Create ApiKeys table
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ApiKeys (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Key TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ExpirationDate TEXT NULL,
                    LastUsed TEXT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    KeyType TEXT NOT NULL DEFAULT 'user',
                    Description TEXT NULL,
                    AllowedConnectionNames TEXT NULL
                );
            ");

            // Create index on Key for fast lookups
            await connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_apikeys_key ON ApiKeys(Key);");

            // Create index on UserId for filtering by user
            await connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_apikeys_userid ON ApiKeys(UserId);");

            // Create API key usage logs table
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS ApiKeyUsageLogs (
                    Id TEXT PRIMARY KEY,
                    ApiKeyId TEXT NOT NULL,
                    UserId TEXT NOT NULL,
                    Timestamp TEXT NOT NULL,
                    Resource TEXT NOT NULL,
                    Method TEXT NOT NULL,
                    IpAddress TEXT NULL,
                    UserAgent TEXT NULL,
                    FOREIGN KEY (ApiKeyId) REFERENCES ApiKeys(Id)
                );
            ");

            // Create index on ApiKeyId for filtering by key
            await connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_usagelogs_apikeyid ON ApiKeyUsageLogs(ApiKeyId);");

            // Create index on UserId for filtering by user
            await connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_usagelogs_userid ON ApiKeyUsageLogs(UserId);");

            // Create index on Timestamp for filtering by time
            await connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS idx_usagelogs_timestamp ON ApiKeyUsageLogs(Timestamp);");

            _initialized = true;
            _logger.LogInformation("API Key repository initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize API Key repository");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<IEnumerable<ApiKey>> GetAllApiKeysAsync()
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var apiKeys = await connection.QueryAsync<ApiKey>(@"
            SELECT * FROM ApiKeys
            ORDER BY CreatedAt DESC;
        ");

        return apiKeys;
    }

    public async Task<IEnumerable<ApiKey>> GetApiKeysForUserAsync(string userId)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var apiKeys = await connection.QueryAsync<ApiKey>(@"
            SELECT * FROM ApiKeys
            WHERE UserId = @UserId
            ORDER BY CreatedAt DESC;",
            new
            {
                UserId = userId
            });

        return apiKeys;
    }

    public async Task<ApiKey?> GetApiKeyByIdAsync(string id)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var apiKey = await connection.QueryFirstOrDefaultAsync<ApiKey>(@"
            SELECT * FROM ApiKeys
            WHERE Id = @Id",
            new
            {
                Id = id
            });

        return apiKey;
    }

    public async Task<ApiKey?> GetApiKeyByValueAsync(string keyValue)
    {
        if (string.IsNullOrEmpty(keyValue))
        {
            _logger.LogWarning("Attempted to get API key with null or empty value");
            return null;
        }

        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // Use the new helper method that tries multiple strategies to find the key
        return await GetApiKeyByDecryptableValueAsync(keyValue, connection);
    }

    public async Task<ApiKey> SaveApiKeyAsync(ApiKey apiKey)
    {
        await EnsureInitializedAsync();

        // Always encrypt the key before storing it
        apiKey.Key = _encryptionService.Encrypt(apiKey.Key);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO ApiKeys (
                Id, Name, Key, UserId, CreatedAt, ExpirationDate, 
                LastUsed, IsActive, KeyType, Description, AllowedConnectionNames
            ) VALUES (
                @Id, @Name, @Key, @UserId, @CreatedAt, @ExpirationDate,
                @LastUsed, @IsActive, @KeyType, @Description, @AllowedConnectionNames
            );",
            apiKey);

        _logger.LogInformation($"Saved API key: {apiKey.Id} for user {apiKey.UserId}");
        return apiKey;
    }

    public async Task<bool> DeleteApiKeyAsync(string id)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        // First delete any associated usage logs
        await connection.ExecuteAsync(@"
            DELETE FROM ApiKeyUsageLogs
            WHERE ApiKeyId = @Id",
            new
            {
                Id = id
            });

        // Then delete the key
        var rowsAffected = await connection.ExecuteAsync(@"
            DELETE FROM ApiKeys
            WHERE Id = @Id",
            new
            {
                Id = id
            });

        _logger.LogInformation($"Deleted API key: {id}, rows affected: {rowsAffected}");
        return rowsAffected > 0;
    }

    public async Task<bool> RevokeApiKeyAsync(string id)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var rowsAffected = await connection.ExecuteAsync(@"
            UPDATE ApiKeys
            SET IsActive = 0
            WHERE Id = @Id",
            new
            {
                Id = id
            });

        _logger.LogInformation($"Revoked API key: {id}, rows affected: {rowsAffected}");
        return rowsAffected > 0;
    }

    public async Task UpdateLastUsedAsync(string id)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            UPDATE ApiKeys
            SET LastUsed = @LastUsed
            WHERE Id = @Id",
            new
            {
                Id = id,
                LastUsed = DateTime.UtcNow
            });
    }

    public async Task<IEnumerable<ApiKeyUsageLog>> GetApiKeyUsageLogsAsync(string apiKeyId, int limit = 100)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var logs = await connection.QueryAsync<ApiKeyUsageLog>(@"
            SELECT * FROM ApiKeyUsageLogs
            WHERE ApiKeyId = @ApiKeyId
            ORDER BY Timestamp DESC
            LIMIT @Limit",
            new
            {
                ApiKeyId = apiKeyId,
                Limit = limit
            });

        return logs;
    }

    public async Task<IEnumerable<ApiKeyUsageLog>> GetUserUsageLogsAsync(string userId, int limit = 100)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var logs = await connection.QueryAsync<ApiKeyUsageLog>(@"
            SELECT * FROM ApiKeyUsageLogs
            WHERE UserId = @UserId
            ORDER BY Timestamp DESC
            LIMIT @Limit",
            new
            {
                UserId = userId,
                Limit = limit
            });

        return logs;
    }

    public async Task LogApiKeyUsageAsync(ApiKeyUsageLog log)
    {
        await EnsureInitializedAsync();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO ApiKeyUsageLogs (
                Id, ApiKeyId, UserId, Timestamp, Resource, Method, IpAddress, UserAgent
            ) VALUES (
                @Id, @ApiKeyId, @UserId, @Timestamp, @Resource, @Method, @IpAddress, @UserAgent
            );",
            log);
    }

    public async Task<bool> ValidateApiKeyAsync(string keyValue)
    {
        if (string.IsNullOrEmpty(keyValue))
        {
            _logger.LogWarning("Attempted to validate null or empty API key");
            return false;
        }

        await EnsureInitializedAsync();

        try
        {
            _logger.LogDebug("Validating API key with length={Length}", keyValue.Length);

            // Check if the key is already encrypted (shouldn't be, but just in case)
            if (_encryptionService.IsEncrypted(keyValue))
            {
                _logger.LogWarning("API key is already encrypted, which is unexpected");
            }

            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Try to find the API key using multiple strategies
            var apiKey = await GetApiKeyByDecryptableValueAsync(keyValue, connection);

            // Check if key was found
            if (apiKey == null)
            {
                // For debugging purposes, check if there are any active keys in the database
                var activeKeyCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM ApiKeys WHERE IsActive = 1");
                _logger.LogDebug("No matching API key found. Total active keys in database: {Count}", activeKeyCount);
                return false;
            }

            _logger.LogDebug("Found API key in database: {Id}, User: {UserId}", apiKey.Id, apiKey.UserId);

            // Check if key is expired
            if (apiKey.ExpirationDate.HasValue && apiKey.ExpirationDate.Value < DateTime.UtcNow)
            {
                _logger.LogInformation("API key {Id} has expired (expiry: {ExpiryDate})",
                    apiKey.Id,
                    apiKey.ExpirationDate);

                // Key is expired, mark it as inactive
                await RevokeApiKeyAsync(apiKey.Id);
                return false;
            }

            // Update the LastUsed timestamp
            await UpdateLastUsedAsync(apiKey.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating API key");
            return false;
        }
    }

    // New method that tries multiple strategies to validate an API key
    private async Task<ApiKey?> GetApiKeyByDecryptableValueAsync(string keyValue, SqliteConnection connection)
    {
        // Strategy 1: Direct match (for keys stored as plaintext or already encrypted)
        var apiKey = await connection.QueryFirstOrDefaultAsync<ApiKey>(@"
            SELECT * FROM ApiKeys
            WHERE Key = @Key AND IsActive = 1",
            new
            {
                Key = keyValue
            });

        if (apiKey != null)
        {
            _logger.LogDebug("Found API key using direct match");
            return apiKey;
        }

        // Strategy 2: Match with encrypted key
        var encryptedKey = _encryptionService.Encrypt(keyValue);
        apiKey = await connection.QueryFirstOrDefaultAsync<ApiKey>(@"
            SELECT * FROM ApiKeys
            WHERE Key = @Key AND IsActive = 1",
            new
            {
                Key = encryptedKey
            });

        if (apiKey != null)
        {
            _logger.LogDebug("Found API key using encrypted value");
            return apiKey;
        }

        // Strategy 3: Get all active keys and try to compare decrypted values
        // This is a last resort and inefficient but helps with encryption inconsistencies
        var allActiveKeys = await connection.QueryAsync<ApiKey>(@"
            SELECT * FROM ApiKeys
            WHERE IsActive = 1");

        foreach (var key in allActiveKeys)
        {
            string decryptedStoredKey;

            try
            {
                // Try to decrypt the stored key
                decryptedStoredKey = _encryptionService.Decrypt(key.Key);

                // If the decrypted key matches the provided key
                if (decryptedStoredKey == keyValue)
                {
                    _logger.LogDebug("Found API key by decrypting stored key");
                    return key;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error decrypting stored key during validation");
                // Continue to next key
            }
        }

        _logger.LogDebug("No matching API key found using any validation strategy");
        return null;
    }

    public void Dispose()
    {
        _initLock.Dispose();
    }
}
