using GymTrackerMobile.Domain;
using GymTrackerMobile.Persistence.Backup;

namespace GymTrackerMobile.Persistence.Tests;

public sealed class BackupSerializationTests
{
    [Fact]
    public void Versioned_document_round_trips_nested_values_and_string_enums()
    {
        var setId = Guid.NewGuid();
        var document = new GymTrackerBackupDocument
        {
            SchemaVersion = 1,
            ExportedAtUtc = new DateTime(2026, 9, 10, 8, 0, 0, DateTimeKind.Utc),
            Exercises =
            [new ExerciseBackupDto
            {
                Id = Guid.NewGuid(), Name = "Bench", PrimaryMuscleGroup = "Chest",
                EquipmentType = "Barbell", WeightEntryConvention = WeightEntryConvention.TotalLoad,
                DefaultMinimumRepetitions = 8, DefaultMaximumRepetitions = 10,
                DefaultSetCount = 3, ExerciseMode = ExerciseMode.Barbell
            }],
            WorkoutSessions =
            [new WorkoutSessionBackupDto
            {
                Id = Guid.NewGuid(), TemplateId = Guid.NewGuid(), TemplateName = "Push",
                StartedAtUtc = DateTime.UtcNow,
                Exercises = [new WorkoutExerciseBackupDto
                {
                    Id = Guid.NewGuid(), ExerciseId = Guid.NewGuid(), ExerciseName = "Bench",
                    PrimaryMuscleGroup = "Chest", EquipmentType = "Barbell",
                    WeightEntryConvention = WeightEntryConvention.TotalLoad,
                    Sets = [new WorkoutSetBackupDto
                    {
                        Id = setId, SetNumber = 1, Status = SetStatus.Completed,
                        WeightKilograms = 80, Repetitions = 8, RecordedAtUtc = DateTime.UtcNow
                    }]
                }]
            }]
        };

        var json = GymTrackerBackupJson.Serialize(document);
        var restored = GymTrackerBackupJson.Deserialize(json);

        Assert.Contains("\"status\": \"Completed\"", json);
        Assert.Equal(1, restored.SchemaVersion);
        Assert.Equal(setId, restored.WorkoutSessions.Single().Exercises.Single().Sets.Single().Id);
        Assert.Equal(SetStatus.Completed, restored.WorkoutSessions.Single().Exercises.Single().Sets.Single().Status);
        Assert.Equal(DateTimeKind.Utc, restored.ExportedAtUtc.Kind);
    }
}
