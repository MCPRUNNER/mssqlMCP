using Xunit;
using mssqlMCP.Models;
using System;

namespace mssqlMCP.Tests.Models
{
    public class ConnectionEntryTests
    {
        [Fact]
        public void ConnectionEntry_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var entry = new ConnectionEntry();

            // Assert
            Assert.Equal(string.Empty, entry.Name);
            Assert.Equal(string.Empty, entry.ConnectionString);
            Assert.Equal("SqlServer", entry.ServerType);
            Assert.Null(entry.Description);
            Assert.Null(entry.LastUsed);
            // CreatedOn should be around now
            var timeSpan = DateTime.UtcNow - entry.CreatedOn;
            Assert.True(timeSpan.TotalSeconds < 10);
        }

        [Fact]
        public void ConnectionEntry_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var name = "TestConnection";
            var connectionString = "Server=test;Database=testdb;";
            var serverType = "PostgreSQL";
            var description = "Test description";
            var lastUsed = DateTime.UtcNow.AddDays(-1);
            var createdOn = DateTime.UtcNow.AddDays(-7);

            // Act
            var entry = new ConnectionEntry
            {
                Name = name,
                ConnectionString = connectionString,
                ServerType = serverType,
                Description = description,
                LastUsed = lastUsed,
                CreatedOn = createdOn
            };

            // Assert
            Assert.Equal(name, entry.Name);
            Assert.Equal(connectionString, entry.ConnectionString);
            Assert.Equal(serverType, entry.ServerType);
            Assert.Equal(description, entry.Description);
            Assert.Equal(lastUsed, entry.LastUsed);
            Assert.Equal(createdOn, entry.CreatedOn);
        }
    }
}
