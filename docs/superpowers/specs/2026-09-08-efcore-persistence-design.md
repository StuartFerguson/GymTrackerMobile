# EF Core Persistence Design

**Goal:** Establish versioned, offline-first SQLite persistence for the Gym Tracker MVP described by issue #22 and the product requirements baseline.

## Scope

This change defines the domain persistence model, EF Core SQLite `DbContext`, initial schema, migrations, fixed catalogue/template seed data, and offline persistence tests. It does not implement application screens, progression rules, JSON export/import workflows, or any account, backend, calendar, GPS, wearable, or cloud-sync integration.

## Architecture

`GymTrackerMobile.Domain` contains framework-independent records, enums, and persistence-facing contracts. `GymTrackerMobile.Persistence` references the domain and owns EF Core entity configuration, `GymTrackerDbContext`, database initialization, migrations, seed data, and repositories. The MAUI executable supplies the platform database path and registers the persistence services; UI code does not reference EF Core types.

EF Core migrations are the authoritative schema history. The database uses EF's migration history plus an application-owned settings/metadata record exposing the current schema version for backup and diagnostics. Database initialization applies pending migrations before seeding required immutable catalogue/template data.

## Data model

The initial model contains:

- `Exercise`: fixed catalogue name, muscle group, equipment type, weight-entry convention, default repetition range, default set count, and exercise mode.
- `WorkoutTemplate` and `TemplateExercise`: named templates with ordered exercise targets.
- `WorkoutSession` and `WorkoutExercise`: a started template snapshot with session-owned exercise name, target, convention, and order values so later catalogue/template edits cannot rewrite history.
- `WorkoutSet`: ordered planned/actual set data, weight in kilograms when applicable, repetitions, completion status, optional difficulty/RPE, notes, and timestamps.
- `ActivityRecord`: date, activity type, optional duration, distance in kilometres, steps, notes, and derived pace when applicable.
- `Recommendation` and `RecommendationOutcome`: proposed target values, explanation, confidence, status, and the accepted/edited/ignored result associated with the next workout only.
- `UserSetting`: key/value settings including units and default preferences.
- `BackupMetadata`: schema version, export/import timestamps, file identity, and last successful backup/import information.

All entities use stable generated identifiers, UTC timestamps, explicit foreign keys, indexes for history queries, and restrictive/nullability rules appropriate to the requirements. SQLite-compatible representations are used for dates, durations, enums, and optional numeric values.

## Historical and recovery invariants

- Starting a workout copies template-derived values into `WorkoutExercise`; historical sessions never depend on mutable template rows for display or analysis.
- Active sessions remain persisted after every meaningful set/workout update so an unexpected app close can be recovered.
- Deletes are restricted or explicitly handled so historical workout and activity records are never silently removed.
- Migrations preserve existing rows and are tested from an older fixture database.
- Database initialization is local-only and deterministic; tests use temporary database files or SQLite in-memory connections and require no network.

## Migration strategy

The first migration creates all MVP tables, indexes, foreign keys, and seed-compatible columns. Subsequent model changes must be introduced through named EF Core migrations committed under the Persistence project. SQLite table rebuilds are allowed where EF requires them, and any custom SQL must be covered by a fixture migration test. Initialization calls `MigrateAsync` once at startup through a dedicated service; it does not wrap `MigrateAsync` in an explicit transaction.

The application-owned schema metadata stores the latest application schema number after successful initialization. A failed migration must prevent normal data access and must not report the new version as applied.

## Testing

Persistence tests will verify:

1. A new database migrates to the expected EF migration and application schema version.
2. An older fixture migrates successfully while preserving representative data.
3. Every required entity can be inserted, queried, and round-tripped without loss.
4. Template changes after a workout starts do not change the workout snapshot.
5. Active workout/set state survives reopening the database.
6. Required relationships, uniqueness rules, and invalid references fail safely.
7. All tests run offline against isolated local SQLite databases.

## Technology constraints

- .NET SDK `10.0.400` and libraries targeting `net10.0`.
- EF Core SQLite packages must use one explicit, consistent version compatible with the pinned SDK/runtime.
- EF Core types remain inside `GymTrackerMobile.Persistence`.
- No backend, account, network, calendar, GPS, wearable, or cloud-sync dependency.
