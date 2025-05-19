using EFramework.Data;
using Microsoft.EntityFrameworkCore;

public static class TestDbContextFactory
{
    public static OBDbContext Create()
    {
        var options = new DbContextOptionsBuilder<OBDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new OBDbContext(options);

        context.Database.EnsureCreated();
        return context;
    }
}
