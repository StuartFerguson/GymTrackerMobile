namespace GymTrackerMobile.UI;

public sealed record DestinationPageDescriptor(string Title, string EmptyStateMessage);

public static class DestinationPageContent
{
    public static DestinationPageDescriptor History { get; } =
        new("History", "Your completed workouts and activities will appear here.");

    public static DestinationPageDescriptor ExerciseProgress { get; } =
        new("Exercise progress", "Log a workout to start tracking exercise progress.");

    public static DestinationPageDescriptor BackupSettings { get; } =
        new("Backup & Settings", "Keep your training data safe and manage app preferences.");
}
