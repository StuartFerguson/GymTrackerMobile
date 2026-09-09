using System.Globalization;
using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class ExerciseProgressPage : ContentPage
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private readonly ExerciseProgressViewModel _viewModel;
    private readonly VerticalStackLayout _body = new() { Spacing = 16 };
    private readonly Picker _picker;
    private bool _isRendering;

    public ExerciseProgressPage(ExerciseProgressViewModel viewModel)
    {
        _viewModel = viewModel;
        Shell.SetNavBarIsVisible(this, false);
        _picker = new Picker { Title = "Choose an exercise", TextColor = Ink, FontSize = 17 };
        _picker.SelectedIndexChanged += OnExerciseChanged;
        var scrollBody = new VerticalStackLayout { Padding = new Thickness(20, 18, 20, 28), Spacing = 16 };
        scrollBody.Children.Add(Header());
        scrollBody.Children.Add(Card(_picker));
        scrollBody.Children.Add(_body);
        var scroll = new ScrollView { Content = scrollBody };
        var layout = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(76) } };
        layout.Add(scroll, 0, 0);
        layout.Add(BuildBottomNavigation(), 0, 1);
        Content = layout;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
        Render();
    }

    private void Render()
    {
        _isRendering = true;
        try
        {
            var state = _viewModel.State;
            _body.Children.Clear();
            _picker.ItemsSource = state.ExerciseList.Select(x => x.Name).ToList();
            _picker.SelectedIndex = Math.Max(0, state.ExerciseList.ToList().FindIndex(x => x.Id == state.SelectedExerciseId));
            if (state.ErrorMessage is not null)
            {
                _body.Children.Add(Card(new Label { Text = state.ErrorMessage, TextColor = Muted, FontSize = 16 }));
            }
            else if (state.SelectedExercise is not null)
            {
                _body.Children.Add(ExerciseHeader(state.SelectedExercise));
                _body.Children.Add(Metrics(state));
                _body.Children.Add(ProgressChart(state));
                _body.Children.Add(WeeklyConsistency(state));
                _body.Children.Add(History(state));
                _body.Children.Add(LogWorkoutButton());
            }
            else
            {
                _body.Children.Add(Card(new Label { Text = "No exercises are available yet.", TextColor = Muted, FontSize = 16 }));
            }
        }
        finally
        {
            _isRendering = false;
        }
    }

    private View Header()
    {
        var back = new Label { Text = "‹", FontSize = 42, TextColor = Ink, VerticalTextAlignment = TextAlignment.Center };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await Shell.Current.GoToAsync("..");
        back.GestureRecognizers.Add(tap);
        var header = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(44), new(GridLength.Star), new(36) } };
        header.Add(back, 0, 0);
        header.Add(new Label { Text = "Exercise Progress", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Ink, VerticalTextAlignment = TextAlignment.Center }, 1, 0);
        header.Add(new Label { Text = "⋮", FontSize = 32, TextColor = Ink, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center }, 2, 0);
        return header;
    }

    private void OnExerciseChanged(object? sender, EventArgs args)
    {
        if (_isRendering) return;
        var selectedIndex = _picker.SelectedIndex;
        if (selectedIndex >= 0 && selectedIndex < _viewModel.State.ExerciseList.Count)
        {
            _viewModel.SelectExercise(_viewModel.State.ExerciseList[selectedIndex].Id);
            Render();
        }
    }

    private static View ExerciseHeader(ExerciseProgressExercise exercise)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(88), new(GridLength.Star) }, ColumnSpacing = 14 };
        grid.Add(new Border { BackgroundColor = Color.FromArgb("#EAF4FE"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = new Image { Source = exercise.ImageSource, Aspect = Aspect.AspectFit } }, 0, 0);
        grid.Add(new VerticalStackLayout { Spacing = 3, Children = { new Label { Text = exercise.Name, FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = exercise.Subtitle, FontSize = 16, TextColor = Muted }, new Label { Text = exercise.IsBodyweight ? "Track repetitions and completed sets." : "Track load, repetitions, and completed sets.", FontSize = 15, TextColor = Muted } } }, 1, 0);
        return Card(grid);
    }

    private static View Metrics(ExerciseProgressState state)
    {
        var weight = state.HeaviestWeightKilograms is double value ? $"{value.ToString("0.##", CultureInfo.InvariantCulture)} kg" : "Bodyweight";
        var best = state.BestRepetitions is int reps ? $"{reps} reps" : "—";
        var volume = state.SelectedExercise?.IsBodyweight == true ? $"{state.TrainingVolumeKilograms:0} reps" : $"{state.TrainingVolumeKilograms:0.##} kg";
        return new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) }, RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }, ColumnSpacing = 10, RowSpacing = 10, Children = { Metric("▰", "Heaviest weight", weight, Color.FromArgb("#FFF0E7"), 0, 0), Metric("♜", "Best reps", best, Color.FromArgb("#E2F7F4"), 1, 0), Metric("▤", "Training volume", volume, Color.FromArgb("#EAF4FE"), 0, 1), Metric("✓", "Sets", $"{state.CompletedSetCount} / {state.PlannedSetCount}", Color.FromArgb("#F1EAFE"), 1, 1) } };
    }

    private static View Metric(string icon, string label, string value, Color iconBackground, int column, int row)
    {
        var card = Card(new HorizontalStackLayout { Spacing = 10, Children = { new Border { BackgroundColor = iconBackground, StrokeThickness = 0, WidthRequest = 42, HeightRequest = 42, StrokeShape = new RoundRectangle { CornerRadius = 21 }, Content = new Label { Text = icon, FontSize = 19, TextColor = Teal, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center } }, new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = label, FontSize = 13, TextColor = Muted }, new Label { Text = value, FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = Ink } } } } });
        Grid.SetColumn(card, column); Grid.SetRow(card, row); return card;
    }

    private static View History(ExerciseProgressState state)
    {
        var layout = new VerticalStackLayout { Spacing = 10 };
        layout.Children.Add(new Label { Text = "Recent workouts", FontSize = 25, FontAttributes = FontAttributes.Bold, TextColor = Ink });
        if (state.HistoryList.Count == 0)
        {
            layout.Children.Add(Card(new Label { Text = state.EmptyStateMessage, TextColor = Muted, FontSize = 16 }));
            return layout;
        }
        foreach (var item in state.HistoryList)
        {
            var load = item.WeightKilograms is double weight ? $"{weight:0.##} kg" : "Bodyweight";
            var date = item.DateLocal.ToString("MMM\nd", CultureInfo.InvariantCulture).ToUpperInvariant();
            var row = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(64), new(GridLength.Star), new(24) }, ColumnSpacing = 12 };
            row.Add(new Border { BackgroundColor = Color.FromArgb("#F5F8FC"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 12 }, Content = new Label { Text = date, FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Ink, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center } }, 0, 0);
            row.Add(new VerticalStackLayout { Spacing = 1, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = $"{load} × {item.Repetitions ?? 0} reps", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = $"{item.CompletedSets} / {item.PlannedSets} sets", FontSize = 14, TextColor = Muted } } }, 1, 0);
            row.Add(new Label { Text = "›", FontSize = 30, TextColor = Muted, HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center }, 2, 0);
            layout.Children.Add(Card(row));
        }
        return layout;
    }

    private static View WeeklyConsistency(ExerciseProgressState state)
    {
        var weeks = new HorizontalStackLayout { Spacing = 8 };
        foreach (var week in state.WeeklyConsistencyList)
        {
            weeks.Children.Add(new Border
            {
                BackgroundColor = week.CompletedWorkoutCount > 0 ? Color.FromArgb("#D5F4F6") : Color.FromArgb("#F2F5F8"),
                StrokeThickness = 0,
                Padding = new Thickness(8, 7),
                StrokeShape = new RoundRectangle { CornerRadius = 10 },
                Content = new VerticalStackLayout
                {
                    Spacing = 1,
                    Children =
                    {
                        new Label { Text = week.WeekStartLocal.ToString("dd MMM", CultureInfo.InvariantCulture), FontSize = 11, TextColor = Muted },
                        new Label { Text = week.CompletedWorkoutCount.ToString(CultureInfo.InvariantCulture), FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink }
                    }
                }
            });
        }

        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label { Text = "Weekly consistency", FontSize = 21, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                new ScrollView { Orientation = ScrollOrientation.Horizontal, Content = weeks }
            }
        };
    }

    private static View ProgressChart(ExerciseProgressState state)
    {
        var chart = new GraphicsView { HeightRequest = 190, Drawable = new ProgressChartDrawable(state.HistoryList, state.SelectedExercise?.IsBodyweight == true) };
        var header = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(110) } };
        header.Add(new Label { Text = "Progress Over Time", FontSize = 21, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0);
        header.Add(new Border { BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = new Thickness(8, 5), StrokeShape = new RoundRectangle { CornerRadius = 10 }, Content = new Label { Text = state.SelectedExercise?.IsBodyweight == true ? "Repetitions  ˅" : "Weight lifted  ˅", FontSize = 13, TextColor = Ink, HorizontalTextAlignment = TextAlignment.Center } }, 1, 0);
        return Card(new VerticalStackLayout { Spacing = 8, Children = { header, chart } });
    }

    private static View LogWorkoutButton() => new Button { Text = "+   Log Workout", FontSize = 18, FontAttributes = FontAttributes.Bold, BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 24, HeightRequest = 54 };

    private static View BuildBottomNavigation() => new Grid { BackgroundColor = Colors.White, Padding = new Thickness(12, 9), ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) }, Children = { Nav("dashboard_home.svg", "Home", false, 0), Nav("dashboard_progress.svg", "Progress", true, 1), Nav("dashboard_history.svg", "Workouts", false, 2), Nav("dashboard_more.svg", "Profile", false, 3) } };

    private static View Nav(string icon, string label, bool selected, int column)
    {
        var item = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Image { Source = icon, WidthRequest = 27, HeightRequest = 27, Opacity = selected ? 1 : 0.75 }, new Label { Text = label, FontSize = 13, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } };
        Grid.SetColumn(item, column);
        return item;
    }

    private static Border Card(View content) => new() { BackgroundColor = Colors.White, Stroke = Color.FromArgb("#E2EBF5"), StrokeThickness = 1, Padding = new Thickness(16, 14), StrokeShape = new RoundRectangle { CornerRadius = 16 }, Content = content };
}

