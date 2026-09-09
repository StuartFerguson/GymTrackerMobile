using GymTrackerMobile.UI;

namespace GymTrackerMobile.UI.Tests;

public sealed class NavigationShellTests
{
    [Fact]
    public void Shell_destinations_include_every_primary_feature()
    {
        Assert.Equal(
            ["dashboard", "weekly-plan", "start-workout", "log-activity", "history", "exercise-progress", "backup-settings"],
            NavigationRoutes.All);
    }

    [Fact]
    public void Destination_pages_have_stable_titles_and_empty_state_copy()
    {
        Assert.Equal("History", DestinationPageContent.History.Title);
        Assert.Equal("Exercise progress", DestinationPageContent.ExerciseProgress.Title);
        Assert.Equal("Backup & Settings", DestinationPageContent.BackupSettings.Title);
        Assert.All(
            [DestinationPageContent.History, DestinationPageContent.ExerciseProgress, DestinationPageContent.BackupSettings],
            destination => Assert.False(string.IsNullOrWhiteSpace(destination.EmptyStateMessage)));
    }
}
