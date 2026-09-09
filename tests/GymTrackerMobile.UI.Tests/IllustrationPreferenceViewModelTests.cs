using GymTrackerMobile.Persistence;
using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class IllustrationPreferenceViewModelTests
{
    [Fact]
    public async Task Missing_setting_defaults_to_neutral()
    {
        var settings = new RecordingSettingsRepository();
        var viewModel = new IllustrationPreferenceViewModel(settings);

        await viewModel.LoadAsync();

        Assert.Equal(IllustrationStyle.Neutral, viewModel.SelectedStyle);
    }

    [Fact]
    public async Task Invalid_setting_defaults_to_neutral()
    {
        var settings = new RecordingSettingsRepository { Value = "not-a-style" };
        var viewModel = new IllustrationPreferenceViewModel(settings);

        await viewModel.LoadAsync();

        Assert.Equal(IllustrationStyle.Neutral, viewModel.SelectedStyle);
    }

    [Fact]
    public async Task Stored_setting_is_loaded_and_selection_is_saved()
    {
        var settings = new RecordingSettingsRepository { Value = "Male" };
        var viewModel = new IllustrationPreferenceViewModel(settings);

        await viewModel.LoadAsync();
        await viewModel.SelectAsync(IllustrationStyle.Female);

        Assert.Equal(IllustrationStyle.Female, viewModel.SelectedStyle);
        Assert.Equal("Female", settings.SavedValue);
    }

    [Fact]
    public async Task Save_failure_keeps_in_memory_selection_and_exposes_error()
    {
        var settings = new RecordingSettingsRepository { Error = new InvalidOperationException("Settings unavailable") };
        var viewModel = new IllustrationPreferenceViewModel(settings);

        await viewModel.SelectAsync(IllustrationStyle.Male);

        Assert.Equal(IllustrationStyle.Male, viewModel.SelectedStyle);
        Assert.Equal("Settings unavailable", viewModel.ErrorMessage);
    }

    private sealed class RecordingSettingsRepository : ISettingsRepository
    {
        public string? Value { get; set; }
        public string? SavedValue { get; private set; }
        public Exception? Error { get; set; }

        public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) => Task.FromResult(Value);

        public Task SetSettingAsync(string key, string value, CancellationToken cancellationToken = default)
        {
            if (Error is not null) throw Error;
            SavedValue = value;
            Value = value;
            return Task.CompletedTask;
        }
    }
}
