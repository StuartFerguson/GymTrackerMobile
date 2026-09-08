using GymTrackerMobile.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class DatabaseInitializationTests
{
    [Fact]
    public async Task New_database_applies_schema_and_seeds_catalogue()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False")
                .Options;

            await using (var context = new GymTrackerDbContext(options))
            {
                var initializer = new DatabaseInitializer(context);

                await initializer.InitializeAsync();

                Assert.Equal(20, await context.Exercises.CountAsync());
                Assert.Equal(4, await context.WorkoutTemplates.CountAsync());
                Assert.Single(await context.BackupMetadata.ToListAsync());
                Assert.Single(await context.Database.GetAppliedMigrationsAsync());
                await context.Database.CloseConnectionAsync();
            }
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
