using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class ExerciseProgressViewModelTests
{
    [Fact]
    public async Task Loads_fixed_catalogue_and_selects_an_exercise()
    {
        var exercise = Exercise("Chest Press Machine");
        var viewModel = CreateViewModel([exercise], []);

        await viewModel.LoadAsync();
        viewModel.SelectExercise(exercise.Id);

        Assert.Equal(exercise.Id, viewModel.State.SelectedExercise?.Id);
        Assert.Equal("Chest Press Machine", viewModel.State.SelectedExercise?.Name);
    }

    [Fact]
    public async Task Changing_selection_updates_the_selected_exercise_and_history()
    {
        var first = Exercise("Chest Press Machine");
        var second = Exercise("Leg Press");
        var viewModel = CreateViewModel([first, second], [Session(first, DateTime.UtcNow.AddDays(-1), Set(80, 8))]);

        await viewModel.LoadAsync();
        viewModel.SelectExercise(second.Id);

        Assert.Equal(second.Id, viewModel.State.SelectedExerciseId);
        Assert.Equal("Leg Press", viewModel.State.SelectedExercise?.Name);
        Assert.Empty(viewModel.State.HistoryList);
    }

    [Fact]
    public async Task Populated_history_calculates_metrics_and_planned_completed_sets()
    {
        var exercise = Exercise("Chest Press Machine");
        var session = Session(exercise, DateTime.UtcNow.AddDays(-2),
            Set(100, 8), Set(120, 6), Set(120, 10), Set(null, null, SetStatus.Incomplete));
        var viewModel = CreateViewModel([exercise], [session]);

        await viewModel.LoadAsync();

        var state = viewModel.State;
        Assert.Equal(120, state.HeaviestWeightKilograms);
        Assert.Equal(10, state.BestRepetitions);
        Assert.Equal(120, state.BestRepetitionsWeightKilograms);
        Assert.Equal(2720, state.TrainingVolumeKilograms);
        Assert.Equal(3, state.CompletedSetCount);
        Assert.Equal(4, state.PlannedSetCount);
        Assert.Single(state.HistoryList);
    }

    [Fact]
    public async Task Bodyweight_history_does_not_fabricate_a_weight()
    {
        var exercise = Exercise("Pull Up", WeightEntryConvention.BodyweightOnly);
        var session = Session(exercise, DateTime.UtcNow.AddDays(-1), Set(null, 12), Set(null, 10));
        var viewModel = CreateViewModel([exercise], [session]);

        await viewModel.LoadAsync();

        Assert.Null(viewModel.State.HeaviestWeightKilograms);
        Assert.Null(viewModel.State.BestRepetitionsWeightKilograms);
        Assert.Equal(22, viewModel.State.TrainingVolumeKilograms);
        Assert.Null(viewModel.State.HistoryList.Single().WeightKilograms);
    }

    [Fact]
    public async Task Exercise_without_history_exposes_a_useful_empty_state()
    {
        var exercise = Exercise("Leg Press");
        var viewModel = CreateViewModel([exercise], []);

        await viewModel.LoadAsync();

        Assert.Empty(viewModel.State.HistoryList);
        Assert.Equal("No history yet. Complete a workout to start tracking this exercise.", viewModel.State.EmptyStateMessage);
    }

    private static ExerciseProgressViewModel CreateViewModel(IReadOnlyList<Exercise> exercises, IReadOnlyList<WorkoutSession> sessions) =>
        new(new RecordingExerciseRepository(exercises), new RecordingWorkoutRepository(sessions), new NoOpDatabaseInitializer());

    private static Exercise Exercise(string name, WeightEntryConvention convention = WeightEntryConvention.TotalLoad) => new()
    {
        Id = Guid.NewGuid(), Name = name, PrimaryMuscleGroup = "Chest", EquipmentType = "Machine", WeightEntryConvention = convention
    };

    private static WorkoutSession Session(Exercise exercise, DateTime completedAt, params WorkoutSet[] sets)
    {
        var workoutExercise = new WorkoutExercise
        {
            ExerciseId = exercise.Id, ExerciseName = exercise.Name, PrimaryMuscleGroup = exercise.PrimaryMuscleGroup,
            EquipmentType = exercise.EquipmentType, WeightEntryConvention = exercise.WeightEntryConvention,
            PlannedSetCount = sets.Length, Sets = sets
        };
        return new WorkoutSession { TemplateName = "Push", StartedAtUtc = completedAt.AddMinutes(-45), CompletedAtUtc = completedAt, Exercises = [workoutExercise] };
    }

    private static WorkoutSet Set(double? weight, int? reps, SetStatus status = SetStatus.Completed) =>
        new() { Status = status, WeightKilograms = weight, Repetitions = reps };

    private sealed class RecordingExerciseRepository(IReadOnlyList<Exercise> exercises) : IExerciseRepository
    {
        public Task<IReadOnlyList<Exercise>> GetExercisesAsync(CancellationToken cancellationToken = default) => Task.FromResult(exercises);
    }

    private sealed class NoOpDatabaseInitializer : IDatabaseInitializer
    {
        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
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
}
