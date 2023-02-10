namespace SecureTransferBackend.Services.Keys;

public interface IKeysService
{
    public Task<bool> DoesKeyExistOnUser(Guid publicKeyId, Guid userId);
}