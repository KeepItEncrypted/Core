namespace SecureTransferBackend.Services.Transfers.Dtos;

public class InboxItemDetail
{
    public Guid Id { get; set; }
    public Guid BundleId { get; set; }
    public Guid PublicKeyIdForSignature { get; set; }
    public string Message { get; set; }
    public List<AttachmentDto> Attachments { get; set; }
    public List<DecryptorKeyDto> DecryptorKeys { get; set; }
    
}