using GymTrackerMobile.UI.AutomationTests.Infrastructure;
using GymTrackerMobile.UI.AutomationTests.Pages;

namespace GymTrackerMobile.UI.AutomationTests.Journeys;

[Collection("Android UI")]
public sealed class AppLaunchTests
{
    private readonly AndroidDriverFixture _fixture;

    public AppLaunchTests(AndroidDriverFixture fixture)
    {
        _fixture = fixture;
    }

    [AndroidFact]
    public void App_launches_to_the_start_workout_flow()
    {
        Assert.NotNull(_fixture.Driver);
        var page = new AppPageProbe(_fixture.Driver!);
        page.WaitForStartWorkout();
    }

    private sealed class AppPageProbe(OpenQA.Selenium.Appium.Android.AndroidDriver driver) : AppPage(driver)
    {
        public void WaitForStartWorkout() => Find("start-workout");
    }
}
