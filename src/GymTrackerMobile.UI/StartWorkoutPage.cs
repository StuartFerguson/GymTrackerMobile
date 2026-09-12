using Microsoft.Maui.Controls.Shapes;

namespace GymTrackerMobile.UI;

public sealed class StartWorkoutPage : ContentPage, IQueryAttributable
{
    private static readonly Color Ink = Color.FromArgb("#102A50");
    private static readonly Color Muted = Color.FromArgb("#687A95");
    private static readonly Color Teal = Color.FromArgb("#169F9A");
    private readonly StartWorkoutViewModel _viewModel;
    private readonly Grid _templates = new() { ColumnSpacing = 10, RowSpacing = 10 };
    private readonly HorizontalStackLayout _styleSelector = new() { Spacing = 8, HorizontalOptions = LayoutOptions.Center };
    private readonly Button _start = new() { AutomationId = UiAutomationIds.StartWorkout, Text = "▶  Start Workout", FontSize = 20, FontAttributes = FontAttributes.Bold, BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 30, HeightRequest = 60 };
    private readonly Button _resume = new() { Text = "↻  Resume workout", FontSize = 20, FontAttributes = FontAttributes.Bold, BackgroundColor = Teal, TextColor = Colors.White, CornerRadius = 30, HeightRequest = 60, IsVisible = false };
    private readonly Label _error = new() { FontSize = 15, TextColor = Color.FromArgb("#B42318"), HorizontalTextAlignment = TextAlignment.Center, IsVisible = false };

    public StartWorkoutPage(StartWorkoutViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "Start workout";
        BackgroundColor = Color.FromArgb("#F8FBFF");
        _start.Command = viewModel.StartCommand;
        _resume.Clicked += async (_, _) => await _viewModel.ResumeWorkoutAsync();

        var content = new Grid { RowDefinitions = new RowDefinitionCollection { new(GridLength.Star), new(76), new(76) } };
        content.Add(BuildBody(), 0, 0);
        content.Add(_start, 0, 1);
        content.Add(BuildBottomNavigation(), 0, 2);
        Content = content;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync(_pendingTemplateId);
        Render();
    }

