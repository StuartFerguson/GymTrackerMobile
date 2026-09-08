using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public static class SeedData
{
    private static readonly Guid MetadataId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static readonly CatalogueExercise[] Exercises =
    [
        new(10, "Chest Press Machine", "Chest and shoulders", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(11, "Pec Fly Machine", "Chest and shoulders", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(12, "Seated Shoulder Press Machine", "Chest and shoulders", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(13, "Incline Dumbbell Press", "Chest and shoulders", "Dumbbell", WeightEntryConvention.PerDumbbell, ExerciseMode.Dumbbell),
        new(14, "Dumbbell Shoulder Press", "Chest and shoulders", "Dumbbell", WeightEntryConvention.PerDumbbell, ExerciseMode.Dumbbell),
        new(15, "Lat Pulldown Machine", "Back and rear delts", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(16, "Close Grip Lat Pulldown", "Back and rear delts", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(17, "Seated Cable Row", "Back and rear delts", "Cable", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(18, "Rear Delt Fly Machine", "Back and rear delts", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(19, "One-Arm Dumbbell Row", "Back and rear delts", "Dumbbell", WeightEntryConvention.PerDumbbell, ExerciseMode.Dumbbell),
        new(20, "Tricep Extension Machine", "Arms", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(21, "Bicep Curl Machine", "Arms", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(22, "Barbell Curl", "Arms", "Barbell", WeightEntryConvention.TotalLoad, ExerciseMode.Barbell),
        new(23, "Overhead Tricep Extension", "Arms", "Dumbbell", WeightEntryConvention.PerDumbbell, ExerciseMode.Dumbbell),
        new(24, "Incline Bicep Curl", "Arms", "Dumbbell", WeightEntryConvention.PerDumbbell, ExerciseMode.Dumbbell),
        new(25, "Concentration Bicep Curl", "Arms", "Dumbbell", WeightEntryConvention.PerDumbbell, ExerciseMode.Dumbbell),
        new(26, "Hammer Curl", "Arms", "Dumbbell", WeightEntryConvention.PerDumbbell, ExerciseMode.Dumbbell),
        new(27, "Leg Press", "Legs", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(28, "Seated Leg Curl", "Legs", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine),
        new(29, "Leg Extension", "Legs", "Machine", WeightEntryConvention.TotalLoad, ExerciseMode.Machine)
    ];

    private static readonly string[] TemplateNames = ["Push", "Pull", "Legs", "Full Body"];

    public static async Task EnsureSeededAsync(GymTrackerDbContext context, CancellationToken cancellationToken)
    {
        var existingExercises = await context.Exercises.ToDictionaryAsync(x => x.Id, cancellationToken);
        foreach (var item in Exercises)
        {
            if (!existingExercises.TryGetValue(item.Id, out var exercise))
            {
                exercise = new Exercise { Id = item.Id };
                context.Exercises.Add(exercise);
            }

            exercise.Name = item.Name;
            exercise.PrimaryMuscleGroup = item.Muscle;
            exercise.EquipmentType = item.Equipment;
            exercise.WeightEntryConvention = item.WeightConvention;
            exercise.DefaultMinimumRepetitions = item.MinimumRepetitions;
            exercise.DefaultMaximumRepetitions = item.MaximumRepetitions;
            exercise.DefaultSetCount = item.SetCount;
            exercise.ExerciseMode = item.Mode;
        }

        if (!await context.WorkoutTemplates.AnyAsync(cancellationToken))
        {
            for (var index = 0; index < TemplateNames.Length; index++)
            {
                context.WorkoutTemplates.Add(new WorkoutTemplate
                {
                    Id = StableId(index + 100),
                    Name = TemplateNames[index],
                    IsBuiltIn = true
                });
            }
        }

        if (!await context.BackupMetadata.AnyAsync(cancellationToken))
        {
            context.BackupMetadata.Add(new BackupMetadata { Id = MetadataId, SchemaVersion = 1 });
        }

        await context.SaveChangesAsync(cancellationToken);

        if (!await context.TemplateExercises.AnyAsync(cancellationToken))
        {
            var exercises = await context.Exercises.OrderBy(x => x.Name).ToListAsync(cancellationToken);
            var templates = await context.WorkoutTemplates.OrderBy(x => x.Name).ToListAsync(cancellationToken);
            foreach (var template in templates)
            {
                foreach (var (exercise, order) in exercises.Select((exercise, order) => (exercise, order)))
                {
                    context.TemplateExercises.Add(new TemplateExercise
                    {
                        WorkoutTemplateId = template.Id,
                        ExerciseId = exercise.Id,
                        SortOrder = order,
                        TargetMinimumRepetitions = exercise.DefaultMinimumRepetitions,
                        TargetMaximumRepetitions = exercise.DefaultMaximumRepetitions,
                        PlannedSetCount = exercise.DefaultSetCount
                    });
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private sealed record CatalogueExercise(
        int StableIdValue,
        string Name,
        string Muscle,
        string Equipment,
        WeightEntryConvention WeightConvention,
        ExerciseMode Mode,
        int MinimumRepetitions = 8,
        int MaximumRepetitions = 12,
        int SetCount = 3)
    {
        public Guid Id => SeedData.StableId(StableIdValue);
    }

    private static Guid StableId(int value) => new($"00000000-0000-0000-0000-{value:000000000000}");
}
