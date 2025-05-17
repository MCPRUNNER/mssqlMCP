using Xunit;
using mssqlMCP.Models;

namespace mssqlMCP.Tests.Models
{
    public class UpdateConnectionRequestTests
    {
        [Fact]
        public void UpdateConnectionRequest_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var request = new UpdateConnectionRequest();

            // Assert
            Assert.Equal(string.Empty, request.Name);
            Assert.Equal(string.Empty, request.ConnectionString);
            Assert.Null(request.Description);
        }

        [Fact]
        public void UpdateConnectionRequest_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var name = "TestConnection";
            var connectionString = "Server=newServer;Database=newDb;User Id=newUser;Password=newPass;";
            var description = "Updated test description";

            // Act
            var request = new UpdateConnectionRequest
            {
                Name = name,
                ConnectionString = connectionString,
                Description = description
            };

            // Assert
            Assert.Equal(name, request.Name);
            Assert.Equal(connectionString, request.ConnectionString);
            Assert.Equal(description, request.Description);
        }
    }
}
