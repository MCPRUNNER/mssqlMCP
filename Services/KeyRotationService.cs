using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using mssqlMCP.Models;

namespace mssqlMCP.Services;

/// <summary>
/// Service that handles encryption key rotation operations
/// </summary>
public interface IKeyRotationService
{
    /// <summary>
    /// Rotates the encryption key and re-encrypts all connection strings
    /// </summary>
    /// <param name="newKey">The new encryption key to use</param>
    /// <returns>Number of re-encrypted connection strings</returns>
    Task<int> RotateKeyAsync(string newKey);

    /// <summary>
    /// Migrates unencrypted connection strings to encrypted format
    /// </summary>
    /// <returns>Number of migrated connection strings</returns>
    Task<int> MigrateUnencryptedConnectionsAsync();
}

/// <summary>
/// Implementation of the key rotation service
/// </summary>
public class KeyRotationService : IKeyRotationService
{
    private readonly ILogger<KeyRotationService> _logger;
    private readonly IConnectionRepository _repository;
    private readonly IEncryptionService _currentEncryptionService;

    public KeyRotationService(
        ILogger<KeyRotationService> logger,
        IConnectionRepository repository,
        IEncryptionService encryptionService)
    {
        _logger = logger;
        _repository = repository;
        _currentEncryptionService = encryptionService;
    }        /// <summary>
             /// Rotates the encryption key and re-encrypts all connection strings
             /// </summary>
             /// <param name="newKey">The new encryption key to use</param>
             /// <returns>Number of re-encrypted connection strings</returns>
    public async Task<int> RotateKeyAsync(string newKey)
    {
        if (string.IsNullOrEmpty(newKey))
        {
            throw new ArgumentException("New encryption key cannot be empty", nameof(newKey));
        }

        try
        {
            _logger.LogInformation("Starting encryption key rotation");                // Create a temporary encryption service with the new key
            var tempEncryptionService = CreateTemporaryEncryptionService(newKey);

            // Get all current connections
            var connections = await _repository.GetAllConnectionsAsync();
            int count = 0;
            int failures = 0;

            foreach (var connection in connections)
            {
                try
                {
                    // The connection is already decrypted by the repository using the current key

                    // Validate the decrypted connection string
                    if (string.IsNullOrEmpty(connection.ConnectionString))
                    {
                        _logger.LogWarning("Connection {Name} has an empty connection string - skipping", connection.Name);
                        continue;
                    }

                    // Re-encrypt with the new key 
                    var reencrypted = new ConnectionEntry
                    {
                        Name = connection.Name,
                        ConnectionString = tempEncryptionService.Encrypt(connection.ConnectionString),
                        ServerType = connection.ServerType,
                        Description = connection.Description,
                        LastUsed = connection.LastUsed,
                        CreatedOn = connection.CreatedOn
                    };

                    // Verify that the new encryption service can decrypt it properly
                    string verifyDecrypt = tempEncryptionService.Decrypt(reencrypted.ConnectionString);
                    if (verifyDecrypt != connection.ConnectionString)
                    {
                        _logger.LogError("Failed to verify encryption for connection {Name} - the decrypted string doesn't match the original", connection.Name);
                        failures++;
                        continue;
                    }

                    // Save with the still-encrypted connection string (repository won't re-encrypt)
                    await _repository.SaveConnectionStringDirectlyAsync(reencrypted);
                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to re-encrypt connection {Name}", connection.Name);
                    failures++;
                }
            }

            if (failures > 0)
            {
                _logger.LogWarning("Key rotation completed with {FailureCount} failures. Successfully re-encrypted {SuccessCount} connection strings",
                    failures, count);
            }
            else
            {
                _logger.LogInformation("Key rotation completed successfully. Re-encrypted {Count} connection strings", count);
            }

            _logger.LogWarning("NOTE: You must restart the application with the new key set in the MSSQL_MCP_KEY environment variable");

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Key rotation failed");
            throw;
        }
    }        /// <summary>
             /// Migrates unencrypted connection strings to encrypted format
             /// </summary>
             /// <returns>Number of migrated connection strings</returns>
    public async Task<int> MigrateUnencryptedConnectionsAsync()
    {
        try
        {
            _logger.LogInformation("Starting migration of unencrypted connection strings");

            // Get all current connections 
            var connections = await _repository.GetAllConnectionsRawAsync();
            int count = 0;
            int failures = 0;

            foreach (var connection in connections)
            {
                try
                {
                    // Check if this connection string is not yet encrypted
                    if (!_currentEncryptionService.IsEncrypted(connection.ConnectionString))
                    {
                        // Validate the unencrypted connection string
                        if (string.IsNullOrEmpty(connection.ConnectionString))
                        {
                            _logger.LogWarning("Connection {Name} has an empty connection string - skipping", connection.Name);
                            continue;
                        }

                        // Encrypt it 
                        string encrypted = _currentEncryptionService.Encrypt(connection.ConnectionString);

                        // Verify the encryption round-trip
                        string decrypted = _currentEncryptionService.Decrypt(encrypted);
                        if (decrypted != connection.ConnectionString)
                        {
                            _logger.LogError("Failed to verify encryption for connection {Name} - the decrypted string doesn't match the original", connection.Name);
                            failures++;
                            continue;
                        }

                        var encryptedConnection = new ConnectionEntry
                        {
                            Name = connection.Name,
                            ConnectionString = encrypted,
                            ServerType = connection.ServerType,
                            Description = connection.Description,
                            LastUsed = connection.LastUsed,
                            CreatedOn = connection.CreatedOn
                        };

                        // Save with the encrypted connection string
                        await _repository.SaveConnectionStringDirectlyAsync(encryptedConnection);
                        count++;

                        _logger.LogInformation("Successfully encrypted connection string for {Name}", connection.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to encrypt connection {Name}", connection.Name);
                    failures++;
                }
            }

            if (failures > 0)
            {
                _logger.LogWarning("Migration completed with {FailureCount} failures. Successfully encrypted {SuccessCount} connection strings",
                    failures, count);
            }
            else
            {
                _logger.LogInformation("Migration completed successfully. Encrypted {Count} connection strings", count);
            }

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Migration failed");
            throw;
        }
    }

    /// <summary>
    /// Creates a temporary encryption service with a specific key
    /// </summary>
    /// <param name="newKey">The encryption key to use</param>
    /// <returns>An encryption service using the specified key</returns>
    protected virtual IEncryptionService CreateTemporaryEncryptionService(string newKey)
    {
        return new TemporaryEncryptionService(newKey);
    }

    /// <summary>
    /// Temporary encryption service that uses a specific key
    /// </summary>
    private class TemporaryEncryptionService : IEncryptionService
    {
        private readonly EncryptionService _encryptionService;

        public TemporaryEncryptionService(string key)
        {
            // Create a logger factory
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<EncryptionService>();

            // Set the environment variable temporarily
            string? originalKey = Environment.GetEnvironmentVariable("MSSQL_MCP_KEY");
            try
            {
                Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", key);
                _encryptionService = new EncryptionService(logger);
            }
            finally
            {
                // Restore the original environment variable
                Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", originalKey);
            }
        }
        public string Encrypt(string plainText) => _encryptionService.Encrypt(plainText);
        public string Decrypt(string cipherText) => _encryptionService.Decrypt(cipherText);
        public bool IsEncrypted(string text) => _encryptionService.IsEncrypted(text);
        public string GenerateSecureKey(int length = 32) => _encryptionService.GenerateSecureKey(length);
    }
}
