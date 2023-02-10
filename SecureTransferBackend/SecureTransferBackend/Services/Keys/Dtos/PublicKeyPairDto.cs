using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfer.Dtos;

public class PublicKeyPairDto
{
    public Guid Id { get; set; }
    public string PublicVerifierKey { get; set; }
    public string PublicEncryptionKey { get; set; }
    public Guid ApplicationUserId { get; set; }
}