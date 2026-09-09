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
    public async Task Starting_the_same_template_twice_creates_distinct_sessions()
    {
        await using var context = await CreateContextAsync();
        var repository = new WorkoutRepository(context);
        var template = await context.WorkoutTemplates.FirstAsync();
        var firstStartedAt = DateTime.UtcNow;

        var first = await repository.StartWorkoutAsync(template.Id, firstStartedAt);
        await repository.CompleteWorkoutAsync(first.Id, firstStartedAt.AddMinutes(45), null);
        var second = await repository.StartWorkoutAsync(template.Id, firstStartedAt.AddDays(1));

        Assert.NotEqual(first.Id, second.Id);
        Assert.Equal(template.Id, first.TemplateId);
        Assert.Equal(template.Id, second.TemplateId);
        Assert.NotEqual(first.StartedAtUtc, second.StartedAtUtc);
    }

    [Fact]
    public async Task Empty_active_workout_survives_context_reopen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            Guid sessionId;
            await using (var context = await CreateContextAsync(path))
            {
                var repository = new WorkoutRepository(context);
                var template = await context.WorkoutTemplates.FirstAsync();
                var session = await repository.StartWorkoutAsync(template.Id, DateTime.UtcNow);
                sessionId = session.Id;
                await context.Database.CloseConnectionAsync();
            }

            await using var reopened = await CreateContextAsync(path);
            var recovered = await new WorkoutRepository(reopened).GetActiveWorkoutAsync();

            Assert.NotNull(recovered);
            Assert.Equal(sessionId, recovered.Id);
            Assert.All(recovered.Exercises.SelectMany(x => x.Sets), set => Assert.Equal(SetStatus.Planned, set.Status));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Partial_active_workout_and_sets_survive_context_reopen()
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

    [Fact]
    public async Task Edited_set_notes_and_status_survive_context_reopen()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            Guid sessionId;
            await using (var context = await CreateContextAsync(path))
            {
                var repository = new WorkoutRepository(context);
                var template = await context.WorkoutTemplates.FirstAsync();
                var session = await repository.StartWorkoutAsync(template.Id, DateTime.UtcNow);
                sessionId = session.Id;
                var set = session.Exercises.First().Sets.First();
                await repository.SaveSetAsync(new WorkoutSet
                {
                    Id = set.Id,
                    WorkoutExerciseId = set.WorkoutExerciseId,
                    SetNumber = set.SetNumber,
                    Status = SetStatus.Completed,
                    WeightKilograms = 47.5,
                    Repetitions = 9,
                    Notes = "Felt strong",
                    RecordedAtUtc = DateTime.UtcNow
                });
            }

            await using var reopened = await CreateContextAsync(path);
            var recovered = await new WorkoutRepository(reopened).GetActiveWorkoutAsync();
            var recoveredSet = recovered!.Exercises.First().Sets.First();

            Assert.Equal(sessionId, recovered.Id);
            Assert.Equal(SetStatus.Completed, recoveredSet.Status);
            Assert.Equal(47.5, recoveredSet.WeightKilograms);
            Assert.Equal(9, recoveredSet.Repetitions);
            Assert.Equal("Felt strong", recoveredSet.Notes);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task All_loggable_set_statuses_round_trip_through_persistence()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db");
        try
        {
            Guid exerciseId;
            await using (var context = await CreateContextAsync(path))
            {
                var repository = new WorkoutRepository(context);
                var template = await context.WorkoutTemplates.FirstAsync();
                var session = await repository.StartWorkoutAsync(template.Id, DateTime.UtcNow);
                var exercise = session.Exercises.First();
                exerciseId = exercise.Id;

                for (var index = 0; index < 4; index++)
                {
                    var setNumber = index + 1;
                    var existingSet = exercise.Sets.SingleOrDefault(x => x.SetNumber == setNumber);
                    await repository.SaveSetAsync(new WorkoutSet
                    {
                        Id = existingSet?.Id ?? Guid.NewGuid(),
                        WorkoutExerciseId = exerciseId,
                        SetNumber = setNumber,
                        Status = (SetStatus)(index + 1),
                        Repetitions = index == 0 ? 10 : null,
                        RecordedAtUtc = DateTime.UtcNow
                    });
                }
            }

            await using var reopened = await CreateContextAsync(path);
            var recovered = await new WorkoutRepository(reopened).GetActiveWorkoutAsync();
            var statuses = recovered!.Exercises.Single(x => x.Id == exerciseId).Sets.OrderBy(x => x.SetNumber).Select(x => x.Status);

            Assert.Equal([SetStatus.Completed, SetStatus.Incomplete, SetStatus.Failed, SetStatus.Skipped], statuses);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task Abandoning_active_workout_removes_only_the_active_session()
    {
        await using var context = await CreateContextAsync();
        var repository = new WorkoutRepository(context);
        var template = await context.WorkoutTemplates.FirstAsync();
        var active = await repository.StartWorkoutAsync(template.Id, DateTime.UtcNow);

        await repository.AbandonWorkoutAsync(active.Id);

        Assert.Null(await repository.GetActiveWorkoutAsync());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.CompleteWorkoutAsync(active.Id, DateTime.UtcNow, null));
    }

    [Fact]
    public async Task Abandon_cannot_delete_completed_history()
    {
        await using var context = await CreateContextAsync();
        var repository = new WorkoutRepository(context);
        var template = await context.WorkoutTemplates.FirstAsync();
        var completed = await repository.StartWorkoutAsync(template.Id, DateTime.UtcNow);
        await repository.CompleteWorkoutAsync(completed.Id, DateTime.UtcNow, "Done");

        await Assert.ThrowsAsync<InvalidOperationException>(() => repository.AbandonWorkoutAsync(completed.Id));
        Assert.Single(await repository.GetCompletedWorkoutsAsync(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(1)));
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
