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
    public class EnhancedSecurityTests
    {
        private readonly Mock<ILogger<EncryptionService>> _encryptionLogger;
        private readonly Mock<ILogger<KeyRotationService>> _rotationLogger;
        private readonly Mock<IConnectionRepository> _mockRepository;
        private readonly IEncryptionService _encryptionService;

        public EnhancedSecurityTests()
        {
            _encryptionLogger = new Mock<ILogger<EncryptionService>>();
            _rotationLogger = new Mock<ILogger<KeyRotationService>>();
            _mockRepository = new Mock<IConnectionRepository>();

            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            _encryptionService = new EncryptionService(_encryptionLogger.Object);
        }

        [Fact]
        public void GenerateSecureKey_ShouldCreateRandomKeys()
        {
            // Arrange
            var key1 = _encryptionService.GenerateSecureKey();
            var key2 = _encryptionService.GenerateSecureKey();

            // Assert
            Assert.NotEqual(key1, key2);
            Assert.Equal(44, key1.Length); // Base64 encoding of 32 bytes
        }

        [Fact]
        public void GenerateSecureKey_WithCustomLength_ShouldCreateCorrectLengthKey()
        {
            // Arrange
            int keyLengthBytes = 16;

            // Act
            var key = _encryptionService.GenerateSecureKey(keyLengthBytes);

            // Assert
            // Base64 encoding formula: ceiling(n / 3) * 4
            int expectedBase64Length = (int)Math.Ceiling(keyLengthBytes / 3.0) * 4;
            Assert.Equal(expectedBase64Length, key.Length);
        }

        [Fact]
        public async Task RotateKeyAsync_WithValidationFailure_ShouldSkipConnection()
        {
            // Arrange
            var connections = new List<ConnectionEntry>
            {
                new ConnectionEntry { Name = "Conn1", ConnectionString = "ServerA", ServerType = "SqlServer" },
                new ConnectionEntry { Name = "Conn2", ConnectionString = "ServerB", ServerType = "SqlServer" }
            };

            _mockRepository.Setup(r => r.GetAllConnectionsAsync())
                .ReturnsAsync(connections);            // Setup mockery to simulate validation failure for the second connection
            var tempEncryptionServiceMock = new Mock<IEncryptionService>();

            // Setup encrypt for both connections
            tempEncryptionServiceMock.Setup(e => e.Encrypt(It.Is<string>(s => s == "ServerA")))
                .Returns("ENC:EncryptedServerA");
            tempEncryptionServiceMock.Setup(e => e.Encrypt(It.Is<string>(s => s == "ServerB")))
                .Returns("ENC:EncryptedServerB");

            // Setup decrypt to succeed for first connection
            tempEncryptionServiceMock.Setup(e => e.Decrypt("ENC:EncryptedServerA"))
                .Returns("ServerA"); // Correct decryption

            // Setup decrypt to fail for second connection
            tempEncryptionServiceMock.Setup(e => e.Decrypt("ENC:EncryptedServerB"))
                .Returns("ModifiedServerB"); // Simulating validation failure - decrypted != original

            // Setup IsEncrypted method
            tempEncryptionServiceMock.Setup(e => e.IsEncrypted(It.IsAny<string>()))
                .Returns<string>(s => s.StartsWith("ENC:"));

            // Setup GenerateSecureKey method
            tempEncryptionServiceMock.Setup(e => e.GenerateSecureKey(It.IsAny<int>()))
                .Returns("TestSecureKey==");

            // Service under test - using a special subclass to inject our mock
            var service = new TestableKeyRotationService(
                _rotationLogger.Object,
                _mockRepository.Object,
                _encryptionService,
                tempEncryptionServiceMock.Object);

            // Act
            var result = await service.RotateKeyAsync("NewKey123");

            // Assert
            Assert.Equal(1, result); // Only one connection should be re-encrypted successfully

            // Verify only the first connection was saved
            _mockRepository.Verify(r => r.SaveConnectionStringDirectlyAsync(
                It.Is<ConnectionEntry>(c => c.Name == "Conn1")), Times.Once);

            // Verify the second connection was not saved
            _mockRepository.Verify(r => r.SaveConnectionStringDirectlyAsync(
                It.Is<ConnectionEntry>(c => c.Name == "Conn2")), Times.Never);
        }

        // Helper class for testing
        private class TestableKeyRotationService : KeyRotationService
        {
            private readonly IEncryptionService _tempEncryptionService;

            public TestableKeyRotationService(
                ILogger<KeyRotationService> logger,
                IConnectionRepository repository,
                IEncryptionService encryptionService,
                IEncryptionService tempEncryptionService)
                : base(logger, repository, encryptionService)
            {
                _tempEncryptionService = tempEncryptionService;
            }

            // Override to use our mock
            protected override IEncryptionService CreateTemporaryEncryptionService(string newKey)
            {
                return _tempEncryptionService;
            }
        }
    }
}
