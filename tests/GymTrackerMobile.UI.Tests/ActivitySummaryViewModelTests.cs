using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class ActivitySummaryViewModelTests
{
    [Fact]
    public async Task Loads_full_activity_details()
    {
        var activity = new ActivityRecord
        {
            Id = Guid.NewGuid(),
            ActivityDateUtc = new DateTime(2026, 9, 9, 8, 30, 0, DateTimeKind.Utc),
            ActivityType = ActivityType.Running,
            DurationMinutes = 35,
            DistanceKilometres = 5.2,
            Steps = 6000,
            Notes = "Steady pace"
        };
        var viewModel = new ActivitySummaryViewModel(new RecordingActivityRepository([activity]));

        await viewModel.LoadAsync(activity.Id);

        Assert.Equal(activity.Id, viewModel.State.ActivityId);
        Assert.Equal("Running", viewModel.State.ActivityType);
        Assert.Equal("35 min · 5.2 km · 6,000 steps", viewModel.State.Details);
        Assert.Equal("Steady pace", viewModel.State.Notes);
    }

    private sealed class RecordingActivityRepository(IReadOnlyList<ActivityRecord> activities) : IActivityRepository
    {
        public Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(activities);
    }
}