    private Guid? _pendingTemplateId;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("templateId", out var value) && Guid.TryParse(value?.ToString(), out var templateId))
        {
            _pendingTemplateId = templateId;
        }
    }

    private View BuildBody() => new ScrollView
    {
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(20, 22, 20, 28), Spacing = 18,
            Children = { BuildHeader(), BuildWelcome(), BuildHeading(), BuildIllustrationStyleSelector(), _resume, _templates, _error, BuildConsistencyBanner() }
        }
    };

    private View BuildIllustrationStyleSelector()
    {
        var container = new VerticalStackLayout { Spacing = 6 };
        container.Children.Add(new Label { Text = "Illustration style", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Ink, HorizontalTextAlignment = TextAlignment.Center });
        foreach (var style in Enum.GetValues<IllustrationStyle>())
        {
            var button = new Button { Text = style.ToString(), FontSize = 14, Padding = new Thickness(14, 5), HeightRequest = 38, CornerRadius = 19 };
            button.Clicked += async (_, _) =>
            {
                await _viewModel.SelectIllustrationStyleAsync(style);
                Render();
            };
            _styleSelector.Children.Add(button);
        }
        container.Children.Add(_styleSelector);
        return container;
    }

    private static View BuildHeader()
    {
        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(48) },
            RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) },
            RowSpacing = 2
        };
        header.Add(new Label { Text = "Gym Tracker", FontSize = 34, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 0);
        header.Add(new Label { Text = "Stronger Today\nA Healthier Tomorrow", FontSize = 21, TextColor = Muted }, 0, 1);
        var settings = new Image { Source = "dashboard_settings.svg", WidthRequest = 36, HeightRequest = 36, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
        header.Add(settings, 1, 0);
        Grid.SetRowSpan(settings, 2);
        return header;
    }

    private static View BuildWelcome() => new Border
    {
        BackgroundColor = Color.FromArgb("#EAF4FE"), StrokeThickness = 0, Padding = new Thickness(18), StrokeShape = new RoundRectangle { CornerRadius = 24 },
        Content = new HorizontalStackLayout
        {
            Spacing = 16,
            Children = { new Border { BackgroundColor = Color.FromArgb("#DCEAF7"), StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 48 }, Padding = 18, Content = new Image { Source = "start_welcome.svg", WidthRequest = 42, HeightRequest = 42 } }, new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center, Children = { new Label { Text = "Good to see you!", FontSize = 21, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = "Pick a workout and let’s get stronger.", FontSize = 17, TextColor = Muted } } } }
        }
    };

    private static View BuildHeading() => new VerticalStackLayout { Spacing = 2, Children = { new Label { Text = "Start Workout", FontSize = 30, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = "Choose a workout type to begin.", FontSize = 19, TextColor = Muted } } };

    private View BuildTemplateCard(StartWorkoutTemplate template, int index)
    {
        var selected = _viewModel.State.SelectedTemplateId == template.Id;
        var icon = new Border { BackgroundColor = template.IconBackgroundColor, StrokeThickness = 0, StrokeShape = new RoundRectangle { CornerRadius = 48 }, Padding = 16, Content = new Image { Source = template.IconSource, WidthRequest = 58, HeightRequest = 58 } };
        var cardGrid = new Grid { RowDefinitions = new RowDefinitionCollection { new(82), new(GridLength.Auto), new(GridLength.Auto) }, RowSpacing = 2 };
        cardGrid.Add(icon, 0, 0);
        cardGrid.Add(new Image { Source = "start_chevron.svg", WidthRequest = 22, HeightRequest = 22, HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center }, 0, 0);
        cardGrid.Add(new Label { Text = template.Name, FontSize = 21, FontAttributes = FontAttributes.Bold, TextColor = Ink }, 0, 1);
        cardGrid.Add(new Label { Text = template.Description, FontSize = 15, TextColor = Muted, LineBreakMode = LineBreakMode.TailTruncation }, 0, 2);
        var card = new Border { BackgroundColor = selected ? Color.FromArgb("#F0FBFA") : Colors.White, Stroke = selected ? Teal : Color.FromArgb("#E2EBF5"), StrokeThickness = selected ? 2 : 1, Padding = new Thickness(14, 16), StrokeShape = new RoundRectangle { CornerRadius = 20 }, Content = cardGrid };
        var select = new Button
        {
            AutomationId = template.Name.Equals("Push", StringComparison.OrdinalIgnoreCase) ? UiAutomationIds.StartTemplatePush : $"start-template-{template.Name.ToLowerInvariant().Replace(' ', '-')}",
            BackgroundColor = Colors.Transparent,
            BorderWidth = 0,
            CornerRadius = 20,
            Text = template.Name,
            TextColor = Colors.Transparent,
            Padding = 0,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };
        select.Clicked += (_, _) => { _viewModel.SelectTemplate(template.Id); Render(); };
        var container = new Grid { Children = { card, select } };
        Grid.SetColumn(container, index % 2);
        Grid.SetRow(container, index / 2);
        return container;
    }

    private static View BuildConsistencyBanner() => new Border
    {
        BackgroundColor = Color.FromArgb("#EAF4FE"), StrokeThickness = 0, Padding = new Thickness(18, 15), StrokeShape = new RoundRectangle { CornerRadius = 20 },
        Content = new HorizontalStackLayout { Spacing = 16, Children = { new Image { Source = "dashboard_progress.svg", WidthRequest = 42, HeightRequest = 42, VerticalOptions = LayoutOptions.Center }, new VerticalStackLayout { Spacing = 1, Children = { new Label { Text = "Consistency builds results.", FontSize = 17, FontAttributes = FontAttributes.Bold, TextColor = Ink }, new Label { Text = "Show up. Do the work.", FontSize = 16, TextColor = Muted } } } } }
    };

    private static View BuildBottomNavigation() => new Grid
    {
        BackgroundColor = Colors.White, Padding = new Thickness(12, 9), ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) },
        Children = { NavItem("dashboard_home.svg", "Workout", true, 0), NavItem("dashboard_progress.svg", "Progress", false, 1), NavItem("dashboard_history.svg", "History", false, 2), NavItem("dashboard_more.svg", "More", false, 3) }
    };

    private static View NavItem(string icon, string label, bool selected, int column)
    {
        var stack = new VerticalStackLayout { Spacing = 2, HorizontalOptions = LayoutOptions.Center, Children = { new Image { Source = icon, WidthRequest = 27, HeightRequest = 27, Opacity = selected ? 1 : 0.75 }, new Label { Text = label, FontSize = 13, TextColor = selected ? Teal : Muted, HorizontalTextAlignment = TextAlignment.Center } } };
        Grid.SetColumn(stack, column);
        return stack;
    }

    private void Render()
    {
        _templates.Children.Clear();
        _templates.RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto), new(GridLength.Auto) };
        _templates.ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) };
        for (var index = 0; index < _viewModel.State.Templates.Count; index++) _templates.Children.Add(BuildTemplateCard(_viewModel.State.Templates[index], index));
        // Keep the native control in Android's accessibility tree even before a template is selected.
        // StartWorkoutViewModel.StartWorkoutAsync guards the command when starting is not allowed.
        _start.IsEnabled = true;
        _start.Opacity = _viewModel.State.CanStart ? 1 : 0.6;
        _resume.IsVisible = _viewModel.State.CanResume;
        _resume.Text = _viewModel.State.HasActiveWorkout ? $"↻  Resume {_viewModel.State.ActiveWorkoutName}" : "↻  Resume workout";
        _start.Text = _viewModel.State.IsStarting ? "Starting…" : _viewModel.State.SelectedTemplateId is null ? "Select a workout" : $"▶  Start {_viewModel.State.SelectedTemplateName}";
        _error.Text = _viewModel.State.ErrorMessage;
        _error.IsVisible = _viewModel.State.ErrorMessage is not null;
        for (var index = 0; index < _styleSelector.Children.Count; index++)
        {
            if (_styleSelector.Children[index] is not Button button) continue;
            var style = Enum.GetValues<IllustrationStyle>()[index];
            button.BackgroundColor = style == _viewModel.State.IllustrationStyle ? Teal : Color.FromArgb("#EAF4FE");
            button.TextColor = style == _viewModel.State.IllustrationStyle ? Colors.White : Ink;
        }
    }
}
