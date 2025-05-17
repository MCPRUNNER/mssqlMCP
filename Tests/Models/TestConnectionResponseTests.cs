using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class TestConnectionResponseTests
    {
        [Fact]
        public void TestConnectionResponse_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var response = new TestConnectionResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
        }

        [Fact]
        public void TestConnectionResponse_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var success = true;
            var message = "Connection test successful";

            // Act
            var response = new TestConnectionResponse
            {
                Success = success,
                Message = message
            };

            // Assert
            Assert.Equal(success, response.Success);
            Assert.Equal(message, response.Message);
        }
    }
}
