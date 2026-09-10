using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Backup;

public sealed class GymTrackerBackupMapper(GymTrackerDbContext context)
{
    public async Task<GymTrackerBackupDocument> ExportAsync(CancellationToken cancellationToken)
    {
        var document = new GymTrackerBackupDocument
        {
            SchemaVersion = 1,
            ExportedAtUtc = DateTime.UtcNow,
            Exercises = await context.Exercises.AsNoTracking().Select(x => new ExerciseBackupDto
            {
                Id = x.Id, Name = x.Name, PrimaryMuscleGroup = x.PrimaryMuscleGroup, EquipmentType = x.EquipmentType,
                WeightEntryConvention = x.WeightEntryConvention, DefaultMinimumRepetitions = x.DefaultMinimumRepetitions,
                DefaultMaximumRepetitions = x.DefaultMaximumRepetitions, DefaultSetCount = x.DefaultSetCount, ExerciseMode = x.ExerciseMode
            }).ToListAsync(cancellationToken),
            WorkoutTemplates = await context.WorkoutTemplates.AsNoTracking().Select(x => new WorkoutTemplateBackupDto
            { Id = x.Id, Name = x.Name, IsBuiltIn = x.IsBuiltIn }).ToListAsync(cancellationToken),
            TemplateExercises = await context.TemplateExercises.AsNoTracking().Select(x => new TemplateExerciseBackupDto
            { Id = x.Id, WorkoutTemplateId = x.WorkoutTemplateId, ExerciseId = x.ExerciseId, SortOrder = x.SortOrder,
              TargetMinimumRepetitions = x.TargetMinimumRepetitions, TargetMaximumRepetitions = x.TargetMaximumRepetitions,
              PlannedSetCount = x.PlannedSetCount }).ToListAsync(cancellationToken),
            ActivityRecords = await context.ActivityRecords.AsNoTracking().Select(x => new ActivityRecordBackupDto
            { Id = x.Id, ActivityDateUtc = DateTime.SpecifyKind(x.ActivityDateUtc, DateTimeKind.Utc), ActivityType = x.ActivityType, DurationMinutes = x.DurationMinutes,
              DistanceKilometres = x.DistanceKilometres, Steps = x.Steps, PoolLengthMetres = x.PoolLengthMetres,
              PoolLengths = x.PoolLengths, Notes = x.Notes, AveragePaceMinutesPerKilometre = x.AveragePaceMinutesPerKilometre }).ToListAsync(cancellationToken),
            UserSettings = await context.UserSettings.AsNoTracking().Select(x => new UserSettingBackupDto
            { Id = x.Id, Key = x.Key, Value = x.Value, UpdatedAtUtc = DateTime.SpecifyKind(x.UpdatedAtUtc, DateTimeKind.Utc) }).ToListAsync(cancellationToken)
        };

        var sessions = await context.WorkoutSessions.AsNoTracking().Include(x => x.Exercises).ThenInclude(x => x.Sets).ToListAsync(cancellationToken);
        document.WorkoutSessions = sessions.Select(x => new WorkoutSessionBackupDto
        {
            Id = x.Id, TemplateId = x.TemplateId, TemplateName = x.TemplateName, StartedAtUtc = DateTime.SpecifyKind(x.StartedAtUtc, DateTimeKind.Utc),
            CompletedAtUtc = x.CompletedAtUtc is null ? null : DateTime.SpecifyKind(x.CompletedAtUtc.Value, DateTimeKind.Utc), IsActive = x.IsActive, Notes = x.Notes,
            Exercises = x.Exercises.OrderBy(e => e.SortOrder).Select(e => new WorkoutExerciseBackupDto
            {
                Id = e.Id, ExerciseId = e.ExerciseId, ExerciseName = e.ExerciseName, PrimaryMuscleGroup = e.PrimaryMuscleGroup,
                EquipmentType = e.EquipmentType, WeightEntryConvention = e.WeightEntryConvention,
                TargetMinimumRepetitions = e.TargetMinimumRepetitions, TargetMaximumRepetitions = e.TargetMaximumRepetitions,
                PlannedSetCount = e.PlannedSetCount, SortOrder = e.SortOrder,
                Sets = e.Sets.OrderBy(s => s.SetNumber).Select(s => new WorkoutSetBackupDto
                { Id = s.Id, SetNumber = s.SetNumber, Status = s.Status, WeightKilograms = s.WeightKilograms,
                  Repetitions = s.Repetitions, Rpe = s.Rpe, Difficulty = s.Difficulty, Notes = s.Notes, RecordedAtUtc = s.RecordedAtUtc is null ? null : DateTime.SpecifyKind(s.RecordedAtUtc.Value, DateTimeKind.Utc) }).ToList()
            }).ToList()
        }).ToList();

        var recommendations = await context.Recommendations.AsNoTracking().Include(x => x.Outcome).ToListAsync(cancellationToken);
        document.Recommendations = recommendations.Select(x => new RecommendationBackupDto
        {
            Id = x.Id, WorkoutExerciseId = x.WorkoutExerciseId, ProposedWeightKilograms = x.ProposedWeightKilograms,
            ProposedMinimumRepetitions = x.ProposedMinimumRepetitions, ProposedMaximumRepetitions = x.ProposedMaximumRepetitions,
            ProposedSetCount = x.ProposedSetCount, Explanation = x.Explanation, Confidence = x.Confidence,
            Status = x.Status, CreatedAtUtc = DateTime.SpecifyKind(x.CreatedAtUtc, DateTimeKind.Utc),
            Outcome = x.Outcome is null ? null : new RecommendationOutcomeBackupDto
            { Id = x.Outcome.Id, OutcomeType = x.Outcome.OutcomeType, AppliedWeightKilograms = x.Outcome.AppliedWeightKilograms,
              AppliedMinimumRepetitions = x.Outcome.AppliedMinimumRepetitions, AppliedMaximumRepetitions = x.Outcome.AppliedMaximumRepetitions,
              AppliedSetCount = x.Outcome.AppliedSetCount, RecordedAtUtc = DateTime.SpecifyKind(x.Outcome.RecordedAtUtc, DateTimeKind.Utc) }
        }).ToList();

        var metadata = await context.BackupMetadata.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        document.BackupMetadata = metadata is null ? new() : new BackupMetadataDto
        { Id = metadata.Id, SchemaVersion = metadata.SchemaVersion, LastExportedAtUtc = metadata.LastExportedAtUtc,
          LastImportedAtUtc = metadata.LastImportedAtUtc, LastBackupFileName = metadata.LastBackupFileName,
          LastBackupFileIdentity = metadata.LastBackupFileIdentity };
        return document;
    }

