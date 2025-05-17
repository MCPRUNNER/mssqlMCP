using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class TestConnectionRequestTests
    {
        [Fact]
        public void TestConnectionRequest_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var request = new TestConnectionRequest();

            // Assert
            Assert.Equal(string.Empty, request.ConnectionString);
        }

        [Fact]
        public void TestConnectionRequest_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var connectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

            // Act
            var request = new TestConnectionRequest
            {
                ConnectionString = connectionString
            };

            // Assert
            Assert.Equal(connectionString, request.ConnectionString);
        }
    }
}