internal sealed class ProgressChartDrawable(IReadOnlyList<ExerciseProgressEntry> entries, bool isBodyweight) : IDrawable
{
    private static readonly Color Grid = Color.FromArgb("#DDE7F1");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private static readonly Color Muted = Color.FromArgb("#687A95");

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        canvas.StrokeColor = Grid;
        canvas.StrokeSize = 1;
        for (var line = 0; line < 4; line++)
        {
            var y = 24 + line * 42;
            canvas.DrawLine(34, y, dirtyRect.Width - 8, y);
            canvas.FontColor = Muted;
            canvas.FontSize = 11;
            canvas.DrawString(isBodyweight ? $"{(3 - line) * 10}" : $"{(3 - line) * 40}", 0, y - 7, 28, 16, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        var points = entries.Reverse().Take(5).Reverse().ToList();
        if (points.Count == 0) return;
        var max = Math.Max(1, points.Max(x => isBodyweight ? x.Repetitions ?? 0 : x.WeightKilograms ?? 0));
        var chartWidth = dirtyRect.Width - 50;
        var chartHeight = 126;
        var coordinates = points.Select((entry, index) => new PointF(42 + (points.Count == 1 ? chartWidth / 2 : index * chartWidth / (points.Count - 1)), 150 - (float)((isBodyweight ? entry.Repetitions ?? 0 : entry.WeightKilograms ?? 0) / max * chartHeight))).ToList();
        canvas.StrokeColor = Teal;
        canvas.StrokeSize = 3;
        for (var index = 1; index < coordinates.Count; index++) canvas.DrawLine(coordinates[index - 1], coordinates[index]);
        canvas.FillColor = Teal;
        foreach (var point in coordinates) canvas.FillCircle(point, 5);
    }
}
