using GymTrackerMobile.Domain;
using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class ActiveWorkoutPage : ContentPage, IQueryAttributable
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private static readonly Color PaleBlue = Color.FromArgb("#EAF4FE");
    private readonly ActiveWorkoutViewModel _viewModel;
    private Guid? _sessionId;

    public ActiveWorkoutPage(ActiveWorkoutViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "Workout";
        BackgroundColor = Color.FromArgb("#F8FBFF");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query) { if (query.TryGetValue("sessionId", out var value) && Guid.TryParse(value?.ToString(), out var sessionId)) _sessionId = sessionId; }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_sessionId is Guid sessionId) { await _viewModel.LoadAsync(sessionId); Render(); }
    }

    private void Render()
    {
        var exercise = _viewModel.State.CurrentExercise;
        if (exercise is null) { Content = new Label { Text = _viewModel.State.ErrorMessage ?? "No active workout", TextColor = Ink, Margin = 24 }; return; }
        var layout = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(74) } };
        layout.Add(new ScrollView { Content = BuildBody(exercise) }, 0, 0);
        layout.Add(BuildBottomNavigation(), 0, 1);
        Content = layout;
    }

    private View BuildBody(ActiveWorkoutExercise exercise)
    {
        var heading = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(52) } };
        heading.Add(new VerticalStackLayout { Spacing = 2, Children = { new Label { Text = exercise.Name, FontSize = 29, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = exercise.MuscleAndMode, FontSize = 18, TextColor = Muted } } }, 0, 0);
        heading.Add(new Label { Text = $"{_viewModel.State.ExerciseNumber} of {_viewModel.State.ExerciseList.Count}", FontSize = 17, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center }, 1, 0);
        return new VerticalStackLayout { Padding = new Thickness(20, 18, 20, 24), Spacing = 18, Children = { BuildHeader(), heading, BuildExerciseSummary(exercise), BuildSets(exercise), AutomationText("active-error", _viewModel.State.ErrorMessage, Color.FromArgb("#B42318")), BuildRecommendation(), BuildNavigation() } };
    }

    private static Button AutomationText(string automationId, string? text, Color color) => new()
    {
        AutomationId = automationId,
        Text = text,
        TextColor = color,
        BackgroundColor = Colors.Transparent,
        BorderWidth = 0,
        Padding = 0,
        HeightRequest = 40,
        HorizontalOptions = LayoutOptions.Fill,
        IsVisible = text is not null
    };

    private View BuildHeader()
    {
        var finish = new Button { Text = "Finish", FontSize = 14, BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 16, Padding = new Thickness(12, 4) };
        finish.AutomationId = UiAutomationIds.ActiveComplete;
        finish.Clicked += async (_, _) =>
        {
            await _viewModel.CompleteAsync();
            if (_sessionId is Guid sessionId)
            {
                await Shell.Current.GoToAsync($"{NavigationRoutes.WorkoutSummary}?sessionId={sessionId}");
            }
        };
        var abandon = new Button { Text = "Abandon", FontSize = 14, BackgroundColor = Colors.White, TextColor = Color.FromArgb("#B42318"), BorderColor = Color.FromArgb("#F0B4AE"), BorderWidth = 1, CornerRadius = 16, Padding = new Thickness(10, 4) };
        abandon.Clicked += async (_, _) =>
        {
            if (await DisplayAlertAsync("Abandon workout?", "Your in-progress workout will be discarded. Completed workouts are not affected.", "Abandon", "Keep working"))
            {
                await _viewModel.AbandonAsync();
                await Shell.Current.GoToAsync("..");
            }
        };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Auto) } };
        grid.Add(new Label { Text = "‹  Workout", FontSize = 27, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0);
        grid.Add(new HorizontalStackLayout { Spacing = 6, Children = { abandon, finish } }, 1, 0);
        return grid;
    }

    private static View BuildExerciseSummary(ActiveWorkoutExercise exercise)
    {
        var image = new Border { BackgroundColor = PaleBlue, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 20 }, Padding = 0, Content = new Image { Source = exercise.ImageSource, Aspect = Aspect.AspectFill } };
        var cards = new VerticalStackLayout { Spacing = 12, Children = { InfoCard("◎", "Target", exercise.TargetSummary, "#F47B20"), InfoCard("▮▮", "Previous performance", "3 sets × 10 reps × 60 kg", "#687A95") } };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(148), new(GridLength.Star) }, ColumnSpacing = 14, HeightRequest = 148 };
        grid.Add(image, 0, 0); grid.Add(cards, 1, 0); return grid;
    }

    private static View InfoCard(string icon, string title, string value, string iconColor) => new Border { BackgroundColor = PaleBlue, StrokeThickness = 0, Padding = new Thickness(14, 10), StrokeShape = new RoundRectangle { CornerRadius = 18 }, Content = new HorizontalStackLayout { Spacing = 12, Children = { new Label { Text = icon, FontSize = 28, TextColor = Color.FromArgb(iconColor), VerticalTextAlignment = TextAlignment.Center }, new VerticalStackLayout { Spacing = 0, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = title, FontSize = 15, TextColor = Muted }, new Label { Text = value, FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Ink } } } } } };

    private View BuildSets(ActiveWorkoutExercise exercise)
    {
        var rows = new VerticalStackLayout { Spacing = 10 };
        var heading = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(70) } };
        heading.Add(new Label { Text = "Log your sets", FontSize = 25, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0);
        heading.Add(new Label { Text = $"{exercise.Sets.Count} sets", FontSize = 16, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End }, 1, 0);
        rows.Children.Add(heading);
        foreach (var set in exercise.Sets) rows.Children.Add(BuildSetRow(exercise, set));
        return rows;
    }

    private View BuildSetRow(ActiveWorkoutExercise exercise, ActiveWorkoutSet set)
    {
        var weightLabel = exercise.WeightEntryConvention == WeightEntryConvention.PerDumbbell ? "kg / dumbbell" : "kg";
        var weight = new Entry { AutomationId = UiAutomationIds.ActiveWeight(set.SetNumber), Text = set.WeightKilograms?.ToString("0.##"), Placeholder = weightLabel, Keyboard = Keyboard.Numeric, FontSize = 18, HorizontalTextAlignment = TextAlignment.Center, BackgroundColor = Colors.White };
        var reps = new Entry { AutomationId = UiAutomationIds.ActiveRepetitions(set.SetNumber), Text = set.Repetitions?.ToString(), Keyboard = Keyboard.Numeric, FontSize = 18, HorizontalTextAlignment = TextAlignment.Center, BackgroundColor = Colors.White };
        var notes = new Entry { Text = set.Notes, Placeholder = "Notes (optional)", FontSize = 15, TextColor = Muted, BackgroundColor = Colors.White };
        var save = new Button { AutomationId = UiAutomationIds.ActiveSaveSet(set.SetNumber), Text = set.Status == SetStatus.Completed ? "✓  Complete" : "Mark complete", FontSize = 14, BackgroundColor = set.Status == SetStatus.Completed ? Teal : Colors.White, TextColor = set.Status == SetStatus.Completed ? Colors.White : Ink, BorderColor = Color.FromArgb("#D5E0EC"), BorderWidth = 1, CornerRadius = 16, Padding = 4 };
        save.Clicked += async (_, _) => { await _viewModel.UpdateSetAsync(set.SetNumber, exercise.ShowsWeight && double.TryParse(weight.Text, out var parsedWeight) ? parsedWeight : null, int.TryParse(exercise.ShowsWeight ? reps.Text : weight.Text, out var parsedReps) ? parsedReps : null, notes.Text); await _viewModel.SaveSetAsync(set.SetNumber); Render(); };
        var status = new Picker { Title = "Set status", FontSize = 14, TextColor = Ink, BackgroundColor = Colors.White, ItemsSource = Enum.GetValues<SetStatus>().Where(x => x != SetStatus.Planned).Select(x => x.ToString()).ToList(), SelectedItem = set.Status == SetStatus.Planned ? SetStatus.Incomplete.ToString() : set.Status.ToString() };
        status.SelectedIndexChanged += async (_, _) =>
        {
            if (Enum.TryParse<SetStatus>(status.SelectedItem?.ToString(), out var selectedStatus) && selectedStatus != set.Status)
            {
                await _viewModel.UpdateSetAsync(set.SetNumber, exercise.ShowsWeight && double.TryParse(weight.Text, out var parsedWeight) ? parsedWeight : null, int.TryParse(exercise.ShowsWeight ? reps.Text : weight.Text, out var parsedReps) ? parsedReps : null, notes.Text);
                await _viewModel.SetStatusAsync(set.SetNumber, selectedStatus);
                Render();
            }
        };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(34), new(GridLength.Star), new(GridLength.Star), new(118) }, ColumnSpacing = 8 };
        grid.Add(new Label { Text = set.SetNumber.ToString(), FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Ink, VerticalTextAlignment = TextAlignment.Center }, 0, 0);
        if (exercise.ShowsWeight) { grid.Add(EntryCard(weight), 1, 0); grid.Add(EntryCard(reps), 2, 0); grid.Add(save, 3, 0); }
        else { grid.Add(EntryCard(reps), 1, 0); grid.Add(save, 2, 0); Grid.SetColumnSpan(save, 2); }
        return new VerticalStackLayout { Spacing = 2, Children = { grid, new HorizontalStackLayout { Spacing = 8, Children = { new Label { Text = "Status", FontSize = 14, TextColor = Muted, VerticalTextAlignment = TextAlignment.Center }, status, notes } } } };
    }

    private static View EntryCard(Entry entry) => new Border { Stroke = Color.FromArgb("#D5E0EC"), StrokeThickness = 1, StrokeShape = new RoundRectangle { CornerRadius = 14 }, Padding = 0, Content = entry };

    private View BuildRecommendation()
    {
        if (_viewModel.State.Recommendation is not { } recommendation) return new BoxView { HeightRequest = 1 };
        var details = new VerticalStackLayout
        {
            Spacing = 1,
            HorizontalOptions = LayoutOptions.Fill,
            Children =
            {
                new Label { Text = "RECOMMENDATION", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Muted },
                new Label { AutomationId = "recommendation-outcome", Text = recommendation.Outcome is null ? "Keep it up!" : recommendation.Outcome.ToString(), FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                new Label { Text = recommendation.Explanation, FontSize = 15, TextColor = Muted }
            }
        };
        var actions = new HorizontalStackLayout { Spacing = 8 };
        var accept = new Button { AutomationId = UiAutomationIds.RecommendationAccept, Text = "Accept", FontSize = 13, BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 14, Padding = new Thickness(12, 3) };
        var edit = new Button { AutomationId = UiAutomationIds.RecommendationEdit, Text = "Edit", FontSize = 13, BackgroundColor = Colors.White, TextColor = Ink, CornerRadius = 14, Padding = new Thickness(12, 3) };
        var ignore = new Button { AutomationId = UiAutomationIds.RecommendationIgnore, Text = "Ignore", FontSize = 13, BackgroundColor = Colors.White, TextColor = Muted, CornerRadius = 14, Padding = new Thickness(12, 3) };
        var editWeight = new Entry { AutomationId = "recommendation-edit-weight", Text = recommendation.ProposedWeightKilograms?.ToString("0.##"), Keyboard = Keyboard.Numeric, WidthRequest = 90, BackgroundColor = Colors.White };
        accept.Clicked += async (_, _) => { await _viewModel.AcceptRecommendationAsync(); Render(); };
        edit.Clicked += async (_, _) => { if (double.TryParse(editWeight.Text, out var weight)) await _viewModel.EditRecommendationAsync(weight); Render(); };
        ignore.Clicked += async (_, _) => { await _viewModel.IgnoreRecommendationAsync(); Render(); };
        actions.Children.Add(accept); actions.Children.Add(editWeight); actions.Children.Add(edit); actions.Children.Add(ignore);
        details.Children.Add(actions);
        return new Border
        {
            BackgroundColor = Color.FromArgb("#E8F8F7"),
            StrokeThickness = 0,
            Padding = new Thickness(18, 14),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Content = new HorizontalStackLayout { Spacing = 14, Children = { new Label { Text = "♧", FontSize = 38, TextColor = Color.FromArgb("#F47B20") }, details } }
        };
    }

    private View BuildNavigation()
    {
        var previous = new Button { Text = "‹  Previous", FontSize = 18, BackgroundColor = PaleBlue, TextColor = Ink, CornerRadius = 18 };
        var next = new Button { Text = "Next exercise  ›", FontSize = 18, BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 18 };
        previous.Clicked += async (_, _) => { await _viewModel.SelectExerciseAsync(_viewModel.State.CurrentExerciseIndex - 1); Render(); };
        next.Clicked += async (_, _) => { await _viewModel.SelectExerciseAsync(_viewModel.State.CurrentExerciseIndex + 1); Render(); };
        previous.IsEnabled = _viewModel.State.CurrentExerciseIndex > 0; next.IsEnabled = _viewModel.State.CurrentExerciseIndex < _viewModel.State.ExerciseList.Count - 1;
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) }, ColumnSpacing = 12 };
        grid.Add(previous, 0, 0); grid.Add(next, 1, 0); return grid;
    }

    private static View BuildBottomNavigation()
    {
        var grid = new Grid { BackgroundColor = Colors.White, Padding = new Thickness(12, 8), ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) } };
        grid.Add(Nav("dashboard_home.svg", "Home", false), 0, 0); grid.Add(Nav("dashboard_dumbbell.svg", "Workout", true), 1, 0); grid.Add(Nav("dashboard_progress.svg", "Progress", false), 2, 0); grid.Add(Nav("dashboard_more.svg", "Profile", false), 3, 0); return grid;
    }
    private static View Nav(string source, string text, bool selected) => new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Image { Source = source, WidthRequest = 28, HeightRequest = 28, Opacity = selected ? 1 : 0.75 }, new Label { Text = text, FontSize = 13, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } };
}
