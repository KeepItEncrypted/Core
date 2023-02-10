using SecureTransferBackend.Services.Transfers.Dtos;

namespace SecureTransferBackend.Services.Transfers;

public interface ITransferService
{
    public Task SendBundle(TransferBundle transferBundle, Guid senderId);
    
    public Task<Guid> CreateBundle(CreateBundle createBundle, Guid senderId);
    public Task AddFileToBundle(Guid bundleId, Guid senderId, EncryptedFileDto encryptedFile);
    public Task<List<InboxItem>> GetInbox(Guid userId);
    public Task<InboxItemDetail?> GetInboxItem(Guid bundleRecipientId, Guid userId);

    public Task CancelSentBundle(Guid bundleId, Guid userId);
    public Task<List<InboxItem>> GetSentBundles(string userId);
}
