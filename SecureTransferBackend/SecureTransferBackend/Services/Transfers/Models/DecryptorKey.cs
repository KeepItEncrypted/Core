using System.ComponentModel.DataAnnotations;
using SecureTransferBackend.Data.Common;

namespace SecureTransferBackend.Services.Transfer.Models;

public class DecryptorKey : BaseModel
{
    [Required] [MaxLength(750)] public string EncryptedSymmetricKey { get; set; }
    [Required] public PublicKeyPair PublicKeyPair { get; set; }
    public Guid PublicKeyPairId { get; set; }

    [Required] public BundleRecipient BundleRecipient { get; set; }
    [Required] public Guid BundleRecipientId { get; set; }
}