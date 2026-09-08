using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymTrackerMobile.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActivityDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ActivityType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: true),
                    DistanceKilometres = table.Column<double>(type: "REAL", nullable: true),
                    Steps = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AveragePaceMinutesPerKilometre = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BackupMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    LastExportedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastImportedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastBackupFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                    LastBackupFileIdentity = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    PrimaryMuscleGroup = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EquipmentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WeightEntryConvention = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    DefaultMinimumRepetitions = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultMaximumRepetitions = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultSetCount = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseMode = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TemplateName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkoutSessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExerciseName = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    PrimaryMuscleGroup = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EquipmentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    WeightEntryConvention = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    TargetMinimumRepetitions = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetMaximumRepetitions = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedSetCount = table.Column<int>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutExercises_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkoutTemplateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetMinimumRepetitions = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetMaximumRepetitions = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedSetCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateExercises_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkoutExerciseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProposedWeightKilograms = table.Column<double>(type: "REAL", nullable: true),
                    ProposedMinimumRepetitions = table.Column<int>(type: "INTEGER", nullable: true),
                    ProposedMaximumRepetitions = table.Column<int>(type: "INTEGER", nullable: true),
                    ProposedSetCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Explanation = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_WorkoutExercises_WorkoutExerciseId",
                        column: x => x.WorkoutExerciseId,
                        principalTable: "WorkoutExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    WorkoutExerciseId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SetNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    WeightKilograms = table.Column<double>(type: "REAL", nullable: true),
                    Repetitions = table.Column<int>(type: "INTEGER", nullable: true),
                    Rpe = table.Column<int>(type: "INTEGER", nullable: true),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutSets_WorkoutExercises_WorkoutExerciseId",
                        column: x => x.WorkoutExerciseId,
                        principalTable: "WorkoutExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationOutcomes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecommendationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OutcomeType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    AppliedWeightKilograms = table.Column<double>(type: "REAL", nullable: true),
                    AppliedMinimumRepetitions = table.Column<int>(type: "INTEGER", nullable: true),
                    AppliedMaximumRepetitions = table.Column<int>(type: "INTEGER", nullable: true),
                    AppliedSetCount = table.Column<int>(type: "INTEGER", nullable: true),
                    RecordedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationOutcomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationOutcomes_Recommendations_RecommendationId",
                        column: x => x.RecommendationId,
                        principalTable: "Recommendations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityRecords_ActivityDateUtc_ActivityType",
                table: "ActivityRecords",
                columns: new[] { "ActivityDateUtc", "ActivityType" });

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationOutcomes_RecommendationId",
                table: "RecommendationOutcomes",
                column: "RecommendationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_WorkoutExerciseId",
                table: "Recommendations",
                column: "WorkoutExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateExercises_ExerciseId",
                table: "TemplateExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateExercises_WorkoutTemplateId_SortOrder",
                table: "TemplateExercises",
                columns: new[] { "WorkoutTemplateId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_Key",
                table: "UserSettings",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_WorkoutSessionId_SortOrder",
                table: "WorkoutExercises",
                columns: new[] { "WorkoutSessionId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_IsActive",
                table: "WorkoutSessions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSessions_StartedAtUtc",
                table: "WorkoutSessions",
                column: "StartedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutSets_WorkoutExerciseId_SetNumber",
                table: "WorkoutSets",
                columns: new[] { "WorkoutExerciseId", "SetNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplates_Name",
                table: "WorkoutTemplates",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityRecords");

            migrationBuilder.DropTable(
                name: "BackupMetadata");

            migrationBuilder.DropTable(
                name: "RecommendationOutcomes");

            migrationBuilder.DropTable(
                name: "TemplateExercises");

            migrationBuilder.DropTable(
                name: "UserSettings");

            migrationBuilder.DropTable(
                name: "WorkoutSets");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutTemplates");

            migrationBuilder.DropTable(
                name: "WorkoutExercises");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");
        }
    }
}
