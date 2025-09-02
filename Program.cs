using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using mssqlMCP.Configuration;

using mssqlMCP.Extensions;
using mssqlMCP.Interfaces;
using mssqlMCP.Services;
using mssqlMCP.Tools;
using Serilog;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add MCP services

var myTransportType = Environment.GetEnvironmentVariable("MSSQL_MCP_TRANSPORT") ??
                      "Http";

if (myTransportType.Equals("Http", StringComparison.OrdinalIgnoreCase))
{
    Log.Information("Using HTTP transport for MCP server.");
    builder.Services.AddMcpServer().WithHttpTransport()
    .WithTools<SqlServerTools>()
    .WithTools<ConnectionManagerTool>()
    .WithTools<SecurityTool>()
    .WithTools<ApiKeyManagementTool>();

}
else if (myTransportType.Equals("Stdio", StringComparison.OrdinalIgnoreCase))
{
    Log.Information("Using Stdio transport for MCP server.");
    builder.Services.AddMcpServer().WithStdioServerTransport()
        .WithTools<SqlServerTools>()
    .WithTools<ConnectionManagerTool>()
    .WithTools<SecurityTool>()
    .WithTools<ApiKeyManagementTool>();
}
else
{
    Log.Error($"Invalid MSSQL_MCP_TRANSPORT: {myTransportType}. Defaulting to HTTP transport.");
    builder.Services.AddMcpServer().WithHttpTransport()
    .WithTools<SqlServerTools>()
    .WithTools<ConnectionManagerTool>()
    .WithTools<SecurityTool>()
    .WithTools<ApiKeyManagementTool>();
}



// Add our SQL Server MCP services
builder.Services.AddSingleton<IConnectionStringProvider, ConnectionStringProvider>();
builder.Services.AddTransient<ISqlServerTools, SqlServerTools>();

// Register SqlServerTools directly for the controller
builder.Services.AddTransient<SqlServerTools>();
builder.Services.AddTransient<ConnectionManagerTool>();
builder.Services.AddTransient<SecurityTool>();

// Add encryption service
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

// Add key rotation service
builder.Services.AddSingleton<IKeyRotationService, KeyRotationService>();

// Add connection repository and manager
builder.Services.AddSingleton<IConnectionRepository, SqliteConnectionRepository>();
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();

// Add API key repository and manager
builder.Services.AddSingleton<IApiKeyRepository, SqliteApiKeyRepository>();
builder.Services.AddSingleton<IApiKeyManager, ApiKeyManager>();

// Register ApiKeyManagementTool
builder.Services.AddTransient<ApiKeyManagementTool>();

// Register MCP Controller for JSON-RPC
builder.Services.AddControllers();


// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", corsBuilder =>
    {
        var corsConfig = builder.Configuration.GetSection("Cors");
        var origins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        var methods = corsConfig.GetSection("AllowedMethods").Get<string[]>() ?? Array.Empty<string>();
        var headers = corsConfig.GetSection("AllowedHeaders").Get<string[]>() ?? Array.Empty<string>();
        var exposedHeaders = corsConfig.GetSection("ExposedHeaders").Get<string[]>() ?? Array.Empty<string>();
        var allowCredentials = corsConfig.GetValue<bool>("AllowCredentials");

        corsBuilder
            .WithOrigins(origins)
            .WithMethods(methods)
            .WithHeaders(headers)
            .WithExposedHeaders(exposedHeaders);

        if (allowCredentials)
        {
            corsBuilder.AllowCredentials();
        }
        else
        {
            corsBuilder.AllowAnyOrigin();
        }
    });
});

// Configure JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add the following to properly handle disconnections
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure middleware
app.UseCors("CorsPolicy");

app.UseRouting();

// Add this in the appropriate location after app.UseRouting()
app.UseApiKeyAuthentication();

// Add custom middleware for handling content type negotiation
app.Use(async (context, next) =>
{
    // Set proper content type headers if not already set
    var acceptHeader = context.Request.Headers.Accept.ToString();
    if (string.IsNullOrEmpty(acceptHeader) || !acceptHeader.Contains("application/json"))
    {
        context.Request.Headers.Accept = "application/json";
    }

    // Ensure Content-Type is set for POST requests
    if (context.Request.Method == "POST" &&
        (string.IsNullOrEmpty(context.Request.ContentType) ||
         !context.Request.ContentType.Contains("application/json")))
    {
        context.Request.ContentType = "application/json";
    }

    await next();
});

app.UseSerilogRequestLogging(options =>
{
    // Customize logging based on response status and exceptions
    options.GetLevel = (context, elapsed, ex) =>
    {
        // Don't log operation cancelled exceptions as errors
        if (ex is OperationCanceledException)
            return Serilog.Events.LogEventLevel.Debug;

        return ex != null || context.Response.StatusCode >= 500
            ? Serilog.Events.LogEventLevel.Error
            : Serilog.Events.LogEventLevel.Information;
    };
});