    public static void AddReplacement(GymTrackerDbContext context, GymTrackerBackupDocument d)
    {
        context.Exercises.AddRange(d.Exercises.Select(x => new Exercise { Id=x.Id, Name=x.Name, PrimaryMuscleGroup=x.PrimaryMuscleGroup, EquipmentType=x.EquipmentType, WeightEntryConvention=x.WeightEntryConvention, DefaultMinimumRepetitions=x.DefaultMinimumRepetitions, DefaultMaximumRepetitions=x.DefaultMaximumRepetitions, DefaultSetCount=x.DefaultSetCount, ExerciseMode=x.ExerciseMode }));
        context.WorkoutTemplates.AddRange(d.WorkoutTemplates.Select(x => new WorkoutTemplate { Id=x.Id, Name=x.Name, IsBuiltIn=x.IsBuiltIn }));
        context.TemplateExercises.AddRange(d.TemplateExercises.Select(x => new TemplateExercise { Id=x.Id, WorkoutTemplateId=x.WorkoutTemplateId, ExerciseId=x.ExerciseId, SortOrder=x.SortOrder, TargetMinimumRepetitions=x.TargetMinimumRepetitions, TargetMaximumRepetitions=x.TargetMaximumRepetitions, PlannedSetCount=x.PlannedSetCount }));
        foreach (var s in d.WorkoutSessions)
        {
            var session = new WorkoutSession { Id=s.Id, TemplateId=s.TemplateId, TemplateName=s.TemplateName, StartedAtUtc=s.StartedAtUtc, CompletedAtUtc=s.CompletedAtUtc, IsActive=s.IsActive, Notes=s.Notes };
            foreach (var e in s.Exercises)
            {
                var exercise = new WorkoutExercise { Id=e.Id, WorkoutSessionId=s.Id, ExerciseId=e.ExerciseId, ExerciseName=e.ExerciseName, PrimaryMuscleGroup=e.PrimaryMuscleGroup, EquipmentType=e.EquipmentType, WeightEntryConvention=e.WeightEntryConvention, TargetMinimumRepetitions=e.TargetMinimumRepetitions, TargetMaximumRepetitions=e.TargetMaximumRepetitions, PlannedSetCount=e.PlannedSetCount, SortOrder=e.SortOrder };
                exercise.Sets = e.Sets.Select(x => new WorkoutSet { Id=x.Id, WorkoutExerciseId=e.Id, SetNumber=x.SetNumber, Status=x.Status, WeightKilograms=x.WeightKilograms, Repetitions=x.Repetitions, Rpe=x.Rpe, Difficulty=x.Difficulty, Notes=x.Notes, RecordedAtUtc=x.RecordedAtUtc }).ToList();
                session.Exercises.Add(exercise);
            }
            context.WorkoutSessions.Add(session);
        }
        context.ActivityRecords.AddRange(d.ActivityRecords.Select(x => new ActivityRecord { Id=x.Id, ActivityDateUtc=x.ActivityDateUtc, ActivityType=x.ActivityType, DurationMinutes=x.DurationMinutes, DistanceKilometres=x.DistanceKilometres, Steps=x.Steps, PoolLengthMetres=x.PoolLengthMetres, PoolLengths=x.PoolLengths, Notes=x.Notes, AveragePaceMinutesPerKilometre=x.AveragePaceMinutesPerKilometre }));
        context.Recommendations.AddRange(d.Recommendations.Select(x => new Recommendation { Id=x.Id, WorkoutExerciseId=x.WorkoutExerciseId, ProposedWeightKilograms=x.ProposedWeightKilograms, ProposedMinimumRepetitions=x.ProposedMinimumRepetitions, ProposedMaximumRepetitions=x.ProposedMaximumRepetitions, ProposedSetCount=x.ProposedSetCount, Explanation=x.Explanation, Confidence=x.Confidence, Status=x.Status, CreatedAtUtc=x.CreatedAtUtc, Outcome=x.Outcome is null ? null : new RecommendationOutcome { Id=x.Outcome.Id, OutcomeType=x.Outcome.OutcomeType, AppliedWeightKilograms=x.Outcome.AppliedWeightKilograms, AppliedMinimumRepetitions=x.Outcome.AppliedMinimumRepetitions, AppliedMaximumRepetitions=x.Outcome.AppliedMaximumRepetitions, AppliedSetCount=x.Outcome.AppliedSetCount, RecordedAtUtc=x.Outcome.RecordedAtUtc } }));
        context.UserSettings.AddRange(d.UserSettings.Select(x => new UserSetting { Id=x.Id, Key=x.Key, Value=x.Value, UpdatedAtUtc=x.UpdatedAtUtc }));
        context.BackupMetadata.Add(new BackupMetadata { Id=d.BackupMetadata.Id, SchemaVersion=d.BackupMetadata.SchemaVersion, LastExportedAtUtc=d.BackupMetadata.LastExportedAtUtc, LastImportedAtUtc=d.BackupMetadata.LastImportedAtUtc, LastBackupFileName=d.BackupMetadata.LastBackupFileName, LastBackupFileIdentity=d.BackupMetadata.LastBackupFileIdentity });
    }
}
