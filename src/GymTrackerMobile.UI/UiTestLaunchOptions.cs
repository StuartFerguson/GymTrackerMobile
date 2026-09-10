namespace GymTrackerMobile.UI;

public static class UiTestLaunchOptions
{
#if DEBUG
    public static bool IsEnabled { get; private set; }

    public static void Enable() => IsEnabled = true;
#else
    public static bool IsEnabled => false;
#endif
}
