using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfer.Dtos;

public class CreatePublicKeyPair
{
    [Required]
    [MaxLength(210)]
    public string PublicVerifierKey { get; set; }
    [Required]
    [MaxLength(1000)]
    public string PublicEncryptionKey { get; set; }
}