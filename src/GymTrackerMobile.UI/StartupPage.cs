using Microsoft.Maui.Controls;

namespace GymTrackerMobile.UI;

public sealed class StartupPage : ContentPage
{
    public StartupPage()
    {
        Title = "Gym Tracker";
        Content = new VerticalStackLayout
        {
            Padding = new Thickness(24),
            Spacing = 16,
            Children =
            {
                new Label
                {
                    Text = "Gym Tracker",
                    FontSize = 28,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label
                {
                    Text = "Ready for your next workout.",
                    HorizontalOptions = LayoutOptions.Center
                }
            }
        };
    }
}
