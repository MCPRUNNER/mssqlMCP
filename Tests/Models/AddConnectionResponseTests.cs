using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class AddConnectionResponseTests
    {
        [Fact]
        public void AddConnectionResponse_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var response = new AddConnectionResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
        }

        [Fact]
        public void AddConnectionResponse_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var success = true;
            var message = "Connection added successfully";

            // Act
            var response = new AddConnectionResponse
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
