using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymTrackerMobile.Persistence;

public sealed class GymTrackerDbContextFactory : IDesignTimeDbContextFactory<GymTrackerDbContext>
{
    public GymTrackerDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
            .UseSqlite("Data Source=design-time-gym-tracker.db")
            .Options;

        return new GymTrackerDbContext(options);
    }
}
