namespace GymTrackerMobile.Persistence.Backup;

public sealed record BackupValidationError(string Path, string Message);

public sealed class BackupValidationResult
{
    public GymTrackerBackupDocument? Document { get; init; }
    public IReadOnlyList<BackupValidationError> Errors { get; init; } = [];
    public bool IsValid => Errors.Count == 0 && Document is not null;
}
