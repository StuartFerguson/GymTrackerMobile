using System.ComponentModel;
using System.Runtime.CompilerServices;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class IllustrationPreferenceViewModel : INotifyPropertyChanged
{
    public const string SettingKey = "illustration-style";

    private readonly ISettingsRepository _settings;
    private IllustrationStyle _selectedStyle = IllustrationStyle.Neutral;
    private string? _errorMessage;

    public IllustrationPreferenceViewModel(ISettingsRepository settings) => _settings = settings;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IllustrationStyle SelectedStyle
    {
        get => _selectedStyle;
        private set
        {
            if (_selectedStyle == value) return;
            _selectedStyle = value;
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

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stored = await _settings.GetSettingAsync(SettingKey, cancellationToken);
            SelectedStyle = Enum.TryParse<IllustrationStyle>(stored, true, out var style) && IsValid(style)
                ? style
                : IllustrationStyle.Neutral;
            ErrorMessage = null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            SelectedStyle = IllustrationStyle.Neutral;
            ErrorMessage = exception.Message;
        }
    }

    public async Task SelectAsync(IllustrationStyle style, CancellationToken cancellationToken = default)
    {
        if (!IsValid(style)) style = IllustrationStyle.Neutral;
        SelectedStyle = style;
        ErrorMessage = null;

        try
        {
            await _settings.SetSettingAsync(SettingKey, style.ToString(), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            ErrorMessage = exception.Message;
        }
    }

    private static bool IsValid(IllustrationStyle style) =>
        style is IllustrationStyle.Female or IllustrationStyle.Male or IllustrationStyle.Neutral;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
