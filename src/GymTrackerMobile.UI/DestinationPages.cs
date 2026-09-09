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

public sealed class HistoryPage : ContentPage
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private readonly HistoryViewModel _viewModel;
    private readonly VerticalStackLayout _items = new() { Spacing = 12 };

    public HistoryPage(HistoryViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "History";
        BackgroundColor = Color.FromArgb("#F8FBFF");
        var layout = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(76) } };
        layout.Add(new ScrollView { Content = new VerticalStackLayout { Padding = new Thickness(20, 22, 20, 28), Spacing = 16, Children = { new Label { Text = "History", FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = "Review your completed workouts.", FontSize = 18, TextColor = Muted }, _items } } }, 0, 0);
        layout.Add(BuildBottomNavigation(), 0, 1);
        Content = layout;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        _items.Children.Clear();
        if (_viewModel.Items.Count == 0)
        {
            _items.Children.Add(new Border { BackgroundColor = Colors.White, StrokeThickness = 0, Padding = 20, Content = new Label { Text = "Your completed workouts will appear here.", TextColor = Muted, FontSize = 16 } });
            return;
        }
        foreach (var item in _viewModel.Items) _items.Children.Add(BuildItem(item));
    }

    private static View BuildItem(HistoryWorkoutItem item)
    {
        var card = new Border { BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = new Thickness(16, 14), StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 } };
        card.Content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(90) },
            Children =
            {
                new VerticalStackLayout { Spacing = 4, Children = { new Label { Text = item.Name, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = item.CompletedAtLocal.ToString("ddd, dd MMM yyyy · HH:mm"), FontSize = 15, TextColor = Muted }, new Label { Text = $"{item.CompletedSets} / {item.PlannedSets} sets", FontSize = 15, TextColor = Teal } } },
                new Label { Text = "›", FontSize = 34, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center }
            }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await Shell.Current.GoToAsync($"{NavigationRoutes.WorkoutSummary}?sessionId={item.Id}");
        card.GestureRecognizers.Add(tap);
        return card;
    }

    private static View BuildBottomNavigation() => new Grid
    {
        BackgroundColor = Colors.White, Padding = new Thickness(12, 9),
        ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) },
        Children = { Nav("dashboard_home.svg", "Home", false, 0), Nav("dashboard_progress.svg", "Progress", false, 1), Nav("dashboard_history.svg", "History", true, 2), Nav("dashboard_more.svg", "More", false, 3) }
    };

    private static View Nav(string icon, string label, bool selected, int column)
    {
        var item = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Image { Source = icon, WidthRequest = 27, HeightRequest = 27, Opacity = selected ? 1 : 0.75 }, new Label { Text = label, FontSize = 13, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } };
        Grid.SetColumn(item, column);
        return item;
    }
}

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
