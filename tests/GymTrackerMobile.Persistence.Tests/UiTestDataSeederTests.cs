using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class UiTestDataSeederTests
{
    [Fact]
    public async Task Resets_to_the_deterministic_history_and_recommendation_fixture()
    {
        await using var context = await CreateContextAsync();
        context.ActivityRecords.Add(new ActivityRecord { ActivityType = ActivityType.Walking, ActivityDateUtc = DateTime.UtcNow });
        context.WorkoutSessions.Add(new WorkoutSession { TemplateName = "Unsaved", StartedAtUtc = DateTime.UtcNow, IsActive = true });
        await context.SaveChangesAsync();

        await UiTestDataSeeder.SeedAsync(context);

        Assert.Empty(await context.ActivityRecords.ToListAsync());
        Assert.DoesNotContain(await context.WorkoutSessions.ToListAsync(), session => session.IsActive);
        Assert.Contains(await context.WorkoutSessions.Include(x => x.Exercises).ThenInclude(x => x.Sets).ToListAsync(), session =>
            session.TemplateName == "Push Workout" && session.Exercises.SelectMany(x => x.Sets).Any(x => x.Status == SetStatus.Completed));
        Assert.Equal(4, await context.WorkoutTemplates.CountAsync());
    }

    private static async Task<GymTrackerDbContext> CreateContextAsync()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-ui-fixture-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
            .UseSqlite($"Data Source={path};Pooling=False")
            .Options;
        var context = new GymTrackerDbContext(options);
        await new DatabaseInitializer(context).InitializeAsync();
        return context;
    }
}
