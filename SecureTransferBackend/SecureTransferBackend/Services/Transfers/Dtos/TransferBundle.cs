using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfers.Dtos;

public class TransferBundle
{
    [Required] public Guid UserId { get; set; }
    [Required] public Guid UsedPublicKeyId { get; set; }
    [Required] public List<EncryptedFileDto> EncryptedFiles { get; set; }
    [Required] public List<EncryptedKeyDto> EncryptedKeys { get; set; }
}