using Xunit;
using mssqlMCP.Models;
using System;
using System.Collections.Generic;

namespace mssqlMCP.Tests.Models
{
    public class ListConnectionsResponseTests
    {
        [Fact]
        public void ListConnectionsResponse_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var response = new ListConnectionsResponse();

            // Assert
            Assert.False(response.Success);
            Assert.NotNull(response.Connections);
            Assert.Empty(response.Connections);
            Assert.Null(response.ErrorMessage);
        }

        [Fact]
        public void ListConnectionsResponse_PropertiesAssignment_WorksCorrectly()
        {
            // Arrange
            var success = true;
            var errorMessage = "Error retrieving connections";
            var connections = new List<mssqlMCP.Models.ConnectionInfo>
            {
                new mssqlMCP.Models.ConnectionInfo
                {
                    Name = "Test1",
                    ServerType = "SqlServer",
                    Description = "Test Description 1",
                    LastUsed = DateTime.UtcNow.AddDays(-1),
                    CreatedOn = DateTime.UtcNow.AddDays(-10)
                },
                new mssqlMCP.Models.ConnectionInfo
                {
                    Name = "Test2",
                    ServerType = "SqlServer",
                    Description = "Test Description 2",
                    LastUsed = DateTime.UtcNow.AddDays(-2),
                    CreatedOn = DateTime.UtcNow.AddDays(-15)
                }
            };

            // Act
            var response = new ListConnectionsResponse
            {
                Success = success,
                Connections = connections,
                ErrorMessage = errorMessage
            };

            // Assert
            Assert.Equal(success, response.Success);
            Assert.Equal(connections, response.Connections);
            Assert.Equal(2, response.Connections.Count);
            Assert.Equal(errorMessage, response.ErrorMessage);
        }
    }
}
