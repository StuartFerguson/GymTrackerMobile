namespace GymTrackerMobile.Persistence.Backup;

public enum BackupImportMode { Replace, Merge }

public sealed record BackupImportResult(string? RecoveryFilePath = null);

public interface IBackupService
{
    Task<string> ExportAsync(CancellationToken cancellationToken = default);
    Task<BackupValidationResult> ValidateAsync(string json, CancellationToken cancellationToken = default);
    Task<BackupImportResult> ImportAsync(string json, BackupImportMode mode, CancellationToken cancellationToken = default);
}
