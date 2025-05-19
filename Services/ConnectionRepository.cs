using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace mssqlMCP.Services;

/// <summary>
/// Repository for managing database connections stored in SQLite
/// </summary>
public interface IConnectionRepository
{
    Task<IEnumerable<ConnectionEntry>> GetAllConnectionsAsync();
    Task<IEnumerable<ConnectionEntry>> GetAllConnectionsRawAsync();
    Task<ConnectionEntry?> GetConnectionAsync(string name);
    Task SaveConnectionAsync(ConnectionEntry connection);
    Task SaveConnectionStringDirectlyAsync(ConnectionEntry connection);
    Task DeleteConnectionAsync(string name);
    Task UpdateLastUsedAsync(string name);
}

/// <summary>
/// SQLite implementation of the connection repository
/// </summary>
public class SqliteConnectionRepository : IConnectionRepository, IDisposable
{
    private readonly ILogger<SqliteConnectionRepository> _logger;
    private readonly string _connectionString;
    private readonly IEncryptionService _encryptionService;
    private bool _initialized = false;
    private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1); public SqliteConnectionRepository(ILogger<SqliteConnectionRepository> logger, IEncryptionService encryptionService)
    {
        _logger = logger;
        _encryptionService = encryptionService;
        var dataDirectory = Environment.GetEnvironmentVariable("MSSQL_MCP_DATA");

        if (!string.IsNullOrEmpty(dataDirectory))
        {
            // Use the specified data directory
            dataDirectory = Path.GetFullPath(dataDirectory);
            // Create data directory if it doesn't exist
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }
        else
        {
            // Use the default data directory
            dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
            // Create data directory if it doesn't exist
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
        }



        var dbPath = Path.Combine(dataDirectory, "connections.db");
        _connectionString = $"Data Source={dbPath}";
    }

    /// <summary>
    /// Initialize the SQLite database if not already done
    /// </summary>
    private async Task InitializeAsync()
    {
        if (_initialized) return;

        await _initLock.WaitAsync();
        try
        {
            if (_initialized) return;

            _logger.LogInformation("Initializing SQLite database for connection storage");

            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            // Create tables if they don't exist
            await connection.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS Connections (
                        Name TEXT PRIMARY KEY,
                        ConnectionString TEXT NOT NULL,
                        ServerType TEXT NOT NULL,
                        Description TEXT,
                        LastUsed TEXT,
                        CreatedOn TEXT NOT NULL
                    );
                ");

            _initialized = true;
            _logger.LogInformation("SQLite connection database initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize SQLite database");
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }        /// <summary>
             /// Get all stored database connections
             /// </summary>
    public async Task<IEnumerable<ConnectionEntry>> GetAllConnectionsAsync()
    {
        await InitializeAsync();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            var result = await connection.QueryAsync<ConnectionEntry>(@"
                    SELECT Name, ConnectionString, ServerType, Description, LastUsed, CreatedOn 
                    FROM Connections
                    ORDER BY Name;
                ");

            // Decrypt connection strings
            foreach (var entry in result)
            {
                entry.ConnectionString = _encryptionService.Decrypt(entry.ConnectionString);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connections from SQLite");
            throw;
        }
    }

    /// <summary>
    /// Get all stored database connections without decryption
    /// </summary>
    public async Task<IEnumerable<ConnectionEntry>> GetAllConnectionsRawAsync()
    {
        await InitializeAsync();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            var result = await connection.QueryAsync<ConnectionEntry>(@"
                    SELECT Name, ConnectionString, ServerType, Description, LastUsed, CreatedOn 
                    FROM Connections
                    ORDER BY Name;
                ");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving raw connections from SQLite");
            throw;
        }
    }

    /// <summary>
    /// Get a specific connection by name
    /// </summary>
    public async Task<ConnectionEntry?> GetConnectionAsync(string name)
    {
        await InitializeAsync();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            var result = await connection.QueryFirstOrDefaultAsync<ConnectionEntry>(@"
                    SELECT Name, ConnectionString, ServerType, Description, LastUsed, CreatedOn 
                    FROM Connections
                    WHERE Name = @Name;
                ", new { Name = name });

            if (result != null)
            {
                // Decrypt the connection string
                result.ConnectionString = _encryptionService.Decrypt(result.ConnectionString);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection {Name} from SQLite", name);
            throw;
        }
    }        /// <summary>
             /// Save or update a connection
             /// </summary>
    public async Task SaveConnectionAsync(ConnectionEntry connection)
    {
        await InitializeAsync();

        try
        {
            // Encrypt the connection string before saving
            var encryptedConnection = new ConnectionEntry
            {
                Name = connection.Name,
                ConnectionString = _encryptionService.Encrypt(connection.ConnectionString),
                ServerType = connection.ServerType,
                Description = connection.Description,
                LastUsed = connection.LastUsed,
                CreatedOn = connection.CreatedOn
            };

            using var dbConnection = new SqliteConnection(_connectionString);

            // Check if connection already exists
            var existing = await dbConnection.QueryFirstOrDefaultAsync<ConnectionEntry>(
                "SELECT Name FROM Connections WHERE Name = @Name",
                new
                {
                    connection.Name
                });

            if (existing != null)
            {
                // Update existing connection
                await dbConnection.ExecuteAsync(@"
                        UPDATE Connections
                        SET ConnectionString = @ConnectionString,
                            ServerType = @ServerType,
                            Description = @Description,
                            LastUsed = @LastUsed
                        WHERE Name = @Name;
                    ", encryptedConnection);

                _logger.LogInformation("Updated connection: {Name}", connection.Name);
            }
            else
            {
                // Insert new connection
                encryptedConnection.CreatedOn = DateTime.UtcNow;

                await dbConnection.ExecuteAsync(@"
                        INSERT INTO Connections (Name, ConnectionString, ServerType, Description, LastUsed, CreatedOn)
                        VALUES (@Name, @ConnectionString, @ServerType, @Description, @LastUsed, @CreatedOn);
                    ", encryptedConnection);

                _logger.LogInformation("Added new connection: {Name}", connection.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving connection {Name} to SQLite", connection.Name);
            throw;
        }
    }

    /// <summary>
    /// Save connection without encrypting the connection string (used for key rotation)
    /// </summary>
    public async Task SaveConnectionStringDirectlyAsync(ConnectionEntry connection)
    {
        await InitializeAsync();

        try
        {
            using var dbConnection = new SqliteConnection(_connectionString);

            // Check if connection already exists
            var existing = await dbConnection.QueryFirstOrDefaultAsync<ConnectionEntry>(
                "SELECT Name FROM Connections WHERE Name = @Name",
                new
                {
                    connection.Name
                });

            if (existing != null)
            {
                // Update existing connection
                await dbConnection.ExecuteAsync(@"
                        UPDATE Connections
                        SET ConnectionString = @ConnectionString,
                            ServerType = @ServerType,
                            Description = @Description,
                            LastUsed = @LastUsed
                        WHERE Name = @Name;
                    ", connection);

                _logger.LogInformation("Updated connection directly: {Name}", connection.Name);
            }
            else
            {
                // Insert new connection
                if (connection.CreatedOn == default)
                {
                    connection.CreatedOn = DateTime.UtcNow;
                }

                await dbConnection.ExecuteAsync(@"
                        INSERT INTO Connections (Name, ConnectionString, ServerType, Description, LastUsed, CreatedOn)
                        VALUES (@Name, @ConnectionString, @ServerType, @Description, @LastUsed, @CreatedOn);
                    ", connection);

                _logger.LogInformation("Added new connection directly: {Name}", connection.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving connection directly {Name} to SQLite", connection.Name);
            throw;
        }
    }

    /// <summary>
    /// Delete a connection by name
    /// </summary>
    public async Task DeleteConnectionAsync(string name)
    {
        await InitializeAsync();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM Connections WHERE Name = @Name", new { Name = name });
            _logger.LogInformation("Deleted connection: {Name}", name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting connection {Name} from SQLite", name);
            throw;
        }
    }

    /// <summary>
    /// Update the LastUsed timestamp for a connection
    /// </summary>
    public async Task UpdateLastUsedAsync(string name)
    {
        await InitializeAsync();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(@"
                    UPDATE Connections
                    SET LastUsed = @LastUsed
                    WHERE Name = @Name
                ", new { Name = name, LastUsed = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating LastUsed for connection {Name}", name);
            // Don't throw - this is a non-critical operation
        }
    }

    public void Dispose()
    {
        _initLock?.Dispose();
    }
}
