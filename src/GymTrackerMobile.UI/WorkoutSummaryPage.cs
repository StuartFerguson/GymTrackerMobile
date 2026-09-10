using GymTrackerMobile.Domain;
using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class WorkoutSummaryPage : ContentPage, IQueryAttributable
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private readonly WorkoutSummaryViewModel _viewModel;
    private Guid? _sessionId;

    public WorkoutSummaryPage(WorkoutSummaryViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "Workout Summary";
        BackgroundColor = Color.FromArgb("#F8FBFF");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("sessionId", out var value) && Guid.TryParse(value?.ToString(), out var id)) _sessionId = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_sessionId is Guid id) { await _viewModel.LoadAsync(id); Render(); }
    }

    private void Render()
    {
        if (_viewModel.State.ErrorMessage is not null)
        {
            Content = new Label { Text = _viewModel.State.ErrorMessage, TextColor = Ink, Margin = 24 };
            return;
        }
        var state = _viewModel.State;
        var layout = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(76) } };
        layout.Add(new ScrollView { Content = BuildBody(state) }, 0, 0);
        layout.Add(BuildBottomNavigation(), 0, 1);
        Content = layout;
    }

    private View BuildBody(WorkoutSummaryState state) => new VerticalStackLayout
    {
        Padding = new Thickness(20, 18, 20, 28), Spacing = 16,
        Children = { BuildHeader(), BuildWorkoutHeader(state), BuildMetrics(state), BuildStatus(state), BuildExerciseSection(state), BuildNotes(state) }
    };

    private View BuildHeader()
    {
        var back = new Label { Text = "‹", FontSize = 42, TextColor = Ink, VerticalTextAlignment = TextAlignment.Center };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await Shell.Current.GoToAsync("..");
        back.GestureRecognizers.Add(tap);
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(48), new(GridLength.Star), new(44) } };
        grid.Add(back, 0, 0);
        grid.Add(new Label { Text = "Workout Summary", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Ink, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center }, 1, 0);
        grid.Add(new Label { Text = "⋮", FontSize = 32, TextColor = Ink, HorizontalTextAlignment = TextAlignment.End }, 2, 0);
        return grid;
    }

    private static View BuildWorkoutHeader(WorkoutSummaryState state)
    {
        var illustration = new Border
        {
            BackgroundColor = Color.FromArgb("#FFF1E8"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 47 },
            WidthRequest = 88,
            HeightRequest = 88,
            Content = new Image { Source = "dashboard_dumbbell.svg", WidthRequest = 58, HeightRequest = 58, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center }
        };
        var details = new VerticalStackLayout
        {
            Spacing = 3,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label { AutomationId = UiAutomationIds.WorkoutSummaryName, Text = state.WorkoutName, FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Ink, LineBreakMode = LineBreakMode.NoWrap },
                new Label { Text = $"✓  {(state.IsComplete ? "Completed" : "Partially logged")}  ·  {state.CompletedAtUtc?.ToLocalTime():dd MMM yyyy}", FontSize = 14, TextColor = state.IsComplete ? Teal : Color.FromArgb("#B54708"), LineBreakMode = LineBreakMode.NoWrap },
                new Label { Text = state.MuscleGroups, FontSize = 16, TextColor = Muted }
            }
        };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(94), new(GridLength.Star) }, ColumnSpacing = 12 };
        grid.Add(illustration, 0, 0);
        grid.Add(details, 1, 0);
        return grid;
    }

    private static View BuildMetrics(WorkoutSummaryState state)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }, ColumnSpacing = 8 };
        grid.Add(Metric("◷", state.Duration.ToString(@"h\:mm\:ss"), "Duration"), 0, 0);
        grid.Add(Metric("▰", $"{state.TotalVolumeKilograms:N0} kg", "Training Volume"), 1, 0);
        grid.Add(Metric("♜", $"{state.CompletedSetCount} / {state.PlannedSetCount}", "Sets Completed", UiAutomationIds.WorkoutSummaryCompletedSets), 2, 0);
        grid.Add(Metric("♨", state.CaloriesEstimated.ToString(), "Calories Est."), 3, 0);
        return grid;
    }

    private static View Metric(string icon, string value, string label, string? automationId = null) => new Border { BackgroundColor = Color.FromArgb("#EAF4FE"), StrokeThickness = 0, Padding = new Thickness(5, 12), StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Label { Text = icon, FontSize = 26, TextColor = Color.FromArgb("#52667F"), HorizontalTextAlignment = TextAlignment.Center }, new Label { AutomationId = automationId, Text = value, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Ink, HorizontalTextAlignment = TextAlignment.Center }, new Label { Text = label, FontSize = 12, TextColor = Muted, HorizontalTextAlignment = TextAlignment.Center } } } };

    private static View BuildStatus(WorkoutSummaryState state)
    {
        var message = new VerticalStackLayout { Spacing = 2, Children = { new Label { Text = state.IsComplete ? "Great workout!" : "Workout needs review", FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = state.IsComplete ? "You completed all planned sets." : "Incomplete, failed, or skipped sets remain visible below.", FontSize = 15, TextColor = Muted } } };
        var content = new HorizontalStackLayout { Spacing = 12, Children = { new Label { Text = state.IsComplete ? "✓" : "!", FontSize = 32, TextColor = state.IsComplete ? Teal : Color.FromArgb("#B54708"), VerticalTextAlignment = TextAlignment.Center }, message } };
        return new Border { BackgroundColor = state.IsComplete ? Color.FromArgb("#DFF6EF") : Color.FromArgb("#FFF3E8"), StrokeThickness = 0, Padding = new Thickness(18, 14), StrokeShape = new RoundRectangle { CornerRadius = 18 }, Content = content };
    }

    private static View BuildExerciseSection(WorkoutSummaryState state)
    {
        var layout = new VerticalStackLayout { Spacing = 10, Children = { new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(90) }, Children = { new Label { Text = "Exercises", FontSize = 27, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = $"{state.CompletedSetCount} / {state.PlannedSetCount} sets", FontSize = 16, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center } } } } };
        foreach (var exercise in state.ExerciseList) layout.Children.Add(BuildExerciseCard(exercise));
        return layout;
    }

    private static View BuildExerciseCard(WorkoutSummaryExercise exercise)
    {
        var statuses = string.Join(" · ", exercise.Sets.Where(x => x.Status != SetStatus.Completed).Select(x => $"Set {x.SetNumber}: {x.StatusLabel}"));
        var details = new VerticalStackLayout { Spacing = 4, Children = { new Label { Text = exercise.Name, FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = $"{exercise.CompletedSetCount} / {exercise.PlannedSetCount} sets", FontSize = 15, TextColor = Muted }, new Label { Text = $"Planned  {exercise.PlannedSummary}     Completed  {exercise.CompletedSummary}", FontSize = 14, TextColor = Muted } } };
        if (!string.IsNullOrWhiteSpace(statuses)) details.Children.Add(new Label { Text = statuses, FontSize = 14, TextColor = Color.FromArgb("#B54708") });
        var volume = new VerticalStackLayout { Spacing = 0, Children = { new Label { Text = $"{exercise.TotalVolumeKilograms:N0} kg", FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Ink, HorizontalTextAlignment = TextAlignment.End }, new Label { Text = "Total volume", FontSize = 13, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End } } };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(72), new(GridLength.Star), new(68) }, ColumnSpacing = 10 };
        grid.Add(new Border { BackgroundColor = Color.FromArgb("#EAF4FE"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 12 }, Content = new Image { Source = exercise.ImageSource, Aspect = Aspect.AspectFit } }, 0, 0);
        grid.Add(details, 1, 0);
        grid.Add(volume, 2, 0);
        return new Border { BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = new Thickness(14, 12), StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = grid };
    }

    private static View BuildNotes(WorkoutSummaryState state)
    {
        var content = new HorizontalStackLayout { Spacing = 12, Children = { new Label { Text = "▤", FontSize = 32, TextColor = Muted }, new VerticalStackLayout { Children = { new Label { Text = "Notes", FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = state.Notes, FontSize = 15, TextColor = Muted } } } } };
        return new Border { IsVisible = !string.IsNullOrWhiteSpace(state.Notes), BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = new Thickness(16, 14), StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = content };
    }

    private static View BuildBottomNavigation() => new Grid { BackgroundColor = Colors.White, Padding = new Thickness(12, 9), ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }, Children = { Nav("dashboard_home.svg", "Home", false, 0), Nav("dashboard_progress.svg", "Progress", false, 1), Nav("dashboard_add.svg", "Log Workout", false, 2), Nav("dashboard_history.svg", "History", true, 3) } };

    private static View Nav(string icon, string label, bool selected, int column) { var item = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Image { Source = icon, WidthRequest = 27, HeightRequest = 27, Opacity = selected ? 1 : 0.75 }, new Label { Text = label, FontSize = 13, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } }; Grid.SetColumn(item, column); return item; }
}
