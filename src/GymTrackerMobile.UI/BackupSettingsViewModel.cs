using System.ComponentModel;
using GymTrackerMobile.Persistence.Backup;

namespace GymTrackerMobile.UI;

public sealed class BackupSettingsViewModel(IBackupService backups, IBackupFileTransfer files) : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task ExportAsync(CancellationToken cancellationToken = default)
    {
        ClearMessages();
        try
        {
            var path = await files.SaveBackupAsync(await backups.ExportAsync(cancellationToken), $"gym-tracker-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json", cancellationToken);
            if (path is not null) SetStatus($"Backup exported to {path}.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex) { SetError(ex.Message); }
    }

    public async Task ImportAsync(BackupImportMode mode, Func<Task<bool>> confirm, CancellationToken cancellationToken = default)
    {
        ClearMessages();
        try
        {
            var file = await files.PickBackupAsync(cancellationToken);
            if (file is null) return;
            var validation = await backups.ValidateAsync(file.Content, cancellationToken);
            if (!validation.IsValid)
            {
                SetError(string.Join(Environment.NewLine, validation.Errors.Select(x => $"{x.Path}: {x.Message}")));
                return;
            }
            if (!await confirm()) return;
            await backups.ImportAsync(file.Content, mode, cancellationToken);
            SetStatus(mode == BackupImportMode.Replace ? "Backup restored." : "Backup merged.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex) { SetError(ex.Message); }
    }

    private void ClearMessages() { StatusMessage = null; ErrorMessage = null; Notify(nameof(StatusMessage)); Notify(nameof(ErrorMessage)); }
    private void SetStatus(string value) { StatusMessage = value; Notify(nameof(StatusMessage)); }
    private void SetError(string value) { ErrorMessage = value; Notify(nameof(ErrorMessage)); }
    private void Notify(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
