using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using mssqlMCP.Models;
using mssqlMCP.Services;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using mssqlMCP.Validation;

namespace mssqlMCP.Tools;

/// <summary>
/// MCP tool for managing database connections
/// </summary>


[McpServerToolType]
public class ConnectionManagerTool
{
    private readonly ILogger<ConnectionManagerTool> _logger;
    private readonly IConnectionManager _connectionManager;

    public ConnectionManagerTool(
        ILogger<ConnectionManagerTool> logger,
        IConnectionManager connectionManager)
    {
        _logger = logger;
        _connectionManager = connectionManager;
    }

    /// <summary>
    /// List all available connections
    /// </summary>
    [McpServerTool, Description("List all available database connections.")]
    public async Task<ListConnectionsResponse> ListConnectionsAsync()
    {
        try
        {
            _logger.LogInformation("Listing all database connections");
            var connections = await _connectionManager.GetAvailableConnectionsAsync();

            var response = new ListConnectionsResponse
            {
                Success = true,
                Connections = connections.Select(c => new mssqlMCP.Models.ConnectionInfo
                {
                    Name = c.Name,
                    Description = c.Description,
                    ServerType = c.ServerType,
                    LastUsed = c.LastUsed,
                    CreatedOn = c.CreatedOn
                }).ToList()
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing connections");
            return new ListConnectionsResponse
            {
                Success = false,
                ErrorMessage = "Failed to retrieve connections: " + ex.Message
            };
        }
    }    /// <summary>
         /// Test a connection string
         /// </summary>
    [McpServerTool, Description("Test a database connection.")]
    public async Task<TestConnectionResponse> TestConnectionAsync(TestConnectionRequest request)
    {
        // Validate input parameters
        if (request == null)
        {
            _logger.LogError("TestConnectionRequest cannot be null");
            return new TestConnectionResponse
            {
                Success = false,
                Message = "Request cannot be null"
            };
        }

        var validationResult = InputValidator.ValidateConnectionString(request.ConnectionString);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Invalid connection string: {validationResult.ErrorMessage}";
            _logger.LogError(errorMessage);
            return new TestConnectionResponse
            {
                Success = false,
                Message = errorMessage
            };
        }

        try
        {
            _logger.LogInformation("Testing connection string");
            var success = await _connectionManager.TestConnectionAsync(request.ConnectionString);

            return new TestConnectionResponse
            {
                Success = success,
                Message = success
                    ? "Connection test successful"
                    : "Connection test failed - could not connect to the database"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection");
            return new TestConnectionResponse
            {
                Success = false,
                Message = "Connection test failed: " + ex.Message
            };
        }
    }    /// <summary>
         /// Add a new connection
         /// </summary>
    [McpServerTool, Description("Add a new database connection.")]
    public async Task<AddConnectionResponse> AddConnectionAsync(AddConnectionRequest request)
    {
        // Validate input parameters
        if (request == null)
        {
            _logger.LogError("AddConnectionRequest cannot be null");
            return new AddConnectionResponse
            {
                Success = false,
                Message = "Request cannot be null"
            };
        }

        var nameValidation = InputValidator.ValidateConnectionName(request.Name);
        var connectionStringValidation = InputValidator.ValidateConnectionString(request.ConnectionString);
        var descriptionValidation = InputValidator.ValidateDescription(request.Description);
        var combinedValidation = InputValidator.Combine(nameValidation, connectionStringValidation, descriptionValidation);

        if (!combinedValidation.IsValid)
        {
            var errorMessage = $"Invalid input parameters: {combinedValidation.ErrorMessage}";
            _logger.LogError(errorMessage);
            return new AddConnectionResponse
            {
                Success = false,
                Message = errorMessage
            };
        }

        try
        {
            _logger.LogInformation("Adding connection: {ConnectionName}", request.Name);            // Check if connection with this name already exists
            var existing = await _connectionManager.GetConnectionEntryAsync(request.Name);
            if (existing != null)
            {
                return new AddConnectionResponse
                {
                    Success = false,
                    Message = $"A connection named '{request.Name}' already exists"
                };
            }

            var success = await _connectionManager.AddConnectionAsync(
                request.Name,
                request.ConnectionString,
                request.Description);

            return new AddConnectionResponse
            {
                Success = success,
                Message = success
                    ? $"Connection '{request.Name}' added successfully"
                    : "Failed to add connection - could not validate the connection string"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding connection {Name}", request.Name);
            return new AddConnectionResponse
            {
                Success = false,
                Message = "Failed to add connection: " + ex.Message
            };
        }
    }    /// <summary>
         /// Update an existing connection
         /// </summary>
    [McpServerTool, Description("Update an existing database connection.")]
    public async Task<UpdateConnectionResponse> UpdateConnectionAsync(UpdateConnectionRequest request)
    {
        // Validate input parameters
        if (request == null)
        {
            _logger.LogError("UpdateConnectionRequest cannot be null");
            return new UpdateConnectionResponse
            {
                Success = false,
                Message = "Request cannot be null"
            };
        }

        var nameValidation = InputValidator.ValidateConnectionName(request.Name);
        var connectionStringValidation = InputValidator.ValidateConnectionString(request.ConnectionString);
        var descriptionValidation = InputValidator.ValidateDescription(request.Description);
        var combinedValidation = InputValidator.Combine(nameValidation, connectionStringValidation, descriptionValidation);

        if (!combinedValidation.IsValid)
        {
            var errorMessage = $"Invalid input parameters: {combinedValidation.ErrorMessage}";
            _logger.LogError(errorMessage);
            return new UpdateConnectionResponse
            {
                Success = false,
                Message = errorMessage
            };
        }

        try
        {
            _logger.LogInformation("Updating connection: {ConnectionName}", request.Name);

            var success = await _connectionManager.UpdateConnectionAsync(
                request.Name,
                request.ConnectionString,
                request.Description);

            return new UpdateConnectionResponse
            {
                Success = success,
                Message = success
                    ? $"Connection '{request.Name}' updated successfully"
                    : $"Failed to update connection '{request.Name}' - connection not found or invalid connection string"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection {Name}", request.Name);
            return new UpdateConnectionResponse
            {
                Success = false,
                Message = "Failed to update connection: " + ex.Message
            };
        }
    }    /// <summary>
         /// Remove a connection
         /// </summary>
    [McpServerTool, Description("Remove a database connection.")]
    public async Task<RemoveConnectionResponse> RemoveConnectionAsync(RemoveConnectionRequest request)
    {
        // Validate input parameters
        if (request == null)
        {
            _logger.LogError("RemoveConnectionRequest cannot be null");
            return new RemoveConnectionResponse
            {
                Success = false,
                Message = "Request cannot be null"
            };
        }

        var validationResult = InputValidator.ValidateConnectionName(request.Name);
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Invalid connection name: {validationResult.ErrorMessage}";
            _logger.LogError(errorMessage);
            return new RemoveConnectionResponse
            {
                Success = false,
                Message = errorMessage
            };
        }

        try
        {
            _logger.LogInformation("Removing connection: {ConnectionName}", request.Name);

            var existing = await _connectionManager.GetConnectionEntryAsync(request.Name);
            if (existing == null)
            {
                return new RemoveConnectionResponse
                {
                    Success = false,
                    Message = $"Connection '{request.Name}' not found"
                };
            }

            var success = await _connectionManager.RemoveConnectionAsync(request.Name);

            return new RemoveConnectionResponse
            {
                Success = success,
                Message = success
                    ? $"Connection '{request.Name}' removed successfully"
                    : $"Failed to remove connection '{request.Name}'"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection {Name}", request.Name);
            return new RemoveConnectionResponse
            {
                Success = false,
                Message = "Failed to remove connection: " + ex.Message
            };
        }
    }        // All request/response classes moved to separate files in the Models folder
}
