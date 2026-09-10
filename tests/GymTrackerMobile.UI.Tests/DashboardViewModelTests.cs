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

        Assert.Equal(["database", "templates", "active", "workouts", "activities"], events);
    }

    [Fact]
    public async Task Loading_dashboard_exposes_active_workout_and_resume_navigates_to_it()
    {
        var session = new WorkoutSession { Id = Guid.NewGuid(), TemplateName = "Push", IsActive = true };
        var routes = new List<string>();
        var viewModel = new DashboardViewModel(
            new RecordingWorkoutRepository([], session),
            new RecordingActivityRepository([]),
            new RecordingDatabaseInitializer([]),
            route => { routes.Add(route); return Task.CompletedTask; });

        await viewModel.LoadAsync();
        viewModel.ResumeWorkoutCommand.Execute(null);
        await Task.Yield();

        Assert.Equal(session.Id, viewModel.State.ActiveWorkout?.Id);
        Assert.Equal("Push", viewModel.State.ActiveWorkout?.Name);
        Assert.Equal(ActiveWorkoutRoutes.For(session.Id), routes.Single());
    }

    [Fact]
    public async Task Settings_command_navigates_to_backup_and_settings()
    {
        var routes = new List<string>();
        var viewModel = new DashboardViewModel(
            new RecordingWorkoutRepository([]),
            new RecordingActivityRepository([]),
            new RecordingDatabaseInitializer([]),
            route =>
            {
                routes.Add(route);
                return Task.CompletedTask;
            });

        viewModel.SettingsCommand.Execute(null);
        await Task.Yield();

        Assert.Equal([NavigationRoutes.BackupSettings], routes);
    }

    [Fact]
    public async Task Progress_command_navigates_to_exercise_progress()
    {
        var routes = new List<string>();
        var viewModel = new DashboardViewModel(
            new RecordingWorkoutRepository([]),
            new RecordingActivityRepository([]),
            new RecordingDatabaseInitializer([]),
            route => { routes.Add(route); return Task.CompletedTask; });

        viewModel.ProgressCommand.Execute(null);
        await Task.Yield();

        Assert.Equal([NavigationRoutes.ExerciseProgress], routes);
    }

    private sealed class RecordingDatabaseInitializer(List<string> events) : IDatabaseInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            events.Add("database");
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingWorkoutRepository(List<string> events, WorkoutSession? activeWorkout = null) : IWorkoutRepository
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
        public Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default)
        {
            events.Add("active");
            return Task.FromResult(activeWorkout);
        }
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
