namespace GymTrackerMobile.UI;

public static class NavigationRoutes
{
    public const string Dashboard = "dashboard";
    public const string WeeklyPlan = "weekly-plan";
    public const string StartWorkout = "start-workout";
    public const string LogActivity = "log-activity";
    public const string History = "history";
    public const string ExerciseProgress = "exercise-progress";
    public const string BackupSettings = "backup-settings";
    public const string WorkoutSummary = "workout-summary";
    public const string ActivitySummary = "activity-summary";

    public static IReadOnlyList<string> All { get; } =
    [Dashboard, WeeklyPlan, StartWorkout, LogActivity, History, ExerciseProgress, BackupSettings];
}
