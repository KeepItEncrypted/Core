using Microsoft.EntityFrameworkCore;
using SecureTransferBackend.Data;

namespace SecureTransferBackend;

public class ApplicationDbContextTestFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }

    public ApplicationDbContextTestFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        Context = new ApplicationDbContext(options);

        Context.Database.OpenConnection();
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.CloseConnection();
        Context.Dispose();
    }
}