
using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfers.Dtos;

public class EncryptedKeyDto
{
    [Required] public Guid PublicKeyPairId { get; set; }
    [Required] public string EncryptedAesKey { get; set; }
}