using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class WeeklyPlanViewModel
{
    private readonly Func<string, Task> _navigate;
    private readonly IllustrationPreferenceViewModel? _illustrationPreference;

    public WeeklyPlanViewModel(
        IReadOnlyList<WeeklyPlanTemplateSummary> templates,
        Func<string, Task>? navigate = null,
        IllustrationPreferenceViewModel? illustrationPreference = null)
    {
        _navigate = navigate ?? (route => Shell.Current.GoToAsync(route));
        State = WeeklyPlanStateBuilder.Build(templates);
        _illustrationPreference = illustrationPreference;
    }

    public WeeklyPlanViewModel(
        IWorkoutRepository workouts,
        IDatabaseInitializer databaseInitializer,
        Func<string, Task>? navigate = null,
        IllustrationPreferenceViewModel? illustrationPreference = null)
        : this([], navigate, illustrationPreference)
    {
        _workouts = workouts;
        _databaseInitializer = databaseInitializer;
    }

    private readonly IWorkoutRepository? _workouts;
    private readonly IDatabaseInitializer? _databaseInitializer;

    public WeeklyPlanState State { get; private set; }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (_workouts is null || _databaseInitializer is null) return;

        await _databaseInitializer.InitializeAsync(cancellationToken);
        if (_illustrationPreference is not null) await _illustrationPreference.LoadAsync(cancellationToken);
        var templates = await _workouts.GetTemplatesAsync(cancellationToken);
        State = WeeklyPlanStateBuilder.Build(templates.Select(x => new WeeklyPlanTemplateSummary(x.Id, x.Name)).ToList(), _illustrationPreference?.SelectedStyle ?? IllustrationStyle.Neutral);
    }

    public Task StartWorkoutAsync(WeeklyPlanDay day) =>
        day.CanStartWorkout && day.TemplateId is Guid templateId
            ? _navigate(WeeklyPlanRoutes.StartWorkout(templateId))
            : Task.CompletedTask;

    public Task LogActivityAsync(WeeklyPlanDay day) =>
        day.CanLogActivity ? _navigate(WeeklyPlanRoutes.LogActivity) : Task.CompletedTask;
}
