using GymTrackerMobile.UI.AutomationTests.Infrastructure;
using GymTrackerMobile.UI.AutomationTests.Pages;

namespace GymTrackerMobile.UI.AutomationTests.Journeys;

[Collection("Android UI")]
public sealed class ValidationJourneyTests
{
    private readonly AndroidDriverFixture _fixture;

    public ValidationJourneyTests(AndroidDriverFixture fixture) => _fixture = fixture;

    [AndroidFact]
    public void Keeps_workout_input_after_invalid_submission()
    {
        _fixture.ResetApplication();
        var workout = new DashboardPage(_fixture.Driver!).OpenStartWorkout().ChoosePush().EnterSet(1, "-5", "");

        Assert.Equal("-5", workout.Weight(1));
        Assert.False(string.IsNullOrWhiteSpace(workout.ErrorMessage()));
    }
}
