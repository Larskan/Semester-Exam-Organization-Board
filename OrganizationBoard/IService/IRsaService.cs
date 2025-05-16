namespace OrganizationBoard.IService;

public interface IRsaService
{
    string GetPublicKey();
    string Decrypt(string encryptedData);
} 