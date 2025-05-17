using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace mssqlMCP.Services
{    /// <summary>
     /// Service interface for encrypting and decrypting sensitive data
     /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        bool IsEncrypted(string text);
        string GenerateSecureKey(int length = 32);
    }

    /// <summary>
    /// Implementation of the encryption service using AES encryption
    /// with key derived from environment variable
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;
        private readonly byte[] _key;
        private readonly string _prefix = "ENC:";        /// <summary>
                                                         /// Initializes a new instance of the EncryptionService
                                                         /// </summary>
                                                         /// <param name="logger">Logger for the service</param>        
        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;

            // Get encryption key from environment variable
            string? envKey = Environment.GetEnvironmentVariable("MSSQL_MCP_KEY");

            if (string.IsNullOrEmpty(envKey))
            {
                _logger.LogWarning("Environment variable MSSQL_MCP_KEY is not set. Connection strings will not be encrypted securely.");
                // Create a default key (not secure but allows the service to function)
                envKey = "DefaultInsecureKey_DoNotUseInProduction!";
                _logger.LogWarning("Using default insecure key. Set MSSQL_MCP_KEY environment variable for proper security.");
            }

            // Derive a 256-bit key from the environment variable
            using var deriveBytes = new Rfc2898DeriveBytes(
                envKey,
                Encoding.UTF8.GetBytes("mssqlMCP.Salt"),
                10000,
                HashAlgorithmName.SHA256);

            _key = deriveBytes.GetBytes(32); // 256 bits for AES-256
        }

        /// <summary>
        /// Encrypts a string using AES encryption
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <returns>Encrypted text with prefix</returns>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            if (IsEncrypted(plainText))
                return plainText; // Already encrypted

            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;

                // Generate a new random IV for each encryption
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using var encryptor = aes.CreateEncryptor(aes.Key, iv);
                using var memoryStream = new MemoryStream();

                // Write the IV to the beginning of the memory stream
                memoryStream.Write(iv, 0, iv.Length);

                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (var streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                // Convert the encrypted data to Base64 and add the prefix
                return _prefix + Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                // Return unencrypted data rather than throwing, so the application can continue to function
                return plainText;
            }
        }

        /// <summary>
        /// Decrypts a previously encrypted string
        /// </summary>
        /// <param name="cipherText">Encrypted text with prefix</param>
        /// <returns>Original plain text</returns>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            if (!IsEncrypted(cipherText))
                return cipherText; // Not encrypted

            try
            {
                // Remove the prefix
                string actualCipherText = cipherText.Substring(_prefix.Length);
                byte[] cipherBytes = Convert.FromBase64String(actualCipherText);

                using var aes = Aes.Create();
                aes.Key = _key;

                // Get the IV from the beginning of the cipher text
                byte[] iv = new byte[16]; // AES block size is 16 bytes
                Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var memoryStream = new MemoryStream(cipherBytes, iv.Length, cipherBytes.Length - iv.Length);
                using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                using var streamReader = new StreamReader(cryptoStream);

                return streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting data");
                // Return the original encrypted text instead of throwing
                return cipherText;
            }
        }        /// <summary>
                 /// Checks if a string is already encrypted
                 /// </summary>
                 /// <param name="text">Text to check</param>
                 /// <returns>True if the text is encrypted</returns>
        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(_prefix);
        }

        /// <summary>
        /// Generates a cryptographically secure random key for encryption
        /// </summary>
        /// <param name="length">Length of the key in bytes (default 32 for AES-256)</param>
        /// <returns>Base64-encoded string representation of the key</returns>
        public string GenerateSecureKey(int length = 32)
        {
            byte[] keyBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }
            return Convert.ToBase64String(keyBytes);
        }
    }
}
