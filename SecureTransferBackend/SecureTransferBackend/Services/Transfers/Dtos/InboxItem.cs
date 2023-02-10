namespace SecureTransferBackend.Services.Transfers.Dtos;

public class InboxItem
{
    public Guid Id { get; set; }
    public Guid BundleId { get; set; }
    public string Message { get; set; }
}