# EF Core Persistence Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add versioned, offline-first EF Core SQLite persistence for the Gym Tracker MVP in issue #22.

**Architecture:** `GymTrackerMobile.Domain` owns framework-independent entity/value types and enums. `GymTrackerMobile.Persistence` owns EF Core configuration, `GymTrackerDbContext`, migrations, initialization, seed data, and repositories. The MAUI app supplies the database path and registers persistence through a single composition-root extension; UI code never references EF Core.

**Tech Stack:** .NET SDK 10.0.400, .NET MAUI 10.0.20, EF Core SQLite 10.0.0, SQLite, C#, xUnit.

**Spec:** `docs/superpowers/specs/2026-09-08-efcore-persistence-design.md`

## Global Constraints

- Target reusable libraries and tests with `net10.0`.
- Use EF Core SQLite migrations as the authoritative schema history.
- Keep EF Core types inside `GymTrackerMobile.Persistence`.
- Store weights in kilograms initially and store timestamps as UTC `DateTime` values.
- Preserve historical workout snapshot values after template or catalogue changes.
- Persist active workout state after each meaningful change.
- Do not add account, backend, network, calendar, GPS, wearable, or cloud-sync dependencies.
- All persistence tests must run offline against isolated local SQLite databases.

---

### Task 1: Add EF Core dependencies and domain persistence contracts

**Files:**
- Modify: `src/GymTrackerMobile.Persistence/GymTrackerMobile.Persistence.csproj`
- Modify: `src/GymTrackerMobile.Domain/GymTrackerMobile.Domain.csproj`
- Modify: `tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj`
- Create: `src/GymTrackerMobile.Domain/WorkoutEntities.cs`
- Create: `src/GymTrackerMobile.Domain/ActivityEntities.cs`
- Create: `src/GymTrackerMobile.Domain/RecommendationEntities.cs`
- Create: `src/GymTrackerMobile.Domain/SettingsEntities.cs`
- Create: `src/GymTrackerMobile.Domain/Enums.cs`

**Interfaces:**
- Produces `Exercise`, `WorkoutTemplate`, `TemplateExercise`, `WorkoutSession`, `WorkoutExercise`, `WorkoutSet`, `ActivityRecord`, `Recommendation`, `RecommendationOutcome`, `UserSetting`, and `BackupMetadata` types for Persistence.
- Produces `SetStatus`, `ActivityType`, `ExerciseMode`, `WeightEntryConvention`, `RecommendationStatus`, and `RecommendationOutcomeType` enums.

- [ ] Add explicit EF Core 10.0.0 and EF Core Design 10.0.0 package references to Persistence; mark Design as `PrivateAssets=all`.
- [ ] Add `Microsoft.EntityFrameworkCore.Sqlite` 10.0.0 and `Microsoft.EntityFrameworkCore` 10.0.0 to the persistence test project.
- [ ] Define each entity with a `Guid Id`, UTC `DateTime` timestamps where applicable, nullable optional fields, and navigation collections only where they simplify repository queries.
- [ ] Model workout history as two levels: `WorkoutSession` owns `WorkoutExercise` snapshot rows, and each `WorkoutExercise` owns ordered `WorkoutSet` rows; snapshot rows contain copied exercise name, muscle group, equipment, convention, target minimum/maximum repetitions, and planned set count.
- [ ] Model template rows separately: `WorkoutTemplate` owns ordered `TemplateExercise` rows that reference the fixed catalogue `Exercise` by ID and contain target values.
- [ ] Run `dotnet restore GymTrackerMobile.sln` and confirm the new packages resolve without network-dependent test behavior.

### Task 2: Implement the EF Core model and database initialization

**Files:**
- Create: `src/GymTrackerMobile.Persistence/GymTrackerDbContext.cs`
- Create: `src/GymTrackerMobile.Persistence/Configurations/ExerciseConfiguration.cs`
- Create: `src/GymTrackerMobile.Persistence/Configurations/WorkoutConfiguration.cs`
- Create: `src/GymTrackerMobile.Persistence/Configurations/ActivityConfiguration.cs`
- Create: `src/GymTrackerMobile.Persistence/Configurations/RecommendationConfiguration.cs`
- Create: `src/GymTrackerMobile.Persistence/Configurations/SettingsConfiguration.cs`
- Create: `src/GymTrackerMobile.Persistence/DatabaseOptions.cs`
- Create: `src/GymTrackerMobile.Persistence/DatabaseInitializer.cs`
- Create: `src/GymTrackerMobile.Persistence/ServiceCollectionExtensions.cs`
- Create: `src/GymTrackerMobile.Persistence/SeedData.cs`

