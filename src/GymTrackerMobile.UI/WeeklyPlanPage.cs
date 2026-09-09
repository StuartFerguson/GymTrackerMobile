using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class WeeklyPlanPage : ContentPage
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private readonly WeeklyPlanViewModel _viewModel;
    private readonly VerticalStackLayout _days = new() { Spacing = 8 };

    public WeeklyPlanPage(WeeklyPlanViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "Weekly plan";
        BackgroundColor = Color.FromArgb("#F8FBFF");

        var content = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(76) } };
        content.Add(BuildBody(), 0, 0);
        content.Add(BuildBottomNavigation(), 0, 1);
        Content = content;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        Render();
    }

    private View BuildBody() => new ScrollView
    {
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(16, 12, 16, 16),
            Spacing = 9,
            Children = { BuildHeader(), BuildTabs(), new BoxView { HeightRequest = 1, Color = Color.FromArgb("#DCE7F2"), Margin = new Thickness(-20, 0) }, BuildPlanHeading(), _days, BuildConsistencyBanner() }
        }
    };

    private static View BuildHeader()
    {
        var header = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(44) }, RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }, RowSpacing = 2 };
        header.Add(new Label { Text = "Gym Tracker", FontSize = 30, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0);
        header.Add(new Label { Text = "Build healthier habits. A stronger you.", FontSize = 16, TextColor = Muted }, 0, 1);
        var calendar = new Image { Source = "plan_calendar.svg", WidthRequest = 30, HeightRequest = 30, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        header.Add(calendar, 1, 0);
        Grid.SetRowSpan(calendar, 2);
        return header;
    }

    private static View BuildTabs()
    {
        var tabs = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }, HeightRequest = 38, Margin = new Thickness(0, 4, 0, 0) };
        AddTab(tabs, "Progress", 0, false);
        AddTab(tabs, "Weekly Plan", 1, true);
        AddTab(tabs, "Workouts", 2, false);
        AddTab(tabs, "Stats", 3, false);
        return tabs;
    }

    private static void AddTab(Grid tabs, string text, int column, bool selected)
    {
        var stack = new VerticalStackLayout { Spacing = 4, HorizontalOptions = LayoutOptions.Fill, Children = { new Label { Text = text, FontSize = 15, FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } };
        if (selected) stack.Children.Add(new BoxView { HeightRequest = 3, WidthRequest = 62, Color = Teal, CornerRadius = 2, HorizontalOptions = LayoutOptions.Center });
        tabs.Add(stack, column, 0);
    }

    private static View BuildPlanHeading()
    {
        var heading = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(142) },
            Margin = new Thickness(0, 16, 0, 3)
        };
        heading.Add(new VerticalStackLayout
        {
            Spacing = 1,
            Children =
            {
                new Label { Text = "Weekly Plan", FontSize = 26, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                new Label { Text = "A balanced week to keep you moving.", FontSize = 16, TextColor = Muted }
            }
        }, 0, 0);
        heading.Add(new Border
        {
            BackgroundColor = Color.FromArgb("#DDF4F6"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(10, 3),
            Margin = new Thickness(8, 0, 0, 0),
            HeightRequest = 40,
            VerticalOptions = LayoutOptions.Center,
            Content = new Label { Text = "This Week ⌄", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Teal, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }
        }, 1, 0);
        return heading;
    }

    private void Render()
    {
        _days.Children.Clear();
        foreach (var day in _viewModel.State.Days) _days.Children.Add(BuildDayCard(day));
    }

    private View BuildDayCard(WeeklyPlanDay day)
    {
        var card = new Border { BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = new Thickness(12, 10), StrokeShape = new RoundRectangle { CornerRadius = 18 }, Content = BuildDayContent(day) };
        if (day.CanStartWorkout || day.CanLogActivity)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) =>
            {
                if (day.CanStartWorkout) await _viewModel.StartWorkoutAsync(day);
                else await _viewModel.LogActivityAsync(day);
            };
            card.GestureRecognizers.Add(tap);
        }
        return card;
    }

    private static View BuildDayContent(WeeklyPlanDay day)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(64), new(1), new(58), new(GridLength.Star), new(18) }, ColumnSpacing = 9, HeightRequest = 48 };
        grid.Add(new VerticalStackLayout { Spacing = 0, Children = { new Label { Text = day.ShortDayName, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = $"Day {day.DayNumber}", FontSize = 13, TextColor = Muted } } }, 0, 0);
        grid.Add(new BoxView { WidthRequest = 1, Color = Color.FromArgb("#D5E0EC"), VerticalOptions = LayoutOptions.Fill }, 1, 0);
        grid.Add(new Image { Source = day.IconSource, WidthRequest = 42, HeightRequest = 42, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }, 2, 0);
        grid.Add(new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = day.Title, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = day.Detail, FontSize = 13, TextColor = Muted, LineBreakMode = LineBreakMode.TailTruncation } } }, 3, 0);
        grid.Add(new Label { Text = "›", FontSize = 32, TextColor = day.Kind == WeeklyPlanDayKind.Rest ? Muted : Ink, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, 4, 0);
        return grid;
    }

    private static View BuildConsistencyBanner() => new Border
    {
        BackgroundColor = Color.FromArgb("#EAF4FE"), StrokeThickness = 0, Padding = new Thickness(16, 12), StrokeShape = new RoundRectangle { CornerRadius = 18 },
        Content = new HorizontalStackLayout
        {
            Spacing = 12,
            Children = { new Border { BackgroundColor = Color.FromArgb("#D5F4F6"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 24 }, Padding = 9, Content = new Label { Text = "↗", FontSize = 20, TextColor = Teal } }, new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = "Consistency creates results.", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = "Stick to the plan and keep going!", FontSize = 14, TextColor = Muted } } } }
        }
    };

    private static View BuildBottomNavigation()
    {
        var grid = new Grid { BackgroundColor = Colors.White, Padding = new Thickness(16, 10), ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) } };
        AddNavigationItem(grid, "dashboard_home.svg", "Home", 0, false);
        AddNavigationItem(grid, "dashboard_plan.svg", "Plan", 1, true);
        AddNavigationItem(grid, "dashboard_history.svg", "Log", 2, false);
        AddNavigationItem(grid, "dashboard_more.svg", "Profile", 3, false);
        return grid;
    }

    private static void AddNavigationItem(Grid grid, string icon, string label, int column, bool selected)
    {
        grid.Add(new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Image { Source = icon, WidthRequest = 28, HeightRequest = 28, Opacity = selected ? 1 : 0.75 }, new Label { Text = label, FontSize = 14, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } }, column, 0);
    }
}
