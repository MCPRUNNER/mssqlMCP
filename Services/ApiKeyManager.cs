using Microsoft.Extensions.Logging;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace mssqlMCP.Services;

/// <summary>
/// Interface for API key management operations
/// </summary>
public interface IApiKeyManager
{
    Task<ApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request);
    Task<IEnumerable<ApiKeyResponse>> ListApiKeysAsync();
    Task<IEnumerable<ApiKeyResponse>> ListApiKeysForUserAsync(string userId);
    Task<bool> RevokeApiKeyAsync(string id);
    Task<bool> DeleteApiKeyAsync(string id);
    Task<bool> ValidateApiKeyAsync(string keyValue);
    Task<ApiKeyResponse?> GetApiKeyByIdAsync(string id);
    Task<IEnumerable<ApiKeyUsageLog>> GetApiKeyUsageLogsAsync(string apiKeyId, int limit = 100);
    Task<IEnumerable<ApiKeyUsageLog>> GetUserUsageLogsAsync(string userId, int limit = 100);
    Task LogApiKeyUsageAsync(HttpContext context, string apiKeyId, string userId);
    Task<bool> IsMasterKeyAsync(string keyValue);
}

/// <summary>
/// Service for managing API keys
/// </summary>
public class ApiKeyManager : IApiKeyManager
{
    private readonly IApiKeyRepository _repository;
    private readonly ILogger<ApiKeyManager> _logger;
    private readonly string _masterKey;

    public ApiKeyManager(
        IApiKeyRepository repository,
        ILogger<ApiKeyManager> logger,
        IConfiguration configuration)
    {
        _repository = repository;
        _logger = logger;
        _masterKey = Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY") ??
            configuration["ApiSecurity:ApiKey"] ??
            "";
    }

