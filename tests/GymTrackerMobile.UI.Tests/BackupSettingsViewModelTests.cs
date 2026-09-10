using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class BackupSettingsViewModelTests
{
    [Fact]
    public async Task Export_reports_successful_backup()
    {
        var service = new RecordingBackupService { ExportResult = new("backup.json", 4) };
        var viewModel = new BackupSettingsViewModel(service, new RecordingSettingsRepository());

        var result = await viewModel.ExportAsync();

        Assert.True(result);
        Assert.Equal("Backup exported to backup.json.", viewModel.StatusMessage);
        Assert.Null(viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Export_reports_failure()
    {
        var service = new RecordingBackupService { ExportError = new InvalidOperationException("Export failed") };
        var viewModel = new BackupSettingsViewModel(service, new RecordingSettingsRepository());

        var result = await viewModel.ExportAsync();

        Assert.False(result);
        Assert.Equal("Export failed", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Invalid_import_does_not_request_confirmation_or_import()
    {
        var service = new RecordingBackupService { Validation = new(false, "Invalid backup file.", 0) };
        var viewModel = new BackupSettingsViewModel(service, new RecordingSettingsRepository());

        var result = await viewModel.ImportAsync(Stream.Null, _ => Task.FromResult(true));

        Assert.False(result);
        Assert.Equal("Invalid backup file.", viewModel.ErrorMessage);
        Assert.False(service.ImportCalled);
    }

    [Fact]
    public async Task Replacement_import_requires_confirmation()
    {
        var service = new RecordingBackupService { Validation = new(true, null, 5), ImportResult = new("Replaced 5 records.") };
        var viewModel = new BackupSettingsViewModel(service, new RecordingSettingsRepository());

        var result = await viewModel.ImportAsync(Stream.Null, _ => Task.FromResult(false));

        Assert.False(result);
        Assert.False(service.ImportCalled);
    }

    [Fact]
    public async Task Confirmed_replacement_import_reports_completion()
    {
        var service = new RecordingBackupService { Validation = new(true, null, 5), ImportResult = new("Replaced 5 records.") };
        var viewModel = new BackupSettingsViewModel(service, new RecordingSettingsRepository());

        var result = await viewModel.ImportAsync(Stream.Null, _ => Task.FromResult(true));

        Assert.True(result);
        Assert.True(service.ImportCalled);
        Assert.Equal("Replaced 5 records.", viewModel.StatusMessage);
    }

    private sealed class RecordingBackupService : IBackupService
    {
        public BackupExportResult? ExportResult { get; init; }
        public Exception? ExportError { get; init; }
        public BackupImportValidation Validation { get; init; } = new(true, null, 0);
        public BackupImportResult ImportResult { get; init; } = new("Imported.");
        public bool ImportCalled { get; private set; }

        public Task<BackupExportResult> ExportAsync(CancellationToken cancellationToken = default) =>
            ExportError is not null ? Task.FromException<BackupExportResult>(ExportError) : Task.FromResult(ExportResult!);

        public Task<BackupImportValidation> ValidateImportAsync(Stream backup, CancellationToken cancellationToken = default) =>
            Task.FromResult(Validation);

        public Task<BackupImportResult> ImportAsync(Stream backup, BackupImportMode mode, CancellationToken cancellationToken = default)
        {
            ImportCalled = true;
            return Task.FromResult(ImportResult);
        }
    }

    private sealed class RecordingSettingsRepository : ISettingsRepository
    {
        public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult<string?>("kg");
        public Task SetSettingAsync(string key, string value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
