using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class ActivitySummaryPage(ActivitySummaryViewModel viewModel) : ContentPage, IQueryAttributable
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private Guid? _activityId;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("activityId", out var value) && Guid.TryParse(value?.ToString(), out var id)) _activityId = id;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_activityId is Guid id)
        {
            await viewModel.LoadAsync(id);
            Render();
        }
    }

    private void Render()
    {
        if (viewModel.State.ErrorMessage is not null)
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 12,
                Children = { new Label { Text = viewModel.State.ErrorMessage, TextColor = Ink, FontSize = 18 }, BuildBackButton() }
            };
            return;
        }

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = new Thickness(20, 18, 20, 28),
                Spacing = 16,
                Children = { BuildHeader(), BuildActivityCard(), BuildNotes() }
            }
        };
    }

    private View BuildHeader() => new Grid
    {
        ColumnDefinitions = new ColumnDefinitionCollection { new(48), new(GridLength.Star) },
        Children = { BuildBackButton(), new Label { Text = "Activity Details", FontSize = 28, FontAttributes = FontAttributes.Bold, TextColor = Ink, VerticalTextAlignment = TextAlignment.Center } }
    };

    private View BuildBackButton()
    {
        var back = new Label { Text = "‹", FontSize = 42, TextColor = Ink, VerticalTextAlignment = TextAlignment.Center };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, _) => await Shell.Current.GoToAsync("..");
        back.GestureRecognizers.Add(tap);
        return back;
    }

    private View BuildActivityCard() => new Border
    {
        BackgroundColor = Colors.White,
        Stroke = Color.FromArgb("#E2EBF5"),
        StrokeThickness = 1,
        Padding = new Thickness(20, 18),
        StrokeShape = new RoundRectangle { CornerRadius = 20 },
        Content = new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label { Text = viewModel.State.ActivityType, FontSize = 30, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                new Label { Text = $"{viewModel.State.ActivityDateUtc?.ToLocalTime():ddd, dd MMM yyyy · HH:mm}", FontSize = 17, TextColor = Muted },
                new Label { Text = viewModel.State.Details, FontSize = 20, TextColor = Teal }
            }
        }
    };

    private View BuildNotes() => new Border
    {
        IsVisible = !string.IsNullOrWhiteSpace(viewModel.State.Notes),
        BackgroundColor = Colors.White,
        Stroke = Color.FromArgb("#E2EBF5"),
        StrokeThickness = 1,
        Padding = 18,
        StrokeShape = new RoundRectangle { CornerRadius = 18 },
        Content = new VerticalStackLayout
        {
            Spacing = 4,
            Children = { new Label { Text = "Notes", FontSize = 19, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = viewModel.State.Notes, FontSize = 16, TextColor = Muted } }
        }
    };
}
