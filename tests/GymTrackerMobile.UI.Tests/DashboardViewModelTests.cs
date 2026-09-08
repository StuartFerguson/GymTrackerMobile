using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class DashboardViewModelTests
{
    [Fact]
    public async Task Loading_dashboard_initializes_storage_before_reading_data()
    {
        var events = new List<string>();
        var viewModel = new DashboardViewModel(
            new RecordingWorkoutRepository(events),
            new RecordingActivityRepository(events),
            new RecordingDatabaseInitializer(events));

        await viewModel.LoadAsync();

        Assert.Equal(["database", "templates", "workouts", "activities"], events);
    }

    private sealed class RecordingDatabaseInitializer(List<string> events) : IDatabaseInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            events.Add("database");
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingWorkoutRepository(List<string> events) : IWorkoutRepository
    {
        public Task<IReadOnlyList<WorkoutTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default)
        {
            events.Add("templates");
            return Task.FromResult<IReadOnlyList<WorkoutTemplate>>([]);
        }

        public Task<IReadOnlyList<WorkoutSession>> GetCompletedWorkoutsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        {
            events.Add("workouts");
            return Task.FromResult<IReadOnlyList<WorkoutSession>>([]);
        }

        public Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class RecordingActivityRepository(List<string> events) : IActivityRepository
    {
        public Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
        {
            events.Add("activities");
            return Task.FromResult<IReadOnlyList<ActivityRecord>>([]);
        }

        public Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
