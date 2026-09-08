using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class WorkoutRepository(GymTrackerDbContext context) : IWorkoutRepository
{
    public async Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default)
    {
        if (await context.WorkoutSessions.AnyAsync(x => x.IsActive, cancellationToken))
        {
            throw new InvalidOperationException("An active workout already exists.");
        }

        var template = await context.WorkoutTemplates
            .Include(x => x.Exercises)
            .ThenInclude(x => x.Exercise)
            .SingleOrDefaultAsync(x => x.Id == templateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Workout template '{templateId}' was not found.");

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var session = new WorkoutSession
        {
            TemplateId = template.Id,
            TemplateName = template.Name,
            StartedAtUtc = startedAtUtc,
            IsActive = true
        };

        foreach (var templateExercise in template.Exercises.OrderBy(x => x.SortOrder))
        {
            var catalogueExercise = templateExercise.Exercise
                ?? throw new InvalidOperationException("A template exercise is missing its catalogue exercise.");
            var workoutExercise = new WorkoutExercise
            {
                WorkoutSessionId = session.Id,
                ExerciseId = catalogueExercise.Id,
                ExerciseName = catalogueExercise.Name,
                PrimaryMuscleGroup = catalogueExercise.PrimaryMuscleGroup,
                EquipmentType = catalogueExercise.EquipmentType,
                WeightEntryConvention = catalogueExercise.WeightEntryConvention,
                TargetMinimumRepetitions = templateExercise.TargetMinimumRepetitions,
                TargetMaximumRepetitions = templateExercise.TargetMaximumRepetitions,
                PlannedSetCount = templateExercise.PlannedSetCount,
                SortOrder = templateExercise.SortOrder
            };

            for (var setNumber = 1; setNumber <= workoutExercise.PlannedSetCount; setNumber++)
            {
                workoutExercise.Sets.Add(new WorkoutSet { SetNumber = setNumber });
            }

            session.Exercises.Add(workoutExercise);
        }

        context.WorkoutSessions.Add(session);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return session;
    }

    public async Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default)
    {
        var existing = await context.WorkoutSets.SingleOrDefaultAsync(
            x => x.Id == set.Id || (x.WorkoutExerciseId == set.WorkoutExerciseId && x.SetNumber == set.SetNumber),
            cancellationToken);
        if (existing is null)
        {
            context.WorkoutSets.Add(set);
        }
        else
        {
            existing.WorkoutExerciseId = set.WorkoutExerciseId;
            existing.SetNumber = set.SetNumber;
            existing.Status = set.Status;
            existing.WeightKilograms = set.WeightKilograms;
            existing.Repetitions = set.Repetitions;
            existing.Rpe = set.Rpe;
            existing.Difficulty = set.Difficulty;
            existing.Notes = set.Notes;
            existing.RecordedAtUtc = set.RecordedAtUtc;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default) =>
        context.WorkoutSessions
            .Include(x => x.Exercises.OrderBy(y => y.SortOrder))
            .ThenInclude(x => x.Sets.OrderBy(y => y.SetNumber))
            .SingleOrDefaultAsync(x => x.IsActive, cancellationToken);

    public async Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default)
    {
        var session = await context.WorkoutSessions.SingleOrDefaultAsync(x => x.Id == sessionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Workout session '{sessionId}' was not found.");
        session.CompletedAtUtc = completedAtUtc;
        session.Notes = notes;
        session.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}
