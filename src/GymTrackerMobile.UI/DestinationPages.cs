using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using GymTrackerMobile.Persistence.Backup;

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
    private readonly Label _stateMessage = new() { FontSize = 16, TextColor = Muted, IsVisible = false };
    private readonly Button _retry = new() { Text = "Try again", BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 22, IsVisible = false };

    public HistoryPage(HistoryViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "History";
        BackgroundColor = Color.FromArgb("#F8FBFF");
        var layout = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(76) } };
        _retry.Clicked += async (_, _) => await LoadAndRenderAsync();
        layout.Add(new ScrollView { Content = new VerticalStackLayout { Padding = new Thickness(20, 22, 20, 28), Spacing = 16, Children = { new Label { Text = "History", FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = "Review your completed workouts and activities.", FontSize = 18, TextColor = Muted }, _stateMessage, _retry, _items } } }, 0, 0);
        layout.Add(BuildBottomNavigation(), 0, 1);
        Content = layout;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAndRenderAsync();
    }

    private async Task LoadAndRenderAsync()
    {
        _retry.IsVisible = false;
        _stateMessage.IsVisible = true;
        _stateMessage.Text = "Loading history…";
        await _viewModel.LoadAsync();
        _items.Children.Clear();
        if (_viewModel.ErrorMessage is not null)
        {
            _stateMessage.Text = _viewModel.ErrorMessage;
            _retry.IsVisible = true;
            return;
        }

        if (_viewModel.Items.Count == 0)
        {
            _stateMessage.Text = "Your completed workouts and activities will appear here.";
            return;
        }
        _stateMessage.IsVisible = false;
        for (var index = 0; index < _viewModel.Items.Count; index++) _items.Children.Add(BuildItem(_viewModel.Items[index], index));
    }

    private static View BuildItem(HistoryItem item, int index)
    {
        var accent = item.IsWorkout ? Teal : Color.FromArgb("#F28C38");
        var iconBackground = item.IsWorkout ? Color.FromArgb("#E8F8F7") : Color.FromArgb("#FFF1E8");
        var icon = new Border
        {
            BackgroundColor = iconBackground,
            StrokeThickness = 0,
            WidthRequest = 54,
            HeightRequest = 54,
            StrokeShape = new RoundRectangle { CornerRadius = 27 },
            Content = new Image
            {
                Source = item.IsWorkout ? "dashboard_dumbbell.svg" : ActivityIcon(item.Name),
                WidthRequest = 32,
                HeightRequest = 32,
                Aspect = Aspect.AspectFit,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            }
        };
        var date = new Label
        {
            Text = item.OccurredAtLocal.ToString("dd MMM yyyy"),
            FontSize = 13,
            TextColor = Muted,
            HorizontalTextAlignment = TextAlignment.End,
            VerticalTextAlignment = TextAlignment.Center
        };
        var category = new Label { Text = item.IsWorkout ? "WORKOUT" : "ACTIVITY", FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = accent };
        var name = new Label { AutomationId = UiAutomationIds.HistoryName(index), Text = item.Name, FontSize = 19, FontAttributes = FontAttributes.None, TextColor = Ink };
        var itemDetails = new Label { AutomationId = UiAutomationIds.HistoryDetails(index), Text = item.Details, FontSize = 14, TextColor = accent };
        var notes = new Label { Text = item.Notes, FontSize = 13, TextColor = Muted, IsVisible = !string.IsNullOrWhiteSpace(item.Notes), LineBreakMode = LineBreakMode.WordWrap };
        var details = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(92) },
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto), new(GridLength.Auto) },
            RowSpacing = 3,
            ColumnSpacing = 8,
            Children =
            {
                category,
                date,
                name,
                itemDetails,
                notes
            }
        };
        Microsoft.Maui.Controls.Grid.SetColumn(date, 1);
        Microsoft.Maui.Controls.Grid.SetRow(date, 0);
        Microsoft.Maui.Controls.Grid.SetColumn(name, 0);
        Microsoft.Maui.Controls.Grid.SetColumnSpan(name, 2);
        Microsoft.Maui.Controls.Grid.SetRow(name, 1);
        Microsoft.Maui.Controls.Grid.SetColumn(itemDetails, 0);
        Microsoft.Maui.Controls.Grid.SetColumnSpan(itemDetails, 2);
        Microsoft.Maui.Controls.Grid.SetRow(itemDetails, 2);
        Microsoft.Maui.Controls.Grid.SetColumn(notes, 0);
        Microsoft.Maui.Controls.Grid.SetColumnSpan(notes, 2);
        Microsoft.Maui.Controls.Grid.SetRow(notes, 3);

        var chevron = new Label { Text = "›", FontSize = 30, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };
        var cardContent = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(64), new(GridLength.Star), new(24) },
            ColumnSpacing = 10,
            Children =
            {
                icon,
                details,
                chevron
            }
        };
        Microsoft.Maui.Controls.Grid.SetColumn(icon, 0);
        Microsoft.Maui.Controls.Grid.SetColumn(details, 1);
        Microsoft.Maui.Controls.Grid.SetColumn(chevron, 2);

        var card = new Border { BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = new Thickness(14, 13), StrokeShape = new RoundRectangle { CornerRadius = 18 }, Content = cardContent };
        var open = new Button
        {
            AutomationId = UiAutomationIds.HistoryItem(index),
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            CornerRadius = 18,
            Text = string.Empty,
            Padding = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        open.Clicked += async (_, _) => await Shell.Current.GoToAsync(item.IsWorkout
            ? $"{NavigationRoutes.WorkoutSummary}?sessionId={item.Id}"
            : $"{NavigationRoutes.ActivitySummary}?activityId={item.Id}");
        return new Grid { Children = { card, open } };
    }

    private static string ActivityIcon(string activityName) => activityName switch
    {
        "Running" => "activity_walk.png",
        "Swimming" => "activity_swim.png",
        _ => "activity_walk.png"
    };

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
    private readonly BackupSettingsViewModel _backupViewModel;

    public BackupSettingsPage(DeveloperResetViewModel viewModel, BackupSettingsViewModel backupViewModel) : base(DestinationPageContent.BackupSettings)
    {
        _viewModel = viewModel;
        _backupViewModel = backupViewModel;
        AddBackupTools();
#if DEBUG
        AddDeveloperTools();
#endif
    }

    private void AddBackupTools()
    {
        var exportButton = new Button { Text = "Export backup", BackgroundColor = Color.FromArgb("#169F9A"), TextColor = Colors.White, CornerRadius = 8 };
        exportButton.Clicked += async (_, _) =>
        {
            exportButton.IsEnabled = false;
            await _backupViewModel.ExportAsync();
            exportButton.IsEnabled = true;
            if (_backupViewModel.StatusMessage is not null) await DisplayAlertAsync("Backup exported", _backupViewModel.StatusMessage, "OK");
            else if (_backupViewModel.ErrorMessage is not null) await DisplayAlertAsync("Export failed", _backupViewModel.ErrorMessage, "OK");
        };

        var replaceButton = new Button { Text = "Restore backup", BackgroundColor = Color.FromArgb("#169F9A"), TextColor = Colors.White, CornerRadius = 8 };
        replaceButton.Clicked += async (_, _) => await ImportAsync(replaceButton, BackupImportMode.Replace);
        var mergeButton = new Button { Text = "Merge backup", BackgroundColor = Colors.White, TextColor = Color.FromArgb("#169F9A"), BorderColor = Color.FromArgb("#169F9A"), BorderWidth = 1, CornerRadius = 8 };
        mergeButton.Clicked += async (_, _) => await ImportAsync(mergeButton, BackupImportMode.Merge);

        AddAdditionalContent(new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label { Text = "Backup", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#169F9A") },
                new Label { Text = "Export your local data or restore it from a validated JSON backup.", TextColor = Color.FromArgb("#687A95") },
                exportButton, replaceButton, mergeButton
            }
        });
    }

    private async Task ImportAsync(Button button, BackupImportMode mode)
    {
        button.IsEnabled = false;
        await _backupViewModel.ImportAsync(mode, () => DisplayAlertAsync(
            mode == BackupImportMode.Replace ? "Restore backup?" : "Merge backup?",
            mode == BackupImportMode.Replace ? "This replaces all current local data. A recovery copy will be created first." : "This adds backup records and updates matching IDs.",
            mode == BackupImportMode.Replace ? "Restore" : "Merge", "Cancel"));
        button.IsEnabled = true;
        if (_backupViewModel.StatusMessage is not null) await DisplayAlertAsync("Backup complete", _backupViewModel.StatusMessage, "OK");
        else if (_backupViewModel.ErrorMessage is not null) await DisplayAlertAsync("Backup failed", _backupViewModel.ErrorMessage, "OK");
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
