using Xunit;
using mssqlMCP.Validation;
using System.Linq;

namespace mssqlMCP.Tests.Validation;

public class InputValidatorTests
{
    [Theory]
    [InlineData("ValidConnection", true)]
    [InlineData("Valid_Connection_123", true)]
    [InlineData("Valid-Connection.Name", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("Invalid Connection Name", false)] // Contains space
    [InlineData("Invalid@Connection", false)] // Contains @
    public void ValidateConnectionName_ShouldReturnExpectedResult(string? connectionName, bool expectedValid)
    {
        // Act
        var result = InputValidator.ValidateConnectionName(connectionName);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void ValidateConnectionName_WithTooLongName_ShouldReturnInvalid()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        var result = InputValidator.ValidateConnectionName(longName);

        // Assert
        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData("dbo", true)]
    [InlineData("Schema123", true)]
    [InlineData("Schema_Name", true)]
    [InlineData("", true)] // Optional parameter
    [InlineData(null, true)] // Optional parameter
    [InlineData("123Schema", false)] // Cannot start with number
    [InlineData("Schema Name", false)] // Contains space
    [InlineData("Schema-Name", false)] // Contains hyphen
    public void ValidateSchemaName_ShouldReturnExpectedResult(string? schemaName, bool expectedValid)
    {
        // Act
        var result = InputValidator.ValidateSchemaName(schemaName);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void ValidateSchemaName_WithTooLongName_ShouldReturnInvalid()
    {
        // Arrange
        var longSchema = new string('a', 129);

        // Act
        var result = InputValidator.ValidateSchemaName(longSchema);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateQuery_WithValidQueries_ShouldReturnValid()
    {
        // Arrange & Act & Assert
        Assert.True(InputValidator.ValidateQuery("SELECT * FROM Users").IsValid);
        Assert.True(InputValidator.ValidateQuery("INSERT INTO Users (Name) VALUES ('Test')").IsValid);
    }

    [Fact]
    public void ValidateQuery_WithInvalidQueries_ShouldReturnInvalid()
    {
        // Arrange & Act & Assert
        Assert.False(InputValidator.ValidateQuery("").IsValid);
        Assert.False(InputValidator.ValidateQuery(null).IsValid);
        Assert.False(InputValidator.ValidateQuery("SELECT * FROM Users; DROP TABLE Users;").IsValid);
        Assert.False(InputValidator.ValidateQuery("SELECT * FROM Users WHERE '1'='1'").IsValid);
        Assert.False(InputValidator.ValidateQuery("EXEC xp_cmdshell 'dir'").IsValid);
    }

    [Theory]
    [InlineData("Server=localhost;Database=TestDB;Trusted_Connection=True;", true)]
    [InlineData("Data Source=server;Initial Catalog=db;User ID=user;Password=pass;", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("InvalidConnectionString", false)] // Missing Server/Data Source
    public void ValidateConnectionString_ShouldReturnExpectedResult(string? connectionString, bool expectedValid)
    {
        // Act
        var result = InputValidator.ValidateConnectionString(connectionString);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void ValidateDescription_WithValidDescriptions_ShouldReturnValid()
    {
        // Arrange & Act & Assert
        Assert.True(InputValidator.ValidateDescription("Valid description").IsValid);
        Assert.True(InputValidator.ValidateDescription("").IsValid);
        Assert.True(InputValidator.ValidateDescription(null).IsValid);
    }

    [Fact]
    public void ValidateDescription_WithTooLongDescription_ShouldReturnInvalid()
    {
        // Arrange
        var longDescription = new string('a', 501);

        // Act
        var result = InputValidator.ValidateDescription(longDescription);

        // Assert
        Assert.False(result.IsValid);
    }
    [Fact]
    public void ValidateEncryptionKey_WithValidKeys_ShouldReturnValid()
    {
        // Arrange & Act & Assert
        Assert.True(InputValidator.ValidateEncryptionKey("StrongKey1234!@#").IsValid);
        Assert.True(InputValidator.ValidateEncryptionKey("ValidKey1234567890").IsValid);
    }

    [Fact]
    public void ValidateEncryptionKey_WithInvalidKeys_ShouldReturnInvalid()
    {
        // Arrange & Act & Assert
        Assert.False(InputValidator.ValidateEncryptionKey("").IsValid);
        Assert.False(InputValidator.ValidateEncryptionKey(null).IsValid);
        Assert.False(InputValidator.ValidateEncryptionKey("weak").IsValid);
        Assert.False(InputValidator.ValidateEncryptionKey("weakkeywithoutcomplexity").IsValid);

        var longKey = new string('a', 257);
        Assert.False(InputValidator.ValidateEncryptionKey(longKey).IsValid);
    }

    [Theory]
    [InlineData("TABLE", true)]
    [InlineData("VIEW", true)]
    [InlineData("PROCEDURE", true)]
    [InlineData("ALL", true)]
    [InlineData("", true)] // Optional parameter
    [InlineData(null, true)] // Optional parameter
    [InlineData("table", true)] // Case insensitive
    [InlineData("INVALID", false)]
    public void ValidateObjectType_ShouldReturnExpectedResult(string? objectType, bool expectedValid)
    {
        // Act
        var result = InputValidator.ValidateObjectType(objectType);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Theory]
    [InlineData(30, true)]
    [InlineData(1, true)]
    [InlineData(3600, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(3601, false)]
    public void ValidateTimeout_ShouldReturnExpectedResult(int? timeout, bool expectedValid)
    {
        // Act
        var result = InputValidator.ValidateTimeout(timeout);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void Combine_ShouldCombineMultipleValidationResults()
    {
        // Arrange
        var validResult = InputValidator.ValidationResult.Success();
        var invalidResult1 = InputValidator.ValidationResult.Failure("Error 1");
        var invalidResult2 = InputValidator.ValidationResult.Failure("Error 2");

        // Act
        var combinedValid = InputValidator.Combine(validResult, validResult);
        var combinedInvalid = InputValidator.Combine(validResult, invalidResult1, invalidResult2);

        // Assert
        Assert.True(combinedValid.IsValid);
        Assert.False(combinedInvalid.IsValid);
        Assert.Equal(2, combinedInvalid.Errors.Count);
        Assert.Contains("Error 1", combinedInvalid.Errors);
        Assert.Contains("Error 2", combinedInvalid.Errors);
    }
}
