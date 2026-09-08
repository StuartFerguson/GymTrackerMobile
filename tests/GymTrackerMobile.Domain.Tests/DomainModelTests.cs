using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Domain.Tests;

public sealed class DomainModelTests
{
    [Fact]
    public void Workout_session_contains_an_independent_exercise_snapshot()
    {
        var exercise = new WorkoutExercise
        {
            Id = Guid.NewGuid(),
            WorkoutSessionId = Guid.NewGuid(),
            ExerciseId = Guid.NewGuid(),
            ExerciseName = "Chest Press Machine",
            PrimaryMuscleGroup = "Chest and shoulders",
            EquipmentType = "Machine",
            WeightEntryConvention = WeightEntryConvention.TotalLoad,
            TargetMinimumRepetitions = 8,
            TargetMaximumRepetitions = 12,
            PlannedSetCount = 3,
            SortOrder = 0
        };

        Assert.Equal("Chest Press Machine", exercise.ExerciseName);
        Assert.Equal(WeightEntryConvention.TotalLoad, exercise.WeightEntryConvention);
        Assert.Equal(3, exercise.PlannedSetCount);
    }

    [Fact]
    public void Set_supports_optional_context_and_non_completed_statuses()
    {
        var set = new WorkoutSet
        {
            Id = Guid.NewGuid(),
            WorkoutExerciseId = Guid.NewGuid(),
            SetNumber = 1,
            Status = SetStatus.Skipped,
            WeightKilograms = null,
            Repetitions = null,
            Difficulty = "Pain reported",
            Notes = "Skipped due to shoulder discomfort"
        };

        Assert.Equal(SetStatus.Skipped, set.Status);
        Assert.Null(set.WeightKilograms);
        Assert.Contains("shoulder", set.Notes, StringComparison.OrdinalIgnoreCase);
    }
}
