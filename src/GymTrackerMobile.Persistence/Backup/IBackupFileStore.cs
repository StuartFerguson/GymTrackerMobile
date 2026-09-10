namespace GymTrackerMobile.Persistence.Backup;

public interface IBackupFileStore
{
    Task<string> SaveRecoveryCopyAsync(string json, CancellationToken cancellationToken = default);
}

public sealed class LocalBackupFileStore(string? directory = null) : IBackupFileStore
{
    private readonly string _directory = directory ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "GymTrackerMobile", "backups");

    public async Task<string> SaveRecoveryCopyAsync(string json, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_directory);
        var path = Path.Combine(_directory, $"gym-tracker-recovery-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}.json");
        await File.WriteAllTextAsync(path, json, cancellationToken);
        return path;
    }
}
