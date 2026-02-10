namespace IndasEstimo.Infrastructure.Security;

public interface IAesEncryptionService
{
    string Encrypt(string plainText, out string iv);
    string Decrypt(string cipherText, string iv);
}
