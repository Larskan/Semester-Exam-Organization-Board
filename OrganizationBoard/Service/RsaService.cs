using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using OrganizationBoard.IService;

namespace OrganizationBoard.Service;

public class RsaService : IRsaService
{
    private readonly RSA _rsa;

    public RsaService()
    {
        _rsa = RSA.Create();
        var keys = GenerateRsaKeys();
        _rsa.ImportFromPem(keys.privateKey);
    }

    public string GetPublicKey()
    {
        return _rsa.ExportSubjectPublicKeyInfoPem();
    }

    public string Decrypt(string encryptedData)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedData);
        var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decryptedBytes);
    }
    public string EncryptInternal(string RawData)
    {
        try
        {
            var bytesToEncrypt = Encoding.UTF8.GetBytes(RawData);
            var encryptedBytes = _rsa.Encrypt(bytesToEncrypt, RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Encryption failed. Data might be too large or padding incorrect.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred during encryption.", ex);
        }
    }

    private (string publicKey, string privateKey) GenerateRsaKeys()
    {
        var rsa = RSA.Create(2048);
        var publicKey = rsa.ExportRSAPublicKeyPem();
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        return (publicKey, privateKey);
    }
    
    public string EncryptOutside(string RawData, string publicKeyPem)
    {
        try
        {
            var rsaDectypt = RSA.Create();
            rsaDectypt.ImportFromPem(publicKeyPem);
            var bytesToEncrypt = Encoding.UTF8.GetBytes(RawData);
            var encryptedBytes = rsaDectypt.Encrypt(bytesToEncrypt, RSAEncryptionPadding.OaepSHA256);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Encryption failed. Data might be too large or padding incorrect.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred during encryption.", ex);
        }
    }
} 