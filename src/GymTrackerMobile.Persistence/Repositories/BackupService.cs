using System.Text.Json;
using System.Text.Json.Serialization;
using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class BackupService(GymTrackerDbContext context, string backupDirectory) : IBackupService
{
    private const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { ReferenceHandler = ReferenceHandler.IgnoreCycles, WriteIndented = true };

    public async Task<BackupExportResult> ExportAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(backupDirectory);
        var fileName = $"gym-tracker-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        var path = Path.Combine(backupDirectory, fileName);
        var document = await CreateDocumentAsync(cancellationToken);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(document, JsonOptions), cancellationToken);
        var metadata = await context.BackupMetadata.SingleAsync(cancellationToken);
        metadata.LastExportedAtUtc = DateTime.UtcNow;
        metadata.LastBackupFileName = fileName;
        metadata.LastBackupFileIdentity = path;
        await context.SaveChangesAsync(cancellationToken);
        return new(fileName, document.WorkoutSessions.Count + document.ActivityRecords.Count, path);
    }

    public async Task<BackupImportValidation> ValidateImportAsync(Stream backup, CancellationToken cancellationToken = default)
    {
        try
        {
            backup.Position = 0;
            var document = await JsonSerializer.DeserializeAsync<BackupDocument>(backup, JsonOptions, cancellationToken);
            if (document is null || document.SchemaVersion != CurrentSchemaVersion) return new(false, "This backup file is unsupported or corrupt.", 0);
            return new(true, null, document.WorkoutSessions.Count + document.ActivityRecords.Count);
        }
        catch (JsonException) { return new(false, "This backup file is unsupported or corrupt.", 0); }
    }

    public async Task<BackupImportResult> ImportAsync(Stream backup, BackupImportMode mode, CancellationToken cancellationToken = default)
    {
        backup.Position = 0;
        var document = await JsonSerializer.DeserializeAsync<BackupDocument>(backup, JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("This backup file is unsupported or corrupt.");
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        if (mode == BackupImportMode.Replace)
        {
            await context.RecommendationOutcomes.ExecuteDeleteAsync(cancellationToken);
            await context.Recommendations.ExecuteDeleteAsync(cancellationToken);
            await context.WorkoutSets.ExecuteDeleteAsync(cancellationToken);
            await context.WorkoutExercises.ExecuteDeleteAsync(cancellationToken);
            await context.WorkoutSessions.ExecuteDeleteAsync(cancellationToken);
            await context.ActivityRecords.ExecuteDeleteAsync(cancellationToken);
            await context.UserSettings.ExecuteDeleteAsync(cancellationToken);
        }
        var sessions = mode == BackupImportMode.Merge ? CloneSessions(document.WorkoutSessions) : document.WorkoutSessions;
        var activities = mode == BackupImportMode.Merge ? document.ActivityRecords.Select(CloneActivity).ToList() : document.ActivityRecords;
        context.WorkoutSessions.AddRange(sessions);
        context.ActivityRecords.AddRange(activities);
        foreach (var setting in document.UserSettings)
        {
            if (mode == BackupImportMode.Merge && await context.UserSettings.AnyAsync(x => x.Key == setting.Key, cancellationToken)) continue;
            context.UserSettings.Add(setting);
        }
        var metadata = await context.BackupMetadata.SingleAsync(cancellationToken);
        metadata.LastImportedAtUtc = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new($"Imported {document.WorkoutSessions.Count + document.ActivityRecords.Count} records.");
    }

    private async Task<BackupDocument> CreateDocumentAsync(CancellationToken cancellationToken) => new()
    {
        SchemaVersion = CurrentSchemaVersion,
        WorkoutSessions = await context.WorkoutSessions.AsNoTracking().Include(x => x.Exercises).ThenInclude(x => x.Sets).ToListAsync(cancellationToken),
        ActivityRecords = await context.ActivityRecords.AsNoTracking().ToListAsync(cancellationToken),
        UserSettings = await context.UserSettings.AsNoTracking().ToListAsync(cancellationToken)
    };

    private static List<WorkoutSession> CloneSessions(IEnumerable<WorkoutSession> source) => source.Select(session =>
    {
        var clone = new WorkoutSession { TemplateId = session.TemplateId, TemplateName = session.TemplateName, StartedAtUtc = session.StartedAtUtc, CompletedAtUtc = session.CompletedAtUtc, IsActive = session.IsActive, Notes = session.Notes };
        clone.Exercises = session.Exercises.Select(exercise =>
        {
            var exerciseClone = new WorkoutExercise { WorkoutSessionId = clone.Id, ExerciseId = exercise.ExerciseId, ExerciseName = exercise.ExerciseName, PrimaryMuscleGroup = exercise.PrimaryMuscleGroup, EquipmentType = exercise.EquipmentType, WeightEntryConvention = exercise.WeightEntryConvention, TargetMinimumRepetitions = exercise.TargetMinimumRepetitions, TargetMaximumRepetitions = exercise.TargetMaximumRepetitions, PlannedSetCount = exercise.PlannedSetCount, SortOrder = exercise.SortOrder };
            exerciseClone.Sets = exercise.Sets.Select(set => new WorkoutSet { WorkoutExerciseId = exerciseClone.Id, SetNumber = set.SetNumber, Status = set.Status, WeightKilograms = set.WeightKilograms, Repetitions = set.Repetitions, Rpe = set.Rpe, Difficulty = set.Difficulty, Notes = set.Notes, RecordedAtUtc = set.RecordedAtUtc }).ToList();
            return exerciseClone;
        }).ToList();
        return clone;
    }).ToList();

    private static ActivityRecord CloneActivity(ActivityRecord activity) => new() { ActivityDateUtc = activity.ActivityDateUtc, ActivityType = activity.ActivityType, DurationMinutes = activity.DurationMinutes, DistanceKilometres = activity.DistanceKilometres, Steps = activity.Steps, PoolLengthMetres = activity.PoolLengthMetres, PoolLengths = activity.PoolLengths, Notes = activity.Notes, AveragePaceMinutesPerKilometre = activity.AveragePaceMinutesPerKilometre };

    private sealed class BackupDocument
    {
        public int SchemaVersion { get; set; }
        public List<WorkoutSession> WorkoutSessions { get; set; } = [];
        public List<ActivityRecord> ActivityRecords { get; set; } = [];
        public List<UserSetting> UserSettings { get; set; } = [];
    }
}
