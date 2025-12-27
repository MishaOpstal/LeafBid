using LeafBidAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace LeafBidAPITest.Helpers;

public class DummyDatabase
{
    public static ApplicationDbContext CreateDbContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        ApplicationDbContext dbContext = new(options);

        // Ensures IdentityDbContext tables & model are initialized for this in-memory database instance.
        dbContext.Database.EnsureCreated();

        return dbContext;
    }
}