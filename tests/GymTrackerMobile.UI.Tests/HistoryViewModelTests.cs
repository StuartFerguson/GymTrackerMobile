using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class HistoryViewModelTests
{
    [Fact]
    public async Task History_merges_completed_workouts_and_activities_newest_first()
    {
        var workout = new WorkoutSession
        {
            Id = Guid.NewGuid(), TemplateName = "Push", CompletedAtUtc = DateTime.UtcNow.AddDays(-1),
            Exercises = [new WorkoutExercise { PlannedSetCount = 1, Sets = [new WorkoutSet { Status = SetStatus.Completed, Repetitions = 8, WeightKilograms = 80 }] }]
        };
        var activity = new ActivityRecord
        {
            ActivityDateUtc = DateTime.UtcNow.AddHours(-2), ActivityType = ActivityType.Walking,
            DurationMinutes = 30, Notes = "Easy pace", Steps = 4000
        };
        var viewModel = new HistoryViewModel(new RecordingWorkoutRepository([workout]), new RecordingActivityRepository([activity]));

        await viewModel.LoadAsync();

        Assert.Equal([HistoryItemKind.Activity, HistoryItemKind.Workout], viewModel.Items.Select(x => x.Kind));
        Assert.Equal("Easy pace", viewModel.Items[0].Notes);
        Assert.Contains("30 min", viewModel.Items[0].Details);
        Assert.Equal("1 / 1 sets", viewModel.Items[1].Details);
    }

    [Fact]
    public async Task History_excludes_in_progress_workouts()
    {
        var active = new WorkoutSession { TemplateName = "Push", IsActive = true, CompletedAtUtc = DateTime.UtcNow };
        var viewModel = new HistoryViewModel(new RecordingWorkoutRepository([active]), new RecordingActivityRepository([]));

        await viewModel.LoadAsync();

        Assert.Empty(viewModel.Items);
    }

    private sealed class RecordingWorkoutRepository(IReadOnlyList<WorkoutSession> sessions) : IWorkoutRepository
    {
        public Task<IReadOnlyList<WorkoutSession>> GetCompletedWorkoutsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(sessions);
        public Task<IReadOnlyList<WorkoutTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<WorkoutTemplate>>([]);
        public Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class RecordingActivityRepository(IReadOnlyList<ActivityRecord> activities) : IActivityRepository
    {
        public Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(activities);
    }
}
