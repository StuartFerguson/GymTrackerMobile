using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;
using GymTrackerMobile.Persistence.Backup;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class BackupImportTests
{
    [Fact]
    public async Task Replacement_import_creates_recovery_copy_and_restores_document()
    {
        var recoveryDirectory = Path.Combine(Path.GetTempPath(), $"gym-backup-{Guid.NewGuid():N}");
        try
        {
            await using var context = await CreateContextAsync();
            context.UserSettings.Add(new UserSetting { Key = "units", Value = "kg", UpdatedAtUtc = DateTime.UtcNow });
            await context.SaveChangesAsync();
            var service = CreateService(context, recoveryDirectory);
            var json = await service.ExportAsync();
            var validation = await service.ValidateAsync(json);
            Assert.True(validation.IsValid, string.Join("; ", validation.Errors.Select(x => $"{x.Path}: {x.Message}")));

            var result = await service.ImportAsync(json, BackupImportMode.Replace);

            Assert.NotNull(result.RecoveryFilePath);
            Assert.True(File.Exists(result.RecoveryFilePath));
            Assert.Equal("kg", await context.UserSettings.Select(x => x.Value).SingleAsync());
            Assert.Equal(22, await context.Exercises.CountAsync());
        }
        finally { if (Directory.Exists(recoveryDirectory)) Directory.Delete(recoveryDirectory, true); }
    }

    [Fact]
    public async Task Invalid_import_does_not_mutate_current_data()
    {
        await using var context = await CreateContextAsync();
        context.UserSettings.Add(new UserSetting { Key = "units", Value = "kg", UpdatedAtUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var service = CreateService(context, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));

        await Assert.ThrowsAsync<BackupValidationException>(() => service.ImportAsync("{\"schemaVersion\":99}", BackupImportMode.Replace));

        Assert.Equal(22, await context.Exercises.CountAsync());
        Assert.Equal("kg", await context.UserSettings.Select(x => x.Value).SingleAsync());
    }

    [Fact]
    public async Task Merge_import_retains_existing_rows_not_in_document()
    {
        await using var context = await CreateContextAsync();
        var existing = new ActivityRecord { ActivityDateUtc = DateTime.UtcNow.Date, ActivityType = ActivityType.Walking, DurationMinutes = 20 };
        context.ActivityRecords.Add(existing);
        await context.SaveChangesAsync();
        var service = CreateService(context, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var json = await service.ExportAsync();
        var validation = await service.ValidateAsync(json);
        Assert.True(validation.IsValid, string.Join("; ", validation.Errors.Select(x => $"{x.Path}: {x.Message}")));

        await service.ImportAsync(json, BackupImportMode.Merge);

        Assert.True(await context.ActivityRecords.AnyAsync(x => x.Id == existing.Id));
    }

    private static IBackupService CreateService(GymTrackerDbContext context, string recoveryDirectory) =>
        new BackupService(context, new LocalBackupFileStore(recoveryDirectory), new GymTrackerBackupValidator(), new GymTrackerBackupMapper(context));

    private static async Task<GymTrackerDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<GymTrackerDbContext>().UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), $"gym-tracker-{Guid.NewGuid():N}.db")};Pooling=False").Options;
        var context = new GymTrackerDbContext(options);
        await new DatabaseInitializer(context).InitializeAsync();
        return context;
    }
}