#if DEBUG
// Add a direct endpoint for testing
app.MapGet("/api/test", () => "SQL Server MCP Server is running!");

// Add a direct endpoint for testing schema filtering
app.MapGet("/api/tables", async (string? schema, string? connectionName, IConfiguration config, ILoggerFactory loggerFactory) =>
{
    // Default to DefaultConnection if not specified
    connectionName ??= "DefaultConnection";

    try
    {
        var connectionString = config.GetConnectionString(connectionName);
        if (string.IsNullOrEmpty(connectionString))
        {
            return Results.Problem(
                title: "Configuration Error",
                detail: $"Connection string '{connectionName}' not found.",
                statusCode: 400);
        }

        var logger = loggerFactory.CreateLogger<mssqlMCP.Services.DatabaseMetadataProvider>();
        var metadataProvider = new mssqlMCP.Services.DatabaseMetadataProvider(connectionString, logger);

        try
        {
            var tables = await metadataProvider.GetDatabaseSchemaAsync(default, schema);
            return Results.Ok(tables);
        }
        catch (SqlException sqlEx)
        {
            // Handle SQL exceptions with user-friendly messages
            if (sqlEx.Number == 4060 || sqlEx.Number == 18456 || sqlEx.Number == 18452)
            {
                return Results.Problem(
                    title: "Authentication failed",
                    detail: "Database authentication failed. Check your connection credentials.",
                    statusCode: 401);
            }
            else if (sqlEx.Number == 2 || sqlEx.Number == 53)
            {
                return Results.Problem(
                    title: "Server unavailable",
                    detail: "Database server not found or not accessible.",
                    statusCode: 503);
            }

            return Results.Problem(
                title: "Database error",
                detail: sqlEx.Message,
                statusCode: 500);
        }
        catch (OperationCanceledException)
        {
            return Results.Problem(
                title: "Operation timeout",
                detail: "The operation timed out or was canceled.",
                statusCode: 408);
        }
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Error retrieving database tables",
            detail: ex.Message,
            statusCode: 500);
    }
});
#endif

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var error = exceptionHandlerFeature?.Error;

        if (error is OperationCanceledException)
        {
            // Handle client disconnect gracefully
            Log.Debug("Client disconnected - operation canceled");
            context.Response.StatusCode = 499; // Client Closed Request
            await context.Response.WriteAsync("{\"error\": \"Client disconnected\"}");
            return;
        }
        else if (error is SqlException sqlEx)
        {
            // Handle SQL exceptions with specific error messages
            Log.Error(sqlEx, "SQL exception occurred");

            if (sqlEx.Number == 4060 || sqlEx.Number == 18456 || sqlEx.Number == 18452)
            {
                // Login failed
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("{\"error\": \"Database authentication failed. Check your connection credentials.\"}");
                return;
            }
            else if (sqlEx.Number == 2 || sqlEx.Number == 53)
            {
                // Server not found or not accessible
                context.Response.StatusCode = 503; // Service Unavailable
                await context.Response.WriteAsync("{\"error\": \"Database server not found or not accessible. Check server name and network connection.\"}");
                return;
            }
            else if (sqlEx.Number == 4064)
            {
                // Database not found
                context.Response.StatusCode = 404; // Not Found
                await context.Response.WriteAsync("{\"error\": \"Database not found. Check database name.\"}");
                return;
            }

            // General SQL error
            await context.Response.WriteAsync("{\"error\": \"A database error occurred. Please check your query or connection details.\"}");
            return;
        }
        else if (error is TimeoutException)
        {
            // Handle timeout errors
            Log.Warning(error, "Operation timed out");
            context.Response.StatusCode = 504; // Gateway Timeout
            await context.Response.WriteAsync("{\"error\": \"Operation timed out. The database server might be busy or the query is too complex.\"}");
            return;
        }

        // Generic error handler
        Log.Error(error, "Unhandled exception");
        await context.Response.WriteAsync("{\"error\": \"An unexpected error occurred. Please try again later.\"}");
    });
});

// Run the application
app.Lifetime.ApplicationStarted.Register(() => Log.Information("SQL Server MCP Server started"));

if (myTransportType.Equals("Stdio", StringComparison.OrdinalIgnoreCase))
{
    // In stdio mode, we only want to use the MCP server transport
    var mcpServer = app.Services.GetRequiredService<ModelContextProtocol.Server.IMcpServer>();
    await mcpServer.RunAsync(CancellationToken.None);
}
else
{
    // For HTTP mode, start the web server
    app.MapMcp("/mcp");
    //  app.MapMcpJsonRpc();
    app.Run();
}

// Global logger factory for static classes
public static class LoggerFactory
{
    public static ILoggerFactory Create(Action<ILoggingBuilder> configure)
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(configure);
    }
}
