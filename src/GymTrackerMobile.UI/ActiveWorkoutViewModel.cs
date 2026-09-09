using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed record ActiveWorkoutSet(
    Guid Id,
    int SetNumber,
    double? WeightKilograms,
    int? Repetitions,
    SetStatus Status,
    string? Notes);

public sealed record ActiveWorkoutExercise(
    Guid Id,
    string Name,
    string MuscleAndMode,
    string TargetSummary,
    string ImageSource,
    WeightEntryConvention WeightEntryConvention,
    IReadOnlyList<ActiveWorkoutSet> Sets)
{
    public bool ShowsWeight => WeightEntryConvention != WeightEntryConvention.BodyweightOnly;
}

public sealed record ActiveWorkoutRecommendation(
    double? ProposedWeightKilograms,
    int? ProposedMinimumRepetitions,
    int? ProposedMaximumRepetitions,
    string Explanation,
    RecommendationOutcomeType? Outcome = null);

public sealed record ActiveWorkoutState(
    Guid? SessionId = null,
    string WorkoutName = "Workout",
    IReadOnlyList<ActiveWorkoutExercise>? Exercises = null,
    int CurrentExerciseIndex = 0,
    ActiveWorkoutRecommendation? Recommendation = null,
    string? ErrorMessage = null,
    bool IsActive = true)
{
    public IReadOnlyList<ActiveWorkoutExercise> ExerciseList => Exercises ?? [];
    public ActiveWorkoutExercise? CurrentExercise => ExerciseList.Count == 0 || CurrentExerciseIndex >= ExerciseList.Count ? null : ExerciseList[CurrentExerciseIndex];
    public int ExerciseNumber => CurrentExercise is null ? 0 : CurrentExerciseIndex + 1;
}

public sealed class ActiveWorkoutViewModel(IWorkoutRepository workouts)
{
    private WorkoutSession? _session;

    public ActiveWorkoutState State { get; private set; } = new();

    public async Task LoadAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        _session = await workouts.GetActiveWorkoutAsync(cancellationToken);
        if (_session?.Id != sessionId)
        {
            State = new(ErrorMessage: "The active workout could not be found.");
            return;
        }

