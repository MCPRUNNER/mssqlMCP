using Xunit;
using mssqlMCP.Models;
using System;

namespace mssqlMCP.Tests.Models
{
    public class ConnectionInfoTests
    {
        [Fact]
        public void ConnectionInfo_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var info = new mssqlMCP.Models.ConnectionInfo();

            // Assert
            Assert.Equal(string.Empty, info.Name);
            Assert.Equal(string.Empty, info.ServerType);
            Assert.Null(info.Description);
            Assert.Null(info.LastUsed);
            Assert.Equal(default, info.CreatedOn);
        }

        [Fact]
        public void ConnectionInfo_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var name = "TestConnection";
            var serverType = "SqlServer";
            var description = "Test description";
            var lastUsed = DateTime.UtcNow.AddDays(-1);
            var createdOn = DateTime.UtcNow.AddDays(-7);

            // Act
            var info = new mssqlMCP.Models.ConnectionInfo
            {
                Name = name,
                ServerType = serverType,
                Description = description,
                LastUsed = lastUsed,
                CreatedOn = createdOn
            };

            // Assert
            Assert.Equal(name, info.Name);
            Assert.Equal(serverType, info.ServerType);
            Assert.Equal(description, info.Description);
            Assert.Equal(lastUsed, info.LastUsed);
            Assert.Equal(createdOn, info.CreatedOn);
        }
    }
}
