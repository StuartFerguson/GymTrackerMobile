using System.Globalization;
using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed record WorkoutSummarySet(int SetNumber, double? WeightKilograms, int? Repetitions, SetStatus Status)
{
    public string StatusLabel => Status.ToString();
}

public sealed record WorkoutSummaryExercise(
    Guid Id,
    string Name,
    string MuscleGroup,
    string ImageSource,
    int PlannedSetCount,
    int CompletedSetCount,
    double TotalVolumeKilograms,
    string PlannedSummary,
    string CompletedSummary,
    IReadOnlyList<WorkoutSummarySet> Sets);

public sealed record WorkoutSummaryState(
    Guid? SessionId = null,
    string WorkoutName = "Workout Summary",
    DateTime? CompletedAtUtc = null,
    TimeSpan Duration = default,
    double TotalVolumeKilograms = 0,
    int CompletedSetCount = 0,
    int PlannedSetCount = 0,
    int CaloriesEstimated = 0,
    string MuscleGroups = "",
    string? Notes = null,
    bool IsComplete = false,
    string StatusMessage = "No workout summary available.",
    IReadOnlyList<WorkoutSummaryExercise>? Exercises = null,
    string? ErrorMessage = null)
{
    public IReadOnlyList<WorkoutSummaryExercise> ExerciseList => Exercises ?? [];
}

public sealed class WorkoutSummaryViewModel(IWorkoutRepository workouts)
{
    public WorkoutSummaryState State { get; private set; } = new();

    public async Task LoadAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var sessions = await workouts.GetCompletedWorkoutsAsync(DateTime.MinValue, DateTime.MaxValue, cancellationToken);
        var session = sessions.FirstOrDefault(x => x.Id == sessionId);
        if (session is null)
        {
            State = new(SessionId: sessionId, ErrorMessage: "The workout summary could not be found.");
            return;
        }

        var exercises = session.Exercises.OrderBy(x => x.SortOrder).Select(BuildExercise).ToList();
        var plannedSets = exercises.Sum(x => x.PlannedSetCount);
        var completedSets = exercises.Sum(x => x.CompletedSetCount);
        var isComplete = plannedSets > 0 && completedSets == plannedSets && exercises.SelectMany(x => x.Sets).All(x => x.Status == SetStatus.Completed);
        var duration = session.CompletedAtUtc is { } completedAt ? completedAt - session.StartedAtUtc : TimeSpan.Zero;

        State = new(
            session.Id,
            session.TemplateName,
            session.CompletedAtUtc,
            duration,
            exercises.Sum(x => x.TotalVolumeKilograms),
            completedSets,
            plannedSets,
            duration.TotalMinutes > 0 ? (int)Math.Round(duration.TotalMinutes * 6) : 0,
            string.Join(" · ", session.Exercises.Select(x => x.PrimaryMuscleGroup).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()),
            session.Notes,
            isComplete,
            isComplete ? "Great workout! You completed all planned sets." : "Workout partially logged. Review incomplete sets below.",
            exercises);
    }

    private static WorkoutSummaryExercise BuildExercise(WorkoutExercise exercise)
    {
        var sets = exercise.Sets.OrderBy(x => x.SetNumber).Select(x => new WorkoutSummarySet(x.SetNumber, x.WeightKilograms, x.Repetitions, x.Status)).ToList();
        var completed = sets.Count(x => x.Status == SetStatus.Completed);
        var completedSet = sets.FirstOrDefault(x => x.Status == SetStatus.Completed);
        var reps = completedSet?.Repetitions ?? exercise.TargetMinimumRepetitions;
        var weight = completedSet?.WeightKilograms;
        var completedSummary = completedSet is null ? "No completed sets" : FormatSetSummary(completed, reps, weight);

        return new(
            exercise.Id,
            exercise.ExerciseName,
            exercise.PrimaryMuscleGroup,
            ExerciseImageResolver.Resolve(exercise.ExerciseName),
            exercise.PlannedSetCount,
            completed,
            sets.Where(x => x.Status == SetStatus.Completed).Sum(x => (x.WeightKilograms ?? 0) * (x.Repetitions ?? 0)),
            FormatSetSummary(exercise.PlannedSetCount, exercise.TargetMinimumRepetitions, null, exercise.TargetMaximumRepetitions),
            completedSummary,
            sets);
    }

    private static string FormatSetSummary(int count, int reps, double? weight, int? maximumReps = null) =>
        $"{count} × {reps}{(maximumReps is not null && maximumReps != reps ? $"–{maximumReps}" : "")}" +
        (weight is double value ? $" @ {value.ToString("0.##", CultureInfo.InvariantCulture)} kg" : "");
}
