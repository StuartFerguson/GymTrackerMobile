using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class EntityRoundTripTests
{
    [Fact]
    public async Task Required_persistence_entities_round_trip_without_data_loss()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            await using (var context = await CreateContextAsync(path))
            {
                var session = await new WorkoutRepository(context)
                    .StartWorkoutAsync(await context.WorkoutTemplates.Select(x => x.Id).FirstAsync(), DateTime.UtcNow);
                var workoutExercise = session.Exercises.First();
                context.ActivityRecords.Add(new ActivityRecord
                {
                    ActivityDateUtc = DateTime.UtcNow.Date,
                    ActivityType = ActivityType.Walking,
                    DurationMinutes = 45,
                    DistanceKilometres = 4.2,
                    Steps = 6000,
                    Notes = "Lunch walk",
                    AveragePaceMinutesPerKilometre = 10.7
                });
                var recommendation = new Recommendation
                {
                    WorkoutExerciseId = workoutExercise.Id,
                    ProposedWeightKilograms = 44,
                    ProposedMinimumRepetitions = 8,
                    ProposedMaximumRepetitions = 10,
                    ProposedSetCount = 3,
                    Explanation = "All sets completed",
                    Confidence = 0.9,
                    CreatedAtUtc = DateTime.UtcNow
                };
                recommendation.Outcome = new RecommendationOutcome
                {
                    OutcomeType = RecommendationOutcomeType.Accepted,
                    AppliedWeightKilograms = 44,
                    RecordedAtUtc = DateTime.UtcNow
                };
                context.Recommendations.Add(recommendation);
                context.UserSettings.Add(new UserSetting { Key = "units", Value = "kg", UpdatedAtUtc = DateTime.UtcNow });
                await context.SaveChangesAsync();
                await context.Database.CloseConnectionAsync();
            }

            await using var reopened = await CreateContextAsync(path);
            Assert.Equal(1, await reopened.ActivityRecords.CountAsync());
            Assert.Equal(1, await reopened.Recommendations.Include(x => x.Outcome).CountAsync());
            Assert.Equal("kg", await reopened.UserSettings.Select(x => x.Value).SingleAsync());
            Assert.Equal("All sets completed", await reopened.Recommendations.Select(x => x.Explanation).SingleAsync());
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static async Task<GymTrackerDbContext> CreateContextAsync(string path)
    {
        var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
            .UseSqlite($"Data Source={path};Pooling=False")
            .Options;
        var context = new GymTrackerDbContext(options);
        await new DatabaseInitializer(context).InitializeAsync();
        return context;
    }
}
