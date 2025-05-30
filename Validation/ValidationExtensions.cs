using System;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace mssqlMCP.Validation;

/// <summary>
/// Extension methods for validation and error handling in MCP tools
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Creates a standardized JSON error response
    /// </summary>
    public static string ToJsonError(this InputValidator.ValidationResult validationResult)
    {
        if (validationResult.IsValid)
            throw new InvalidOperationException("Cannot create error response from valid validation result");

        return JsonSerializer.Serialize(new { error = validationResult.ErrorMessage });
    }

    /// <summary>
    /// Creates a standardized JSON error response with custom message
    /// </summary>
    public static string ToJsonError(this string errorMessage)
    {
        return JsonSerializer.Serialize(new { error = errorMessage });
    }

    /// <summary>
    /// Logs validation errors at the appropriate level
    /// </summary>
    public static void LogValidationErrors(this ILogger logger, InputValidator.ValidationResult validationResult, string context)
    {
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                logger.LogError("Validation error in {Context}: {Error}", context, error);
            }
        }
    }

    /// <summary>
    /// Validates and returns early if validation fails
    /// </summary>
    public static bool ValidateAndReturnEarly<T>(
        this InputValidator.ValidationResult validationResult,
        ILogger logger,
        string context,
        out T? errorResponse) where T : class
    {
        errorResponse = null;

        if (validationResult.IsValid)
            return false; // Continue processing

        logger.LogValidationErrors(validationResult, context);

        // Try to create a response object with Success=false and Message properties
        try
        {
            var responseType = typeof(T);
            var response = Activator.CreateInstance<T>();

            var successProperty = responseType.GetProperty("Success");
            var messageProperty = responseType.GetProperty("Message");

            if (successProperty != null && successProperty.PropertyType == typeof(bool))
                successProperty.SetValue(response, false);

            if (messageProperty != null && messageProperty.PropertyType == typeof(string))
                messageProperty.SetValue(response, validationResult.ErrorMessage);

            errorResponse = response;
        }
        catch
        {
            // If we can't create the response object, leave it null
            errorResponse = null;
        }

        return true; // Stop processing, return error
    }
}

/// <summary>
/// Attribute to mark parameters that should be validated
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class ValidateAttribute : Attribute
{
    public string ValidationType
    {
        get;
    }

    public ValidateAttribute(string validationType)
    {
        ValidationType = validationType;
    }
}

/// <summary>
/// Common validation types for use with ValidateAttribute
/// </summary>
public static class ValidationTypes
{
    public const string ConnectionName = "ConnectionName";
    public const string Query = "Query";
    public const string Schema = "Schema";
    public const string ConnectionString = "ConnectionString";
    public const string Description = "Description";
    public const string EncryptionKey = "EncryptionKey";
    public const string ObjectType = "ObjectType";
}
