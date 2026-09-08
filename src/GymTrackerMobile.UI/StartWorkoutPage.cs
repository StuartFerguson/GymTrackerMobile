using Microsoft.Maui.Controls;

namespace GymTrackerMobile.UI;

public sealed class StartWorkoutPage : ContentPage
{
    public StartWorkoutPage()
    {
        Title = "Start workout";
        Content = new VerticalStackLayout
        {
            Padding = 24,
            Children = { new Label { Text = "Start workout", FontSize = 28, FontAttributes = FontAttributes.Bold } }
        };
    }
}
