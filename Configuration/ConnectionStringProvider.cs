using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using mssqlMCP.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace mssqlMCP.Configuration
{
    /// <summary>
    /// Provides access to database connection strings from configuration and files
    /// </summary>
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConnectionStringProvider> _logger;
        private readonly string _connectionsFilePath;
        private Dictionary<string, string>? _cachedConnections;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the ConnectionStringProvider
        /// </summary>
        /// <param name="configuration">The application configuration</param>
        /// <param name="logger">The logger</param>
        public ConnectionStringProvider(
            IConfiguration configuration,
            ILogger<ConnectionStringProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _connectionsFilePath = Path.Combine(AppContext.BaseDirectory, "connections.json");
        }

        /// <summary>
        /// Gets a database connection string by name
        /// </summary>
        /// <param name="connectionName">Name of the connection string, null or empty for default</param>
        /// <returns>The connection string</returns>
        public string GetConnectionString(string? connectionName = null)
        {
            // Get synchronously (not ideal but needed for backward compatibility)
            var result = GetConnectionStringAsync(connectionName).GetAwaiter().GetResult();

            if (string.IsNullOrEmpty(result))
            {
                throw new InvalidOperationException($"Connection string '{connectionName ?? "DefaultConnection"}' not found");
            }

            return result;
        }

        /// <summary>
        /// Gets a database connection string by name asynchronously
        /// </summary>
        /// <param name="connectionName">Name of the connection string</param>
        /// <returns>The connection string</returns>
        public async Task<string> GetConnectionStringAsync(string? connectionName = null)
        {
            // Use "DefaultConnection" if connectionName is null or empty
            if (string.IsNullOrEmpty(connectionName))
            {
                connectionName = "DefaultConnection";
            }

            // Try to get from cached connections first
            await LoadConnectionsFromFileAsync();

            if (_cachedConnections != null && _cachedConnections.TryGetValue(connectionName, out var connStr))
            {
                return connStr;
            }

            // Otherwise try to get from IConfiguration
            connStr = _configuration.GetConnectionString(connectionName) ??
                      _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connStr))
            {
                _logger.LogWarning("Connection string '{ConnectionName}' not found", connectionName);
            }

            return connStr ?? string.Empty;
        }

        /// <summary>
        /// Saves a connection string
        /// </summary>
        /// <param name="name">Name of the connection</param>
        /// <param name="connectionString">Connection string to save</param>
        public async Task SaveConnectionStringAsync(string name, string connectionString)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Connection name cannot be empty", nameof(name));
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));
            }

            await _connectionLock.WaitAsync();
            try
            {
                await LoadConnectionsFromFileAsync();
                _cachedConnections ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _cachedConnections[name] = connectionString;
                await SaveConnectionsToFileAsync();
                _logger.LogInformation("Saved connection string: {Name}", name);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Removes a connection string
        /// </summary>
        /// <param name="name">Name of the connection to remove</param>
        public async Task RemoveConnectionStringAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Connection name cannot be empty", nameof(name));
            }

            await _connectionLock.WaitAsync();
            try
            {
                await LoadConnectionsFromFileAsync();

                if (_cachedConnections != null && _cachedConnections.Remove(name))
                {
                    await SaveConnectionsToFileAsync();
                    _logger.LogInformation("Removed connection string: {Name}", name);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Gets all available connection names
        /// </summary>
        /// <returns>Collection of connection names</returns>
        public async Task<IEnumerable<string>> GetConnectionNamesAsync()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Add connections from file
            await LoadConnectionsFromFileAsync();
            if (_cachedConnections != null)
            {
                foreach (var key in _cachedConnections.Keys)
                {
                    result.Add(key);
                }
            }

            // Add connections from configuration
            var configSection = _configuration.GetSection("ConnectionStrings");
            foreach (var child in configSection.GetChildren())
            {
                result.Add(child.Key);
            }

            return result;
        }

        /// <summary>
        /// Loads connection strings from file
        /// </summary>
        private async Task LoadConnectionsFromFileAsync()
        {
            if (_cachedConnections != null)
                return;

            await _connectionLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (_cachedConnections != null)
                    return;

                _cachedConnections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (File.Exists(_connectionsFilePath))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(_connectionsFilePath);
                        var connections = JsonSerializer.Deserialize<Dictionary<string, string>>(
                            json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (connections != null)
                        {
                            foreach (var conn in connections)
                            {
                                _cachedConnections[conn.Key] = conn.Value;
                            }
                        }

                        _logger.LogInformation("Loaded {Count} connections from file", _cachedConnections.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading connections from file");
                    }
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        /// <summary>
        /// Saves connection strings to file
        /// </summary>
        private async Task SaveConnectionsToFileAsync()
        {
            if (_cachedConnections == null || _cachedConnections.Count == 0)
                return;

            try
            {
                var directory = Path.GetDirectoryName(_connectionsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_cachedConnections, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(_connectionsFilePath, json);
                _logger.LogInformation("Saved {Count} connections to file", _cachedConnections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving connections to file");
            }
        }
    }
}
