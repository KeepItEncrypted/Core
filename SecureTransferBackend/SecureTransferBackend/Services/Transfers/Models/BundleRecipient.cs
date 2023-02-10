using System.ComponentModel.DataAnnotations;
using SecureTransferBackend.Data.Common;
using SecureTransferBackend.Services.Auth.Models;

namespace SecureTransferBackend.Services.Transfer.Models;

public class BundleRecipient : BaseModel
{
    [Required]
    public ApplicationUser RecipientUser { get; set; }
    public Guid RecipientUserId { get; set; }
    [Required]
    public Bundle Bundle { get; set; }
    public Guid BundleId { get; set; }

    public List<DecryptorKey> DecryptorKeys { get; set; }
}