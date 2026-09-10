using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class DashboardPage : ContentPage
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private static readonly Color PaleBlue = Color.FromArgb("#EAF4FE");
    private static readonly Color PaleTeal = Color.FromArgb("#E8F8F7");

    private readonly DashboardViewModel _viewModel;
    private readonly Label _nextDay = new() { FontSize = 24, TextColor = Muted };
    private readonly Label _nextSession = new() { FontSize = 40, FontAttributes = FontAttributes.Bold, TextColor = Ink };
    private readonly Label _sessionDetails = new() { FontSize = 18, TextColor = Muted };
    private readonly Button _startWorkout = CreatePrimaryButton("▶  Start workout");
    private readonly Button _resumeWorkout = CreatePrimaryButton("↻  Resume workout");
    private readonly Button _logActivity = CreatePrimaryButton("Log activity");
    private readonly Label _recent = new() { FontSize = 16, TextColor = Muted, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Label _trainingSummary = new() { FontSize = 16, TextColor = Muted, HorizontalTextAlignment = TextAlignment.Center };
    private readonly VerticalStackLayout _history = new() { Spacing = 8 };

    public DashboardPage(DashboardViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "Dashboard";
        BackgroundColor = Color.FromArgb("#F8FBFF");
        _startWorkout.Command = viewModel.StartWorkoutCommand;
        _startWorkout.AutomationId = UiAutomationIds.DashboardStartWorkout;
        _resumeWorkout.Command = viewModel.ResumeWorkoutCommand;
        _logActivity.Command = viewModel.LogActivityCommand;

        var content = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(74) }
        };
        content.Add(BuildScrollContent(), 0, 0);
        content.Add(BuildBottomNavigation(), 0, 1);
        Content = content;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        Render();
    }

    private View BuildScrollContent() => new ScrollView
    {
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(20, 22, 20, 28),
            Spacing = 18,
            Children = { BuildHeader(), BuildNextSessionCard(), BuildQuickActions(), BuildRecentActivityCard(), BuildProgressBanner() }
        }
    };

    private View BuildHeader()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(48) },
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
        };
        grid.Add(new Label { Text = "Gym Tracker", FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0);
        grid.Add(new Label { Text = "Dashboard", FontSize = 28, TextColor = Muted }, 0, 1);
        var settings = new Image { Source = "dashboard_settings.svg", WidthRequest = 36, HeightRequest = 36, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        var settingsTap = new TapGestureRecognizer();
        settingsTap.Tapped += (_, _) => _viewModel.SettingsCommand.Execute(null);
        settings.GestureRecognizers.Add(settingsTap);
        grid.Add(settings, 1, 0);
        Grid.SetRowSpan(settings, 2);
        return grid;
    }

    private View BuildNextSessionCard()
    {
        var details = new VerticalStackLayout
        {
            Spacing = 2,
            Children = { new Label { Text = "NEXT SESSION", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Muted }, _nextDay, _nextSession, _sessionDetails }
        };
        var content = new Grid
        {
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) },
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(116) },
            ColumnSpacing = 12,
            RowSpacing = 14
        };
        content.Add(details, 0, 0);
        Grid.SetColumnSpan(details, 2);
        var dumbbell = new Border
        {
            BackgroundColor = Color.FromArgb("#FFF1E8"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 58 },
            WidthRequest = 116,
            HeightRequest = 116,
            Content = new Image { Source = "dashboard_dumbbell.svg", WidthRequest = 76, HeightRequest = 76, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        };
        content.Add(dumbbell, 1, 0);
        Grid.SetRowSpan(dumbbell, 2);
        var actions = new VerticalStackLayout { Spacing = 8, Children = { _resumeWorkout, _startWorkout } };
        content.Add(actions, 0, 1);
        Grid.SetColumnSpan(actions, 2);
        return new Border
        {
            BackgroundColor = PaleTeal,
            StrokeThickness = 0,
            Padding = new Thickness(24, 22),
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Content = content
        };
    }

    private View BuildQuickActions()
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 10
        };
        grid.Add(CreateActionTile("dashboard_add.svg", "Log activity", _viewModel.LogActivityCommand, UiAutomationIds.DashboardLogActivity), 0, 0);
        grid.Add(CreateActionTile("dashboard_plan.svg", "Plan", _viewModel.WeeklyPlanCommand), 1, 0);
        grid.Add(CreateActionTile("dashboard_history.svg", "History", _viewModel.HistoryCommand, UiAutomationIds.DashboardHistory), 2, 0);
        grid.Add(CreateActionTile("dashboard_more.svg", "More", _viewModel.SettingsCommand), 3, 0);
        return grid;
    }

    private View BuildRecentActivityCard()
    {
        var content = new VerticalStackLayout
        {
            Spacing = 10,
            Children = { new Label { Text = "RECENT ACTIVITY", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Muted }, _recent, _trainingSummary, _history, _logActivity }
        };
        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E2EBF5"),
            StrokeThickness = 1,
            Padding = new Thickness(24, 22),
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Content = content
        };
    }

    private static View BuildProgressBanner()
    {
        var content = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(80) }
        };
        content.Add(new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                new Label { Text = "Small steps. Big results.", FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                new Label { Text = "Stay consistent and track your progress.", FontSize = 16, TextColor = Muted }
            }
        }, 0, 0);
        content.Add(new Image
        {
            Source = "dashboard_progress.svg",
            WidthRequest = 72,
            HeightRequest = 54,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        }, 1, 0);

        return new Border
        {
            BackgroundColor = Colors.White,
            Stroke = Color.FromArgb("#E2EBF5"),
            StrokeThickness = 1,
            Padding = new Thickness(24, 20),
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Content = content
        };
    }

    private View BuildBottomNavigation()
    {
        var grid = new Grid
        {
            BackgroundColor = Colors.White,
            Padding = new Thickness(12, 8),
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }
        };
        grid.Add(CreateNavigationItem("dashboard_home.svg", "Home", true), 0, 0);
        grid.Add(CreateNavigationItem("dashboard_progress.svg", "Progress", false, _viewModel.ProgressCommand), 1, 0);
        grid.Add(CreateNavigationItem("dashboard_history.svg", "History", false, _viewModel.HistoryCommand), 2, 0);
        grid.Add(CreateNavigationItem("dashboard_more.svg", "More", false, _viewModel.SettingsCommand), 3, 0);
        return grid;
    }

    private static View CreateActionTile(string image, string title, System.Windows.Input.ICommand? command = null, string? automationId = null)
    {
        var tile = new Border
        {
            AutomationId = automationId,
            BackgroundColor = PaleBlue,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(4, 10),
            HeightRequest = 92,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                HorizontalOptions = LayoutOptions.Center,
                Children = { new Image { Source = image, WidthRequest = 34, HeightRequest = 34 }, new Label { Text = title, FontSize = 14, TextColor = Ink, HorizontalTextAlignment = TextAlignment.Center } }
            }
        };
        if (command is not null)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => command.Execute(null);
            tile.GestureRecognizers.Add(tap);
        }

        return tile;
    }

    private static View CreateNavigationItem(string image, string title, bool selected, System.Windows.Input.ICommand? command = null)
    {
        var item = new VerticalStackLayout
        {
            Spacing = 3,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Image { Source = image, WidthRequest = 28, HeightRequest = 28, Opacity = selected ? 1 : 0.75 },
                new Label { Text = title, FontSize = 13, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center }
            }
        };
        if (command is not null)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => command.Execute(null);
            item.GestureRecognizers.Add(tap);
        }

        return item;
    }

    private static Button CreatePrimaryButton(string text) => new()
    {
        Text = text,
        FontSize = 18,
        FontAttributes = FontAttributes.Bold,
        BackgroundColor = Teal,
        TextColor = Colors.White,
        CornerRadius = 26,
        HeightRequest = 56
    };

    private void Render()
    {
        var state = _viewModel.State;
        _nextDay.Text = state.NextSessionDay;
        _nextSession.Text = state.NextSessionName;
        _sessionDetails.Text = state.NextSessionName == "Activity" ? "Move, recover, and stay active" : "Strength · Progress · Consistency";
        _recent.Text = state.IsEmptyState ? "No recent records" : string.Join(Environment.NewLine, state.RecentItems.Select(x => $"{x.Title} · {x.Detail}"));
        _trainingSummary.Text = state.IsEmptyState ? "Log your workouts to see them here." : state.TrainingSummary;
        _startWorkout.IsVisible = state.ShowGymQuickStart;
        _resumeWorkout.IsVisible = state.ActiveWorkout is not null;
        _resumeWorkout.Text = state.ActiveWorkout is { } active ? $"↻  Resume {active.Name}" : "↻  Resume workout";
        _logActivity.IsVisible = state.ShowActivityQuickStart || state.IsEmptyState;
        _history.Children.Clear();
    }
}
