namespace SecureTransferBackend.Services.Transfers.Dtos;

public class DecryptorKeyDto
{
    public string EncryptedSymmetricKey { get; set; }
    public string PublicKeyUsedForEncryption { get; set; }
}