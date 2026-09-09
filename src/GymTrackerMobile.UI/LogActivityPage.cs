using GymTrackerMobile.Domain;
using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class LogActivityPage : ContentPage
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private readonly ActivityLogViewModel _viewModel;
    private readonly DatePicker _date = new() { Format = "dd MMM yyyy", MaximumDate = DateTime.Today };
    private readonly Entry _duration = NumberEntry("e.g. 45");
    private readonly Entry _distance = NumberEntry("e.g. 5.25");
    private readonly Entry _steps = NumberEntry("e.g. 6200");
    private readonly Editor _notes = new() { Placeholder = "How did it feel?", AutoSize = EditorAutoSizeOption.TextChanges, MinimumHeightRequest = 90 };
    private readonly Button _save = new() { Text = "Save activity", FontSize = 18, FontAttributes = FontAttributes.Bold, BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 26, HeightRequest = 54 };
    private readonly Label _status = new() { FontSize = 15, HorizontalTextAlignment = TextAlignment.Center, IsVisible = false };
    private readonly VerticalStackLayout _errors = new() { Spacing = 4 };
    private readonly Grid _typeSelector = new() { ColumnSpacing = 8, ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) } };
    private readonly List<Border> _typeCards = [];
    private readonly List<Label> _typeLabels = [];
    private readonly Label _typeError = ErrorLabel();
    private readonly Label _dateError = ErrorLabel();
    private readonly Label _durationError = ErrorLabel();
    private readonly Label _distanceError = ErrorLabel();
    private readonly Label _stepsError = ErrorLabel();
    private readonly Entry _poolLength = NumberEntry("e.g. 25");
    private readonly Entry _poolLengths = NumberEntry("e.g. 40");
    private readonly Label _poolLengthError = ErrorLabel();
    private readonly Label _poolLengthsError = ErrorLabel();
    private readonly Label _swimmingDistance = new() { FontSize = 14, TextColor = Muted };
    private View _distanceField = new VerticalStackLayout();
    private View _stepsField = new VerticalStackLayout();
    private View _swimmingFields = new VerticalStackLayout { IsVisible = false };

    public LogActivityPage(ActivityLogViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "Log activity";
        BackgroundColor = Color.FromArgb("#F8FBFF");
        _viewModel.SetDate(_date.Date ?? DateTime.Today);
        _date.DateSelected += (_, args) => _viewModel.SetDate(args.NewDate ?? DateTime.Today);
        _duration.TextChanged += (_, _) => _viewModel.DurationText = _duration.Text ?? string.Empty;
        _distance.TextChanged += (_, _) => _viewModel.DistanceText = _distance.Text ?? string.Empty;
        _steps.TextChanged += (_, _) => _viewModel.StepsText = _steps.Text ?? string.Empty;
        _poolLength.TextChanged += (_, _) => { _viewModel.PoolLengthText = _poolLength.Text ?? string.Empty; Render(); };
        _poolLengths.TextChanged += (_, _) => { _viewModel.PoolLengthsText = _poolLengths.Text ?? string.Empty; Render(); };
        _notes.TextChanged += (_, _) => _viewModel.NotesText = _notes.Text ?? string.Empty;
        _save.Clicked += async (_, _) => await SaveAsync();
        var content = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(76) } };
        content.Add(BuildBody(), 0, 0);
        content.Add(BuildBottomNavigation(), 0, 1);
        Content = content;
    }

    private View BuildBody() => new ScrollView
    {
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(20, 18, 20, 28), Spacing = 16,
            Children =
            {
                BuildHeader(),
                new Label { Text = "Activity Log", FontSize = 31, FontAttributes = FontAttributes.Bold, TextColor = Ink, Margin = new Thickness(0, 18, 0, -10) },
                new Label { Text = "Track your workouts and stay consistent.", FontSize = 18, TextColor = Muted },
                BuildFormCard()
            }
        }
    };

    private static View BuildHeader()
    {
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(46) },
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) }
        };
        header.Add(new Label { Text = "Gym Tracker", FontSize = 29, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0);
        header.Add(new Label { Text = "Log your activity. Build a healthier you.", FontSize = 16, TextColor = Muted }, 0, 1);
        var calendar = new Image { Source = "plan_calendar.svg", WidthRequest = 32, HeightRequest = 32, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        header.Add(calendar, 1, 0);
        Microsoft.Maui.Controls.Grid.SetRowSpan(calendar, 2);
        return header;
    }

    private View BuildFormCard()
    {
        var metrics = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) }, ColumnSpacing = 14, RowSpacing = 12 };
        metrics.Add(BuildMetric("Duration", _duration, _durationError), 0, 0);
        _distanceField = BuildMetric("Distance (km)", _distance, _distanceError);
        metrics.Add(_distanceField, 1, 0);
        _stepsField = BuildMetric("Steps", _steps, _stepsError);
        metrics.Add(_stepsField, 0, 1);

        var swimGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
            ColumnSpacing = 14
        };
        swimGrid.Add(BuildMetric("Pool length (m, optional)", _poolLength, _poolLengthError), 0, 0);
        swimGrid.Add(BuildMetric("Lengths (optional)", _poolLengths, _poolLengthsError), 1, 0);
        _swimmingFields = new VerticalStackLayout
        {
            Spacing = 8,
            IsVisible = false,
            Children =
            {
                swimGrid,
                _swimmingDistance
            }
        };

        var content = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                new Label { Text = "Activity Type", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink },
                BuildTypeSelector(), _typeError,
                FieldLabel("Date"), BuildOutlinedInput(_date), _dateError,
                metrics,
                _swimmingFields,
                FieldLabel("Notes (optional)"), BuildOutlinedInput(_notes),
                _errors, _status,
                _save
            }
        };
        return new Border { BackgroundColor = Colors.White, StrokeThickness = 0, Padding = new Thickness(14, 18, 14, 16), StrokeShape = new RoundRectangle { CornerRadius = 24 }, Content = content };
    }

    private View BuildTypeSelector()
    {
        var types = Enum.GetValues<ActivityType>();
        for (var index = 0; index < types.Length; index++)
        {
            var type = types[index];
            var label = new Label { Text = type.ToString(), FontSize = 12, FontAttributes = FontAttributes.Bold, TextColor = Ink, VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Center };
            var card = new Border
            {
                BackgroundColor = Color.FromArgb("#F1F4FA"), StrokeThickness = 0, Padding = new Thickness(4, 6), HeightRequest = 58,
                StrokeShape = new RoundRectangle { CornerRadius = 14 },
                Content = new HorizontalStackLayout
                {
                    Spacing = 4, HorizontalOptions = LayoutOptions.Center,
                    Children = { new Image { Source = type == ActivityType.Swimming ? "activity_swim.png" : "activity_walk.png", WidthRequest = 30, HeightRequest = 30, Aspect = Aspect.AspectFit }, label }
                }
            };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (_, _) => { _viewModel.SelectActivityType(type); Render(); };
            card.GestureRecognizers.Add(tap);
            _typeCards.Add(card);
            _typeLabels.Add(label);
            _typeSelector.Add(card, index, 0);
        }
        return _typeSelector;
    }

    private static View BuildMetric(string label, Entry entry, Label error) => new VerticalStackLayout
    {
        Spacing = 5,
        Children = { FieldLabel(label), BuildOutlinedInput(entry), error }
    };

    private static Border BuildOutlinedInput(View input) => new Border
    {
        Stroke = Color.FromArgb("#CDD8E8"), StrokeThickness = 1, BackgroundColor = Color.FromArgb("#FCFDFF"), Padding = new Thickness(10, 2), StrokeShape = new RoundRectangle { CornerRadius = 14 }, Content = input
    };

    private async Task SaveAsync()
    {
        await _viewModel.SaveAsync();
        Render();
    }

    private void Render()
    {
        _errors.Children.Clear();
        SetError(_typeError, _viewModel.State.ActivityTypeError);
        SetError(_dateError, _viewModel.State.DateError);
        SetError(_durationError, _viewModel.State.DurationError);
        SetError(_distanceError, _viewModel.State.DistanceError);
        SetError(_stepsError, _viewModel.State.StepsError);
        SetError(_poolLengthError, _viewModel.State.PoolLengthError);
        SetError(_poolLengthsError, _viewModel.State.PoolLengthsError);
        AddError(_viewModel.State.ErrorMessage);
        var swimming = _viewModel.State.ActivityType == ActivityType.Swimming;
        _distanceField.IsVisible = !swimming;
        _stepsField.IsVisible = !swimming;
        _swimmingFields.IsVisible = swimming;
        _swimmingDistance.Text = BuildSwimmingDistanceText();
        _status.Text = _viewModel.State.IsSaved ? "Activity saved to your history." : string.Empty;
        _status.TextColor = _viewModel.State.IsSaved ? Color.FromArgb("#087443") : Color.FromArgb("#B42318");
        _status.IsVisible = _viewModel.State.IsSaved;
        _save.IsEnabled = !_viewModel.State.IsSaving;
        _save.Text = _viewModel.State.IsSaving ? "Saving…" : "Save activity";
        for (var index = 0; index < _typeCards.Count; index++)
        {
            var type = Enum.GetValues<ActivityType>()[index];
            var selected = type == _viewModel.State.ActivityType;
            _typeCards[index].BackgroundColor = selected ? Teal : Color.FromArgb("#F1F4FA");
            _typeLabels[index].TextColor = selected ? Colors.White : Ink;
        }
    }

    private void AddError(string? message)
    {
        if (!string.IsNullOrWhiteSpace(message)) _errors.Children.Add(new Label { Text = message, TextColor = Color.FromArgb("#B42318"), FontSize = 14 });
    }

    private static void SetError(Label label, string? message)
    {
        label.Text = message;
        label.IsVisible = !string.IsNullOrWhiteSpace(message);
    }

    private string BuildSwimmingDistanceText()
    {
        if (!int.TryParse(_viewModel.PoolLengthText, out var poolLength) || !int.TryParse(_viewModel.PoolLengthsText, out var lengths) || poolLength <= 0 || lengths <= 0)
            return "Distance will be calculated from your pool length and lengths.";
        return $"Calculated distance: {poolLength * lengths / 1000d:0.###} km";
    }

    private static Label FieldLabel(string text) => new() { Text = text, FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Ink, Margin = new Thickness(0, 4, 0, -4) };
    private static Label ErrorLabel() => new() { TextColor = Color.FromArgb("#B42318"), FontSize = 14, IsVisible = false };
    private static Entry NumberEntry(string placeholder) => new() { Placeholder = placeholder, Keyboard = Keyboard.Numeric };

    private static View BuildBottomNavigation() => new Grid
    {
        BackgroundColor = Colors.White, Padding = new Thickness(12, 9),
        ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) },
        Children = { NavItem("dashboard_home.svg", "Workout", false, 0), NavItem("dashboard_progress.svg", "Progress", false, 1), NavItem("dashboard_history.svg", "History", true, 2), NavItem("dashboard_more.svg", "More", false, 3) }
    };

    private static View NavItem(string icon, string label, bool selected, int column)
    {
        var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Image { Source = icon, WidthRequest = 27, HeightRequest = 27, Opacity = selected ? 1 : 0.75 }, new Label { Text = label, FontSize = 13, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } };
        Grid.SetColumn(stack, column);
        return stack;
    }
}
