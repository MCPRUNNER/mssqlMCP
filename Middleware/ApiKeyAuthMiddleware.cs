using mssqlMCP.Services;
using System.Text.Json;
using mssqlMCP.Models;
using Newtonsoft.Json.Linq;

namespace mssqlMCP.Middleware;

/// <summary>
/// Middleware that validates API key authentication for incoming requests
/// Supports both master key and user-specific API keys
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private readonly string _masterKey;
    private readonly IApiKeyManager _apiKeyManager;
    private readonly IApiKeyRepository _apiKeyRepository;

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthMiddleware> logger,
        IConfiguration configuration,
        IApiKeyManager apiKeyManager,
        IApiKeyRepository apiKeyRepository)
    {
        _next = next;
        _logger = logger;
        _apiKeyManager = apiKeyManager;
        _apiKeyRepository = apiKeyRepository;
        _masterKey = Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY") ??
            configuration["ApiSecurity:ApiKey"] ??
            "";
    }

    private static readonly HashSet<string> UserAllowedApiNames = new()
    {
        "notifications/initialized", "tools/list",
        "Initialize", "ExecuteQuery", "GetTableMetadata", "GetDatabaseObjectsMetadata", "GetDatabaseObjectsByType",
        "GetSqlServerAgentJobs", "GetSqlServerAgentJobDetails", "GetSsisCatalogInfo", "GetAzureDevOpsInfo"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth check if API key is not configured and we have no API key manager
        if (string.IsNullOrEmpty(_masterKey) && _apiKeyManager == null)
        {
            _logger.LogWarning("API key authentication is disabled. No API key configured and no API key manager available.");
            await _next(context);
            return;
        }

        string? token = null;
        string? authError = null;
        string? authSource = null;

        var authHeader = context.Request.Headers["Authorization"].ToString();
        var apiKeyHeader = context.Request.Headers["X-API-Key"].ToString();

        context.Request.EnableBuffering();
        var requestBody = await new StreamReader(context.Request.Body, leaveOpen: true).ReadToEndAsync();
        context.Request.Body.Position = 0; // Reset stream for next middleware

        if (!string.IsNullOrEmpty(authHeader))
        {
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                authError = "Authorization header must use Bearer scheme";
            }
            else
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
                authSource = "Bearer token";
            }
        }
        else if (!string.IsNullOrEmpty(apiKeyHeader))
        {
            token = apiKeyHeader;
            authSource = "X-API-Key header";
        }
        else
        {
            // If no headers, try getting token from the body for JSON-RPC calls
            token = await GetTokenFromBodyAsync(context, requestBody);
            if (token != null)
            {
                authSource = "JSON-RPC body";
            }
        }

        if (authError != null)
        {
            _logger.LogWarning("Invalid Authorization header format");
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new { error = "Invalid Authorization format", message = authError });
            return;
        }

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("No authentication provided - checked Bearer token, X-API-Key, and JSON-RPC body.");
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new { error = "Authentication required", message = "Please provide a valid API key in the Authorization header, X-API-Key header, or the request body for JSON-RPC calls." });
            return;
        }

        // Validate the token
        // 1. Check master key
        if (!string.IsNullOrEmpty(_masterKey) && string.Equals(token, _masterKey))
        {
            _logger.LogInformation("Successfully authenticated using master key from {AuthSource}", authSource);
            context.Items["IsMaster"] = true;
            await _next(context);
            return;
        }

        // 2. Check API key repository
        if (_apiKeyManager != null)
        {
            var apiKey = await _apiKeyRepository.GetApiKeyByValueAsync(token);
            if (apiKey != null && await _apiKeyManager.ValidateApiKeyAsync(token))
            {
                // Check permissions for user keys
                if (apiKey.KeyType.Equals("user", StringComparison.OrdinalIgnoreCase))
                {
                    if (!await IsUserToolRequest(context, requestBody) ||
                        !await IsConnectionAllowedAsync(context, requestBody, apiKey))
                    {
                        _logger.LogWarning("User key '{ApiKeyName}' ({ApiKeyId}) from {AuthSource} denied access to management endpoint", apiKey.Name, apiKey.Id, authSource);
                        context.Response.StatusCode = 403; // Forbidden
                        await context.Response.WriteAsJsonAsync(new { error = "Permission Denied", message = "This API key does not have permission to perform management operations." });
                        return;
                    }
                }

                _logger.LogInformation("Successfully authenticated using stored API key from {AuthSource}", authSource);
                await _apiKeyManager.LogApiKeyUsageAsync(context, apiKey.Id, apiKey.UserId);
                await _next(context);
                return;
            }
        }

        // If we get here, the token was invalid
        _logger.LogWarning("Invalid authentication credentials provided from {AuthSource}", authSource);
        context.Response.StatusCode = 403; // Forbidden
        await context.Response.WriteAsJsonAsync(new { error = "Invalid authentication", message = "The provided authentication credentials are not valid" });
    }

    private async Task<bool> IsConnectionAllowedAsync(HttpContext context, string requestBody, ApiKey apiKey)
    {
        // If no specific connections are listed, allow access.
        if (string.IsNullOrWhiteSpace(apiKey.AllowedConnectionNames))
        {
            return true;
        }

        // Master key bypasses this check
        if (context.Items.ContainsKey("IsMaster") && (bool)context.Items["IsMaster"]!)
        {
            return true;
        }

        try
        {
            var allowedConnections = JsonSerializer.Deserialize<List<string>>(apiKey.AllowedConnectionNames, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (allowedConnections == null || allowedConnections.Count == 0)
            {
                return true; // No restrictions
            }

            if (string.IsNullOrEmpty(requestBody))
            {
                // If there's no request body, we can't check for a connection name.
                // This might be a request that doesn't require one, so we allow it.
                return true;
            }

            var json = JObject.Parse(requestBody);
            var connectionName = json["params"]?["connectionName"]?.ToString() ?? json["params"]?["arguments"]?["connectionName"]?.ToString();

            {
                if (connectionName != null && allowedConnections.Contains(connectionName, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Connection '{ConnectionName}' is allowed for API key '{ApiKeyName}'", connectionName, apiKey.Name);
                    return true;
                }

                _logger.LogWarning("Forbidden: API key '{ApiKeyName}' is not authorized for connection '{ConnectionName}'", apiKey.Name, connectionName);
                context.Response.StatusCode = 403; // Forbidden
                await context.Response.WriteAsJsonAsync(new { error = "Forbidden", message = $"Access to connection '{connectionName}' is not allowed with the provided API key." });
                return false;
            }

            // If the request doesn't contain a ConnectionName parameter, we'll allow it to proceed.
            // The authorization is only for connection-specific tools.
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during connection authorization");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "Internal Server Error", message = "An error occurred while processing your request." });
            return false;
        }
    }

    private async Task<string?> GetTokenFromBodyAsync(HttpContext context, string requestBody)
    {
        if (!context.Request.Path.Equals("/mcp", StringComparison.OrdinalIgnoreCase) ||
            !context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(requestBody);
            if (jsonDoc.RootElement.TryGetProperty("params", out var paramsProp) &&
                paramsProp.TryGetProperty("_meta", out var metaProp) &&
                metaProp.TryGetProperty("apiKey", out var apiKeyProp) &&
                apiKeyProp.ValueKind == JsonValueKind.String)
            {
                return apiKeyProp.GetString();
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse request body for API key from JSON-RPC.");
        }

        return null;
    }

    private async Task<bool> IsUserToolRequest(HttpContext context, string requestBody)
    {
        // Only apply this logic to the MCP endpoint
        if (!context.Request.Path.Equals("/mcp", StringComparison.OrdinalIgnoreCase) || context.Request.Method != "POST")
        {
            return false;
        }

        try
        {
            using var jsonDoc = JsonDocument.Parse(requestBody);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("method", out var methodProperty))
            {
                var methodName = methodProperty.GetString();
                if (methodName != null)
                {
                    var apiName = methodName;
                    if (methodName.Equals("tools/call", StringComparison.OrdinalIgnoreCase))
                    {
                        if (root.TryGetProperty("params", out var @params) &&
                            @params.TryGetProperty("name", out var name))
                        {
                            apiName = name.GetString();
                        }
                    }

                    _logger.LogInformation("Requesting API: {ToolName}", apiName);

                    // Check for user allowed APIs
                    if (UserAllowedApiNames.Any(prefix => apiName is not null &&
                            apiName.Equals(prefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse request body for MCP method check");
        }

        return false;
    }
}
