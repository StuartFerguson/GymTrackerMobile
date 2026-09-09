namespace GymTrackerMobile.Persistence;

public interface IAppDataResetService
{
    Task ResetAsync(CancellationToken cancellationToken = default);
}
