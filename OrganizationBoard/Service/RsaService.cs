using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace OrganizationBoard.Service;

public interface IRsaService
{
    string GetPublicKey();
    string Decrypt(string encryptedData);
}

public class RsaService : IRsaService
{
    private readonly IConfiguration _configuration;
    private readonly RSA _rsa;

    public RsaService(IConfiguration configuration)
    {
        _configuration = configuration;
        _rsa = RSA.Create();
        
        // Load or generate keys
        var privateKey = _configuration["PubPrv:Prv"];
        if (string.IsNullOrEmpty(privateKey) || privateKey == "Prv Key")
        {
            // Generate new keys if they don't exist
            var keys = GenerateRsaKeys();
            _rsa.ImportFromPem(keys.privateKey);
            
            // Update appsettings.json with new keys
            UpdateAppSettings(keys.publicKey, keys.privateKey);
        }
        else
        {
            // Load existing private key
            _rsa.ImportFromPem(privateKey);
        }
    }

    public string GetPublicKey()
    {
        return _configuration["PubPrv:Pub"];
    }

    public string Decrypt(string encryptedData)
    {
        var encryptedBytes = Convert.FromBase64String(encryptedData);
        var decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    private (string publicKey, string privateKey) GenerateRsaKeys()
    {
        var rsa = RSA.Create(2048);
        var publicKey = rsa.ExportRSAPublicKeyPem();
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        return (publicKey, privateKey);
    }

    private void UpdateAppSettings(string publicKey, string privateKey)
    {
        var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var json = File.ReadAllText(appSettingsPath);
        dynamic config = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
        
        config.PubPrv.Pub = publicKey;
        config.PubPrv.Prv = privateKey;
        
        var updatedJson = Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(appSettingsPath, updatedJson);
    }
} 