using Microsoft.AspNetCore.Builder;
using mssqlMCP.Middleware;

namespace mssqlMCP.Extensions;

/// <summary>
/// Extension methods for API security features
/// </summary>
public static class ApiSecurityExtensions
{
    /// <summary>
    /// Adds API key authentication to the request pipeline
    /// </summary>
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyAuthMiddleware>();
    }
}