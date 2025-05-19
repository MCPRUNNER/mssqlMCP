using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace mssqlMCP.Middleware;

/// <summary>
/// Middleware that validates API keys for incoming requests
/// </summary>
public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyAuthMiddleware> _logger;
    private readonly string _apiKeyHeaderName;
    private readonly string _apiKey;

    public ApiKeyAuthMiddleware(
        RequestDelegate next,
        ILogger<ApiKeyAuthMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _apiKeyHeaderName = configuration["ApiSecurity:HeaderName"] ?? "X-API-Key";
        _apiKey = Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY") ??
            configuration["ApiSecurity:ApiKey"] ??
            "";

    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth check if API key is not configured
        if (string.IsNullOrEmpty(_apiKey))
        {
            System.Console.WriteLine(@$"API key authentication is disabled. No API key configured MSSQL_MCP_API_KEY =  {Environment.GetEnvironmentVariable("MSSQL_MCP_API_KEY")}.");
            _logger.LogWarning("API key authentication is disabled. No API key configured.");
            await _next(context);
            return;
        }

        // Check for API key in header
        if (!context.Request.Headers.TryGetValue(_apiKeyHeaderName, out var apiKeyFromHeader))
        {
            _logger.LogWarning("API key missing in request");
            context.Response.StatusCode = 401; // Unauthorized
            await context.Response.WriteAsJsonAsync(new
            {
                error = "API key required",
                message = "Missing required API key header"
            });
            return;
        }

        // Validate API key
        if (!_apiKey.Equals(apiKeyFromHeader, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid API key provided");
            context.Response.StatusCode = 403; // Forbidden
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Access denied",
                message = "Invalid API key"
            });
            return;
        }

        // Key is valid, continue
        await _next(context);
    }
}