namespace GymTrackerMobile.Persistence;

public enum BackupImportMode { Replace, Merge }

public sealed record BackupExportResult(string FileName, int RecordCount, string? FilePath = null);
public sealed record BackupImportValidation(bool IsValid, string? ErrorMessage, int RecordCount);
public sealed record BackupImportResult(string Message);

public interface IBackupService
{
    Task<BackupExportResult> ExportAsync(CancellationToken cancellationToken = default);
    Task<BackupImportValidation> ValidateImportAsync(Stream backup, CancellationToken cancellationToken = default);
    Task<BackupImportResult> ImportAsync(Stream backup, BackupImportMode mode, CancellationToken cancellationToken = default);
}
