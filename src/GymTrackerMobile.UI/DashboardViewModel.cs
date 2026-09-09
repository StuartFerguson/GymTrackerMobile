using System.Windows.Input;
using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence;

namespace GymTrackerMobile.UI;

public sealed class DashboardViewModel
{
    private readonly IWorkoutRepository _workouts;
    private readonly IActivityRepository _activities;
    private readonly IDatabaseInitializer _databaseInitializer;
    private readonly Func<string, Task> _navigate;

    public DashboardViewModel(
        IWorkoutRepository workouts,
        IActivityRepository activities,
        IDatabaseInitializer databaseInitializer,
        Func<string, Task>? navigate = null)
    {
        _workouts = workouts;
        _activities = activities;
        _databaseInitializer = databaseInitializer;
        _navigate = navigate ?? (route => Shell.Current.GoToAsync(route));
        StartWorkoutCommand = new Command(async () => await StartWorkoutAsync());
        LogActivityCommand = new Command(async () => await LogActivityAsync());
        WeeklyPlanCommand = new Command(async () => await _navigate(DashboardRoutes.WeeklyPlan));
        ProgressCommand = new Command(async () => await _navigate(NavigationRoutes.ExerciseProgress));
        HistoryCommand = new Command(async () => await _navigate(NavigationRoutes.History));
        SettingsCommand = new Command(async () => await _navigate(NavigationRoutes.BackupSettings));
    }

    public ICommand StartWorkoutCommand { get; }
    public ICommand LogActivityCommand { get; }
    public ICommand WeeklyPlanCommand { get; }
    public ICommand ProgressCommand { get; }
    public ICommand HistoryCommand { get; }
    public ICommand SettingsCommand { get; }
    public DashboardState State { get; private set; } = new();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await _databaseInitializer.InitializeAsync(cancellationToken);
        var nowUtc = DateTime.UtcNow;
        var fromUtc = nowUtc.Date.AddDays(-30);
        var templates = await _workouts.GetTemplatesAsync(cancellationToken);
        var workouts = await _workouts.GetCompletedWorkoutsAsync(fromUtc, nowUtc.Date.AddDays(1), cancellationToken);
        var activities = await _activities.GetActivitiesAsync(fromUtc, nowUtc.Date.AddDays(1), cancellationToken);

        State = DashboardStateBuilder.Build(
            DateTime.Now.Date,
            templates.Select(x => new DashboardTemplateSummary(x.Id, x.Name)).ToList(),
            workouts.Where(x => x.CompletedAtUtc is not null)
                .Select(x => new DashboardWorkoutSummary(x.TemplateName, x.CompletedAtUtc!.Value))
                .ToList(),
            activities.Select(x => new DashboardActivitySummary(x.ActivityType, x.ActivityDateUtc, x.DurationMinutes)).ToList());
    }

    private Task StartWorkoutAsync() => _navigate(DashboardRoutes.StartWorkout);

    private Task LogActivityAsync() => _navigate(DashboardRoutes.LogActivity);
}
