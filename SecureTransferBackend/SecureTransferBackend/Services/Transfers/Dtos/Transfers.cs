using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfers.Dtos;

public class Transfers
{
    [Required] public List<TransferBundle> TransferBundles { get; set; }
}