**Interfaces:**
- `GymTrackerDbContext : DbContext` exposes `DbSet<Exercise> Exercises`, `DbSet<WorkoutTemplate> WorkoutTemplates`, `DbSet<TemplateExercise> TemplateExercises`, `DbSet<WorkoutSession> WorkoutSessions`, `DbSet<WorkoutExercise> WorkoutExercises`, `DbSet<WorkoutSet> WorkoutSets`, `DbSet<ActivityRecord> ActivityRecords`, `DbSet<Recommendation> Recommendations`, `DbSet<RecommendationOutcome> RecommendationOutcomes`, `DbSet<UserSetting> UserSettings`, and `DbSet<BackupMetadata> BackupMetadata`.
- `DatabaseOptions` exposes `string DatabasePath`.
- `DatabaseInitializer.InitializeAsync(CancellationToken cancellationToken = default)` applies migrations, seeds defaults, and updates the application schema metadata only after successful completion.
- `IServiceCollection AddGymTrackerPersistence(this IServiceCollection services, string databasePath)` registers the context, initializer, and repositories.

- [ ] Configure table names, keys, required properties, enum conversions, UTC `DateTime` conversion, SQLite-safe numeric types, unique indexes, and foreign-key delete behavior in `OnModelCreating` and focused configuration classes.
- [ ] Add indexes for workout date, exercise history, activity date/type, template ordering, and active-session lookup; enforce one active session at a time with a filtered/partial SQLite index or initializer validation appropriate to EF SQLite support.
- [ ] Configure relationships so deleting a template or catalogue row cannot cascade into historical `WorkoutSession` or `WorkoutExercise` snapshots.
- [ ] Seed the 20 catalogue exercises from requirements sections 3-4, the Push/Pull/Legs/Full Body templates, template exercise ordering and targets, and the default weekly schedule through stable IDs in `SeedData`.
- [ ] Implement initialization with `Database.MigrateAsync(cancellationToken)`, followed by idempotent seed inserts and a metadata update. Do not wrap `MigrateAsync` in an explicit transaction.
- [ ] Register the database path with `UseSqlite` and keep the initializer callable from tests without creating a MAUI app.

### Task 3: Create and validate the initial EF migration

**Files:**
- Create: `src/GymTrackerMobile.Persistence/GymTrackerDbContextFactory.cs`
- Create: `src/GymTrackerMobile.Persistence/Migrations/202609080001_InitialPersistence.cs`
- Create: `src/GymTrackerMobile.Persistence/Migrations/202609080001_InitialPersistence.Designer.cs`
- Create: `src/GymTrackerMobile.Persistence/Migrations/GymTrackerDbContextModelSnapshot.cs`
- Modify: `src/GymTrackerMobile.Persistence/GymTrackerMobile.Persistence.csproj`

**Interfaces:**
- `GymTrackerDbContextFactory : IDesignTimeDbContextFactory<GymTrackerDbContext>` creates a context using a deterministic temporary SQLite path for `dotnet ef` tooling.
- EF migration `202609080001_InitialPersistence` creates all MVP tables, foreign keys, indexes, and the application schema metadata record.

- [ ] Add the design-time factory so `dotnet ef migrations add` does not depend on MAUI startup or platform services.
- [ ] Generate the initial migration with `dotnet ef migrations add InitialPersistence --project src/GymTrackerMobile.Persistence --startup-project src/GymTrackerMobile.Persistence`.
- [ ] Inspect the generated migration to verify all required entities and indexes are present and no network/backend tables are introduced.
- [ ] Ensure migration metadata and application-owned backup/schema metadata are distinct: EF tracks applied migration names, while `BackupMetadata` records the application schema number and backup state.
- [ ] Run `dotnet ef migrations list` and a fresh-database initializer test to confirm the migration applies successfully.

### Task 4: Add persistence repositories and historical snapshot behavior

**Files:**
- Create: `src/GymTrackerMobile.Persistence/Repositories/IWorkoutRepository.cs`
- Create: `src/GymTrackerMobile.Persistence/Repositories/WorkoutRepository.cs`
- Create: `src/GymTrackerMobile.Persistence/Repositories/IActivityRepository.cs`
- Create: `src/GymTrackerMobile.Persistence/Repositories/ActivityRepository.cs`
- Create: `src/GymTrackerMobile.Persistence/Repositories/ISettingsRepository.cs`
- Create: `src/GymTrackerMobile.Persistence/Repositories/SettingsRepository.cs`
- Create: `src/GymTrackerMobile.Persistence/Repositories/IBackupMetadataRepository.cs`
- Create: `src/GymTrackerMobile.Persistence/Repositories/BackupMetadataRepository.cs`
- Modify: `src/GymTrackerMobile.Persistence/ServiceCollectionExtensions.cs`

