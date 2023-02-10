using Microsoft.EntityFrameworkCore;
using SecureTransferBackend.Data;
using SecureTransferBackend.Services.Keys;
using SecureTransferBackend.Services.Transfer.Models;
using SecureTransferBackend.Services.Transfers.Dtos;

namespace SecureTransferBackend.Services.Transfers;

public class TransferService : ITransferService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IKeysService _keysService;

    public TransferService(ApplicationDbContext dbContext, IKeysService keysService)
    {
        _dbContext = dbContext;
        _keysService = keysService;
    }

    public async Task SendBundle(TransferBundle transferBundle, Guid senderId)
    {
        if (!await _keysService.DoesKeyExistOnUser(publicKeyId: transferBundle.UsedPublicKeyId, userId: senderId))
        {
            throw new WrongKeyForUserException();
        }

        var bundle = new Bundle()
        {
            Message = "Test",
            SenderUserId = senderId,
            PublicKeyPairId = transferBundle.UsedPublicKeyId
        };
        await _dbContext.AddAsync(bundle);

        foreach (var encryptedFile in transferBundle.EncryptedFiles)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/media/");
            var generatedId = Guid.NewGuid();
            var filePath = Path.Combine(path, generatedId.ToString());
            var attachment = new Attachment()
            {
                Bundle = bundle,
                FileName = encryptedFile.EncryptedFileName,
                StorageUrl = $"media/{generatedId.ToString()}",
                Identifier = generatedId,
                Signature = encryptedFile.Signature
            };
            await _dbContext.Attachments.AddAsync(attachment);
            await using var fs = File.Create(filePath);
            await encryptedFile.FormFile.CopyToAsync(fs);
        }

        var bundleRecipient = new BundleRecipient()
        {
            Bundle = bundle,
            RecipientUserId = transferBundle.UserId,
        };
        foreach (var encryptedKey in transferBundle.EncryptedKeys)
        {
            await _dbContext.AddAsync(bundleRecipient);
            var decryptorKey = new DecryptorKey()
            {
                EncryptedSymmetricKey = encryptedKey.EncryptedAesKey,
                PublicKeyPairId = encryptedKey.PublicKeyPairId,
                BundleRecipient = bundleRecipient
            };
            await _dbContext.AddAsync(decryptorKey);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Guid> CreateBundle(CreateBundle createBundle, Guid senderId)
    {
        if (!await _keysService.DoesKeyExistOnUser(publicKeyId: createBundle.UsedPublicKeyId, userId: senderId))
        {
            throw new WrongKeyForUserException();
        }

        // TODO: Check if recipients are all contacts
        var bundle = new Bundle()
        {
            Message = createBundle.EncryptedMessage,
            SenderUserId = senderId,
            PublicKeyPairId = createBundle.UsedPublicKeyId
        };
        await _dbContext.Bundles.AddAsync(bundle);

        foreach (var recipient in createBundle.Recipients)
        {
            var bundleRecipient = new BundleRecipient()
            {
                Bundle = bundle,
                RecipientUserId = recipient.UserId,
            };
            foreach (var encryptedKey in recipient.EncryptedKeys)
            {
                await _dbContext.AddAsync(bundleRecipient);
                var decryptorKey = new DecryptorKey()
                {
                    EncryptedSymmetricKey = encryptedKey.EncryptedAesKey,
                    PublicKeyPairId = encryptedKey.PublicKeyPairId,
                    BundleRecipient = bundleRecipient
                };
                await _dbContext.AddAsync(decryptorKey);
            }
        }

        await _dbContext.SaveChangesAsync();
        return bundle.Id;
    }

    public async Task AddFileToBundle(Guid bundleId, Guid senderId, EncryptedFileDto encryptedFile)
    {
        // Only draft bundles can be edited
        var bundle = _dbContext.Bundles.FirstOrDefault(b =>
            b.Id == bundleId && b.SenderUserId == senderId && b.Status == BundleStatus.Draft);
        if (bundle == null)
        {
            throw new BundleNotFoundException();
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/media/");
        var generatedId = Guid.NewGuid();
        var filePath = Path.Combine(path, generatedId.ToString());
        var attachment = new Attachment()
        {
            Bundle = bundle,
            FileName = encryptedFile.EncryptedFileName,
            StorageUrl = $"media/{generatedId.ToString()}",
            Identifier = generatedId,
            Signature = encryptedFile.Signature
        };
        await _dbContext.Attachments.AddAsync(attachment);
        await using var fs = File.Create(filePath);
        await encryptedFile.FormFile.CopyToAsync(fs);
        await _dbContext.SaveChangesAsync();
    }


    public async Task<List<InboxItem>> GetInbox(Guid userId)
    {
        var recipientBundles = await _dbContext.BundleRecipients.Where(
                br => br.RecipientUserId == userId
            ).Select(br =>
                new InboxItem()
                {
                    Id = br.Id,
                    BundleId = br.BundleId,
                    Message = br.Bundle.Message
                }
            )
            .ToListAsync();
        return recipientBundles;
    }

    public async Task<InboxItemDetail?> GetInboxItem(Guid bundleRecipientId, Guid userId)
    {
        var inboxItemDetail = await _dbContext.BundleRecipients
            .Where(br => br.Id == bundleRecipientId && br.RecipientUserId == userId).Select(br => new InboxItemDetail()
            {
                Id = br.Id,
                BundleId = br.BundleId,
                PublicKeyIdForSignature = br.Bundle.PublicKeyPairId,
                Message = br.Bundle.Message,
                DecryptorKeys = br.DecryptorKeys.Select(dk => new DecryptorKeyDto()
                {
                    EncryptedSymmetricKey = dk.EncryptedSymmetricKey,
                    PublicKeyUsedForEncryption = dk.PublicKeyPair.PublicEncryptionKey
                }).ToList()
            }).FirstOrDefaultAsync();
        if (inboxItemDetail == null)
        {
            return null;
        }

        inboxItemDetail.Attachments = await _dbContext.Attachments.Where(a => a.BundleId == inboxItemDetail.BundleId)
            .Select(a => new AttachmentDto()
            {
                FileName = a.FileName,
                StorageLocation = a.StorageUrl,
                Signature = a.Signature
            }).ToListAsync();
        return inboxItemDetail;
    }

    public async Task CancelSentBundle(Guid bundleId, Guid userId)
    {
        var bundle = await _dbContext.Bundles.Where(b => b.SenderUserId == userId && b.Id == bundleId)
            .Include(b => b.Attachments)
            .FirstOrDefaultAsync();
        if (bundle == null)
        {
            throw new NotOwnerOfBundleException();
        }

        foreach (var attachment in bundle.Attachments)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var filePath = Path.Combine(path, attachment.StorageUrl);
            File.Delete(filePath);
        }

        _dbContext.Bundles.Remove(bundle);
        await _dbContext.SaveChangesAsync();
    }

    public Task<List<InboxItem>> GetSentBundles(string userId)
    {
        throw new NotImplementedException();
    }
}