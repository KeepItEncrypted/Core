using Microsoft.Build.Framework;

namespace SecureTransferBackend.Services.Transfers.Dtos;

public class CreateRecipient
{
    [Required] public Guid UserId { get; set; }
    [Required] public List<EncryptedKeyDto> EncryptedKeys { get; set; }
}