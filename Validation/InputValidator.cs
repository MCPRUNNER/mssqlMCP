using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace mssqlMCP.Validation;

/// <summary>
/// Provides comprehensive input validation for MCP tools
/// </summary>
public static class InputValidator
{
    // Maximum lengths for various inputs
    public const int MAX_CONNECTION_NAME_LENGTH = 100;
    public const int MAX_SCHEMA_NAME_LENGTH = 128;
    public const int MAX_QUERY_LENGTH = 50000;
    public const int MAX_DESCRIPTION_LENGTH = 500;
    public const int MAX_CONNECTION_STRING_LENGTH = 4000;
    public const int MIN_KEY_LENGTH = 16;
    public const int MAX_KEY_LENGTH = 256;

    // Regex patterns for validation
    private static readonly Regex ConnectionNamePattern = new(@"^[a-zA-Z0-9_\-\.]+$", RegexOptions.Compiled);
    private static readonly Regex SchemaNamePattern = new(@"^[a-zA-Z][a-zA-Z0-9_]*$", RegexOptions.Compiled);
    private static readonly Regex SqlIdentifierPattern = new(@"^[a-zA-Z][a-zA-Z0-9_]*$", RegexOptions.Compiled);

    // Dangerous SQL keywords that might indicate injection attempts
    private static readonly HashSet<string> DangerousKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "xp_cmdshell", "sp_configure", "openrowset", "opendatasource", "bulk",
        "exec master", "exec xp_", "sp_executesql", "sp_oacreate"
    };

    /// <summary>
    /// Validation result containing success status and error messages
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid
        {
            get; set;
        }
        public List<string> Errors { get; set; } = new();

        public string ErrorMessage => string.Join("; ", Errors);

        public static ValidationResult Success() => new() { IsValid = true };

        public static ValidationResult Failure(string error) => new()
        {
            IsValid = false,
            Errors = { error }
        };

        public static ValidationResult Failure(IEnumerable<string> errors) => new()
        {
            IsValid = false,
            Errors = errors.ToList()
        };
    }

    /// <summary>
    /// Validates a connection name
    /// </summary>
    public static ValidationResult ValidateConnectionName(string? connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionName))
            return ValidationResult.Failure("Connection name cannot be null or empty");

        if (connectionName.Length > MAX_CONNECTION_NAME_LENGTH)
            return ValidationResult.Failure($"Connection name cannot exceed {MAX_CONNECTION_NAME_LENGTH} characters");

        if (!ConnectionNamePattern.IsMatch(connectionName))
            return ValidationResult.Failure("Connection name can only contain letters, numbers, underscores, hyphens, and dots");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a schema name
    /// </summary>
    public static ValidationResult ValidateSchemaName(string? schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            return ValidationResult.Success(); // Schema is optional

        if (schemaName.Length > MAX_SCHEMA_NAME_LENGTH)
            return ValidationResult.Failure($"Schema name cannot exceed {MAX_SCHEMA_NAME_LENGTH} characters");

        if (!SchemaNamePattern.IsMatch(schemaName))
            return ValidationResult.Failure("Schema name must start with a letter and contain only letters, numbers, and underscores");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a SQL query
    /// </summary>
    public static ValidationResult ValidateQuery(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return ValidationResult.Failure("Query cannot be null or empty");

        if (query.Length > MAX_QUERY_LENGTH)
            return ValidationResult.Failure($"Query cannot exceed {MAX_QUERY_LENGTH} characters");

        // Check for dangerous keywords
        foreach (var keyword in DangerousKeywords)
        {
            if (query.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return ValidationResult.Failure($"Query contains potentially dangerous keyword: {keyword}");
        }        // Basic SQL injection checks
        var suspiciousPatterns = new[]
        {
            @";\s*drop\s+table", @";\s*delete\s+from", @";\s*truncate\s+table",
            @"union\s+select.*password", @"'.*or.*'.*=.*'", @"--\s*$",
            @"'\s*=\s*'" // Common injection pattern like '1'='1'
        };

        foreach (var pattern in suspiciousPatterns)
        {
            if (Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                return ValidationResult.Failure("Query contains potentially malicious patterns");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a connection string
    /// </summary>
    public static ValidationResult ValidateConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return ValidationResult.Failure("Connection string cannot be null or empty");

        if (connectionString.Length > MAX_CONNECTION_STRING_LENGTH)
            return ValidationResult.Failure($"Connection string cannot exceed {MAX_CONNECTION_STRING_LENGTH} characters");

        // Basic connection string format validation
        if (!connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            return ValidationResult.Failure("Connection string must contain a Server or Data Source parameter");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a description
    /// </summary>
    public static ValidationResult ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return ValidationResult.Success(); // Description is optional

        if (description.Length > MAX_DESCRIPTION_LENGTH)
            return ValidationResult.Failure($"Description cannot exceed {MAX_DESCRIPTION_LENGTH} characters");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates an encryption key
    /// </summary>
    public static ValidationResult ValidateEncryptionKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return ValidationResult.Failure("Encryption key cannot be null or empty");

        if (key.Length < MIN_KEY_LENGTH)
            return ValidationResult.Failure($"Encryption key must be at least {MIN_KEY_LENGTH} characters long");

        if (key.Length > MAX_KEY_LENGTH)
            return ValidationResult.Failure($"Encryption key cannot exceed {MAX_KEY_LENGTH} characters");

        // Check for minimum complexity
        if (!HasMinimumComplexity(key))
            return ValidationResult.Failure("Encryption key must contain a mix of letters, numbers, and special characters");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates an object type parameter
    /// </summary>
    public static ValidationResult ValidateObjectType(string? objectType)
    {
        if (string.IsNullOrWhiteSpace(objectType))
            return ValidationResult.Success(); // Object type is optional, defaults to "ALL"

        var validTypes = new[] { "TABLE", "TABLES", "VIEW", "VIEWS", "PROCEDURE", "PROCEDURES", "FUNCTION", "FUNCTIONS", "ALL" };

        if (!validTypes.Contains(objectType, StringComparer.OrdinalIgnoreCase))
            return ValidationResult.Failure($"Object type must be one of: {string.Join(", ", validTypes)}");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a boolean parameter
    /// </summary>
    public static ValidationResult ValidateBoolean(bool? value, string parameterName)
    {
        // Boolean validation is typically not needed since the type system handles it,
        // but this is here for completeness and future extensibility
        return ValidationResult.Success();
    }

    /// <summary>
    /// Validates a timeout value
    /// </summary>
    public static ValidationResult ValidateTimeout(int? timeoutSeconds)
    {
        if (!timeoutSeconds.HasValue)
            return ValidationResult.Success(); // Timeout is optional

        if (timeoutSeconds.Value < 1)
            return ValidationResult.Failure("Timeout must be at least 1 second");

        if (timeoutSeconds.Value > 3600) // 1 hour max
            return ValidationResult.Failure("Timeout cannot exceed 3600 seconds (1 hour)");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Combines multiple validation results
    /// </summary>
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var errors = results.Where(r => !r.IsValid).SelectMany(r => r.Errors).ToList();

        if (errors.Any())
            return ValidationResult.Failure(errors);

        return ValidationResult.Success();
    }

    /// <summary>
    /// Checks if a string has minimum complexity for encryption keys
    /// </summary>
    private static bool HasMinimumComplexity(string input)
    {
        var hasLetter = input.Any(char.IsLetter);
        var hasDigit = input.Any(char.IsDigit);
        var hasSpecial = input.Any(c => !char.IsLetterOrDigit(c));

        return hasLetter && (hasDigit || hasSpecial);
    }
}
