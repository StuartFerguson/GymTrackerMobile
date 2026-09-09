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

                Assert.Equal(20, await context.Exercises.CountAsync());
                var exercises = await context.Exercises.AsNoTracking().ToListAsync();
                var expectedNames = new[]
                {
                    "Chest Press Machine", "Pec Fly Machine", "Seated Shoulder Press Machine",
                    "Incline Dumbbell Press", "Dumbbell Shoulder Press", "Lat Pulldown Machine",
                    "Close Grip Lat Pulldown", "Seated Cable Row", "Rear Delt Fly Machine",
                    "One-Arm Dumbbell Row", "Tricep Extension Machine", "Bicep Curl Machine",
                    "Barbell Curl", "Overhead Tricep Extension", "Incline Bicep Curl",
                    "Concentration Bicep Curl", "Hammer Curl", "Leg Press", "Seated Leg Curl",
                    "Leg Extension"
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
                Assert.Single(await context.Database.GetAppliedMigrationsAsync());
                await context.Database.CloseConnectionAsync();
            }
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
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

        Assert.Empty(await context.WorkoutSessions.ToListAsync());
        Assert.Empty(await context.ActivityRecords.ToListAsync());
        Assert.Empty(await context.UserSettings.ToListAsync());
        Assert.Equal(20, await context.Exercises.CountAsync());
        Assert.Equal(4, await context.WorkoutTemplates.CountAsync());
        Assert.Equal(80, await context.TemplateExercises.CountAsync());
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
}
