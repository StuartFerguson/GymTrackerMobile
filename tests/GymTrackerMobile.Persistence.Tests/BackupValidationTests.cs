using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence.Backup;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class BackupValidationTests
{
    [Fact]
    public void Rejects_unsupported_version_with_field_path()
    {
        var result = new GymTrackerBackupValidator().Validate(GymTrackerBackupJson.Serialize(new GymTrackerBackupDocument
        {
            SchemaVersion = 99,
            ExportedAtUtc = DateTime.UtcNow
        }));

        Assert.Contains(result.Errors, error => error.Path == "schemaVersion");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Rejects_invalid_ids_enums_dates_and_values()
    {
        var result = new GymTrackerBackupValidator().Validate(GymTrackerBackupJson.Serialize(new GymTrackerBackupDocument
        {
            SchemaVersion = 1,
            ExportedAtUtc = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
            Exercises = [new ExerciseBackupDto { Id = Guid.Empty, DefaultMinimumRepetitions = 0, DefaultMaximumRepetitions = -1, DefaultSetCount = 0, ExerciseMode = (ExerciseMode)99 }]
        }));

        Assert.Contains(result.Errors, error => error.Path == "exercises[0].id");
        Assert.Contains(result.Errors, error => error.Path == "exercises[0].exerciseMode");
        Assert.Contains(result.Errors, error => error.Path == "exportedAtUtc");
    }

    [Fact]
    public void Rejects_malformed_json_without_throwing()
    {
        var result = new GymTrackerBackupValidator().Validate("{not-json");

        Assert.False(result.IsValid);
        Assert.Equal("$", result.Errors.Single().Path);
    }

    [Fact]
    public void Rejects_incomplete_document_with_missing_fields()
    {
        var result = new GymTrackerBackupValidator().Validate("{\"schemaVersion\":1}");

        Assert.Contains(result.Errors, error => error.Path == "workoutTemplates");
        Assert.Contains(result.Errors, error => error.Path == "backupMetadata");
    }
}
