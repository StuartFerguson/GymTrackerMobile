using System.ComponentModel;
using System.Runtime.CompilerServices;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class BackupSettingsViewModel(IBackupService backupService, ISettingsRepository settings) : INotifyPropertyChanged
{
    private string? _statusMessage;
    private string? _errorMessage;
    private string _unitLabel = "Kilograms (kg)";

    public event PropertyChangedEventHandler? PropertyChanged;
    public string? StatusMessage { get => _statusMessage; private set => Set(ref _statusMessage, value); }
    public string? ErrorMessage { get => _errorMessage; private set => Set(ref _errorMessage, value); }
    public string UnitLabel { get => _unitLabel; private set => Set(ref _unitLabel, value); }
    public string ImportConfirmationMessage { get; private set; } = string.Empty;
    public string? LastExportPath { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var units = await settings.GetSettingAsync("units", cancellationToken);
            UnitLabel = string.Equals(units, "kg", StringComparison.OrdinalIgnoreCase) ? "Kilograms (kg)" : "Kilograms (kg)";
            ErrorMessage = null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException) { ErrorMessage = exception.Message; }
    }

    public async Task<bool> ExportAsync(CancellationToken cancellationToken = default)
    {
        ClearMessages();
        try
        {
            var result = await backupService.ExportAsync(cancellationToken);
            LastExportPath = result.FilePath;
            StatusMessage = $"Backup exported to {result.FileName}.";
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException) { ErrorMessage = exception.Message; return false; }
    }

    public async Task<bool> ImportAsync(Stream backup, Func<string, Task<bool>> confirm, BackupImportMode mode = BackupImportMode.Replace, CancellationToken cancellationToken = default)
    {
        ClearMessages();
        try
        {
            var validation = await backupService.ValidateImportAsync(backup, cancellationToken);
            if (!validation.IsValid) { ErrorMessage = validation.ErrorMessage ?? "The backup file is invalid."; return false; }
            ImportConfirmationMessage = mode == BackupImportMode.Replace
                ? $"This will replace existing workouts, activities, settings, and backup metadata with {validation.RecordCount} imported records."
                : $"This will merge {validation.RecordCount} imported records into your existing data. Existing records will be kept.";
            if (!await confirm(ImportConfirmationMessage)) return false;
            var result = await backupService.ImportAsync(backup, mode, cancellationToken);
            StatusMessage = result.Message;
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException) { ErrorMessage = exception.Message; return false; }
    }

    private void ClearMessages() { StatusMessage = null; ErrorMessage = null; }
    private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
