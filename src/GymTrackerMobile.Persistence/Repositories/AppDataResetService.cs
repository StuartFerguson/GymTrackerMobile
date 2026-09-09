using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class AppDataResetService(GymTrackerDbContext context) : IAppDataResetService
{
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        context.ChangeTracker.Clear();

        await context.RecommendationOutcomes.ExecuteDeleteAsync(cancellationToken);
        await context.Recommendations.ExecuteDeleteAsync(cancellationToken);
        await context.WorkoutSets.ExecuteDeleteAsync(cancellationToken);
        await context.WorkoutExercises.ExecuteDeleteAsync(cancellationToken);
        await context.WorkoutSessions.ExecuteDeleteAsync(cancellationToken);
        await context.ActivityRecords.ExecuteDeleteAsync(cancellationToken);
        await context.UserSettings.ExecuteDeleteAsync(cancellationToken);
        await context.TemplateExercises.ExecuteDeleteAsync(cancellationToken);
        await context.WorkoutTemplates.ExecuteDeleteAsync(cancellationToken);
        await context.Exercises.ExecuteDeleteAsync(cancellationToken);
        await context.BackupMetadata.ExecuteDeleteAsync(cancellationToken);

        await SeedData.EnsureSeededAsync(context, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
