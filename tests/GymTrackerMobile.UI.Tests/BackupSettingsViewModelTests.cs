using GymTrackerMobile.Persistence.Backup;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class BackupSettingsViewModelTests
{
    [Fact]
    public async Task Export_cancel_does_not_save_or_report_success()
    {
        var transfer = new RecordingTransfer { SavePath = null };
        var viewModel = new BackupSettingsViewModel(new RecordingBackupService(), transfer);

        await viewModel.ExportAsync();

        Assert.Null(viewModel.StatusMessage);
        Assert.Equal(1, transfer.SaveCalls);
    }

    [Fact]
    public async Task Import_cancel_does_not_confirm_or_mutate()
    {
        var transfer = new RecordingTransfer { Picked = null };
        var service = new RecordingBackupService();
        var viewModel = new BackupSettingsViewModel(service, transfer);

        await viewModel.ImportAsync(BackupImportMode.Replace, () => Task.FromResult(true));

        Assert.Equal(0, service.ImportCalls);
    }

    [Fact]
    public async Task Import_declined_does_not_mutate()
    {
        var transfer = new RecordingTransfer { Picked = new BackupFile("backup.json", "{}") };
        var service = new RecordingBackupService();
        var viewModel = new BackupSettingsViewModel(service, transfer);

        await viewModel.ImportAsync(BackupImportMode.Replace, () => Task.FromResult(false));

        Assert.Equal(0, service.ImportCalls);
    }

    private sealed class RecordingBackupService : IBackupService
    {
        public int ImportCalls { get; private set; }
        public Task<string> ExportAsync(CancellationToken cancellationToken = default) => Task.FromResult("json");
        public Task<BackupValidationResult> ValidateAsync(string json, CancellationToken cancellationToken = default) => Task.FromResult(new BackupValidationResult());
        public Task<BackupImportResult> ImportAsync(string json, BackupImportMode mode, CancellationToken cancellationToken = default)
        { ImportCalls++; return Task.FromResult(new BackupImportResult()); }
    }

    private sealed class RecordingTransfer : IBackupFileTransfer
    {
        public BackupFile? Picked { get; set; }
        public string? SavePath { get; set; }
        public int SaveCalls { get; private set; }
        public Task<BackupFile?> PickBackupAsync(CancellationToken cancellationToken = default) => Task.FromResult(Picked);
        public Task<string?> SaveBackupAsync(string json, string suggestedName, CancellationToken cancellationToken = default)
        { SaveCalls++; return Task.FromResult(SavePath); }
    }
}
