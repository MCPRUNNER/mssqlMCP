using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class RemoveConnectionRequestTests
    {
        [Fact]
        public void RemoveConnectionRequest_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var request = new RemoveConnectionRequest();

            // Assert
            Assert.Equal(string.Empty, request.Name);
        }

        [Fact]
        public void RemoveConnectionRequest_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var name = "TestConnection";

            // Act
            var request = new RemoveConnectionRequest
            {
                Name = name
            };

            // Assert
            Assert.Equal(name, request.Name);
        }
    }
}
