using Microsoft.Maui.Controls;

namespace GymTrackerMobile.UI;

public abstract class DestinationPage : ContentPage
{
    private readonly Grid _layout;

    protected DestinationPage(DestinationPageDescriptor descriptor)
    {
        Title = descriptor.Title;
        _layout = new Grid
        {
            Padding = 24,
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Star) }
        };
        _layout.Add(new Label { Text = descriptor.Title, Style = (Style)Application.Current!.Resources["PageTitleLabel"] }, 0, 0);
        _layout.Add(new Border
        {
            Style = (Style)Application.Current!.Resources["EmptyStateCard"],
            Content = new Label
            {
                Text = descriptor.EmptyStateMessage,
                Style = (Style)Application.Current!.Resources["EmptyStateLabel"]
            }
        }, 0, 1);
        Content = _layout;
    }

    protected void AddAdditionalContent(View view)
    {
        _layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        _layout.Add(view, 0, _layout.RowDefinitions.Count - 1);
    }
}

public sealed class HistoryPage() : DestinationPage(DestinationPageContent.History);

public sealed class ExerciseProgressPage() : DestinationPage(DestinationPageContent.ExerciseProgress);

public sealed class BackupSettingsPage : DestinationPage
{
    private readonly DeveloperResetViewModel _viewModel;

    public BackupSettingsPage(DeveloperResetViewModel viewModel) : base(DestinationPageContent.BackupSettings)
    {
        _viewModel = viewModel;
#if DEBUG
        AddDeveloperTools();
#endif
    }

#if DEBUG
    private void AddDeveloperTools()
    {
        var resetButton = new Button
        {
            Text = "Reset local app data",
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#B42318"),
            CornerRadius = 8,
            Margin = new Thickness(0, 16, 0, 0)
        };
        resetButton.Clicked += async (_, _) =>
        {
            resetButton.IsEnabled = false;
            await _viewModel.ResetAsync(() => DisplayAlertAsync(
                "Reset local app data?",
                "This permanently deletes active workouts, workout history, activities, settings, and backup metadata, then restores the built-in templates and exercises.",
                "Reset data",
                "Cancel"));
            resetButton.IsEnabled = true;

            if (_viewModel.StatusMessage is not null)
                await DisplayAlertAsync("Reset complete", _viewModel.StatusMessage, "OK");
            else if (_viewModel.ErrorMessage is not null)
                await DisplayAlertAsync("Reset failed", _viewModel.ErrorMessage, "OK");
        };

        AddAdditionalContent(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label { Text = "Developer tools", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#B42318") },
                new Label { Text = "Debug builds only. Use this to reset local data between test runs.", TextColor = Color.FromArgb("#687A95") },
                resetButton
            }
        });
    }
#endif
}
