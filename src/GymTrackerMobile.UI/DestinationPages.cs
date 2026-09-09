using Microsoft.Maui.Controls;

namespace GymTrackerMobile.UI;

public abstract class DestinationPage : ContentPage
{
    protected DestinationPage(DestinationPageDescriptor descriptor)
    {
        Title = descriptor.Title;
        var layout = new Grid
        {
            Padding = 24,
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Star) }
        };
        layout.Add(new Label { Text = descriptor.Title, Style = (Style)Application.Current!.Resources["PageTitleLabel"] }, 0, 0);
        layout.Add(new Border
        {
            Style = (Style)Application.Current!.Resources["EmptyStateCard"],
            Content = new Label
            {
                Text = descriptor.EmptyStateMessage,
                Style = (Style)Application.Current!.Resources["EmptyStateLabel"]
            }
        }, 0, 1);
        Content = layout;
    }
}

public sealed class HistoryPage() : DestinationPage(DestinationPageContent.History);

public sealed class ExerciseProgressPage() : DestinationPage(DestinationPageContent.ExerciseProgress);

public sealed class BackupSettingsPage() : DestinationPage(DestinationPageContent.BackupSettings);
