using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Storage;
using GymTrackerMobile.Persistence;

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
        foreach (var item in _viewModel.Items) _items.Children.Add(BuildItem(item));
    }

    private static View BuildItem(HistoryItem item)
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
        var name = new Label { Text = item.Name, FontSize = 19, FontAttributes = FontAttributes.None, TextColor = Ink };
        var itemDetails = new Label { Text = item.Details, FontSize = 14, TextColor = accent };
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
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await Shell.Current.GoToAsync(item.IsWorkout
            ? $"{NavigationRoutes.WorkoutSummary}?sessionId={item.Id}"
            : $"{NavigationRoutes.ActivitySummary}?activityId={item.Id}");
        card.GestureRecognizers.Add(tap);
        return card;
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

public sealed class BackupSettingsPage : ContentPage
{
    private readonly BackupSettingsViewModel _viewModel;
    private readonly DeveloperResetViewModel _resetViewModel;

    public BackupSettingsPage(BackupSettingsViewModel viewModel, DeveloperResetViewModel resetViewModel)
    {
        _viewModel = viewModel;
        _resetViewModel = resetViewModel;
        Title = "Backup & Settings";
        BackgroundColor = Color.FromArgb("#F8FBFF");
        var status = new Label { TextColor = Color.FromArgb("#169F9A"), IsVisible = false };
        var error = new Label { TextColor = Color.FromArgb("#B42318"), IsVisible = false };
        var export = new Button { Text = "Export backup", BackgroundColor = Color.FromArgb("#169F9A"), TextColor = Colors.White, CornerRadius = 8 };
        var import = new Button { Text = "Replace with backup", BackgroundColor = Color.FromArgb("#102A50"), TextColor = Colors.White, CornerRadius = 8 };
        var merge = new Button { Text = "Merge backup", BackgroundColor = Colors.White, TextColor = Color.FromArgb("#102A50"), BorderColor = Color.FromArgb("#102A50"), BorderWidth = 1, CornerRadius = 8 };
        export.Clicked += async (_, _) =>
        {
            export.IsEnabled = false;
            if (await _viewModel.ExportAsync() && _viewModel.LastExportPath is not null)
                await Microsoft.Maui.ApplicationModel.DataTransfer.Share.Default.RequestAsync(new Microsoft.Maui.ApplicationModel.DataTransfer.ShareFileRequest("Gym Tracker backup", new Microsoft.Maui.ApplicationModel.DataTransfer.ShareFile(_viewModel.LastExportPath)));
            ShowMessages(status, error); export.IsEnabled = true;
        };
        import.Clicked += async (_, _) => await ImportAsync(status, error, BackupImportMode.Replace);
        merge.Clicked += async (_, _) => await ImportAsync(status, error, BackupImportMode.Merge);
        var content = new VerticalStackLayout { Spacing = 14, Padding = new Thickness(20, 22, 20, 28), Children =
        {
            new Label { Text = "Backup & Settings", FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#102A50") },
            new Label { Text = "Export a copy of your data or restore it on this device.", FontSize = 17, TextColor = Color.FromArgb("#687A95") },
            status, error, Section("Backup", export, import, merge),
            Section("Preferences", new Label { Text = "Units", FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#102A50") }, new Label { Text = _viewModel.UnitLabel, TextColor = Color.FromArgb("#687A95") }),
            new Label { Text = "Recommendations are general training guidance only and are not medical advice. Stop if you experience pain and consult a qualified professional when appropriate.", FontSize = 13, TextColor = Color.FromArgb("#687A95") }
        }};
#if DEBUG
        content.Children.Add(BuildDeveloperTools());
#endif
        Content = new ScrollView { Content = content };
    }

    protected override async void OnAppearing() { base.OnAppearing(); await _viewModel.LoadAsync(); }

    private async Task ImportAsync(Label status, Label error, BackupImportMode mode)
    {
        var file = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose a Gym Tracker backup" });
        if (file is null) return;
        await using var stream = await file.OpenReadAsync();
        await _viewModel.ImportAsync(stream, message => DisplayAlertAsync(mode == BackupImportMode.Replace ? "Replace existing data?" : "Merge backup data?", message, mode == BackupImportMode.Replace ? "Replace data" : "Merge data", "Cancel"), mode);
        ShowMessages(status, error);
    }

    private void ShowMessages(Label status, Label error)
    {
        status.Text = _viewModel.StatusMessage; status.IsVisible = status.Text is not null;
        error.Text = _viewModel.ErrorMessage; error.IsVisible = error.Text is not null;
    }

#if DEBUG
    private View BuildDeveloperTools()
    {
        var resetButton = new Button { Text = "Reset local app data", TextColor = Colors.White, BackgroundColor = Color.FromArgb("#B42318"), CornerRadius = 8 };
        resetButton.Clicked += async (_, _) =>
        {
            resetButton.IsEnabled = false;
            await _resetViewModel.ResetAsync(() => DisplayAlertAsync("Reset local app data?", "This permanently deletes active workouts, workout history, activities, settings, and backup metadata, then restores the built-in templates and exercises.", "Reset data", "Cancel"));
            resetButton.IsEnabled = true;
            if (_resetViewModel.StatusMessage is not null) await DisplayAlertAsync("Reset complete", _resetViewModel.StatusMessage, "OK");
            else if (_resetViewModel.ErrorMessage is not null) await DisplayAlertAsync("Reset failed", _resetViewModel.ErrorMessage, "OK");
        };
        return Section("Developer tools", new Label { Text = "Debug builds only. Use this to reset local data between test runs.", TextColor = Color.FromArgb("#687A95") }, resetButton);
    }
#endif

    private static View Section(string title, params View[] children)
    {
        var stack = new VerticalStackLayout { Spacing = 8 };
        stack.Children.Add(new Label { Text = title, FontAttributes = FontAttributes.Bold, FontSize = 19, TextColor = Color.FromArgb("#102A50") });
        foreach (var child in children) stack.Children.Add(child);
        return new Border { BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = 16, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = stack };
    }
}
