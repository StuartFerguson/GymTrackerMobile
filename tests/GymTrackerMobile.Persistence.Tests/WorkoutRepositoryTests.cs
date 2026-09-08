using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class WorkoutRepositoryTests
{
    [Fact]
    public async Task Starting_a_workout_copies_template_values_into_a_snapshot()
    {
        await using var context = await CreateContextAsync();
        var template = await context.WorkoutTemplates.Include(x => x.Exercises).FirstAsync();
        var repository = new WorkoutRepository(context);

        var session = await repository.StartWorkoutAsync(template.Id, DateTime.UtcNow);
        var originalName = session.Exercises.First().ExerciseName;
        template.Exercises.First().TargetMaximumRepetitions = 99;
        await context.SaveChangesAsync();

        var reloaded = await context.WorkoutSessions
            .Include(x => x.Exercises)
            .SingleAsync(x => x.Id == session.Id);

        Assert.Equal(originalName, reloaded.Exercises.First().ExerciseName);
        Assert.NotEqual(99, reloaded.Exercises.First().TargetMaximumRepetitions);
    }

    [Fact]
    public async Task Active_workout_and_sets_survive_context_reopen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            Guid sessionId;
            Guid exerciseId;
            await using (var context = await CreateContextAsync(path))
            {
                var repository = new WorkoutRepository(context);
                var template = await context.WorkoutTemplates.FirstAsync();
                var session = await repository.StartWorkoutAsync(template.Id, DateTime.UtcNow);
                var exercise = session.Exercises.First();
                sessionId = session.Id;
                exerciseId = exercise.Id;
                await repository.SaveSetAsync(new WorkoutSet
                {
                    WorkoutExerciseId = exercise.Id,
                    SetNumber = 1,
                    Status = SetStatus.Completed,
                    WeightKilograms = 42,
                    Repetitions = 10,
                    RecordedAtUtc = DateTime.UtcNow
                });
                await context.Database.CloseConnectionAsync();
            }

            await using var reopened = await CreateContextAsync(path);
            var recovered = await new WorkoutRepository(reopened).GetActiveWorkoutAsync();

            Assert.NotNull(recovered);
            Assert.Equal(sessionId, recovered.Id);
            Assert.Equal(42, recovered.Exercises.Single(x => x.Id == exerciseId).Sets.Single(x => x.SetNumber == 1).WeightKilograms);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static async Task<GymTrackerDbContext> CreateContextAsync(string? path = null)
    {
        path ??= Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        var options = new DbContextOptionsBuilder<GymTrackerDbContext>()
            .UseSqlite($"Data Source={path};Pooling=False")
            .Options;
        var context = new GymTrackerDbContext(options);
        await new DatabaseInitializer(context).InitializeAsync();
        return context;
    }
}
