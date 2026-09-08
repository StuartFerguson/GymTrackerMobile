using GymTrackerMobile.Domain;

namespace GymTrackerMobile.Persistence;

public interface IBackupMetadataRepository
{
    Task<BackupMetadata> GetAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(BackupMetadata metadata, CancellationToken cancellationToken = default);
}
