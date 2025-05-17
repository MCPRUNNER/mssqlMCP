using Microsoft.Extensions.Logging;
using Moq;
using mssqlMCP.Models;
using mssqlMCP.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace mssqlMCP.Tests.Services
{
    public class KeyRotationServiceTests
    {
        private readonly Mock<ILogger<KeyRotationService>> _mockLogger;
        private readonly Mock<IConnectionRepository> _mockRepository;
        private readonly Mock<IEncryptionService> _mockEncryptionService;

        public KeyRotationServiceTests()
        {
            _mockLogger = new Mock<ILogger<KeyRotationService>>();
            _mockRepository = new Mock<IConnectionRepository>();
            _mockEncryptionService = new Mock<IEncryptionService>();
        }

        [Fact]
        public async Task RotateKeyAsync_WithValidKey_ShouldReencryptAll()
        {
            // Arrange
            var connections = new List<ConnectionEntry>
            {
                new ConnectionEntry { Name = "Conn1", ConnectionString = "ENC:EncryptedString1", ServerType = "SqlServer" },
                new ConnectionEntry { Name = "Conn2", ConnectionString = "ENC:EncryptedString2", ServerType = "SqlServer" }
            };

            _mockRepository.Setup(r => r.GetAllConnectionsAsync())
                .ReturnsAsync(connections);

            // Setup the mock to decrypt to plain text for the test
            _mockEncryptionService.Setup(e => e.Decrypt(It.IsAny<string>()))
                .Returns<string>(s => s.Replace("ENC:", "PlainText-"));

            var service = new KeyRotationService(
                _mockLogger.Object,
                _mockRepository.Object,
                _mockEncryptionService.Object);

            // Act
            var result = await service.RotateKeyAsync("NewKey123");

            // Assert
            Assert.Equal(2, result); // Two connections should be re-encrypted

            // Verify repository was called to get all connections
            _mockRepository.Verify(r => r.GetAllConnectionsAsync(), Times.Once);

            // Verify each connection was saved with directly
            _mockRepository.Verify(r => r.SaveConnectionStringDirectlyAsync(It.IsAny<ConnectionEntry>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RotateKeyAsync_WithEmptyKey_ShouldThrowArgumentException()
        {
            // Arrange
            var service = new KeyRotationService(
                _mockLogger.Object,
                _mockRepository.Object,
                _mockEncryptionService.Object);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.RotateKeyAsync(string.Empty));
        }

        [Fact]
        public async Task MigrateUnencryptedConnectionsAsync_ShouldEncryptOnlyUnencrypted()
        {
            // Arrange
            var connections = new List<ConnectionEntry>
            {
                new ConnectionEntry { Name = "Conn1", ConnectionString = "PlainText1", ServerType = "SqlServer" },
                new ConnectionEntry { Name = "Conn2", ConnectionString = "ENC:AlreadyEncrypted", ServerType = "SqlServer" },
                new ConnectionEntry { Name = "Conn3", ConnectionString = "PlainText3", ServerType = "SqlServer" }
            }; _mockRepository.Setup(r => r.GetAllConnectionsRawAsync())
                .ReturnsAsync(connections);

            // Setup to check if a string is encrypted
            _mockEncryptionService.Setup(e => e.IsEncrypted(It.IsAny<string>()))
                .Returns<string>(s => s.StartsWith("ENC:"));

            // Setup encrypt method to just prepend ENC: for the test
            _mockEncryptionService.Setup(e => e.Encrypt(It.IsAny<string>()))
                .Returns<string>(s => "ENC:" + s);

            // Setup decrypt method to return original for round-trip validation
            _mockEncryptionService.Setup(e => e.Decrypt(It.IsAny<string>()))
                .Returns<string>(s => s.StartsWith("ENC:") ? s.Substring(4) : s);

            var service = new KeyRotationService(
                _mockLogger.Object,
                _mockRepository.Object,
                _mockEncryptionService.Object);

            // Act
            var result = await service.MigrateUnencryptedConnectionsAsync();

            // Assert
            Assert.Equal(2, result); // Two connections should be encrypted (Conn1 and Conn3)

            // Verify repository was called to get all connections
            _mockRepository.Verify(r => r.GetAllConnectionsRawAsync(), Times.Once);

            // Verify each unencrypted connection was saved
            _mockRepository.Verify(r => r.SaveConnectionStringDirectlyAsync(It.Is<ConnectionEntry>(c => c.Name == "Conn1")), Times.Once);
            _mockRepository.Verify(r => r.SaveConnectionStringDirectlyAsync(It.Is<ConnectionEntry>(c => c.Name == "Conn3")), Times.Once);

            // Verify the already encrypted one was not saved
            _mockRepository.Verify(r => r.SaveConnectionStringDirectlyAsync(It.Is<ConnectionEntry>(c => c.Name == "Conn2")), Times.Never);
        }

        [Fact]
        public async Task MigrateUnencryptedConnectionsAsync_WithEmptyList_ShouldReturnZero()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllConnectionsRawAsync())
                .ReturnsAsync(new List<ConnectionEntry>());

            var service = new KeyRotationService(
                _mockLogger.Object,
                _mockRepository.Object,
                _mockEncryptionService.Object);

            // Act
            var result = await service.MigrateUnencryptedConnectionsAsync();

            // Assert
            Assert.Equal(0, result);
        }
    }
}
