using System;
using System.Security.Cryptography;
using System.Text;
using OrganizationBoard.Service;
using Xunit;

namespace OrganizationBoard.Tests.ServiceTests.WhiteBox
{
    public class RsaServiceTest
    {
        private readonly RsaService _rsaService;

        public RsaServiceTest()
        {
            _rsaService = new RsaService();
        }

        [Fact]
        public void GetPublicKey_ShouldReturnValidPemFormat()
        {
            // Arrange & Act
            var publicKey = _rsaService.GetPublicKey();

            // Assert
            Assert.NotNull(publicKey);
            Assert.Contains("-----BEGIN PUBLIC KEY-----", publicKey);
            Assert.Contains("-----END PUBLIC KEY-----", publicKey);
        }

        [Fact]
        public void EncryptInternal_WithValidData_ShouldEncryptSuccessfully()
        {
            // Arrange
            var testData = "Test data for encryption";

            // Act
            var encryptedData = _rsaService.EncryptInternal(testData);

            // Assert
            Assert.NotNull(encryptedData);
            Assert.NotEqual(testData, encryptedData);
            Assert.True(IsBase64String(encryptedData));
        }

        [Fact]
        public void EncryptInternal_WithLargeData_ShouldThrowCryptographicException()
        {
            // Arrange
            var largeData = new string('a', 1000); // Data larger than RSA key size

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _rsaService.EncryptInternal(largeData));
        }

        [Fact]
        public void EncryptOutside_WithValidDataAndKey_ShouldEncryptSuccessfully()
        {
            // Arrange
            var testData = "Test data for external encryption";
            var publicKey = _rsaService.GetPublicKey();

            // Act
            var encryptedData = _rsaService.EncryptOutside(testData, publicKey);

            // Assert
            Assert.NotNull(encryptedData);
            Assert.NotEqual(testData, encryptedData);
            Assert.True(IsBase64String(encryptedData));
        }

        [Fact]
        public void EncryptOutside_WithInvalidPublicKey_ShouldThrowException()
        {
            // Arrange
            var testData = "Test data";
            var invalidKey = "invalid key format";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _rsaService.EncryptOutside(testData, invalidKey));
        }

        [Fact]
        public void Decrypt_WithValidEncryptedData_ShouldDecryptSuccessfully()
        {
            // Arrange
            var originalData = "Test data for decryption";
            var encryptedData = _rsaService.EncryptInternal(originalData);

            // Act
            var decryptedData = _rsaService.Decrypt(encryptedData);

            // Assert
            Assert.Equal(originalData, decryptedData);
        }

        [Fact]
        public void Decrypt_WithInvalidEncryptedData_ShouldThrowException()
        {
            // Arrange
            var invalidEncryptedData = "invalid base64 data";

            // Act & Assert
            Assert.Throws<FormatException>(() => _rsaService.Decrypt(invalidEncryptedData));
        }

        [Fact]
        public void EncryptDecrypt_EndToEnd_ShouldWorkCorrectly()
        {
            // Arrange
            var originalData = "Test data for end-to-end encryption/decryption";
            var publicKey = _rsaService.GetPublicKey();

            // Act
            var encryptedData = _rsaService.EncryptOutside(originalData, publicKey);
            var decryptedData = _rsaService.Decrypt(encryptedData);

            // Assert
            Assert.Equal(originalData, decryptedData);
        }

        private bool IsBase64String(string base64)
        {
            try
            {
                Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}