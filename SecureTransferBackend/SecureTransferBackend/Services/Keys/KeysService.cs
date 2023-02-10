using Microsoft.EntityFrameworkCore;
using SecureTransferBackend.Data;

namespace SecureTransferBackend.Services.Keys;

public class KeysService: IKeysService
{
    private readonly ApplicationDbContext _dbContext;

    public KeysService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> DoesKeyExistOnUser(Guid publicKeyId, Guid userId)
    {
        return await _dbContext.PublicKeyPairs.Where(k => k.Id == publicKeyId && k.ApplicationUserId == userId)
            .CountAsync() == 1;
    }
}