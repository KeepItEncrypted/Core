using System.ComponentModel.DataAnnotations;
using SecureTransferBackend.Data.Common;

namespace SecureTransferBackend.Services.Transfer.Models;

public class Attachment : BaseModel
{
    [MaxLength(255)] [Required] public string FileName { get; set; }
    [Required] [MaxLength(1000)] public string StorageUrl { get; set; }
    [Required] public Bundle Bundle { get; set; }
    public Guid BundleId { get; set; }

    [Required] public Guid Identifier { get; set; }
    [Required] public string Signature { get; set; }
    
}