using GymTrackerMobile.Persistence;
using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class ActiveWorkoutPage : ContentPage, IQueryAttributable
{
    private readonly IWorkoutRepository _workouts;
    private readonly VerticalStackLayout _exercises = new() { Spacing = 10 };

    public ActiveWorkoutPage(IWorkoutRepository workouts)
    {
        _workouts = workouts;
        Title = "Active workout";
        BackgroundColor = Color.FromArgb("#F8FBFF");
        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20, 24),
                Spacing = 18,
                Children =
                {
                    new Label { Text = "Active Workout", FontSize = 32, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#102A50") },
                    new Label { Text = "Your workout is ready. Let’s get stronger.", FontSize = 18, TextColor = Color.FromArgb("#687A95") },
                    _exercises
                }
            }
        };
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("sessionId", out var queryValue) || !Guid.TryParse(queryValue?.ToString(), out var sessionId)) return;
        var session = await _workouts.GetActiveWorkoutAsync();
        if (session?.Id != sessionId) return;
        _exercises.Children.Clear();
        foreach (var exercise in session.Exercises.OrderBy(x => x.SortOrder))
        {
            _exercises.Children.Add(new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E2EBF5"),
                StrokeThickness = 1,
                Padding = new Thickness(16, 14),
                StrokeShape = new RoundRectangle { CornerRadius = 18 },
                Content = new VerticalStackLayout
                {
                    Children =
                    {
                        new Label { Text = exercise.ExerciseName, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#102A50") },
                        new Label { Text = $"{exercise.PlannedSetCount} sets · {exercise.TargetMinimumRepetitions}-{exercise.TargetMaximumRepetitions} reps", FontSize = 16, TextColor = Color.FromArgb("#687A95") }
                    }
                }
            });
        }
    }
}
