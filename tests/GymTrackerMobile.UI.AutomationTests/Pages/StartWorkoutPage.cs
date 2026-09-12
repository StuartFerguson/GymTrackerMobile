using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public sealed class StartWorkoutPage(AndroidDriver driver) : AppPage(driver)
{
    public ActiveWorkoutPage ChoosePush()
    {
        Tap("start-template-push");
        Tap("start-workout");
        return new ActiveWorkoutPage(Driver);
    }
}
