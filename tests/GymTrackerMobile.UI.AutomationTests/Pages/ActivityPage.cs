using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public sealed class ActivityPage(AndroidDriver driver) : AppPage(driver)
{
    public ActivityPage ChooseWalking()
    {
        Tap("activity-type-walking");
        return this;
    }

    public ActivityPage EnterWalkingDetails(string duration, string distance)
    {
        var durationInput = Find("activity-duration");
        durationInput.Clear();
        durationInput.SendKeys(duration);
        var distanceInput = Find("activity-distance");
        distanceInput.Clear();
        distanceInput.SendKeys(distance);
        return this;
    }

    public HistoryPage SaveAndOpenHistory()
    {
        Tap("activity-save");
        Driver.Navigate().Back();
        Find("dashboard-history").Click();
        return new HistoryPage(Driver);
    }
}
