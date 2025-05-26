using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using mssqlMCP.Services;
using mssqlMCP.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite; // Ensure this using is present
using System.IO;

namespace mssqlMCP.Tests.Services
{
    public class SqliteConnectionRepositoryTests : IDisposable
    {
        private readonly SqliteConnectionRepository _repository;
        private readonly Mock<ILogger<SqliteConnectionRepository>> _mockLogger;
        private readonly Mock<IEncryptionService> _mockEncryptionService;
        private readonly string _testDbDirectoryPath; // Renamed for clarity

        public SqliteConnectionRepositoryTests()
        {
            _mockLogger = new Mock<ILogger<SqliteConnectionRepository>>();
            _mockEncryptionService = new Mock<IEncryptionService>();

            _mockEncryptionService.Setup(m => m.Encrypt(It.IsAny<string>()))
                .Returns<string>(s => $"encrypted:{s}");
            _mockEncryptionService.Setup(m => m.Decrypt(It.IsAny<string>()))
                .Returns<string>(s => s.StartsWith("encrypted:") ? s.Substring(10) : s);

            // Create a unique temporary directory for the test database for each test instance
            _testDbDirectoryPath = Path.Combine(Path.GetTempPath(), $"mcp_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDbDirectoryPath);

            Environment.SetEnvironmentVariable("MSSQL_MCP_DATA", _testDbDirectoryPath);

            _repository = new SqliteConnectionRepository(_mockLogger.Object, _mockEncryptionService.Object);
        }

        public void Dispose()
        {
            // Reset environment variable
            Environment.SetEnvironmentVariable("MSSQL_MCP_DATA", null);

            // Dispose repository to release its resources (e.g., SemaphoreSlim)
            _repository.Dispose();

            // Clear all SQLite connection pools to release file locks
            SqliteConnection.ClearAllPools(); // Add this line

            // Clean up test database directory
            if (Directory.Exists(_testDbDirectoryPath))
            {
                try
                {
                    Directory.Delete(_testDbDirectoryPath, true); // Delete the directory and its contents
                }
                catch (Exception ex)
                {
                    // Log to console, as logger might be disposed or unavailable here
                    Console.WriteLine($"Warning: Failed to delete test database directory '{_testDbDirectoryPath}': {ex.Message}");
                }
            }
        }

        [Fact]
        public async Task GetAllConnectionsAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllConnectionsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllConnectionsRawAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllConnectionsRawAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllConnectionsRawAsync_WithData_ReturnsRawEncryptedConnections()
        {
            // Arrange
            var connectionName = "RawTestDb";
            var originalConnectionString = "Server=raw;Database=test;";
            // This is what the mock encryption service will produce when "Server=raw;Database=test;" is encrypted
            var expectedEncryptedStringInDb = $"encrypted:{originalConnectionString}";

            await _repository.SaveConnectionAsync(new ConnectionEntry
            {
                Name = connectionName,
                ConnectionString = originalConnectionString,
                ServerType = "SQL Server"
            });

            // Act
            var rawConnections = await _repository.GetAllConnectionsRawAsync();
            var rawTestConnection = rawConnections.FirstOrDefault(c => c.Name == connectionName);

            // Assert
            Assert.NotNull(rawTestConnection);
            // Verify that the connection string retrieved by GetAllConnectionsRawAsync
            // is the same as what was stored (i.e., still in its "encrypted" form from the mock)
            Assert.Equal(expectedEncryptedStringInDb, rawTestConnection.ConnectionString);
        }


        [Fact]
        public async Task GetConnectionAsync_NonExistentConnection_ReturnsNull()
        {
            // Act
            var result = await _repository.GetConnectionAsync("non-existent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveConnectionAsync_NewConnection_ConnectionSaved()
        {
            // Arrange
            var newConnection = new ConnectionEntry
            {
                Name = "TestDb",
                ConnectionString = "Server=test;Database=testdb;",
                ServerType = "SQL Server",
                Description = "Test connection"
            };

            // Act
            await _repository.SaveConnectionAsync(newConnection);
            var savedConnection = await _repository.GetConnectionAsync("TestDb");

            // Assert
            Assert.NotNull(savedConnection);
            Assert.Equal("TestDb", savedConnection.Name);
            Assert.Equal("Server=test;Database=testdb;", savedConnection.ConnectionString); // Decrypted by GetConnectionAsync
            Assert.Equal("SQL Server", savedConnection.ServerType);
            Assert.Equal("Test connection", savedConnection.Description);

            _mockEncryptionService.Verify(m => m.Encrypt(It.Is<string>(s => s == "Server=test;Database=testdb;")), Times.Once);
            _mockEncryptionService.Verify(m => m.Decrypt(It.Is<string>(s => s == "encrypted:Server=test;Database=testdb;")), Times.Once);
        }

        [Fact]
        public async Task SaveConnectionAsync_ExistingConnection_ConnectionUpdated()
        {
            // Arrange
            var connection = new ConnectionEntry
            {
                Name = "UpdateTest",
                ConnectionString = "Server=original;Database=db;",
                ServerType = "SQL Server",
                Description = "Original description"
            };

            await _repository.SaveConnectionAsync(connection); // First save encrypts

            var updatedConnection = new ConnectionEntry
            {
                Name = "UpdateTest",
                ConnectionString = "Server=updated;Database=db;",
                ServerType = "SQL Server",
                Description = "Updated description"
            };

            // Act
            await _repository.SaveConnectionAsync(updatedConnection); // Second save also encrypts
            var result = await _repository.GetConnectionAsync("UpdateTest"); // Retrieval decrypts

            // Assert
            Assert.NotNull(result);
            Assert.Equal("UpdateTest", result.Name);
            Assert.Equal("Server=updated;Database=db;", result.ConnectionString);
            Assert.Equal("Updated description", result.Description);

            // Encrypt called twice (once for original, once for update)
            _mockEncryptionService.Verify(m => m.Encrypt(It.IsAny<string>()), Times.Exactly(2));
            // Decrypt called once for the final GetConnectionAsync
            _mockEncryptionService.Verify(m => m.Decrypt(It.Is<string>(s => s == "encrypted:Server=updated;Database=db;")), Times.Once);
        }

        [Fact]
        public async Task SaveConnectionStringDirectlyAsync_NewConnection_ConnectionSaved()
        {
            // Arrange
            var newConnection = new ConnectionEntry
            {
                Name = "DirectTest",
                ConnectionString = "encrypted:Server=direct;Database=db;",
                ServerType = "SQL Server",
                Description = "Direct test"
            };

            // Act
            await _repository.SaveConnectionStringDirectlyAsync(newConnection);
            var savedConnection = await _repository.GetConnectionAsync("DirectTest");

            // Assert
            Assert.NotNull(savedConnection);
            Assert.Equal("DirectTest", savedConnection.Name);
            Assert.Equal("Server=direct;Database=db;", savedConnection.ConnectionString); // Decrypted
            Assert.Equal("Direct test", savedConnection.Description);

            _mockEncryptionService.Verify(m => m.Encrypt(It.IsAny<string>()), Times.Never);
            _mockEncryptionService.Verify(m => m.Decrypt(It.Is<string>(s => s == "encrypted:Server=direct;Database=db;")), Times.Once);
        }

        [Fact]
        public async Task DeleteConnectionAsync_ExistingConnection_ConnectionRemoved()
        {
            // Arrange
            var connection = new ConnectionEntry
            {
                Name = "DeleteMe",
                ConnectionString = "Server=delete;Database=db;",
                ServerType = "SQL Server"
            };

            await _repository.SaveConnectionAsync(connection);
            var beforeDelete = await _repository.GetConnectionAsync("DeleteMe");
            Assert.NotNull(beforeDelete);

            // Act
            await _repository.DeleteConnectionAsync("DeleteMe");
            var afterDelete = await _repository.GetConnectionAsync("DeleteMe");

            // Assert
            Assert.Null(afterDelete);
        }

        [Fact]
        public async Task UpdateLastUsedAsync_ExistingConnection_LastUsedUpdated()
        {
            // Arrange
            var connectionName = "LastUsedTest";
            var initialLastUsed = DateTime.UtcNow.AddDays(-1);
            var connection = new ConnectionEntry
            {
                Name = connectionName,
                ConnectionString = "Server=test;Database=db;",
                ServerType = "SQL Server",
                LastUsed = initialLastUsed
            };
            // Need to save directly to control the LastUsed timestamp precisely for the setup
            await _repository.SaveConnectionStringDirectlyAsync(new ConnectionEntry
            {
                Name = connection.Name,
                ConnectionString = _mockEncryptionService.Object.Encrypt(connection.ConnectionString), // Manually encrypt for direct save
                ServerType = connection.ServerType,
                Description = connection.Description,
                CreatedOn = connection.CreatedOn, // Preserve if set
                LastUsed = connection.LastUsed
            });


            var beforeUpdate = await _repository.GetConnectionAsync(connectionName);
            Assert.NotNull(beforeUpdate?.LastUsed);
            // Ensure the setup timestamp is close to what we set
            Assert.True(Math.Abs((beforeUpdate.LastUsed.Value - initialLastUsed).TotalSeconds) < 1);


            // Act
            await _repository.UpdateLastUsedAsync(connectionName);

            var afterUpdate = await _repository.GetConnectionAsync(connectionName);

            // Assert
            Assert.NotNull(afterUpdate?.LastUsed);
            Assert.True(afterUpdate.LastUsed.Value > initialLastUsed, $"New LastUsed {afterUpdate.LastUsed.Value} was not after old {initialLastUsed}");
            Assert.True(afterUpdate.LastUsed.Value > beforeUpdate.LastUsed.Value, $"New LastUsed {afterUpdate.LastUsed.Value} was not after old {beforeUpdate.LastUsed.Value}");
        }

        [Fact]
        public async Task GetAllConnectionsAsync_WithMultipleConnections_ReturnsAllConnections()
        {
            // Arrange
            await _repository.SaveConnectionAsync(new ConnectionEntry
            {
                Name = "Conn1",
                ConnectionString = "Server=1;",
                ServerType = "SQL Server"
            });

            await _repository.SaveConnectionAsync(new ConnectionEntry
            {
                Name = "Conn2",
                ConnectionString = "Server=2;",
                ServerType = "SQL Server"
            });

            await _repository.SaveConnectionAsync(new ConnectionEntry
            {
                Name = "Conn3",
                ConnectionString = "Server=3;",
                ServerType = "SQL Server"
            });

            // Act
            var connections = (await _repository.GetAllConnectionsAsync()).ToList();

            // Assert
            Assert.Equal(3, connections.Count);
            Assert.Contains(connections, c => c.Name == "Conn1" && c.ConnectionString == "Server=1;");
            Assert.Contains(connections, c => c.Name == "Conn2" && c.ConnectionString == "Server=2;");
            Assert.Contains(connections, c => c.Name == "Conn3" && c.ConnectionString == "Server=3;");
        }

        [Fact]
        public async Task SaveConnectionAsync_CreatesCreatedOnTimestamp()
        {
            // Arrange
            var connection = new ConnectionEntry
            {
                Name = "TimestampTest",
                ConnectionString = "Server=test;Database=db;",
                ServerType = "SQL Server"
            };

            // Act
            await _repository.SaveConnectionAsync(connection);
            var saved = await _repository.GetConnectionAsync("TimestampTest");

            // Assert
            Assert.NotNull(saved?.CreatedOn);
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);
            var fiveMinutesHence = DateTime.UtcNow.AddMinutes(5); // Allow for slight clock skew
            Assert.True(saved.CreatedOn >= fiveMinutesAgo && saved.CreatedOn <= fiveMinutesHence,
                $"CreatedOn {saved.CreatedOn} was not within the expected range.");
        }

        [Fact]
        public async Task SaveConnectionStringDirectlyAsync_ExistingConnection_ConnectionUpdated()
        {
            // Arrange
            var connection = new ConnectionEntry
            {
                Name = "DirectUpdateTest",
                ConnectionString = "encrypted:Original",
                ServerType = "SQL Server",
                Description = "Original"
            };

            await _repository.SaveConnectionStringDirectlyAsync(connection);

            var updatedConnection = new ConnectionEntry
            {
                Name = "DirectUpdateTest",
                ConnectionString = "encrypted:Updated",
                ServerType = "SQL Server",
                Description = "Updated"
            };

            // Act
            await _repository.SaveConnectionStringDirectlyAsync(updatedConnection);
            var result = await _repository.GetConnectionAsync("DirectUpdateTest");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("DirectUpdateTest", result.Name);
            Assert.Equal("Updated", result.ConnectionString); // Decrypted
            Assert.Equal("Updated", result.Description);
        }

        [Fact]
        public async Task SaveConnectionStringDirectlyAsync_WithCreatedOn_PreservesTimestamp()
        {
            // Arrange
            var expectedTimestamp = new DateTime(2023, 1, 1, 10, 30, 0, DateTimeKind.Utc);
            var connection = new ConnectionEntry
            {
                Name = "TimestampPreserveTest",
                ConnectionString = "encrypted:Server=test;",
                ServerType = "SQL Server",
                CreatedOn = expectedTimestamp
            };

            // Act
            await _repository.SaveConnectionStringDirectlyAsync(connection);
            var saved = await _repository.GetConnectionAsync("TimestampPreserveTest");

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(expectedTimestamp, saved.CreatedOn);
        }
    }
}