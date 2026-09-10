using System.Text;
using Microsoft.Maui.Storage;
using GymTrackerMobile.Persistence.Backup;

namespace GymTrackerMobile.UI;

public sealed class MauiBackupFileTransfer : IBackupFileTransfer
{
    public async Task<BackupFile?> PickBackupAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose a Gym Tracker backup" });
        cancellationToken.ThrowIfCancellationRequested();
        if (result is null) return null;
        await using var stream = await result.OpenReadAsync();
        using var reader = new StreamReader(stream);
        return new BackupFile(result.FileName, await reader.ReadToEndAsync(cancellationToken));
    }

    public async Task<string?> SaveBackupAsync(string json, string suggestedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var directory = Path.Combine(FileSystem.AppDataDirectory, "backups");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, suggestedName);
        await File.WriteAllTextAsync(path, json, Encoding.UTF8, cancellationToken);
        return path;
    }
}
