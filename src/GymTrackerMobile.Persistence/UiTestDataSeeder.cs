namespace GymTrackerMobile.Persistence;

public static class UiTestDataSeeder
{
    public static Task SeedAsync(GymTrackerDbContext context, CancellationToken cancellationToken = default) =>
        new AppDataResetService(context).ResetAsync(cancellationToken);
}
