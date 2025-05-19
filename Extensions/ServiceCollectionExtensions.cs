using Microsoft.Extensions.DependencyInjection;
using mssqlMCP.Configuration;
using mssqlMCP.Interfaces;
using mssqlMCP.Services;
using mssqlMCP.Tools;

namespace mssqlMCP.Extensions;

/// <summary>
/// Extension methods for registering SQL MCP services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQL Server MCP services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSqlServerMcp(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();

        // Register tools for MCP
        services.AddSingleton<SqlServerTools>();

        return services;
    }

    /// <summary>
    /// Adds SQL Server MCP tools to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSqlServerTools(this IServiceCollection services)
    {
        services.AddTransient<ISqlServerTools, SqlServerTools>();
        return services;
    }
}
