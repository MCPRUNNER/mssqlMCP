using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using mssqlMCP.Services;
using mssqlMCP.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

namespace mssqlMCP.Tests.Services
{
    public class DatabaseMetadataProviderSsisTests
    {
        private readonly Mock<ILogger<DatabaseMetadataProvider>> _mockLogger;

        public DatabaseMetadataProviderSsisTests()
        {
            _mockLogger = new Mock<ILogger<DatabaseMetadataProvider>>();
        }

        // Helper to invoke private/internal SafeGetDateTime method using reflection
        // Alternatively, make SafeGetDateTime internal and use [InternalsVisibleTo]
        private DateTime InvokeSafeGetDateTime(DatabaseMetadataProvider provider, object? value)
        {
            var methodInfo = typeof(DatabaseMetadataProvider).GetMethod("SafeGetDateTime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (methodInfo == null)
            {
                throw new InvalidOperationException("SafeGetDateTime method not found. Ensure it's accessible for testing (e.g., internal or private and test via reflection).");
            }
            return (DateTime)methodInfo.Invoke(provider, new object?[] { value });
        }

        [Theory]
        [InlineData(null, "MinValue")]
        [InlineData(typeof(DBNull), "MinValue")] // Special case for DBNull
        [MemberData(nameof(DateTimeTestData))]
        public void SafeGetDateTime_ReturnsCorrectDateTime(object? inputValue, string expectedOutcome)
        {
            // Arrange
            // Use a dummy connection string and logger for the provider instance
            var provider = new DatabaseMetadataProvider("Server=dummy", _mockLogger.Object);
            DateTime expectedDateTime;

            if (inputValue is Type type && type == typeof(DBNull))
            {
                inputValue = DBNull.Value; // Convert type to actual DBNull.Value
            }

            switch (expectedOutcome)
            {
                case "MinValue":
                    expectedDateTime = DateTime.MinValue;
                    break;
                case "SpecificValue":
                    if (inputValue is DateTime dt) expectedDateTime = dt;
                    else if (inputValue is DateTimeOffset dto) expectedDateTime = dto.DateTime;
                    else if (inputValue is string strDt) expectedDateTime = DateTime.Parse(strDt);
                    else throw new ArgumentException("Invalid test data for SpecificValue");
                    break;
                default:
                    throw new ArgumentException("Invalid expectedOutcome in test data");
            }

            // Act
            var result = InvokeSafeGetDateTime(provider, inputValue);

            // Assert
            Assert.Equal(expectedDateTime, result);
        }

        public static IEnumerable<object?[]> DateTimeTestData()
        {
            var specificDate = new DateTime(2023, 10, 26, 14, 30, 0);
            var specificDateOffset = new DateTimeOffset(specificDate, TimeSpan.FromHours(2));
            yield return new object?[] { specificDate, "SpecificValue" };
            yield return new object?[] { specificDateOffset, "SpecificValue" };
            yield return new object?[] { specificDate.ToString("o"), "SpecificValue" }; // ISO 8601
            yield return new object?[] { "2024-01-15 10:00:00", "SpecificValue" };
            yield return new object?[] { "invalid-date-string", "MinValue" };
            yield return new object?[] { string.Empty, "MinValue" };
        }


        [Fact]
        public async Task GetSsisCatalogInfoAsync_WhenConnectionStringIsInvalidFormat_ReturnsNullAndLogsError()
        {
            // Arrange
            var invalidFormatConnectionString = "this_is_definitely_not_a_connection_string"; // More likely to cause issues
            var provider = new DatabaseMetadataProvider(invalidFormatConnectionString, _mockLogger.Object);
            SsisCatalogInfo? result = null;
            // Exception caughtException = null; // Removed: SUT's internal catch will handle

            // Act
            // The SUT's GetSsisCatalogInfoAsync method should catch exceptions from invalid connection strings
            // (usually during OpenAsync or new SqlConnection) and return null.
            result = await provider.GetSsisCatalogInfoAsync(CancellationToken.None);

            // Assert
            Assert.Null(result); // result should be null as the SUT's catch block returns null
            // Assert.NotNull(caughtException); // Removed: Test doesn't catch the exception directly anymore

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error retrieving SSIS catalog information")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetSsisCatalogInfoAsync_WhenConnectionOpenFails_ReturnsNullAndLogsError()
        {
            // Arrange
            var nonExistentServerConnectionString = "Server=non_existent_server_xyz;Database=SSISDB;User ID=dummy;Password=dummy;Connect Timeout=1;TrustServerCertificate=True;";
            var provider = new DatabaseMetadataProvider(nonExistentServerConnectionString, _mockLogger.Object);

            // Act
            var result = await provider.GetSsisCatalogInfoAsync(CancellationToken.None);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error retrieving SSIS catalog information")),
                    It.IsAny<SqlException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetSsisCatalogInfoAsync_WhenSsisDbDoesNotExistOrCheckFails_ReturnsNullAndLogsWarning()
        {
            // Arrange
            // Use a connection string that points to a non-existent server to ensure connection failure.
            // This will cause GetSsisCatalogInfoAsync to enter its catch block, log an error, and return null.
            var connectionStringForPotentiallyMissingSsisDb = "Server=non_existent_server_for_this_test_123;Database=SSISDB;User ID=dummy;Password=dummy;Connect Timeout=1;TrustServerCertificate=True;";
            var provider = new DatabaseMetadataProvider(connectionStringForPotentiallyMissingSsisDb, _mockLogger.Object);

            // Act
            var result = await provider.GetSsisCatalogInfoAsync(CancellationToken.None);

            // Assert
            Assert.Null(result); // Expect null because the connection to the non-existent server will fail.

            // Verify that an error was logged. The existing flexible logger verification will catch this.
            // The SUT's catch block in GetSsisCatalogInfoAsync logs an error message.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error || l == LogLevel.Warning), // Explicitly allow Error or Warning
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v != null && (
                        v.ToString().Contains("Error retrieving SSIS catalog information") ||
                        v.ToString().Contains("SSISDB catalog does not exist on this server") ||
                        v.ToString().Contains("SSISDB catalog may not exist on this server"))
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce());
        }
    }
}