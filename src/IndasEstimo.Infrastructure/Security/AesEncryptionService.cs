using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using IndasEstimo.Infrastructure.Configuration;

namespace IndasEstimo.Infrastructure.Security;

public class AesEncryptionService : IAesEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IOptions<EncryptionSettings> encryptionSettings)
    {
        var keyString = encryptionSettings.Value.AesKey;
        if (string.IsNullOrEmpty(keyString) || keyString.Length != 32)
        {
            throw new ArgumentException("AES key must be exactly 32 characters for AES-256");
        }

        _key = Encoding.UTF8.GetBytes(keyString);
    }

    public string Encrypt(string plainText, out string iv)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentNullException(nameof(plainText));
        }

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        // Store IV for decryption
        iv = Convert.ToBase64String(aes.IV);

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var msEncrypt = new MemoryStream();
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText, string iv)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            throw new ArgumentNullException(nameof(cipherText));
        }

        if (string.IsNullOrEmpty(iv))
        {
            throw new ArgumentNullException(nameof(iv));
        }

        byte[] ivBytes = Convert.FromBase64String(iv);
        byte[] cipherBytes = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = ivBytes;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        return srDecrypt.ReadToEnd();
    }
}