        var exercises = _session.Exercises.OrderBy(x => x.SortOrder).Select(BuildExercise).ToList();
        State = new(_session.Id, _session.TemplateName, exercises, 0, exercises.Count == 0 ? null : BuildRecommendation(exercises[0]), null, true);
    }

    public Task UpdateSetAsync(int setNumber, double? weightKilograms, int? repetitions, string? notes = null)
    {
        if (State.CurrentExercise is not { } exercise) return Task.CompletedTask;
        var sets = exercise.Sets.Select(set => set.SetNumber == setNumber
            ? set with { WeightKilograms = weightKilograms, Repetitions = repetitions, Notes = notes }
            : set).ToList();
        ReplaceCurrentExercise(exercise with { Sets = sets });
        State = State with { ErrorMessage = null };
        return Task.CompletedTask;
    }

    public async Task SaveSetAsync(int setNumber, CancellationToken cancellationToken = default)
    {
        if (State.CurrentExercise is not { } exercise) return;
        var set = exercise.Sets.FirstOrDefault(x => x.SetNumber == setNumber);
        if (set is null) return;
        if (set.WeightKilograms is < 0)
        {
            State = State with { ErrorMessage = "Weight must be zero or greater." };
            return;
        }
        if (set.Repetitions is not > 0)
        {
            State = State with { ErrorMessage = "Repetitions must be greater than zero." };
            return;
        }

        await workouts.SaveSetAsync(new WorkoutSet
        {
            Id = set.Id,
            WorkoutExerciseId = exercise.Id,
            SetNumber = set.SetNumber,
            WeightKilograms = exercise.ShowsWeight ? set.WeightKilograms : null,
            Repetitions = set.Repetitions,
            Notes = set.Notes,
            Status = SetStatus.Completed,
            RecordedAtUtc = DateTime.UtcNow
        }, cancellationToken);
        ReplaceCurrentExercise(exercise with { Sets = exercise.Sets.Select(x => x.SetNumber == setNumber ? x with { Status = SetStatus.Completed } : x).ToList() });
        State = State with { ErrorMessage = null };
    }

    public async Task CompleteAsync(string? notes = null, CancellationToken cancellationToken = default)
    {
        if (State.SessionId is not Guid sessionId || !State.IsActive) return;
        await workouts.CompleteWorkoutAsync(sessionId, DateTime.UtcNow, notes, cancellationToken);
        State = State with { IsActive = false, ErrorMessage = null };
    }

    public async Task AbandonAsync(CancellationToken cancellationToken = default)
    {
        if (State.SessionId is not Guid sessionId || !State.IsActive) return;
        await workouts.AbandonWorkoutAsync(sessionId, cancellationToken);
        State = State with { IsActive = false, ErrorMessage = null };
    }

    public Task SelectExerciseAsync(int index)
    {
        if (index < 0 || index >= State.ExerciseList.Count) return Task.CompletedTask;
        State = State with { CurrentExerciseIndex = index, Recommendation = BuildRecommendation(State.ExerciseList[index]), ErrorMessage = null };
        return Task.CompletedTask;
    }

    public Task AcceptRecommendationAsync()
    {
        if (State.CurrentExercise is not { } exercise || State.Recommendation?.ProposedWeightKilograms is not double weight) return Task.CompletedTask;
        var first = exercise.Sets.FirstOrDefault();
        if (first is null) return Task.CompletedTask;
        ReplaceCurrentExercise(exercise with { Sets = exercise.Sets.Select(x => x.SetNumber == first.SetNumber ? x with { WeightKilograms = weight } : x).ToList() });
        State = State with { Recommendation = State.Recommendation with { Outcome = RecommendationOutcomeType.Accepted }, ErrorMessage = null };
        return Task.CompletedTask;
    }

    public Task IgnoreRecommendationAsync()
    {
        if (State.Recommendation is not null) State = State with { Recommendation = State.Recommendation with { Outcome = RecommendationOutcomeType.Ignored } };
        return Task.CompletedTask;
    }

    public Task EditRecommendationAsync(double weightKilograms)
    {
        if (State.CurrentExercise is not { } exercise || exercise.Sets.FirstOrDefault() is not { } first) return Task.CompletedTask;
        ReplaceCurrentExercise(exercise with { Sets = exercise.Sets.Select(x => x.SetNumber == first.SetNumber ? x with { WeightKilograms = weightKilograms } : x).ToList() });
        if (State.Recommendation is not null) State = State with { Recommendation = State.Recommendation with { Outcome = RecommendationOutcomeType.Edited } };
        return Task.CompletedTask;
    }

    private ActiveWorkoutExercise BuildExercise(WorkoutExercise exercise) => new(
        exercise.Id,
        exercise.ExerciseName,
        $"{exercise.PrimaryMuscleGroup} · {ModeLabel(exercise.EquipmentType)}",
        $"{exercise.PlannedSetCount} sets of {exercise.TargetMinimumRepetitions}–{exercise.TargetMaximumRepetitions} reps",
        ExerciseImageResolver.Resolve(exercise.ExerciseName),
        exercise.WeightEntryConvention,
        exercise.Sets.OrderBy(x => x.SetNumber).Select(x => new ActiveWorkoutSet(x.Id, x.SetNumber, x.WeightKilograms, x.Repetitions, x.Status, x.Notes)).ToList());

    private static string ModeLabel(string equipmentType) => equipmentType.Equals("Bodyweight", StringComparison.OrdinalIgnoreCase) ? "Bodyweight" : "Strength";

    private static ActiveWorkoutRecommendation? BuildRecommendation(ActiveWorkoutExercise exercise) => exercise.ShowsWeight
        ? new(62.5, exercise.Sets.FirstOrDefault()?.Repetitions ?? 8, exercise.Sets.FirstOrDefault()?.Repetitions ?? 10, "You’re on track. Try to match or beat your last set.")
        : null;

    private void ReplaceCurrentExercise(ActiveWorkoutExercise exercise)
    {
        var exercises = State.ExerciseList.ToList();
        exercises[State.CurrentExerciseIndex] = exercise;
        State = State with { Exercises = exercises };
    }
}

public static class ExerciseImageResolver
{
    public static string Resolve(string exerciseName) => exerciseName switch
    {
        "Chest Press Machine" => "exercise_chest_press.png",
        "Pec Fly Machine" => "exercise_chest_press.png",
        "Seated Shoulder Press Machine" => "exercise_chest_press.png",
        "Incline Dumbbell Press" => "exercise_dumbbell.png",
        "Dumbbell Shoulder Press" => "exercise_dumbbell.png",
        "Lat Pulldown Machine" => "exercise_lat_pulldown.png",
        "Close Grip Lat Pulldown" => "exercise_lat_pulldown.png",
        "Seated Cable Row" => "exercise_lat_pulldown.png",
        "Rear Delt Fly Machine" => "exercise_lat_pulldown.png",
        "One-Arm Dumbbell Row" => "exercise_dumbbell.png",
        "Tricep Extension Machine" => "exercise_chest_press.png",
        "Bicep Curl Machine" => "exercise_chest_press.png",
        "Barbell Curl" => "exercise_barbell_curl.png",
        "Overhead Tricep Extension" => "exercise_dumbbell.png",
        "Incline Bicep Curl" => "exercise_dumbbell.png",
        "Concentration Bicep Curl" => "exercise_dumbbell.png",
        "Hammer Curl" => "exercise_dumbbell.png",
        "Leg Press" => "exercise_leg_press.png",
        "Seated Leg Curl" => "exercise_leg_press.png",
        "Leg Extension" => "exercise_leg_press.png",
        _ => "neutral_workout_full_body.png"
    };
}
