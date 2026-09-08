using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class DatabaseInitializer(GymTrackerDbContext context) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);
        await SeedData.EnsureSeededAsync(context, cancellationToken);
    }
}
