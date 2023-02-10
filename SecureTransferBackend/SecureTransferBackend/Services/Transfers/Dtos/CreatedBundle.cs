using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfers.Dtos;

public class CreatedBundle
{
    public CreatedBundle(Guid createdBundleId)
    {
        CreatedBundleId = createdBundleId;
    }

    public Guid CreatedBundleId { get; set; }
}