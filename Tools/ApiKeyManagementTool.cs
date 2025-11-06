using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using mssqlMCP.Models;
using mssqlMCP.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace mssqlMCP.Tools;

/// <summary>
/// API Key management tools for MCP
/// </summary>
[McpServerToolType]
public class ApiKeyManagementTool
{
    private readonly ILogger<ApiKeyManagementTool> _logger;
    private readonly IApiKeyManager _apiKeyManager;

    public ApiKeyManagementTool(ILogger<ApiKeyManagementTool> logger, IApiKeyManager apiKeyManager)
    {
        _logger = logger;
        _apiKeyManager = apiKeyManager;
    }

    /// <summary>
    /// Create a new API key for a user
    /// </summary>
    /// <param name="request">API key creation details</param>
    /// <returns>The created API key with its value (shown only once)</returns>
    [McpServerTool, Description("Create a new API key for a user")]
    public async Task<ApiKeyResponse> CreateApiKey(CreateApiKeyRequest request)
    {
        _logger.LogInformation($"Creating API key for user {request.UserId}");
        try
        {
            return await _apiKeyManager.CreateApiKeyAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating API key");
            throw;
        }
    }

    /// <summary>
    /// List API keys for a user
    /// </summary>
    /// <param name="userId">The user ID whose keys to list</param>
    /// <returns>Collection of API keys for the user</returns>
    [McpServerTool, Description("List API keys for a user")]
    public async Task<IEnumerable<ApiKeyResponse>> ListUserApiKeys(string userId)
    {
        _logger.LogInformation($"Listing API keys for user {userId}");
        try
        {
            return await _apiKeyManager.ListApiKeysForUserAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error listing API keys for user {userId}");
            throw;
        }
    }

    /// <summary>
    /// List API keys for a user (alias for ListUserApiKeys)
    /// </summary>
    /// <param name="userId">The user ID whose keys to list</param>
    /// <returns>Collection of API keys for the user</returns>
    [McpServerTool, Description("List API keys for a user")]
    public async Task<IEnumerable<ApiKeyResponse>> ListApiKeys(string userId)
    {
        return await ListUserApiKeys(userId);
    }

    /// <summary>
    /// List all API keys (admin only)
    /// </summary>
    /// <returns>Collection of all API keys in the system</returns>
    [McpServerTool(Name = "mssql_list_all_keys"), Description("List all API keys (admin only)")]
    public async Task<IEnumerable<ApiKeyResponse>> ListAllApiKeys()
    {
        _logger.LogInformation("Listing all API keys");
        try
        {
            return await _apiKeyManager.ListApiKeysAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing all API keys");
            throw;
        }
    }

    /// <summary>
    /// Revoke an API key
    /// </summary>
    /// <param name="request">Request containing the ID of the key to revoke</param>
    /// <returns>Success status</returns>
    [McpServerTool, Description("Revoke an API key")]
    public async Task<object> RevokeApiKey(RevokeApiKeyRequest request)
    {
        _logger.LogInformation($"Revoking API key {request.Id}");
        try
        {
            var result = await _apiKeyManager.RevokeApiKeyAsync(request.Id);
            return new { success = result, message = result ? "API key revoked successfully" : "Failed to revoke API key" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error revoking API key {request.Id}");
            throw;
        }
    }

    /// <summary>
    /// Delete an API key
    /// </summary>
    /// <param name="id">ID of the key to delete</param>
    /// <returns>Success status</returns>
    [McpServerTool, Description("Delete an API key")]
    public async Task<object> DeleteApiKey(string id)
    {
        _logger.LogInformation($"Deleting API key {id}");
        try
        {
            var result = await _apiKeyManager.DeleteApiKeyAsync(id);
            return new { success = result, message = result ? "API key deleted successfully" : "Failed to delete API key" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting API key {id}");
            throw;
        }
    }

    /// <summary>
    /// Get recent usage logs for a specific API key (admin only)
    /// </summary>
    /// <param name="apiKeyId">ID of the API key to get logs for</param>
    /// <param name="limit">Maximum number of logs to return</param>
    /// <returns>Collection of usage logs for the API key</returns>
    [McpServerTool(Name = "mssql_get_key_usage_logs"), Description("Get recent usage logs for a specific API key (admin only)")]
    public async Task<IEnumerable<ApiKeyUsageLog>> GetApiKeyUsageLogs(string apiKeyId, int limit = 100)
    {
        _logger.LogInformation($"Getting usage logs for API key {apiKeyId}");
        try
        {
            return await _apiKeyManager.GetApiKeyUsageLogsAsync(apiKeyId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting usage logs for API key {apiKeyId}");
            throw;
        }
    }

    /// <summary>
    /// Get recent usage logs for a user (admin only)
    /// </summary>
    /// <param name="userId">ID of the user to get logs for</param>
    /// <param name="limit">Maximum number of logs to return</param>
    /// <returns>Collection of usage logs for the user</returns>
    [McpServerTool(Name = "mssql_get_user_usage_logs"), Description("Get recent usage logs for a user (admin only)")]
    public async Task<IEnumerable<ApiKeyUsageLog>> GetUserUsageLogs(string userId, int limit = 100)
    {
        _logger.LogInformation($"Getting usage logs for user {userId}");
        try
        {
            return await _apiKeyManager.GetUserUsageLogsAsync(userId, limit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting usage logs for user {userId}");
            throw;
        }
    }
}
