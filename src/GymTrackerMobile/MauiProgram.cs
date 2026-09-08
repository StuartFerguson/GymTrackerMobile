using GymTrackerMobile.UI;
using GymTrackerMobile.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace GymTrackerMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddTransient<StartupPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<StartWorkoutPage>();
        builder.Services.AddTransient<LogActivityPage>();
        builder.Services.AddTransient<DashboardViewModel>();
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "gym-tracker.db");
        builder.Services.AddGymTrackerPersistence(databasePath);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
