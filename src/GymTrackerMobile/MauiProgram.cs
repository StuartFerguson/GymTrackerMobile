using GymTrackerMobile.UI;
using GymTrackerMobile.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Handlers;

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

        ConfigureScrollViewEdgeEffects();

        builder.Services.AddTransient<StartupPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<StartWorkoutPage>();
        builder.Services.AddTransient<ActiveWorkoutPage>();
        builder.Services.AddTransient<ActiveWorkoutViewModel>();
        builder.Services.AddTransient<LogActivityPage>();
        builder.Services.AddTransient<ActivityLogViewModel>();
        builder.Services.AddTransient<WeeklyPlanPage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<HistoryViewModel>();
        builder.Services.AddTransient<WorkoutSummaryPage>();
        builder.Services.AddTransient<WorkoutSummaryViewModel>();
        builder.Services.AddTransient<ActivitySummaryPage>();
        builder.Services.AddTransient<ActivitySummaryViewModel>();
        builder.Services.AddTransient<ExerciseProgressPage>();
        builder.Services.AddTransient<ExerciseProgressViewModel>();
        builder.Services.AddTransient<BackupSettingsPage>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<StartWorkoutViewModel>();
        builder.Services.AddTransient<IllustrationPreferenceViewModel>();
        builder.Services.AddTransient<DeveloperResetViewModel>();
        builder.Services.AddTransient<WeeklyPlanViewModel>();
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "gym-tracker.db");
        builder.Services.AddGymTrackerPersistence(databasePath);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void ConfigureScrollViewEdgeEffects()
    {
#if ANDROID
        ScrollViewHandler.Mapper.AppendToMapping("DisableEdgeOverscroll", (handler, _) =>
        {
            handler.PlatformView.OverScrollMode = Android.Views.OverScrollMode.Never;
        });
#elif IOS || MACCATALYST
        ScrollViewHandler.Mapper.AppendToMapping("DisableEdgeBounce", (handler, _) =>
        {
            handler.PlatformView.Bounces = false;
            handler.PlatformView.AlwaysBounceVertical = false;
        });
#endif
    }
}
