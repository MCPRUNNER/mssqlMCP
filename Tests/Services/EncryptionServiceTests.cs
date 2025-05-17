using Microsoft.Extensions.Logging;
using Moq;
using mssqlMCP.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace mssqlMCP.Tests.Services
{
    public class EncryptionServiceTests
    {
        private readonly Mock<ILogger<EncryptionService>> _mockLogger;

        public EncryptionServiceTests()
        {
            _mockLogger = new Mock<ILogger<EncryptionService>>();
        }

        [Fact]
        public void Encrypt_DecryptRoundTrip_ShouldReturnOriginalText()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);
            var plainText = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

            // Act
            var encrypted = service.Encrypt(plainText);
            var decrypted = service.Decrypt(encrypted);

            // Assert
            Assert.NotEqual(plainText, encrypted);
            Assert.StartsWith("ENC:", encrypted);
            Assert.Equal(plainText, decrypted);
        }

        [Fact]
        public void IsEncrypted_WithEncryptedString_ShouldReturnTrue()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);
            var plainText = "Test string";
            var encrypted = service.Encrypt(plainText);

            // Act
            var result = service.IsEncrypted(encrypted);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsEncrypted_WithPlainText_ShouldReturnFalse()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);
            var plainText = "Test string";

            // Act
            var result = service.IsEncrypted(plainText);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Encrypt_WithEmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);
            var plainText = string.Empty;

            // Act
            var encrypted = service.Encrypt(plainText);

            // Assert
            Assert.Equal(plainText, encrypted);
        }

        [Fact]
        public void Decrypt_WithEmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);
            var cipherText = string.Empty;

            // Act
            var decrypted = service.Decrypt(cipherText);

            // Assert
            Assert.Equal(cipherText, decrypted);
        }

        [Fact]
        public void Encrypt_WithAlreadyEncryptedString_ShouldReturnSameString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);
            var plainText = "Test string";
            var alreadyEncrypted = service.Encrypt(plainText);

            // Act
            var doubleEncrypted = service.Encrypt(alreadyEncrypted);

            // Assert
            Assert.Equal(alreadyEncrypted, doubleEncrypted);
        }

        [Fact]
        public void Decrypt_WithNonEncryptedString_ShouldReturnSameString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);
            var plainText = "Test string";

            // Act
            var decrypted = service.Decrypt(plainText);

            // Assert
            Assert.Equal(plainText, decrypted);
        }

        [Fact]
        public void Encrypt_WithDifferentKey_ShouldProduceDifferentCipherText()
        {
            // Arrange
            var plainText = "Same plain text";

            // First encryption with key 1
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "Key1");
            var service1 = new EncryptionService(_mockLogger.Object);
            var encrypted1 = service1.Encrypt(plainText);

            // Second encryption with key 2
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "Key2");
            var service2 = new EncryptionService(_mockLogger.Object);
            var encrypted2 = service2.Encrypt(plainText);

            // Assert
            Assert.NotEqual(encrypted1, encrypted2);
        }

        [Fact]
        public void Decrypt_WithWrongKey_ShouldNotDecryptCorrectly()
        {
            // Arrange
            var plainText = "Text to encrypt";

            // Encrypt with key 1
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "Key1");
            var service1 = new EncryptionService(_mockLogger.Object);
            var encrypted = service1.Encrypt(plainText);

            // Try to decrypt with key 2
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "Key2");
            var service2 = new EncryptionService(_mockLogger.Object);
            var decrypted = service2.Decrypt(encrypted);

            // Assert
            Assert.NotEqual(plainText, decrypted);
            // Service should return original ciphertext when decryption fails
            Assert.Equal(encrypted, decrypted);
        }

        [Fact]
        public void EnvVariableNotSet_ShouldUseDefaultKey()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", null);
            var service = new EncryptionService(_mockLogger.Object);
            var plainText = "Test with default key";

            // Act
            var encrypted = service.Encrypt(plainText);
            var decrypted = service.Decrypt(encrypted);

            // Assert
            Assert.NotEqual(plainText, encrypted);
            Assert.Equal(plainText, decrypted);
        }        // Clean up after all tests
        ~EncryptionServiceTests()
        {
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", null);
        }

        [Fact]
        public void Decrypt_WithInvalidEncryptedData_ShouldReturnOriginalString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);

            // Create an invalid encrypted string (too short to contain proper IV)
            var invalidEncrypted = "ENC:VGhpc0lzTm90VmFsaWQ="; // This is base64 for "ThisIsNotValid"

            // Act
            var result = service.Decrypt(invalidEncrypted);

            // Assert
            // When decryption fails due to invalid data, it should return the original string
            Assert.Equal(invalidEncrypted, result);
        }

        [Fact]
        public void Decrypt_WithCorruptData_ShouldReturnOriginalString()
        {
            // Arrange
            Environment.SetEnvironmentVariable("MSSQL_MCP_KEY", "TestEncryptionKey123!");
            var service = new EncryptionService(_mockLogger.Object);

            // First encrypt some valid data
            var plainText = "Test string";
            var encrypted = service.Encrypt(plainText);

            // Now corrupt the data (remove some characters from the base64 part)
            var corrupted = encrypted.Substring(0, encrypted.Length - 10);

            // Act
            var result = service.Decrypt(corrupted);

            // Assert
            // When decryption fails due to corrupt data, it should return the original string
            Assert.Equal(corrupted, result);
        }
    }
}
