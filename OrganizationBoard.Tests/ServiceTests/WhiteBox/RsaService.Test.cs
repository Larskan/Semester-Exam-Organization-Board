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
        public void Decrypt_WithInvalidEncryptedData_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var invalidEncryptedData = "invalid base64 data";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _rsaService.Decrypt(invalidEncryptedData));
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

        [Fact]
        public void EncryptInternal_WithNullData_ShouldThrowArgumentNullException()
        {
            // Arrange
            string nullData = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _rsaService.EncryptInternal(nullData));
        }

        [Fact]
        public void EncryptInternal_WithEmptyString_ShouldEncryptSuccessfully()
        {
            // Arrange
            var emptyData = "";

            // Act
            var encryptedData = _rsaService.EncryptInternal(emptyData);

            // Assert
            Assert.NotNull(encryptedData);
            Assert.NotEqual(emptyData, encryptedData);
            Assert.True(IsBase64String(encryptedData));
        }

        [Fact]
        public void EncryptOutside_WithNullData_ShouldThrowArgumentNullException()
        {
            // Arrange
            string nullData = null;
            var publicKey = _rsaService.GetPublicKey();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _rsaService.EncryptOutside(nullData, publicKey));
        }

        [Fact]
        public void EncryptOutside_WithNullKey_ShouldThrowArgumentNullException()
        {
            // Arrange
            var testData = "Test data";
            string nullKey = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _rsaService.EncryptOutside(testData, nullKey));
        }

        [Fact]
        public void EncryptOutside_WithEmptyStringData_ShouldEncryptSuccessfully()
        {
            // Arrange
            var emptyData = "";
            var publicKey = _rsaService.GetPublicKey();

            // Act
            var encryptedData = _rsaService.EncryptOutside(emptyData, publicKey);

            // Assert
            Assert.NotNull(encryptedData);
            Assert.NotEqual(emptyData, encryptedData);
            Assert.True(IsBase64String(encryptedData));
        }

        [Fact]
        public void EncryptOutside_WithEmptyStringKey_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var testData = "Test data";
            var emptyKey = "";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _rsaService.EncryptOutside(testData, emptyKey));
        }

        [Fact]
        public void Decrypt_WithNullData_ShouldThrowArgumentNullException()
        {
            // Arrange
            string nullData = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _rsaService.Decrypt(nullData));
        }

        [Fact]
        public void Decrypt_WithEmptyString_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var emptyData = "";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _rsaService.Decrypt(emptyData));
        }

        [Fact]
        public void Decrypt_WithValidBase64ButInvalidRsaData_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var validBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 }); // Valid base64 but not valid RSA data

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _rsaService.Decrypt(validBase64));
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