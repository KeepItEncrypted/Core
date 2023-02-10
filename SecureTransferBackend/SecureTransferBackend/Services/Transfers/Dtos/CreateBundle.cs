using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfers.Dtos;

public class CreateBundle
{
    public string EncryptedMessage { get; set; }
    [Required] public Guid UsedPublicKeyId { get; set; }
    [Required] public List<CreateRecipient> Recipients { get; set; }
}