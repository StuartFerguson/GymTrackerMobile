using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class ActivityRepository(GymTrackerDbContext context) : IActivityRepository
{
    public async Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default)
    {
        var existing = await context.ActivityRecords.FindAsync([activity.Id], cancellationToken);
        if (existing is null) context.ActivityRecords.Add(activity);
        else context.Entry(existing).CurrentValues.SetValues(activity);
        await context.SaveChangesAsync(cancellationToken);
        return activity;
    }

    public async Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) =>
        await context.ActivityRecords.AsNoTracking()
            .Where(x => x.ActivityDateUtc >= fromUtc && x.ActivityDateUtc < toUtc)
            .OrderBy(x => x.ActivityDateUtc)
            .ToListAsync(cancellationToken);
}
