using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Persistence.Backup;

public sealed class GymTrackerBackupDocument
{
    public int SchemaVersion { get; set; }
    public DateTime ExportedAtUtc { get; set; }
    public string Application { get; set; } = "GymTrackerMobile";
    public List<ExerciseBackupDto> Exercises { get; set; } = [];
    public List<WorkoutTemplateBackupDto> WorkoutTemplates { get; set; } = [];
    public List<TemplateExerciseBackupDto> TemplateExercises { get; set; } = [];
    public List<WorkoutSessionBackupDto> WorkoutSessions { get; set; } = [];
    public List<ActivityRecordBackupDto> ActivityRecords { get; set; } = [];
    public List<RecommendationBackupDto> Recommendations { get; set; } = [];
    public List<UserSettingBackupDto> UserSettings { get; set; } = [];
    public BackupMetadataDto BackupMetadata { get; set; } = new();
}

public sealed class ExerciseBackupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PrimaryMuscleGroup { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public WeightEntryConvention WeightEntryConvention { get; set; }
    public int DefaultMinimumRepetitions { get; set; }
    public int DefaultMaximumRepetitions { get; set; }
    public int DefaultSetCount { get; set; }
    public ExerciseMode ExerciseMode { get; set; }
}

public sealed class WorkoutTemplateBackupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; }
}

public sealed class TemplateExerciseBackupDto
{
    public Guid Id { get; set; }
    public Guid WorkoutTemplateId { get; set; }
    public Guid ExerciseId { get; set; }
    public int SortOrder { get; set; }
    public int TargetMinimumRepetitions { get; set; }
    public int TargetMaximumRepetitions { get; set; }
    public int PlannedSetCount { get; set; }
}

public sealed class WorkoutSessionBackupDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public List<WorkoutExerciseBackupDto> Exercises { get; set; } = [];
}

public sealed class WorkoutExerciseBackupDto
{
    public Guid Id { get; set; }
    public Guid ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string PrimaryMuscleGroup { get; set; } = string.Empty;
    public string EquipmentType { get; set; } = string.Empty;
    public WeightEntryConvention WeightEntryConvention { get; set; }
    public int TargetMinimumRepetitions { get; set; }
    public int TargetMaximumRepetitions { get; set; }
    public int PlannedSetCount { get; set; }
    public int SortOrder { get; set; }
    public List<WorkoutSetBackupDto> Sets { get; set; } = [];
}

public sealed class WorkoutSetBackupDto
{
    public Guid Id { get; set; }
    public int SetNumber { get; set; }
    public SetStatus Status { get; set; }
    public double? WeightKilograms { get; set; }
    public int? Repetitions { get; set; }
    public int? Rpe { get; set; }
    public string? Difficulty { get; set; }
    public string? Notes { get; set; }
    public DateTime? RecordedAtUtc { get; set; }
}

public sealed class ActivityRecordBackupDto
{
    public Guid Id { get; set; }
    public DateTime ActivityDateUtc { get; set; }
    public ActivityType ActivityType { get; set; }
    public int? DurationMinutes { get; set; }
    public double? DistanceKilometres { get; set; }
    public int? Steps { get; set; }
    public int? PoolLengthMetres { get; set; }
    public int? PoolLengths { get; set; }
    public string? Notes { get; set; }
    public double? AveragePaceMinutesPerKilometre { get; set; }
}

public sealed class RecommendationBackupDto
{
    public Guid Id { get; set; }
    public Guid WorkoutExerciseId { get; set; }
    public double? ProposedWeightKilograms { get; set; }
    public int? ProposedMinimumRepetitions { get; set; }
    public int? ProposedMaximumRepetitions { get; set; }
    public int? ProposedSetCount { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public RecommendationStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public RecommendationOutcomeBackupDto? Outcome { get; set; }
}

public sealed class RecommendationOutcomeBackupDto
{
    public Guid Id { get; set; }
    public RecommendationOutcomeType OutcomeType { get; set; }
    public double? AppliedWeightKilograms { get; set; }
    public int? AppliedMinimumRepetitions { get; set; }
    public int? AppliedMaximumRepetitions { get; set; }
    public int? AppliedSetCount { get; set; }
    public DateTime RecordedAtUtc { get; set; }
}

public sealed class UserSettingBackupDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class BackupMetadataDto
{
    public Guid Id { get; set; }
    public int SchemaVersion { get; set; }
    public DateTime? LastExportedAtUtc { get; set; }
    public DateTime? LastImportedAtUtc { get; set; }
    public string? LastBackupFileName { get; set; }
    public string? LastBackupFileIdentity { get; set; }
}
