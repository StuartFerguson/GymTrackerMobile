using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymTrackerMobile.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGymTrackerPersistence(this IServiceCollection services, string databasePath)
    {
        services.AddDbContext<GymTrackerDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
        services.AddScoped<DatabaseInitializer>();
        services.AddScoped<IDatabaseInitializer>(services => services.GetRequiredService<DatabaseInitializer>());
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IBackupMetadataRepository, BackupMetadataRepository>();
        return services;
    }
}
