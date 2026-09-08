using GymTrackerMobile.Domain;
using Microsoft.EntityFrameworkCore;

namespace GymTrackerMobile.Persistence;

public sealed class SettingsRepository(GymTrackerDbContext context) : ISettingsRepository
{
    public Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default) =>
        context.UserSettings.AsNoTracking().Where(x => x.Key == key).Select(x => (string?)x.Value).SingleOrDefaultAsync(cancellationToken);

    public async Task SetSettingAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        var setting = await context.UserSettings.SingleOrDefaultAsync(x => x.Key == key, cancellationToken);
        if (setting is null)
        {
            context.UserSettings.Add(new UserSetting { Key = key, Value = value, UpdatedAtUtc = DateTime.UtcNow });
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAtUtc = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
