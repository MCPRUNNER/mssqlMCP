using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class RemoveConnectionResponseTests
    {
        [Fact]
        public void RemoveConnectionResponse_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var response = new RemoveConnectionResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
        }

        [Fact]
        public void RemoveConnectionResponse_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var success = true;
            var message = "Connection removed successfully";

            // Act
            var response = new RemoveConnectionResponse
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
