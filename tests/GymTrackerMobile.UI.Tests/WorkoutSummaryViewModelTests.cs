using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class WorkoutSummaryViewModelTests
{
    [Fact]
    public async Task Fully_completed_workout_exposes_mockup_summary_metrics()
    {
        var session = CreateSession();
        var viewModel = new WorkoutSummaryViewModel(new RecordingWorkoutRepository(session));

        await viewModel.LoadAsync(session.Id);

        Assert.Equal("Push Workout", viewModel.State.WorkoutName);
        Assert.True(viewModel.State.IsComplete);
        Assert.Equal(15, viewModel.State.CompletedSetCount);
        Assert.Equal(15, viewModel.State.PlannedSetCount);
        Assert.Equal(4720, viewModel.State.TotalVolumeKilograms);
        Assert.Equal(4, viewModel.State.ExerciseList.Count);
        Assert.Equal("Barbell Bench Press", viewModel.State.ExerciseList[0].Name);
        Assert.Equal(1920, viewModel.State.ExerciseList[0].TotalVolumeKilograms);
        Assert.Equal("4 × 8 @ 60 kg", viewModel.State.ExerciseList[0].CompletedSummary);
    }

    [Fact]
    public async Task Partially_logged_workout_keeps_incomplete_statuses_and_optional_notes_empty()
    {
        var session = CreateSession();
        var firstExercise = session.Exercises.First();
        firstExercise.Sets.First(x => x.SetNumber == 4).Status = SetStatus.Incomplete;
        firstExercise.Sets.First(x => x.SetNumber == 4).Repetitions = null;
        session.Notes = null;
        var viewModel = new WorkoutSummaryViewModel(new RecordingWorkoutRepository(session));

        await viewModel.LoadAsync(session.Id);

        Assert.False(viewModel.State.IsComplete);
        Assert.Equal(14, viewModel.State.CompletedSetCount);
        Assert.Equal("Incomplete", viewModel.State.ExerciseList[0].Sets[3].StatusLabel);
        Assert.Null(viewModel.State.Notes);
        Assert.Contains("incomplete", viewModel.State.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Workout_with_no_logged_sets_reports_zero_progress()
    {
        var session = CreateSession();
        foreach (var set in session.Exercises.SelectMany(x => x.Sets))
        {
            set.Status = SetStatus.Planned;
            set.WeightKilograms = null;
            set.Repetitions = null;
        }

        var viewModel = new WorkoutSummaryViewModel(new RecordingWorkoutRepository(session));

        await viewModel.LoadAsync(session.Id);

        Assert.False(viewModel.State.IsComplete);
        Assert.Equal(0, viewModel.State.CompletedSetCount);
        Assert.Equal(15, viewModel.State.PlannedSetCount);
        Assert.Equal(0, viewModel.State.TotalVolumeKilograms);
        Assert.All(viewModel.State.ExerciseList, exercise => Assert.Equal(0, exercise.CompletedSetCount));
    }

    private static WorkoutSession CreateSession()
    {
        var exercises = new[]
        {
            CreateExercise("Barbell Bench Press", "Chest", 4, 8, 60),
            CreateExercise("Incline Dumbbell Press", "Chest", 4, 10, 24),
            CreateExercise("Overhead Press", "Shoulders", 4, 8, 35),
            CreateExercise("Triceps Pushdown", "Triceps", 3, 12, 20)
        };
        return new WorkoutSession
        {
            Id = Guid.NewGuid(),
            TemplateName = "Push Workout",
            StartedAtUtc = new DateTime(2025, 4, 26, 9, 22, 0, DateTimeKind.Utc),
            CompletedAtUtc = new DateTime(2025, 4, 26, 10, 24, 15, DateTimeKind.Utc),
            IsActive = false,
            Notes = "Felt strong today.",
            Exercises = { exercises[0], exercises[1], exercises[2], exercises[3] }
        };
    }

    private static WorkoutExercise CreateExercise(string name, string muscle, int setCount, int reps, double weight)
    {
        var exercise = new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            ExerciseName = name,
            PrimaryMuscleGroup = muscle,
            EquipmentType = "Strength",
            PlannedSetCount = setCount,
            TargetMinimumRepetitions = reps,
            TargetMaximumRepetitions = reps
        };
        for (var number = 1; number <= setCount; number++)
            exercise.Sets.Add(new WorkoutSet { SetNumber = number, WeightKilograms = weight, Repetitions = reps, Status = SetStatus.Completed });
        return exercise;
    }

    private sealed class RecordingWorkoutRepository(WorkoutSession session) : IWorkoutRepository
    {
        public Task<IReadOnlyList<WorkoutTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<WorkoutTemplate>>([]);
        public Task<IReadOnlyList<WorkoutSession>> GetCompletedWorkoutsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<WorkoutSession>>([session]);
        public Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default) => Task.FromResult(session);
        public Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default) => Task.FromResult<WorkoutSession?>(null);
        public Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
