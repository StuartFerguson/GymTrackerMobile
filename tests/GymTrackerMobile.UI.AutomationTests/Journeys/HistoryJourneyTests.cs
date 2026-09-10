using GymTrackerMobile.UI.AutomationTests.Infrastructure;
using GymTrackerMobile.UI.AutomationTests.Pages;

namespace GymTrackerMobile.UI.AutomationTests.Journeys;

[Collection("Android UI")]
public sealed class HistoryJourneyTests
{
    private readonly AndroidDriverFixture _fixture;

    public HistoryJourneyTests(AndroidDriverFixture fixture) => _fixture = fixture;

    [AndroidFact]
    public void Opens_exercise_progress_from_stored_workout_history()
    {
        var progress = new DashboardPage(_fixture.Driver!).OpenHistory().OpenFirstWorkout().OpenProgress();

        Assert.False(string.IsNullOrWhiteSpace(progress.ExerciseName()));
    }
}
