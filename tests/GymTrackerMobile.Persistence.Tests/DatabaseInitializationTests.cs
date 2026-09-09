using GymTrackerMobile.Persistence;
using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class DatabaseInitializationTests
{
    [Fact]
    public async Task New_database_applies_schema_and_seeds_catalogue()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False")
                .Options;

            await using (var context = new GymTrackerDbContext(options))
            {
                var initializer = new DatabaseInitializer(context);

                await initializer.InitializeAsync();

                Assert.Equal(22, await context.Exercises.CountAsync());
                var exercises = await context.Exercises.AsNoTracking().ToListAsync();
                var expectedNames = new[]
                {
                    "Chest Press Machine", "Pec Fly Machine", "Seated Shoulder Press Machine",
                    "Incline Dumbbell Press", "Dumbbell Shoulder Press", "Lat Pulldown Machine",
                    "Close Grip Lat Pulldown", "Seated Cable Row", "Rear Delt Fly Machine",
                    "One-Arm Dumbbell Row", "Tricep Extension Machine", "Bicep Curl Machine",
                    "Barbell Curl", "Overhead Tricep Extension", "Incline Bicep Curl",
                    "Concentration Bicep Curl", "Hammer Curl", "Leg Press", "Seated Leg Curl",
                    "Leg Extension", "Push Up", "Pull Up"
                };

                Assert.Equal(expectedNames.Order(), exercises.Select(x => x.Name).Order());
                Assert.All(exercises, exercise =>
                {
                    Assert.NotEqual(Guid.Empty, exercise.Id);
                    Assert.False(string.IsNullOrWhiteSpace(exercise.Name));
                    Assert.False(string.IsNullOrWhiteSpace(exercise.PrimaryMuscleGroup));
                    Assert.False(string.IsNullOrWhiteSpace(exercise.EquipmentType));
                    Assert.InRange(exercise.DefaultMinimumRepetitions, 1, exercise.DefaultMaximumRepetitions);
                    Assert.True(exercise.DefaultSetCount > 0);
                    Assert.True(Enum.IsDefined(exercise.WeightEntryConvention));
                    Assert.True(Enum.IsDefined(exercise.ExerciseMode));
                });
                Assert.Equal(exercises.Count, exercises.Select(x => x.Id).Distinct().Count());
                Assert.Equal(4, await context.WorkoutTemplates.CountAsync());
                Assert.Single(await context.BackupMetadata.ToListAsync());
                Assert.Equal(2, (await context.Database.GetAppliedMigrationsAsync()).Count());
                await context.Database.CloseConnectionAsync();
            }
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Initialization_seeds_template_specific_exercises_in_order_with_explicit_targets()
    {
        await using var context = await CreateContextAsync();

        var templates = await context.WorkoutTemplates
            .Include(x => x.Exercises.OrderBy(y => y.SortOrder))
            .ToDictionaryAsync(x => x.Name);

        var expected = new Dictionary<string, int[]>
        {
            ["Push"] = [10, 11, 12, 13, 14, 20, 23, 30],
            ["Pull"] = [15, 16, 17, 18, 19, 21, 22, 24, 25, 26, 31],
            ["Legs"] = [27, 28, 29],
            ["Full Body"] = [10, 15, 27, 14, 22, 20]
        };

        Assert.Equal(expected.Keys.Order(), templates.Keys.Order());

        foreach (var (name, exerciseIds) in expected)
        {
            var exercises = templates[name].Exercises.ToList();
            Assert.Equal(exerciseIds.Length, exercises.Count);
            Assert.Equal(exerciseIds.Select(StableId), exercises.Select(x => x.ExerciseId));
            Assert.Equal(Enumerable.Range(0, exercises.Count), exercises.Select(x => x.SortOrder));
            Assert.All(exercises, exercise =>
            {
                Assert.InRange(exercise.TargetMinimumRepetitions, 1, exercise.TargetMaximumRepetitions);
                Assert.Equal(8, exercise.TargetMinimumRepetitions);
                Assert.Equal(12, exercise.TargetMaximumRepetitions);
                Assert.Equal(3, exercise.PlannedSetCount);
            });
        }
    }

    [Fact]
    public async Task Initialization_repairs_incomplete_fixed_catalogue_metadata()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
                .UseSqlite($"Data Source={path};Pooling=False")
                .Options;

            await using (var context = new GymTrackerDbContext(options))
            {
                var initializer = new DatabaseInitializer(context);
                await initializer.InitializeAsync();

                var exercise = await context.Exercises.SingleAsync(x => x.Name == "Hammer Curl");
                exercise.PrimaryMuscleGroup = string.Empty;
                exercise.EquipmentType = string.Empty;
                exercise.DefaultMinimumRepetitions = 0;
                exercise.DefaultMaximumRepetitions = 0;
                exercise.DefaultSetCount = 0;
                await context.SaveChangesAsync();

                await initializer.InitializeAsync();

                var repaired = await context.Exercises.SingleAsync(x => x.Name == "Hammer Curl");
                Assert.Equal("Arms", repaired.PrimaryMuscleGroup);
                Assert.Equal("Dumbbell", repaired.EquipmentType);
                Assert.Equal(WeightEntryConvention.PerDumbbell, repaired.WeightEntryConvention);
                Assert.Equal(8, repaired.DefaultMinimumRepetitions);
                Assert.Equal(12, repaired.DefaultMaximumRepetitions);
                Assert.Equal(3, repaired.DefaultSetCount);
                Assert.Equal(ExerciseMode.Dumbbell, repaired.ExerciseMode);
            }
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Initialization_repairs_incomplete_built_in_template_rows()
    {
        await using var context = await CreateContextAsync();

        var push = await context.WorkoutTemplates
            .Include(x => x.Exercises)
            .SingleAsync(x => x.Name == "Push");
        context.TemplateExercises.Remove(push.Exercises.OrderByDescending(x => x.SortOrder).First());
        await context.SaveChangesAsync();

        await new DatabaseInitializer(context).InitializeAsync();

        var repaired = await context.WorkoutTemplates
            .Include(x => x.Exercises.OrderBy(y => y.SortOrder))
            .SingleAsync(x => x.Name == "Push");
        Assert.Equal(8, repaired.Exercises.Count);
        Assert.Equal(new[] { 10, 11, 12, 13, 14, 20, 23, 30 }.Select(StableId), repaired.Exercises.Select(x => x.ExerciseId));
    }

    [Fact]
    public async Task Initialization_seeds_complete_and_partial_workout_examples_once()
    {
        await using var context = await CreateContextAsync();

        var sessions = await context.WorkoutSessions
            .Include(x => x.Exercises)
            .ThenInclude(x => x.Sets)
            .OrderBy(x => x.TemplateName)
            .ToListAsync();

        Assert.Equal(2, sessions.Count);
        Assert.Contains(sessions, x => x.TemplateName == "Push Workout" && x.Exercises.SelectMany(y => y.Sets).All(y => y.Status == SetStatus.Completed));
        Assert.Contains(sessions, x => x.TemplateName == "Pull Workout" && x.Exercises.SelectMany(y => y.Sets).Any(y => y.Status == SetStatus.Incomplete));

        await new DatabaseInitializer(context).InitializeAsync();

        Assert.Equal(2, await context.WorkoutSessions.CountAsync());
    }

    [Fact]
    public async Task Reset_clears_user_data_and_reseeds_built_in_catalogue()
    {
        await using var context = await CreateContextAsync();
        context.UserSettings.Add(new UserSetting { Key = "illustration-style", Value = "Male" });
        context.ActivityRecords.Add(new ActivityRecord { ActivityType = ActivityType.Walking, ActivityDateUtc = DateTime.UtcNow });
        var template = await context.WorkoutTemplates.FirstAsync();
        var session = new WorkoutSession
        {
            TemplateId = template.Id,
            TemplateName = template.Name,
            StartedAtUtc = DateTime.UtcNow,
            IsActive = true
        };
        context.WorkoutSessions.Add(session);
        await context.SaveChangesAsync();

        var service = new AppDataResetService(context);

        await service.ResetAsync();

        Assert.Equal(2, await context.WorkoutSessions.CountAsync());
        Assert.Empty(await context.ActivityRecords.ToListAsync());
        Assert.Empty(await context.UserSettings.ToListAsync());
        Assert.Equal(22, await context.Exercises.CountAsync());
        Assert.Equal(4, await context.WorkoutTemplates.CountAsync());
        Assert.Equal(28, await context.TemplateExercises.CountAsync());
        Assert.Single(await context.BackupMetadata.ToListAsync());
    }

    private static async Task<GymTrackerMobile.Persistence.GymTrackerDbContext> CreateContextAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<GymTrackerMobile.Persistence.GymTrackerDbContext>()
            .UseSqlite($"Data Source={path};Pooling=False")
            .Options;
        var context = new GymTrackerMobile.Persistence.GymTrackerDbContext(options);
        await new DatabaseInitializer(context).InitializeAsync();
        return context;
    }

    private static Guid StableId(int value) => new($"00000000-0000-0000-0000-{value:000000000000}");
}
