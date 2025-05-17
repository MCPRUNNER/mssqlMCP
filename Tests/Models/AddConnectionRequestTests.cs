using Xunit;
using mssqlMCP.Models;
using System;

namespace mssqlMCP.Tests.Models
{
    public class AddConnectionRequestTests
    {
        [Fact]
        public void AddConnectionRequest_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var request = new AddConnectionRequest();

            // Assert
            Assert.Equal(string.Empty, request.Name);
            Assert.Equal(string.Empty, request.ConnectionString);
            Assert.Null(request.Description);
        }

        [Fact]
        public void AddConnectionRequest_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var name = "TestConnection";
            var connectionString = "Server=test;Database=testdb;";
            var description = "Test description";

            // Act
            var request = new AddConnectionRequest
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
