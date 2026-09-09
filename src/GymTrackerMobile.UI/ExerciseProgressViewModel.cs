using System.Globalization;
using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed record ExerciseProgressExercise(Guid Id, string Name, string MuscleGroup, string EquipmentType, WeightEntryConvention WeightEntryConvention, string ImageSource)
{
    public bool IsBodyweight => WeightEntryConvention == WeightEntryConvention.BodyweightOnly;
    public string Subtitle => $"{MuscleGroup} · {EquipmentType}";
}

public sealed record ExerciseProgressEntry(DateTime DateLocal, double? WeightKilograms, int? Repetitions, int CompletedSets, int PlannedSets, double Volume)
{
    public string DateLabel => DateLocal.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
}

public sealed record ExerciseProgressState(
    IReadOnlyList<ExerciseProgressExercise>? Exercises = null,
    Guid? SelectedExerciseId = null,
    ExerciseProgressExercise? SelectedExercise = null,
    IReadOnlyList<ExerciseProgressEntry>? History = null,
    double? HeaviestWeightKilograms = null,
    int? BestRepetitions = null,
    double? BestRepetitionsWeightKilograms = null,
    double TrainingVolumeKilograms = 0,
    int CompletedSetCount = 0,
    int PlannedSetCount = 0,
    string EmptyStateMessage = "No history yet. Complete a workout to start tracking this exercise.",
    string? ErrorMessage = null)
{
    public IReadOnlyList<ExerciseProgressExercise> ExerciseList => Exercises ?? [];
    public IReadOnlyList<ExerciseProgressEntry> HistoryList => History ?? [];
    public string VolumeLabel => SelectedExercise?.IsBodyweight == true ? "Repetitions" : "Training volume";
}

public sealed class ExerciseProgressViewModel(IExerciseRepository exercises, IWorkoutRepository workouts, IDatabaseInitializer databaseInitializer)
{
    private IReadOnlyList<WorkoutSession> _sessions = [];
    public ExerciseProgressState State { get; private set; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await databaseInitializer.InitializeAsync(cancellationToken);
            var catalogue = await exercises.GetExercisesAsync(cancellationToken);
            _sessions = await workouts.GetCompletedWorkoutsAsync(DateTime.UtcNow.AddYears(-10), DateTime.UtcNow.AddDays(1), cancellationToken);
            State = State with { Exercises = catalogue.OrderBy(x => x.Name).Select(MapExercise).ToList(), ErrorMessage = null };
            if (State.ExerciseList.Count > 0) SelectExercise(State.ExerciseList[0].Id);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            State = State with { ErrorMessage = exception.Message };
        }
    }

    public void SelectExercise(Guid exerciseId)
    {
        var selected = State.ExerciseList.FirstOrDefault(x => x.Id == exerciseId);
        if (selected is null) return;
        var entries = BuildHistory(selected, _sessions);
        var completedSets = entries.Sum(x => x.CompletedSets);
        var plannedSets = entries.Sum(x => x.PlannedSets);
        var weightedEntries = entries.Where(x => x.WeightKilograms is not null).ToList();
        var best = entries.Where(x => x.Repetitions is not null).OrderByDescending(x => x.Repetitions).FirstOrDefault();
        State = State with
        {
            SelectedExerciseId = exerciseId, SelectedExercise = selected, History = entries,
            HeaviestWeightKilograms = weightedEntries.Count == 0 ? null : weightedEntries.Max(x => x.WeightKilograms),
            BestRepetitions = best?.Repetitions,
            BestRepetitionsWeightKilograms = best?.WeightKilograms,
            TrainingVolumeKilograms = entries.Sum(x => x.Volume), CompletedSetCount = completedSets, PlannedSetCount = plannedSets,
            EmptyStateMessage = entries.Count == 0 ? "No history yet. Complete a workout to start tracking this exercise." : string.Empty
        };
    }

    private static ExerciseProgressExercise MapExercise(Exercise exercise) => new(exercise.Id, exercise.Name, exercise.PrimaryMuscleGroup, exercise.EquipmentType, exercise.WeightEntryConvention, ExerciseImageResolver.Resolve(exercise.Name));

    private static IReadOnlyList<ExerciseProgressEntry> BuildHistory(ExerciseProgressExercise exercise, IReadOnlyList<WorkoutSession> sessions) =>
        sessions.Where(x => x.CompletedAtUtc is not null)
            .SelectMany(session => session.Exercises.Where(x => x.ExerciseId == exercise.Id).Select(workoutExercise =>
            {
                var sets = workoutExercise.Sets.Where(x => x.Status == SetStatus.Completed && x.Repetitions is not null).ToList();
                var weight = exercise.IsBodyweight ? null : sets.Where(x => x.WeightKilograms is not null).MaxBy(x => x.WeightKilograms)?.WeightKilograms;
                var reps = sets.Count == 0 ? null : sets.Max(x => x.Repetitions);
                var volume = exercise.IsBodyweight ? sets.Sum(x => x.Repetitions ?? 0) : sets.Sum(x => (x.WeightKilograms ?? 0) * (x.Repetitions ?? 0));
                return new ExerciseProgressEntry(session.CompletedAtUtc!.Value.ToLocalTime().Date, weight, reps, sets.Count, workoutExercise.PlannedSetCount, volume);
            }))
            .OrderByDescending(x => x.DateLocal)
            .ToList();
}
