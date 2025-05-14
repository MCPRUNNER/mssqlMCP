using Microsoft.Data.SqlClient;
using ModelContextProtocol.Server;
using Serilog;
using System.ComponentModel;

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

var app = builder.Build();

// Configure middleware
app.UseSerilogRequestLogging();
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