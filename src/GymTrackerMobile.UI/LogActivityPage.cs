using Microsoft.Maui.Controls;

namespace GymTrackerMobile.UI;

public sealed class LogActivityPage : ContentPage
{
    public LogActivityPage()
    {
        Title = "Log activity";
        Content = new VerticalStackLayout
        {
            Padding = 24,
            Children = { new Label { Text = "Log activity", FontSize = 28, FontAttributes = FontAttributes.Bold } }
        };
    }
}
