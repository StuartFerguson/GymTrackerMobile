using Microsoft.Extensions.DependencyInjection;

namespace GymTrackerMobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Items.Add(new ShellContent
        {
            Route = GymTrackerMobile.UI.NavigationRoutes.Dashboard,
            Title = "Gym Tracker",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<GymTrackerMobile.UI.DashboardPage>())
        });
        Routing.RegisterRoute(GymTrackerMobile.UI.DashboardRoutes.StartWorkout, typeof(GymTrackerMobile.UI.StartWorkoutPage));
        Routing.RegisterRoute(GymTrackerMobile.UI.ActiveWorkoutRoutes.Page, typeof(GymTrackerMobile.UI.ActiveWorkoutPage));
        Routing.RegisterRoute(GymTrackerMobile.UI.DashboardRoutes.LogActivity, typeof(GymTrackerMobile.UI.LogActivityPage));
        Routing.RegisterRoute(GymTrackerMobile.UI.WeeklyPlanRoutes.Page, typeof(GymTrackerMobile.UI.WeeklyPlanPage));
        Routing.RegisterRoute(GymTrackerMobile.UI.NavigationRoutes.History, typeof(GymTrackerMobile.UI.HistoryPage));
        Routing.RegisterRoute(GymTrackerMobile.UI.NavigationRoutes.WorkoutSummary, typeof(GymTrackerMobile.UI.WorkoutSummaryPage));
        Routing.RegisterRoute(GymTrackerMobile.UI.NavigationRoutes.ExerciseProgress, typeof(GymTrackerMobile.UI.ExerciseProgressPage));
        Routing.RegisterRoute(GymTrackerMobile.UI.NavigationRoutes.BackupSettings, typeof(GymTrackerMobile.UI.BackupSettingsPage));
    }
}
