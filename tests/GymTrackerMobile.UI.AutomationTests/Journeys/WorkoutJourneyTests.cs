using GymTrackerMobile.UI.AutomationTests.Infrastructure;
using GymTrackerMobile.UI.AutomationTests.Pages;

namespace GymTrackerMobile.UI.AutomationTests.Journeys;

public sealed class WorkoutJourneyTests : IClassFixture<AndroidDriverFixture>
{
    private readonly AndroidDriverFixture _fixture;

    public WorkoutJourneyTests(AndroidDriverFixture fixture) => _fixture = fixture;

    [AndroidFact]
    public void Starts_push_records_a_set_and_shows_the_summary()
    {
        var summary = new DashboardPage(_fixture.Driver!).OpenStartWorkout().ChoosePush()
            .EnterSet(1, "40", "8")
            .Complete();

        Assert.Equal("Push", summary.WorkoutName());
        Assert.Contains("1 /", summary.CompletedSets());
    }
}
