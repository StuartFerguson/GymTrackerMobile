using GymTrackerMobile.UI.AutomationTests.Infrastructure;
using GymTrackerMobile.UI.AutomationTests.Pages;

namespace GymTrackerMobile.UI.AutomationTests.Journeys;

[Collection("Android UI")]
public sealed class RecommendationJourneyTests
{
    private readonly AndroidDriverFixture _fixture;

    public RecommendationJourneyTests(AndroidDriverFixture fixture) => _fixture = fixture;

    [AndroidFact]
    public void Accepts_a_recommendation()
    {
        _fixture.ResetApplication();
        var workout = new DashboardPage(_fixture.Driver!).OpenStartWorkout().ChoosePush().AcceptRecommendation();

        Assert.Equal("Accepted", workout.RecommendationOutcome());
    }

    [AndroidFact]
    public void Edits_a_recommendation()
    {
        _fixture.ResetApplication();
        var workout = new DashboardPage(_fixture.Driver!).OpenStartWorkout().ChoosePush().EditRecommendation("42.5");

        Assert.Equal("Edited", workout.RecommendationOutcome());
        Assert.Equal("42.5", workout.Weight(1));
    }

    [AndroidFact]
    public void Ignores_a_recommendation()
    {
        _fixture.ResetApplication();
        var workout = new DashboardPage(_fixture.Driver!).OpenStartWorkout().ChoosePush().IgnoreRecommendation();

        Assert.Equal("Ignored", workout.RecommendationOutcome());
    }
}
