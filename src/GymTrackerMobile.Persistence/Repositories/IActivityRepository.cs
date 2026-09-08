using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Persistence;

public interface IActivityRepository
{
    Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}
