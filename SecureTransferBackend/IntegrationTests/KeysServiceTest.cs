using SecureTransferBackend;
using SecureTransferBackend.Services.Auth.Models;
using SecureTransferBackend.Services.Keys;
using SecureTransferBackend.Services.Transfer.Models;

namespace IntegrationTests;

[Collection("ApplicationDbContextTestCollection")]
public class KeyServiceTest
{
    private readonly ApplicationDbContextTestFixture _fixture;

    public KeyServiceTest(ApplicationDbContextTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async void TestExample()
    {
        var context = _fixture.Context;
        var keyService = new KeysService(context);
        var user = new ApplicationUser()
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@test.com",
        };
        var user2 = new ApplicationUser()
        {
            Id = Guid.NewGuid().ToString(),
            Email = "test@test.com",
        };
        var publicKey = new PublicKeyPair()
        {
            ApplicationUser = user,
            PublicEncryptionKey = "abc",
            PublicVerifierKey = "abc"
        };
        await context.AddAsync(user);
        await context.AddAsync(user2);
        await context.AddAsync(publicKey);
        await context.SaveChangesAsync();
        Assert.True(await keyService.DoesKeyExistOnUser(publicKey.Id, user.Id));
        Assert.False(await keyService.DoesKeyExistOnUser(publicKey.Id, user2.Id));
    }


}