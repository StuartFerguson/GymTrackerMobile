using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class StartupPage : ContentPage
{
    private static readonly Color Navy = Color.FromArgb("#071A32");
    private static readonly Color Lime = Color.FromArgb("#C7FF24");
    private static readonly Color Muted = Color.FromArgb("#9BAEC8");
    private readonly StartupViewModel _viewModel;
    private readonly Func<Task> _onReady;
    private readonly ActivityIndicator _loading = new() { Color = Lime, WidthRequest = 34, HeightRequest = 34 };
    private readonly Label _status = new() { TextColor = Muted, FontSize = 15, HorizontalTextAlignment = TextAlignment.Center };
    private readonly Button _retry = new()
    {
        Text = "Retry", TextColor = Navy, BackgroundColor = Lime, CornerRadius = 24,
        FontAttributes = FontAttributes.Bold, HeightRequest = 48, WidthRequest = 140, IsVisible = false
    };

    public StartupPage(StartupViewModel viewModel, Func<Task> onReady)
    {
        _viewModel = viewModel;
        _onReady = onReady;
        Title = "Gym Tracker";
        BackgroundColor = Navy;
        Shell.SetNavBarIsVisible(this, false);
        _retry.Clicked += async (_, _) => await StartAsync();

        Content = new Grid
        {
            Padding = new Thickness(28, 48),
            Children =
            {
                new Border
                {
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 36 },
                    Background = new LinearGradientBrush(
                        new GradientStopCollection
                        {
                            new GradientStop(Color.FromArgb("#173D50"), 0),
                            new GradientStop(Navy, 0.38f),
                            new GradientStop(Color.FromArgb("#102C3C"), 1)
                        }, new Point(0, 0), new Point(1, 1))
                },
                new VerticalStackLayout
                {
                    Spacing = 18, VerticalOptions = LayoutOptions.Center,
                    Children =
                    {
                        new Image { Source = "startup_mark.svg", WidthRequest = 132, HeightRequest = 132, HorizontalOptions = LayoutOptions.Center },
                        new Label
                        {
                            FormattedText = new FormattedString
                            {
                                Spans =
                                {
                                    new Span { Text = "Gym ", TextColor = Colors.White, FontSize = 42, FontAttributes = FontAttributes.Bold },
                                    new Span { Text = "Tracker", TextColor = Lime, FontSize = 42, FontAttributes = FontAttributes.Bold }
                                }
                            }, HorizontalTextAlignment = TextAlignment.Center
                        },
                        _status, _loading, _retry
                    }
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await StartAsync();
    }

    private async Task StartAsync()
    {
        Render();
        await _viewModel.InitializeAsync();
        Render();
        if (_viewModel.State.IsReady) await _onReady();
    }

    private void Render()
    {
        var state = _viewModel.State;
        _loading.IsVisible = state.IsLoading;
        _loading.IsRunning = state.IsLoading;
        _retry.IsVisible = state.ErrorMessage is not null;
        _status.Text = state.ErrorMessage is null ? "Getting things ready…" : "We couldn’t prepare your local data. Try again.";
    }
}
