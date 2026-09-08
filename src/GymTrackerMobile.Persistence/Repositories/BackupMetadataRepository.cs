using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class BackupMetadataRepository(GymTrackerDbContext context) : IBackupMetadataRepository
{
    public Task<BackupMetadata> GetAsync(CancellationToken cancellationToken = default) =>
        context.BackupMetadata.AsNoTracking().SingleAsync(cancellationToken);

    public async Task SaveAsync(BackupMetadata metadata, CancellationToken cancellationToken = default)
    {
        var existing = await context.BackupMetadata.FindAsync([metadata.Id], cancellationToken);
        if (existing is null) context.BackupMetadata.Add(metadata);
        else context.Entry(existing).CurrentValues.SetValues(metadata);
        await context.SaveChangesAsync(cancellationToken);
    }
}
