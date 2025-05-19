namespace OrganizationBoard.IService;

public interface IRsaService
{
    string GetPublicKey();
    string Decrypt(string encryptedData);
    public string EncryptInternal(string RawData);
    public string EncryptOutside(string RawData, string publicKeyPem);
} 