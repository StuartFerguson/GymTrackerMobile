using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Backup;

public sealed class BackupService(
    GymTrackerDbContext context,
    IBackupFileStore fileStore,
    GymTrackerBackupValidator validator,
    GymTrackerBackupMapper mapper) : IBackupService
{
    public async Task<string> ExportAsync(CancellationToken cancellationToken = default) =>
        GymTrackerBackupJson.Serialize(await mapper.ExportAsync(cancellationToken));

    public Task<BackupValidationResult> ValidateAsync(string json, CancellationToken cancellationToken = default) =>
        Task.FromResult(validator.Validate(json));

    public async Task<BackupImportResult> ImportAsync(string json, BackupImportMode mode, CancellationToken cancellationToken = default)
    {
        var validation = validator.Validate(json);
        if (!validation.IsValid) throw new BackupValidationException(validation.Errors);

        string? recoveryPath = null;
        if (mode == BackupImportMode.Replace)
        {
            recoveryPath = await fileStore.SaveRecoveryCopyAsync(await ExportAsync(cancellationToken), cancellationToken);
        }

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        context.ChangeTracker.Clear();
        if (mode == BackupImportMode.Replace)
        {
            await context.RecommendationOutcomes.ExecuteDeleteAsync(cancellationToken);
            await context.Recommendations.ExecuteDeleteAsync(cancellationToken);
            await context.WorkoutSets.ExecuteDeleteAsync(cancellationToken);
            await context.WorkoutExercises.ExecuteDeleteAsync(cancellationToken);
            await context.WorkoutSessions.ExecuteDeleteAsync(cancellationToken);
            await context.ActivityRecords.ExecuteDeleteAsync(cancellationToken);
            await context.UserSettings.ExecuteDeleteAsync(cancellationToken);
            await context.TemplateExercises.ExecuteDeleteAsync(cancellationToken);
            await context.WorkoutTemplates.ExecuteDeleteAsync(cancellationToken);
            await context.Exercises.ExecuteDeleteAsync(cancellationToken);
            await context.BackupMetadata.ExecuteDeleteAsync(cancellationToken);
            GymTrackerBackupMapper.AddReplacement(context, validation.Document!);
        }
        else
        {
            await MergeAsync(validation.Document!, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(recoveryPath);
    }

    private async Task MergeAsync(GymTrackerBackupDocument d, CancellationToken cancellationToken)
    {
        foreach (var incoming in d.Exercises)
        {
            var current = await context.Exercises.FindAsync([incoming.Id], cancellationToken);
            if (current is null) context.Exercises.Add(new Exercise { Id=incoming.Id, Name=incoming.Name, PrimaryMuscleGroup=incoming.PrimaryMuscleGroup, EquipmentType=incoming.EquipmentType, WeightEntryConvention=incoming.WeightEntryConvention, DefaultMinimumRepetitions=incoming.DefaultMinimumRepetitions, DefaultMaximumRepetitions=incoming.DefaultMaximumRepetitions, DefaultSetCount=incoming.DefaultSetCount, ExerciseMode=incoming.ExerciseMode });
            else { current.Name=incoming.Name; current.PrimaryMuscleGroup=incoming.PrimaryMuscleGroup; current.EquipmentType=incoming.EquipmentType; current.WeightEntryConvention=incoming.WeightEntryConvention; current.DefaultMinimumRepetitions=incoming.DefaultMinimumRepetitions; current.DefaultMaximumRepetitions=incoming.DefaultMaximumRepetitions; current.DefaultSetCount=incoming.DefaultSetCount; current.ExerciseMode=incoming.ExerciseMode; }
        }
        foreach (var incoming in d.WorkoutTemplates)
        {
            var current = await context.WorkoutTemplates.FindAsync([incoming.Id], cancellationToken);
            if (current is null) context.WorkoutTemplates.Add(new WorkoutTemplate { Id=incoming.Id, Name=incoming.Name, IsBuiltIn=incoming.IsBuiltIn });
            else { current.Name=incoming.Name; current.IsBuiltIn=incoming.IsBuiltIn; }
        }
        foreach (var incoming in d.TemplateExercises)
        {
            var current = await context.TemplateExercises.FindAsync([incoming.Id], cancellationToken);
            if (current is null) context.TemplateExercises.Add(new TemplateExercise { Id=incoming.Id, WorkoutTemplateId=incoming.WorkoutTemplateId, ExerciseId=incoming.ExerciseId, SortOrder=incoming.SortOrder, TargetMinimumRepetitions=incoming.TargetMinimumRepetitions, TargetMaximumRepetitions=incoming.TargetMaximumRepetitions, PlannedSetCount=incoming.PlannedSetCount });
            else { current.WorkoutTemplateId=incoming.WorkoutTemplateId; current.ExerciseId=incoming.ExerciseId; current.SortOrder=incoming.SortOrder; current.TargetMinimumRepetitions=incoming.TargetMinimumRepetitions; current.TargetMaximumRepetitions=incoming.TargetMaximumRepetitions; current.PlannedSetCount=incoming.PlannedSetCount; }
        }
        foreach (var incoming in d.ActivityRecords)
        {
            var current = await context.ActivityRecords.FindAsync([incoming.Id], cancellationToken);
            if (current is null) context.ActivityRecords.Add(new ActivityRecord { Id=incoming.Id, ActivityDateUtc=incoming.ActivityDateUtc, ActivityType=incoming.ActivityType, DurationMinutes=incoming.DurationMinutes, DistanceKilometres=incoming.DistanceKilometres, Steps=incoming.Steps, PoolLengthMetres=incoming.PoolLengthMetres, PoolLengths=incoming.PoolLengths, Notes=incoming.Notes, AveragePaceMinutesPerKilometre=incoming.AveragePaceMinutesPerKilometre });
        }
    }
}

public sealed class BackupValidationException(IReadOnlyList<BackupValidationError> errors) : Exception("Backup validation failed.")
{
    public IReadOnlyList<BackupValidationError> Errors { get; } = errors;
}
