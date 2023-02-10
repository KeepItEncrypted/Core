using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using SecureTransferBackend.Data.Common;
using SecureTransferBackend.Services.Auth.Models;
using SecureTransferBackend.Services.Transfers;

namespace SecureTransferBackend.Services.Transfer.Models;

public class Bundle : BaseModel
{
    [Required]
    public string Message { get; set; }
    [Required]
    public ApplicationUser SenderUser { get; set; }
    public Guid SenderUserId { get; set; }
    
    [Required]
    public PublicKeyPair PublicKeyPair { get; set; }
    public Guid PublicKeyPairId { get; set; }
    
    [DefaultValue(BundleStatus.Draft)]
    [Required] public BundleStatus Status { get; set; }

    public List<Attachment> Attachments { get; set; }

}