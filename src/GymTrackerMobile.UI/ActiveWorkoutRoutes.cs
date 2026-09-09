namespace GymTrackerMobile.UI;

public static class ActiveWorkoutRoutes
{
    public const string Page = "active-workout";

    public static string For(Guid sessionId) => $"{Page}?sessionId={sessionId}";
}