**Interfaces:**
- `Task<WorkoutSession> StartWorkoutAsync(Guid templateId, DateTime startedAtUtc, CancellationToken cancellationToken = default)` loads a template and copies all mutable template/catalogue display and target values into a new session snapshot.
- `Task SaveSetAsync(WorkoutSet set, CancellationToken cancellationToken = default)` inserts or updates one set and persists its completion/status/context fields.
- `Task<WorkoutSession?> GetActiveWorkoutAsync(CancellationToken cancellationToken = default)` returns the recoverable active session with exercises and sets.
- `Task CompleteWorkoutAsync(Guid sessionId, DateTime completedAtUtc, string? notes, CancellationToken cancellationToken = default)` marks the session complete without deleting or rewriting prior set values.
- `Task<ActivityRecord> SaveActivityAsync(ActivityRecord activity, CancellationToken cancellationToken = default)` and `Task<IReadOnlyList<ActivityRecord>> GetActivitiesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)` provide local activity persistence.
- `Task<string?> GetSettingAsync(string key, CancellationToken cancellationToken = default)` and `Task SetSettingAsync(string key, string value, CancellationToken cancellationToken = default)` provide settings persistence.

- [ ] Implement `StartWorkoutAsync` in one database transaction: load the template with ordered exercises, copy all snapshot fields, create planned set rows, and mark the session active.
- [ ] Implement set upsert and completion operations with UTC timestamps and cancellation support; ensure reopening the context returns the same active session and set values.
- [ ] Implement activity, settings, and backup metadata repositories using async EF APIs and `AsNoTracking` for read-only queries.
- [ ] Register repository interfaces with scoped lifetimes and keep DTO mapping/domain behavior outside the MAUI UI project.

### Task 5: Build migration, round-trip, and offline test fixtures

**Files:**
- Modify: `tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj`
- Create: `tests/GymTrackerMobile.Persistence.Tests/Infrastructure/SqliteDatabaseFixture.cs`
- Create: `tests/GymTrackerMobile.Persistence.Tests/Infrastructure/LegacyDatabaseFixture.cs`
- Create: `tests/GymTrackerMobile.Persistence.Tests/DatabaseInitializationTests.cs`
- Create: `tests/GymTrackerMobile.Persistence.Tests/EntityRoundTripTests.cs`
- Create: `tests/GymTrackerMobile.Persistence.Tests/WorkoutSnapshotTests.cs`
- Create: `tests/GymTrackerMobile.Persistence.Tests/ActiveWorkoutRecoveryTests.cs`
- Create: `tests/GymTrackerMobile.Persistence.Tests/ConstraintTests.cs`

**Interfaces:**
- `SqliteDatabaseFixture.CreateContextAsync()` returns an isolated temporary-file `GymTrackerDbContext` with migrations applied.
- `LegacyDatabaseFixture.CreateVersionOneDatabaseAsync()` creates an older SQLite fixture containing representative catalogue, template, and workout data for migration testing.

- [ ] Write a failing fresh-database test asserting EF migration history, application schema metadata version, seeded exercise count, seeded template count, and default settings.
- [ ] Write a failing legacy-fixture test that opens the older schema, runs initialization, and verifies representative rows and values remain present.
- [ ] Write a failing round-trip test covering every required entity, optional notes/RPE/difficulty values, incomplete/failed/skipped sets, bodyweight sets, activity fields, recommendation outcomes, settings, and backup metadata.
- [ ] Write a failing snapshot test that starts a template workout, changes the template exercise name/target, reloads the session, and asserts the session snapshot is unchanged.
- [ ] Write a failing active-recovery test that saves an incomplete session/set, disposes the context, reopens it, and asserts the active session and set values are recoverable.
- [ ] Write constraint tests for required fields, foreign keys, unique settings keys, ordering, and prevention of cascade deletion into historical sessions.
- [ ] Run `dotnet test tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj --no-restore`; expect the new tests to fail before the corresponding implementation is complete, then rerun until all pass.

### Task 6: Integrate persistence into MAUI startup and verify the solution

**Files:**
- Modify: `src/GymTrackerMobile/MauiProgram.cs`
- Modify: `src/GymTrackerMobile/GymTrackerMobile.csproj`
- Modify: `README.md`
- Modify: `tests/GymTrackerMobile.Persistence.Tests/Infrastructure/SqliteDatabaseFixture.cs`

- [ ] Add the EF Core SQLite package to the executable only if required by the platform runtime; keep `DbContext` and migrations owned by Persistence.
- [ ] Resolve the platform database path with `FileSystem.AppDataDirectory` and call `AddGymTrackerPersistence` from `MauiProgram.CreateMauiApp()`.
- [ ] Register a startup initializer that runs once before storage-dependent screens are used; preserve the current local-only startup page.
- [ ] Document the local database location, migration-on-start behavior, offline test command, and the fact that JSON backup/import is a later phase.
- [ ] Run `dotnet restore GymTrackerMobile.sln`.
- [ ] Run `dotnet build GymTrackerMobile.sln`.
- [ ] Run `dotnet test GymTrackerMobile.sln`.
- [ ] Run `dotnet ef migrations has-pending-model-changes --project src/GymTrackerMobile.Persistence --startup-project src/GymTrackerMobile.Persistence` and confirm it reports no pending model changes.
- [ ] Run `git diff --check` and `git status --short`; verify only the planned files changed and no generated database/build artifact is tracked.
