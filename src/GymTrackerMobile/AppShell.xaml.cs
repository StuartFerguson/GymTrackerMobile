using Microsoft.Extensions.DependencyInjection;

namespace GymTrackerMobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        Items.Add(new ShellContent
        {
            Title = "Gym Tracker",
            ContentTemplate = new DataTemplate(() => services.GetRequiredService<GymTrackerMobile.UI.StartupPage>())
        });
    }
}
