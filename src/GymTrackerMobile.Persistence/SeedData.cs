using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public static class SeedData
{
    private static readonly Guid MetadataId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    private static readonly (string Name, string Muscle, string Equipment, ExerciseMode Mode)[] Exercises =
    [
        ("Chest Press Machine", "Chest and shoulders", "Machine", ExerciseMode.Machine),
        ("Pec Fly Machine", "Chest and shoulders", "Machine", ExerciseMode.Machine),
        ("Seated Shoulder Press Machine", "Chest and shoulders", "Machine", ExerciseMode.Machine),
        ("Incline Dumbbell Press", "Chest and shoulders", "Dumbbell", ExerciseMode.Dumbbell),
        ("Dumbbell Shoulder Press", "Chest and shoulders", "Dumbbell", ExerciseMode.Dumbbell),
        ("Lat Pulldown Machine", "Back and rear delts", "Machine", ExerciseMode.Machine),
        ("Close Grip Lat Pulldown", "Back and rear delts", "Machine", ExerciseMode.Machine),
        ("Seated Cable Row", "Back and rear delts", "Cable", ExerciseMode.Machine),
        ("Rear Delt Fly Machine", "Back and rear delts", "Machine", ExerciseMode.Machine),
        ("One-Arm Dumbbell Row", "Back and rear delts", "Dumbbell", ExerciseMode.Dumbbell),
        ("Tricep Extension Machine", "Arms", "Machine", ExerciseMode.Machine),
        ("Bicep Curl Machine", "Arms", "Machine", ExerciseMode.Machine),
        ("Barbell Curl", "Arms", "Barbell", ExerciseMode.Barbell),
        ("Overhead Tricep Extension", "Arms", "Dumbbell", ExerciseMode.Dumbbell),
        ("Incline Bicep Curl", "Arms", "Dumbbell", ExerciseMode.Dumbbell),
        ("Concentration Bicep Curl", "Arms", "Dumbbell", ExerciseMode.Dumbbell),
        ("Hammer Curl", "Arms", "Dumbbell", ExerciseMode.Dumbbell),
        ("Leg Press", "Legs", "Machine", ExerciseMode.Machine),
        ("Seated Leg Curl", "Legs", "Machine", ExerciseMode.Machine),
        ("Leg Extension", "Legs", "Machine", ExerciseMode.Machine)
    ];

    private static readonly string[] TemplateNames = ["Push", "Pull", "Legs", "Full Body"];

    public static async Task EnsureSeededAsync(GymTrackerDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Exercises.AnyAsync(cancellationToken))
        {
            for (var index = 0; index < Exercises.Length; index++)
            {
                var item = Exercises[index];
                context.Exercises.Add(new Exercise
                {
                    Id = StableId(index + 10),
                    Name = item.Name,
                    PrimaryMuscleGroup = item.Muscle,
                    EquipmentType = item.Equipment,
                    WeightEntryConvention = item.Mode == ExerciseMode.Dumbbell
                        ? WeightEntryConvention.PerDumbbell
                        : item.Mode == ExerciseMode.Bodyweight
                            ? WeightEntryConvention.BodyweightOnly
                            : WeightEntryConvention.TotalLoad,
                    DefaultMinimumRepetitions = 8,
                    DefaultMaximumRepetitions = 12,
                    DefaultSetCount = 3,
                    ExerciseMode = item.Mode
                });
            }
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

    private static Guid StableId(int value) => new($"00000000-0000-0000-0000-{value:000000000000}");
}
