namespace SecureTransferBackend.Services.Transfers.Dtos;

public class AttachmentDto
{
    public string FileName { get; set; }
    public string StorageLocation { get; set; }
    public string Signature { get; set; }
    
}