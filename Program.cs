using Microsoft.Data.SqlClient;
using ModelContextProtocol.Server;
using Serilog;
using System.ComponentModel;
using mssqlMCP; // Add reference to our namespace

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
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
app.UseCors("AllowAll");

app.UseRouting();

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

// Add a direct endpoint for testing
app.MapGet("/api/test", () => "SQL Server MCP Server is running!");

// Add a direct endpoint for testing schema filtering
app.MapGet("/api/tables", async (string? schema, IConfiguration config, ILoggerFactory loggerFactory) =>
{
    try
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            return Results.Problem("Connection string not found");
        }

        var logger = loggerFactory.CreateLogger<DatabaseMetadataProvider>();
        var metadataProvider = new DatabaseMetadataProvider(connectionString, logger);

        var tables = await metadataProvider.GetDatabaseSchemaAsync(default, schema);
        return Results.Ok(tables);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error getting tables: {ex.Message}");
    }
});

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "text/plain";

        var exceptionHandlerFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionHandlerFeature?.Error is OperationCanceledException)
        {
            // Handle client disconnect gracefully
            Log.Debug("Client disconnected - operation canceled");
            context.Response.StatusCode = 499; // Client Closed Request
            await context.Response.WriteAsync("Client disconnected");
            return;
        }

        await context.Response.WriteAsync("An error occurred. Please try again later.");
    });
});

app.MapMcp();

// Run the application
app.Lifetime.ApplicationStarted.Register(() => Log.Information("SQL Server MCP Server started"));
app.Lifetime.ApplicationStopped.Register(() => Log.Information("SQL Server MCP Server stopped"));

app.Run("http://localhost:3001");

// Global logger factory for static classes
public static class LoggerFactory
{
    public static ILoggerFactory Create(Action<ILoggingBuilder> configure)
    {
        return Microsoft.Extensions.Logging.LoggerFactory.Create(configure);
    }
}