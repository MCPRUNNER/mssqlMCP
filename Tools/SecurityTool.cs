using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using mssqlMCP.Models;
using mssqlMCP.Services;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using mssqlMCP.Validation;

namespace mssqlMCP.Tools;

/// <summary>
/// MCP tool for security operations like key rotation
/// </summary>
[McpServerToolType]
public class SecurityTool
{
    private readonly ILogger<SecurityTool> _logger;
    private readonly IKeyRotationService _keyRotationService;
    private readonly IEncryptionService _encryptionService;

    public SecurityTool(
        ILogger<SecurityTool> logger,
        IKeyRotationService keyRotationService,
        IEncryptionService encryptionService)
    {
        _logger = logger;
        _keyRotationService = keyRotationService;
        _encryptionService = encryptionService;
    }    /// <summary>
         /// Rotate the encryption key for all connection strings
         /// </summary>
    [McpServerTool, Description("Rotate encryption key for connection strings")]
    public async Task<object> RotateKeyAsync(string newKey)
    {
        // Validate input parameters
        var validationResult = InputValidator.ValidateEncryptionKey(newKey);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Invalid encryption key: {validationResult.ErrorMessage}";
            _logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage);
        }

        _logger.LogInformation("Request to rotate encryption key received");

        try
        {            // Perform key rotation
            var count = await _keyRotationService.RotateKeyAsync(newKey);

            // Return success response
            var result = new
            {
                count,
                message = "Encryption key rotated successfully. Restart the server with the new key."
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rotating encryption key");
            throw;
        }
    }        /// <summary>
             /// Migrate unencrypted connection strings to encrypted format
             /// </summary>
    [McpServerTool, Description("Migrate unencrypted connection strings to encrypted format")]
    public async Task<object> MigrateConnectionsToEncryptedAsync()
    {
        _logger.LogInformation("Request to migrate unencrypted connections received");

        try
        {            // Perform migration
            var count = await _keyRotationService.MigrateUnencryptedConnectionsAsync();

            // Return success response
            var result = new
            {
                count,
                message = $"Successfully migrated {count} connection strings to encrypted format"
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating connections to encrypted format");
            throw;
        }
    }    /// <summary>
         /// Generate a secure random key for encryption
         /// </summary>
    [McpServerTool, Description("Generate a secure random key for connection string encryption")]
    public object GenerateSecureKey(int length = 32)
    {
        // Validate input parameters
        if (length < 16 || length > 64)
        {
            var errorMessage = "Key length must be between 16 and 64 bytes";
            _logger.LogError(errorMessage);
            throw new ArgumentException(errorMessage, nameof(length));
        }

        _logger.LogInformation("Request to generate secure key received");

        try
        {            // Generate a secure key
            var key = _encryptionService.GenerateSecureKey(length);

            // Return the generated key
            var result = new
            {
                key,
                length,
                message = "Generated a new secure encryption key. Use this key with the MSSQL_MCP_KEY environment variable."
            };
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating secure key");
            throw;
        }
    }
}
