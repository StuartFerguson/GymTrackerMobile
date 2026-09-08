namespace GymTrackerMobile.Domain;

public sealed class UserSetting
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class BackupMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int SchemaVersion { get; set; }
    public DateTime? LastExportedAtUtc { get; set; }
    public DateTime? LastImportedAtUtc { get; set; }
    public string? LastBackupFileName { get; set; }
    public string? LastBackupFileIdentity { get; set; }
}
