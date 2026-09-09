using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class ActiveWorkoutViewModelTests
{
    [Fact]
    public async Task Loading_selects_first_exercise_and_exposes_mockup_details()
    {
        var session = CreateSession();
        var viewModel = CreateViewModel(session);

        await viewModel.LoadAsync(session.Id);

        Assert.Equal("Chest Press Machine", viewModel.State.CurrentExercise!.Name);
        Assert.Equal("Chest · Strength", viewModel.State.CurrentExercise.MuscleAndMode);
        Assert.Equal("3 sets of 8–10 reps", viewModel.State.CurrentExercise.TargetSummary);
        Assert.Equal("exercise_chest_press.png", viewModel.State.CurrentExercise.ImageSource);
        Assert.Equal(3, viewModel.State.CurrentExercise.Sets.Count);
    }

    [Fact]
    public async Task Bodyweight_exercise_hides_weight_and_saves_repetitions()
    {
        var session = CreateSession();
        session.Exercises.Clear();
        session.Exercises.Add(new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            ExerciseName = "Push Up",
            PrimaryMuscleGroup = "Chest",
            EquipmentType = "Bodyweight",
            WeightEntryConvention = WeightEntryConvention.BodyweightOnly,
            TargetMinimumRepetitions = 8,
            TargetMaximumRepetitions = 12,
            PlannedSetCount = 1,
            Sets = { new WorkoutSet { SetNumber = 1 } }
        });
        var repository = new RecordingWorkoutRepository(session);
        var viewModel = CreateViewModel(session, repository);
        await viewModel.LoadAsync(session.Id);

        Assert.False(viewModel.State.CurrentExercise!.ShowsWeight);
        await viewModel.UpdateSetAsync(1, null, 10);
        await viewModel.SaveSetAsync(1);

        Assert.Equal(10, repository.SavedSet!.Repetitions);
        Assert.Equal(SetStatus.Completed, repository.SavedSet.Status);
    }

    [Fact]
    public async Task Different_sets_keep_their_own_values_and_recorded_timestamp()
    {
        var session = CreateSession();
        var repository = new RecordingWorkoutRepository(session);
        var viewModel = CreateViewModel(session, repository);
        await viewModel.LoadAsync(session.Id);

        await viewModel.UpdateSetAsync(1, 40, 12, "Warm-up");
        await viewModel.SaveSetAsync(1);
        await viewModel.UpdateSetAsync(2, 45, 8, "Hard set");
        await viewModel.SaveSetAsync(2);

        Assert.Equal((40, 12, "Warm-up"), (repository.SavedSets[0].WeightKilograms, repository.SavedSets[0].Repetitions, repository.SavedSets[0].Notes));
        Assert.Equal((45, 8, "Hard set"), (repository.SavedSets[1].WeightKilograms, repository.SavedSets[1].Repetitions, repository.SavedSets[1].Notes));
        Assert.NotNull(repository.SavedSets[0].RecordedAtUtc);
        Assert.NotNull(repository.SavedSets[1].RecordedAtUtc);
    }

    [Fact]
    public async Task Non_completed_statuses_can_be_saved_without_repetitions()
    {
        var session = CreateSession();
        var repository = new RecordingWorkoutRepository(session);
        var viewModel = CreateViewModel(session, repository);
        await viewModel.LoadAsync(session.Id);

        await viewModel.SetStatusAsync(1, SetStatus.Failed);
        await viewModel.SetStatusAsync(2, SetStatus.Skipped);
        await viewModel.SetStatusAsync(3, SetStatus.Incomplete);

        Assert.Equal([SetStatus.Failed, SetStatus.Skipped, SetStatus.Incomplete], repository.SavedSets.Select(x => x.Status));
    }

    [Fact]
    public async Task Invalid_set_values_are_rejected_without_losing_draft()
    {
        var session = CreateSession();
        var viewModel = CreateViewModel(session);
        await viewModel.LoadAsync(session.Id);

        await viewModel.UpdateSetAsync(1, -1, 0);
        await viewModel.SaveSetAsync(1);

        Assert.Equal("Weight must be zero or greater.", viewModel.State.ErrorMessage);
        Assert.Equal(-1, viewModel.State.CurrentExercise!.Sets[0].WeightKilograms);
        Assert.Equal(0, viewModel.State.CurrentExercise.Sets[0].Repetitions);
    }

    [Fact]
    public async Task Recommendation_can_be_accepted_and_updates_the_visible_outcome()
    {
        var session = CreateSession();
        var viewModel = CreateViewModel(session);
        await viewModel.LoadAsync(session.Id);

        await viewModel.AcceptRecommendationAsync();

        Assert.Equal(RecommendationOutcomeType.Accepted, viewModel.State.Recommendation!.Outcome);
        Assert.Equal(62.5, viewModel.State.CurrentExercise!.Sets[0].WeightKilograms);
    }

    [Fact]
    public async Task Completing_active_workout_marks_it_terminal()
    {
        var session = CreateSession();
        var repository = new RecordingWorkoutRepository(session);
        var viewModel = CreateViewModel(session, repository);
        await viewModel.LoadAsync(session.Id);

        await viewModel.CompleteAsync("Finished strong");

        Assert.Equal(session.Id, repository.CompletedSessionId);
        Assert.Equal("Finished strong", repository.CompletedNotes);
        Assert.False(viewModel.State.IsActive);
    }

    [Fact]
    public async Task Abandon_active_workout_delegates_to_repository()
    {
        var session = CreateSession();
        var repository = new RecordingWorkoutRepository(session);
        var viewModel = CreateViewModel(session, repository);
        await viewModel.LoadAsync(session.Id);

        await viewModel.AbandonAsync();

        Assert.Equal(session.Id, repository.AbandonedSessionId);
        Assert.False(viewModel.State.IsActive);
    }

    [Fact]
    public void Seeded_exercises_resolve_to_non_fallback_artwork()
    {
        var names = new[]
        {
            "Chest Press Machine", "Pec Fly Machine", "Seated Shoulder Press Machine", "Incline Dumbbell Press",
            "Dumbbell Shoulder Press", "Lat Pulldown Machine", "Close Grip Lat Pulldown", "Seated Cable Row",
            "Rear Delt Fly Machine", "One-Arm Dumbbell Row", "Tricep Extension Machine", "Bicep Curl Machine",
            "Barbell Curl", "Overhead Tricep Extension", "Incline Bicep Curl", "Concentration Bicep Curl",
            "Hammer Curl", "Leg Press", "Seated Leg Curl", "Leg Extension"
        };

        Assert.All(names, name => Assert.DoesNotContain("neutral_", ExerciseImageResolver.Resolve(name), StringComparison.Ordinal));
    }

    private static ActiveWorkoutViewModel CreateViewModel(WorkoutSession session, RecordingWorkoutRepository? repository = null) =>
        new(repository ?? new RecordingWorkoutRepository(session));

    private static WorkoutSession CreateSession()
    {
        var exercise = new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            ExerciseName = "Chest Press Machine",
            PrimaryMuscleGroup = "Chest",
            EquipmentType = "Strength",
            WeightEntryConvention = WeightEntryConvention.TotalLoad,
            TargetMinimumRepetitions = 8,
            TargetMaximumRepetitions = 10,
            PlannedSetCount = 3,
            SortOrder = 0,
            Sets =
            {
                new WorkoutSet { SetNumber = 1, WeightKilograms = 60, Repetitions = 10, Status = SetStatus.Completed },
                new WorkoutSet { SetNumber = 2, WeightKilograms = 60, Repetitions = 10, Status = SetStatus.Completed },
                new WorkoutSet { SetNumber = 3, WeightKilograms = 60, Repetitions = 8 }
            }
        };
        return new WorkoutSession { Id = Guid.NewGuid(), TemplateName = "Push", IsActive = true, Exercises = { exercise } };
    }

    private sealed class RecordingWorkoutRepository(WorkoutSession session) : IWorkoutRepository
    {
        public WorkoutSet? SavedSet { get; private set; }
        public List<WorkoutSet> SavedSets { get; } = [];
        public Guid? CompletedSessionId { get; private set; }
        public string? CompletedNotes { get; private set; }
        public Guid? AbandonedSessionId { get; private set; }
        public Task<IReadOnlyList<WorkoutTemplate>> GetTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<WorkoutTemplate>>([]);
        public Task<IReadOnlyList<WorkoutSession>> GetCompletedWorkoutsAsync(DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<WorkoutSession>>([]);
        public Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default) => Task.FromResult(session);
        public Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default) { SavedSet = set; SavedSets.Add(set); return Task.CompletedTask; }
        public Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default) => Task.FromResult<WorkoutSession?>(session);
        public Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default) { CompletedSessionId = sessionId; CompletedNotes = notes; return Task.CompletedTask; }
        public Task AbandonWorkoutAsync(Guid sessionId, CancellationToken cancellationToken = default) { AbandonedSessionId = sessionId; return Task.CompletedTask; }
    }
}
