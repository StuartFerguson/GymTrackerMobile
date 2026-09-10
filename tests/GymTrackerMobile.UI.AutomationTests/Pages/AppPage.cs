using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;

namespace GymTrackerMobile.UI.AutomationTests.Pages;

public abstract class AppPage(AndroidDriver driver)
{
    protected AndroidDriver Driver { get; } = driver;

    protected IWebElement Find(string automationId, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(20));
        Exception? lastError = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var element = Driver.FindElement(MobileBy.AccessibilityId(automationId));
                if (element.Displayed) return element;
            }
            catch (Exception error) when (error is WebDriverException or InvalidOperationException)
            {
                lastError = error;
            }

            Thread.Sleep(250);
        }

        throw new WebDriverTimeoutException($"Control '{automationId}' was not visible.", lastError);
    }

    protected void Tap(string automationId) => Find(automationId).Click();
}
