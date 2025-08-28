using System;

namespace mssqlMCP.Models;

/// <summary>
/// Represents an API key used for authentication
/// </summary>
public class ApiKey
{
    /// <summary>
    /// Unique identifier for the API key
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The name of the API key (for user reference)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The actual key value (encrypted)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The user or service that owns this API key
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the key was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the key expires (null for no expiration)
    /// </summary>
    public DateTime? ExpirationDate
    {
        get; set;
    }

    /// <summary>
    /// When the key was last used
    /// </summary>
    public DateTime? LastUsed
    {
        get; set;
    }

    /// <summary>
    /// Indicates if the key is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Type of key (e.g., "user", "service", "admin")
    /// </summary>
    public string KeyType { get; set; } = "user";

    /// <summary>
    /// Optional description of the key's purpose
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// JSON string array of allowed connection names. If null or empty, all connections are allowed.
    /// This only applies to user type keys.
    /// </summary>
    /// <example>["conn1", "conn2"]</example>
    public string? AllowedConnectionNames { get; set; }
}

/// <summary>
/// Request model for creating a new API key
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// Name for the new API key
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// User ID that owns the key
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Type of key being created
    /// </summary>
    public string KeyType { get; set; } = "user";

    /// <summary>
    /// Optional expiration date
    /// </summary>
    public DateTime? ExpirationDate
    {
        get; set;
    }

    /// <summary>
    /// Optional list of connection names that this key is allowed to access.
    /// </summary>
    public List<string>? AllowedConnectionNames { get; set; }
}

/// <summary>
/// Response model for API key operations
/// </summary>
public class ApiKeyResponse
{
    /// <summary>
    /// Key ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Key name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// API key value (only included when creating a new key)
    /// </summary>
    public string? Key
    {
        get; set;
    }

    /// <summary>
    /// Key owner
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// When the key was created
    /// </summary>
    public DateTime CreatedAt
    {
        get; set;
    }

    /// <summary>
    /// When the key expires (if applicable)
    /// </summary>
    public DateTime? ExpirationDate
    {
        get; set;
    }

    /// <summary>
    /// Last time the key was used
    /// </summary>
    public DateTime? LastUsed
    {
        get; set;
    }

    /// <summary>
    /// Whether the key is currently active
    /// </summary>
    public bool IsActive
    {
        get; set;
    }

    /// <summary>
    /// Type of API key
    /// </summary>
    public string KeyType { get; set; } = string.Empty;

    /// <summary>
    /// Optional description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional list of connection names that this key is allowed to access.
    /// </summary>
    public List<string>? AllowedConnectionNames { get; set; }
}

/// <summary>
/// Request to revoke an API key
/// </summary>
public class RevokeApiKeyRequest
{
    /// <summary>
    /// ID of the key to revoke
    /// </summary>
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Model for tracking API key usage
/// </summary>
public class ApiKeyUsageLog
{
    /// <summary>
    /// Unique ID for the log entry
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// API key used
    /// </summary>
    public string ApiKeyId { get; set; } = string.Empty;

    /// <summary>
    /// User associated with the key
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp of the API call
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Endpoint or resource accessed
    /// </summary>
    public string Resource { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method used
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the requester
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// User agent of the client
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
}
