using GymTrackerMobile.UI.AutomationTests.Infrastructure;
using GymTrackerMobile.UI.AutomationTests.Pages;

namespace GymTrackerMobile.UI.AutomationTests.Journeys;

public sealed class ActivityJourneyTests : IClassFixture<AndroidDriverFixture>
{
    private readonly AndroidDriverFixture _fixture;

    public ActivityJourneyTests(AndroidDriverFixture fixture) => _fixture = fixture;

    [AndroidFact]
    public void Logs_walking_and_finds_it_in_history()
    {
        var history = new DashboardPage(_fixture.Driver!).OpenActivityLog()
            .ChooseWalking()
            .EnterWalkingDetails("25", "2.5")
            .SaveAndOpenHistory();

        Assert.Equal("Walking", history.ItemName(0));
        Assert.Contains("2.5 km", history.ItemDetails(0));
    }
}