    public async Task<ApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            throw new ArgumentException("API Key name is required");
        }

        if (string.IsNullOrEmpty(request.UserId))
        {
            throw new ArgumentException("User ID is required");
        }

        // Generate a secure random API key
        string apiKeyValue = GenerateSecureApiKey();

        var apiKey = new ApiKey
        {
            Name = request.Name,
            Key = apiKeyValue, // This will be encrypted by the repository
            UserId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            ExpirationDate = request.ExpirationDate,
            IsActive = true,
            KeyType = request.KeyType,
            AllowedConnectionNames = request.AllowedConnectionNames is not null && request.AllowedConnectionNames.Count > 0
                ? JsonSerializer.Serialize(request.AllowedConnectionNames)
                : null
        };

        // Save the API key (repository will encrypt it)
        var savedKey = await _repository.SaveApiKeyAsync(apiKey);

        // Return the response with the unencrypted key (only time it's shown)
        return new ApiKeyResponse
        {
            Id = savedKey.Id,
            Name = savedKey.Name,
            Key = apiKeyValue, // Only return the plaintext key once
            UserId = savedKey.UserId,
            CreatedAt = savedKey.CreatedAt,
            ExpirationDate = savedKey.ExpirationDate,
            LastUsed = null,
            IsActive = savedKey.IsActive,
            KeyType = savedKey.KeyType,
            Description = savedKey.Description
        };
    }

    public async Task<IEnumerable<ApiKeyResponse>> ListApiKeysAsync()
    {
        var apiKeys = await _repository.GetAllApiKeysAsync();
        var responses = new List<ApiKeyResponse>();

        foreach (var apiKey in apiKeys)
        {
            responses.Add(new ApiKeyResponse
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                Key = null, // Never return the key after creation
                UserId = apiKey.UserId,
                CreatedAt = apiKey.CreatedAt,
                ExpirationDate = apiKey.ExpirationDate,
                LastUsed = apiKey.LastUsed,
                IsActive = apiKey.IsActive,
                KeyType = apiKey.KeyType,
                Description = apiKey.Description
            });
        }

        return responses;
    }

    public async Task<IEnumerable<ApiKeyResponse>> ListApiKeysForUserAsync(string userId)
    {
        var apiKeys = await _repository.GetApiKeysForUserAsync(userId);
        var responses = new List<ApiKeyResponse>();

        foreach (var apiKey in apiKeys)
        {
            responses.Add(new ApiKeyResponse
            {
                Id = apiKey.Id,
                Name = apiKey.Name,
                Key = null, // Never return the key after creation
                UserId = apiKey.UserId,
                CreatedAt = apiKey.CreatedAt,
                ExpirationDate = apiKey.ExpirationDate,
                LastUsed = apiKey.LastUsed,
                IsActive = apiKey.IsActive,
                KeyType = apiKey.KeyType,
                Description = apiKey.Description
            });
        }

        return responses;
    }

    public async Task<bool> RevokeApiKeyAsync(string id)
    {
        return await _repository.RevokeApiKeyAsync(id);
    }

    public async Task<bool> DeleteApiKeyAsync(string id)
    {
        return await _repository.DeleteApiKeyAsync(id);
    }

    public async Task<bool> ValidateApiKeyAsync(string keyValue)
    {
        // Check if it's the master key
        if (!string.IsNullOrEmpty(_masterKey) && keyValue == _masterKey)
        {
            _logger.LogInformation("User authenticated with master key");
            return true;
        }

        // Otherwise check the repository
        _logger.LogInformation("Validating user-specific API key: {KeyLength} chars", keyValue?.Length ?? 0);

        try
        {
            bool isValid = await _repository.ValidateApiKeyAsync(keyValue);

            if (isValid)
            {
                _logger.LogInformation("API key validation successful");
            }
            else
            {
                _logger.LogWarning("API key validation failed - key not found or inactive");
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during API key validation");
            return false;
        }
    }

    public async Task<ApiKeyResponse?> GetApiKeyByIdAsync(string id)
    {
        var apiKey = await _repository.GetApiKeyByIdAsync(id);

        if (apiKey == null)
        {
            return null;
        }

        return new ApiKeyResponse
        {
            Id = apiKey.Id,
            Name = apiKey.Name,
            Key = null, // Never return the key after creation
            UserId = apiKey.UserId,
            CreatedAt = apiKey.CreatedAt,
            ExpirationDate = apiKey.ExpirationDate,
            LastUsed = apiKey.LastUsed,
            IsActive = apiKey.IsActive,
            KeyType = apiKey.KeyType,
            Description = apiKey.Description
        };
    }

    public async Task<IEnumerable<ApiKeyUsageLog>> GetApiKeyUsageLogsAsync(string apiKeyId, int limit = 100)
    {
        return await _repository.GetApiKeyUsageLogsAsync(apiKeyId, limit);
    }

    public async Task<IEnumerable<ApiKeyUsageLog>> GetUserUsageLogsAsync(string userId, int limit = 100)
    {
        return await _repository.GetUserUsageLogsAsync(userId, limit);
    }

    public async Task LogApiKeyUsageAsync(HttpContext context, string apiKeyId, string userId)
    {
        var log = new ApiKeyUsageLog
        {
            ApiKeyId = apiKeyId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Resource = context.Request.Path,
            Method = context.Request.Method,
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = context.Request.Headers["User-Agent"].ToString()
        };

        await _repository.LogApiKeyUsageAsync(log);
    }

    public Task<bool> IsMasterKeyAsync(string keyValue)
    {
        return Task.FromResult(IsMasterKey(keyValue));
    }

    private bool IsMasterKey(string keyValue)
    {
        return !string.IsNullOrEmpty(_masterKey) && keyValue == _masterKey;
    }

    private string GenerateSecureApiKey()
    {
        // Generate a 48-byte random key
        var bytes = new byte[48];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        var key = Convert.ToBase64String(bytes);

        if (string.IsNullOrEmpty(key) || IsMasterKey(key))
        {
            throw new Exception("Generated key is invalid");
        }

        return key;
    }
}
