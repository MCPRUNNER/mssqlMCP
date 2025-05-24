using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using mssqlMCP.Interfaces;
namespace mssqlMCP.Services;

/// <summary>
/// Service to manage database connections
/// </summary>
public interface IConnectionManager
{
    Task<SqlConnection> GetConnectionAsync(string connectionName);
    Task<IEnumerable<ConnectionEntry>> GetAvailableConnectionsAsync();
    Task<ConnectionEntry?> GetConnectionEntryAsync(string name);
    Task<bool> AddConnectionAsync(string name, string connectionString, string? description = null);
    Task<bool> UpdateConnectionAsync(string name, string connectionString, string? description = null);
    Task<bool> RemoveConnectionAsync(string name);
    Task<bool> TestConnectionAsync(string connectionString);
}

/// <summary>
/// Implementation of the connection manager service
/// </summary>
public class ConnectionManager : IConnectionManager
{
    private readonly ILogger<ConnectionManager> _logger;
    private readonly IConnectionRepository _repository;
    private readonly IConnectionStringProvider _legacyProvider;

    public ConnectionManager(
        ILogger<ConnectionManager> logger,
        IConnectionRepository repository,
        IConnectionStringProvider legacyProvider)
    {
        _logger = logger;
        _repository = repository;
        _legacyProvider = legacyProvider;
    }

    /// <summary>
    /// Get a SQL connection using a stored connection name
    /// </summary>
    public async Task<SqlConnection> GetConnectionAsync(string connectionName)
    {
        // Try to get from SQLite repository first
        var connectionEntry = await _repository.GetConnectionAsync(connectionName);
        string? connectionString = null;

        if (connectionEntry != null)
        {
            connectionString = connectionEntry.ConnectionString;
            // Update LastUsed timestamp
            await _repository.UpdateLastUsedAsync(connectionName);
        }
        else
        {
            // Fall back to legacy connection string provider
            connectionString = await _legacyProvider.GetConnectionStringAsync(connectionName);
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException($"Connection string for '{connectionName}' not found");
        }

        try
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("Successfully connected to database: {ConnectionName}", connectionName);
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to database: {ConnectionName}", connectionName);
            throw;
        }
    }

    /// <summary>
    /// Get all available connection entries
    /// </summary>
    public async Task<IEnumerable<ConnectionEntry>> GetAvailableConnectionsAsync()
    {
        return await _repository.GetAllConnectionsAsync();
    }

    /// <summary>
    /// Get a specific connection entry by name
    /// </summary>
    public async Task<ConnectionEntry?> GetConnectionEntryAsync(string name)
    {
        return await _repository.GetConnectionAsync(name);
    }

    /// <summary>
    /// Add a new connection
    /// </summary>
    public async Task<bool> AddConnectionAsync(string name, string connectionString, string? description = null)
    {
        try
        {
            // Validate connection string by testing it
            if (!await TestConnectionAsync(connectionString))
            {
                _logger.LogWarning("Failed to validate connection string for {Name}", name);
                return false;
            }

            var connection = new ConnectionEntry
            {
                Name = name,
                ConnectionString = connectionString,
                Description = description,
                ServerType = "SqlServer",
                LastUsed = null,
                CreatedOn = DateTime.UtcNow
            };

            await _repository.SaveConnectionAsync(connection);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding connection {Name}", name);
            return false;
        }
    }

    /// <summary>
    /// Update an existing connection
    /// </summary>
    public async Task<bool> UpdateConnectionAsync(string name, string connectionString, string? description = null)
    {
        try
        {
            var existingConnection = await _repository.GetConnectionAsync(name);
            if (existingConnection == null)
            {
                _logger.LogWarning("Cannot update non-existent connection {Name}", name);
                return false;
            }

            // Validate connection string by testing it
            if (!await TestConnectionAsync(connectionString))
            {
                _logger.LogWarning("Failed to validate connection string for {Name}", name);
                return false;
            }

            existingConnection.ConnectionString = connectionString;
            existingConnection.Description = description ?? existingConnection.Description;
            existingConnection.LastUsed = DateTime.UtcNow;

            await _repository.SaveConnectionAsync(existingConnection);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection {Name}", name);
            return false;
        }
    }

    /// <summary>
    /// Remove a connection by name
    /// </summary>
    public async Task<bool> RemoveConnectionAsync(string name)
    {
        try
        {
            await _repository.DeleteConnectionAsync(name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection {Name}", name);
            return false;
        }
    }

    /// <summary>
    /// Test if a connection string is valid
    /// </summary>
    public async Task<bool> TestConnectionAsync(string connectionStringOrName)
    {
        try
        {
            // If the input is a known connection name, fetch the connection string from repository
            var entry = await _repository.GetConnectionAsync(connectionStringOrName);
            if (entry != null && !string.IsNullOrWhiteSpace(entry.ConnectionString))
            {
                connectionStringOrName = entry.ConnectionString;
            }
            else if (connectionStringOrName.Contains(";"))
            {
                // Assume it's a raw connection string
            }
            else
            {
                // Try legacy provider as fallback
                var legacy = await _legacyProvider.GetConnectionStringAsync(connectionStringOrName);
                if (!string.IsNullOrWhiteSpace(legacy))
                {
                    connectionStringOrName = legacy;
                }
            }
            using var connection = new SqlConnection(connectionStringOrName);
            await connection.OpenAsync();
            _logger.LogInformation("Connection test successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection test failed: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }
}
