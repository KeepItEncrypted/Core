using System.ComponentModel.DataAnnotations;

namespace SecureTransferBackend.Services.Transfers.Dtos;

public class EncryptedFileDto
{
    [Required] public IFormFile FormFile { get; set; }
    [Required] public string EncryptedFileName { get; set; }
    [Required] public string Signature { get; set; }
}