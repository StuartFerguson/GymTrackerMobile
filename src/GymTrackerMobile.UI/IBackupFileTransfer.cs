namespace GymTrackerMobile.UI;

public sealed record BackupFile(string Name, string Content);

public interface IBackupFileTransfer
{
    Task<BackupFile?> PickBackupAsync(CancellationToken cancellationToken = default);
    Task<string?> SaveBackupAsync(string json, string suggestedName, CancellationToken cancellationToken = default);
}
