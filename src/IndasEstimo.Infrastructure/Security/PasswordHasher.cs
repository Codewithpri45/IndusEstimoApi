using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using IndasEstimo.Infrastructure.Configuration;

namespace IndasEstimo.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private readonly int _iterations;

    public PasswordHasher(IOptions<EncryptionSettings> encryptionSettings)
    {
        _iterations = encryptionSettings.Value.KeyDerivationIterations;
    }

    public string HashPassword(string password)
    {
        // Generate a random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash the password using PBKDF2
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            _iterations,
            HashAlgorithmName.SHA256);

        byte[] hash = pbkdf2.GetBytes(HashSize);

        // Combine salt and hash for storage
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Convert to Base64 for storage
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        try
        {
            // Extract the bytes
            byte[] hashBytes = Convert.FromBase64String(passwordHash);

            // Extract salt (first 16 bytes)
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // Extract hash (remaining 32 bytes)
            byte[] storedHash = new byte[HashSize];
            Array.Copy(hashBytes, SaltSize, storedHash, 0, HashSize);

            // Compute hash of provided password
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                _iterations,
                HashAlgorithmName.SHA256);

            byte[] computedHash = pbkdf2.GetBytes(HashSize);

            // Compare hashes
            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch
        {
            return false;
        }
    }
}
