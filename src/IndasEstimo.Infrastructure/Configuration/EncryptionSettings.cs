namespace IndasEstimo.Infrastructure.Configuration;

public class EncryptionSettings
{
    public string AesKey { get; set; } = string.Empty;
    public int KeyDerivationIterations { get; set; } = 10000;
}
