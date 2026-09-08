namespace GymTrackerMobile.Persistence;

public interface ISettingsRepository
{
    Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default);
    Task SetSettingAsync(string key, string value, CancellationToken cancellationToken = default);
}
