namespace GymTrackerMobile.UI;

public static class WeeklyPlanRoutes
{
    public const string Page = "weekly-plan";
    public const string LogActivity = DashboardRoutes.LogActivity;

    public static string StartWorkout(Guid templateId) => $"{DashboardRoutes.StartWorkout}?templateId={templateId}";
}
