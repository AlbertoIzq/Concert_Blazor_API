namespace Concert.DataAccess.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string sourceText);
        string Decrypt(string encryptedText);
    }
}