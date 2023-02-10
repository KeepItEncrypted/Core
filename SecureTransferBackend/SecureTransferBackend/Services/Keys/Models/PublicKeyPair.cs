using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SecureTransferBackend.Data.Common;
using SecureTransferBackend.Services.Auth.Models;

namespace SecureTransferBackend.Services.Transfer.Models;

public class PublicKeyPair : BaseModel
{
    [MaxLength(1000)] [Required] public string PublicEncryptionKey { get; set; }

    [MaxLength(250)] [Required] public string PublicVerifierKey { get; set; }
    [Required] public ApplicationUser ApplicationUser { get; set; }
    public Guid ApplicationUserId { get; set; }

    [DefaultValue(false)]
    [Required] public bool IsArchived { get; set; }
}