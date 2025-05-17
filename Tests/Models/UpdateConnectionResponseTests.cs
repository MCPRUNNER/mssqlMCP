using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class UpdateConnectionResponseTests
    {
        [Fact]
        public void UpdateConnectionResponse_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var response = new UpdateConnectionResponse();

            // Assert
            Assert.False(response.Success);
            Assert.Equal(string.Empty, response.Message);
        }

        [Fact]
        public void UpdateConnectionResponse_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var success = true;
            var message = "Connection updated successfully";

            // Act
            var response = new UpdateConnectionResponse
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
