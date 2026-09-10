using Microsoft.Extensions.DependencyInjection;

namespace GymTrackerMobile;

public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window? window = null;
        window = new Window(new GymTrackerMobile.UI.StartupPage(
            _services.GetRequiredService<GymTrackerMobile.UI.StartupViewModel>(),
            () =>
            {
                window!.Page = new AppShell(_services);
                return Task.CompletedTask;
            }));
        return window;
    }
}
