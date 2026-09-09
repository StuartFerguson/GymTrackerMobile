using System.ComponentModel;
using System.Runtime.CompilerServices;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class DeveloperResetViewModel(IAppDataResetService resetService) : INotifyPropertyChanged
{
    private string? _statusMessage;
    private string? _errorMessage;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string? StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public async Task<bool> ResetAsync(Func<Task<bool>> confirm, CancellationToken cancellationToken = default)
    {
        StatusMessage = null;
        ErrorMessage = null;
        if (!await confirm()) return false;

        try
        {
            await resetService.ResetAsync(cancellationToken);
            StatusMessage = "Local app data was reset.";
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            ErrorMessage = exception.Message;
            return false;
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
