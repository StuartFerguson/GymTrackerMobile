# Versioned JSON Export and Safe Import Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add versioned JSON export plus validated, transactional replacement and merge import with recoverable backups and Backup & Settings UI actions.

**Architecture:** Explicit backup DTOs are serialized independently of EF entities. A validator produces field-path errors without touching the database; a persistence service applies validated documents through a transaction and a file-store abstraction. The MAUI page owns file picking and confirmation while the service owns backup semantics.

**Tech Stack:** .NET 10, C#, EF Core SQLite, `System.Text.Json`, .NET MAUI Essentials file picker/file saver, xUnit.

**Spec:** `docs/superpowers/specs/2026-09-10-versioned-json-backup-design.md`

## Global Constraints

- Support schema version `1` only.
- Validate detached DTOs before any database mutation.
- Serialize enums as strings and dates as UTC ISO-8601 values.
- Replacement and merge require confirmation at the UI boundary.
- Replacement creates a timestamped recoverable JSON backup before mutation.
- Import writes all database changes inside one transaction.
- Preserve existing reset, repository, and navigation behavior.

---

### Task 1: Define the backup document contract and serializer

**Files:**
- Create: `src/GymTrackerMobile.Persistence/Backup/GymTrackerBackupDocument.cs`
- Create: `src/GymTrackerMobile.Persistence/Backup/GymTrackerBackupJson.cs`
- Test: `tests/GymTrackerMobile.Persistence.Tests/BackupSerializationTests.cs`

**Interfaces:**
- Produces `GymTrackerBackupDocument` with metadata and explicit DTO collections for every persisted entity.
- Produces `GymTrackerBackupJson.Serialize(GymTrackerBackupDocument)` and `GymTrackerBackupJson.Deserialize(string)`.

- [ ] Write a failing test that serializes a populated document, deserializes it, and preserves IDs, nested sets, enum names, and UTC timestamps.
- [ ] Run `dotnet test tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj --filter FullyQualifiedName~BackupSerializationTests` and verify the missing contract/serializer failure.
- [ ] Implement explicit nullable-safe DTOs and `System.Text.Json` options with string enums and camel-case property names.
- [ ] Run the focused test and verify it passes.
- [ ] Commit with `git add -A; git commit -m "feat: add versioned backup document contract"`.

### Task 2: Build detached document export and field-level validation

**Files:**
- Create: `src/GymTrackerMobile.Persistence/Backup/BackupValidationError.cs`
- Create: `src/GymTrackerMobile.Persistence/Backup/GymTrackerBackupValidator.cs`
- Create: `src/GymTrackerMobile.Persistence/Backup/GymTrackerBackupMapper.cs`
- Create: `src/GymTrackerMobile.Persistence/Backup/IBackupService.cs`
- Create: `src/GymTrackerMobile.Persistence/Backup/BackupService.cs`
- Modify: `src/GymTrackerMobile.Persistence/ServiceCollectionExtensions.cs`
- Test: `tests/GymTrackerMobile.Persistence.Tests/BackupValidationTests.cs`

**Interfaces:**
- `IBackupService.ExportAsync(CancellationToken)` returns a serialized JSON string.
- `IBackupService.ValidateAsync(string, CancellationToken)` returns `BackupValidationResult` containing a nullable document and read-only errors.
- `BackupValidationError` exposes `Path` and `Message`.

- [ ] Add failing tests for complete export coverage, malformed JSON, unsupported version, duplicate IDs, invalid enum/date/value, broken references, and validation without database mutation.
- [ ] Run the focused validation tests and verify they fail because the service and validator do not yet exist.
- [ ] Implement mapping from all EF tables to detached DTOs and validation for required fields, IDs, enums, dates, ranges, uniqueness, ordering, and relationships.
- [ ] Implement export using `AsNoTracking` queries and invariant JSON serialization.
- [ ] Register `IBackupService` in persistence DI.
- [ ] Run focused validation tests and verify they pass.
- [ ] Commit with `git add -A; git commit -m "feat: export and validate backup documents"`.

### Task 3: Implement transactional replacement, merge, and recovery storage

**Files:**
- Create: `src/GymTrackerMobile.Persistence/Backup/IBackupFileStore.cs`
- Create: `src/GymTrackerMobile.Persistence/Backup/LocalBackupFileStore.cs`
- Modify: `src/GymTrackerMobile.Persistence/Backup/IBackupService.cs`
- Modify: `src/GymTrackerMobile.Persistence/Backup/BackupService.cs`
- Modify: `src/GymTrackerMobile.Persistence/ServiceCollectionExtensions.cs`
- Test: `tests/GymTrackerMobile.Persistence.Tests/BackupImportTests.cs`

**Interfaces:**
- `IBackupFileStore.SaveRecoveryCopyAsync(string, CancellationToken)` returns the saved path.
- `IBackupService.ImportAsync(string json, BackupImportMode mode, CancellationToken)` validates then applies replacement or merge and returns an import result.
- `BackupImportMode` has `Replace` and `Merge` values.

- [ ] Add failing tests for replacement round-trip, merge retaining absent rows, invalid imports leaving counts unchanged, transaction rollback after an apply failure, and recovery-file creation.
- [ ] Run the focused import tests and verify the expected missing-method failures.
- [ ] Implement local recovery storage using an app-data backup directory and timestamped filenames.
- [ ] Implement replacement deletes in foreign-key-safe order, inserts original IDs, updates metadata, and commits once.
- [ ] Implement merge upserts by stable IDs, validates combined references and uniqueness conflicts before mutation, and commits once.
- [ ] Ensure validation errors and exceptions occur before commit and clear EF tracking before applying rows.
- [ ] Run focused import tests and verify they pass.
- [ ] Commit with `git add -A; git commit -m "feat: safely import and recover backups"`.

### Task 4: Add file transfer boundary and Backup & Settings view-model actions

**Files:**
- Create: `src/GymTrackerMobile.UI/BackupSettingsViewModel.cs`
- Create: `src/GymTrackerMobile.UI/IBackupFileTransfer.cs`
- Create: `src/GymTrackerMobile.UI/MauiBackupFileTransfer.cs`
- Modify: `src/GymTrackerMobile.UI/DestinationPages.cs`
- Modify: `src/GymTrackerMobile/MauiProgram.cs`
- Test: `tests/GymTrackerMobile.UI.Tests/BackupSettingsViewModelTests.cs`

**Interfaces:**
- `IBackupFileTransfer.PickBackupAsync(CancellationToken)` returns nullable file content/name.
- `IBackupFileTransfer.SaveBackupAsync(string json, string suggestedName, CancellationToken)` returns a nullable saved path.
- `BackupSettingsViewModel` exposes async export, import-replace, and import-merge operations accepting confirmation delegates.

- [ ] Add failing UI tests for export cancellation, import cancellation, validation error display, rejected confirmation, successful replacement/merge, and operation error display.
- [ ] Run the focused UI tests and verify the expected missing-view-model failures.
- [ ] Implement injected file transfer and view-model commands; keep confirmation outside the service and never call import when canceled or declined.
- [ ] Implement MAUI file picking/saving adapters and register them with DI.
- [ ] Replace the placeholder Backup & Settings content with buttons and alerts bound to the view-model actions.
- [ ] Run focused UI tests and verify they pass.
- [ ] Commit with `git add -A; git commit -m "feat: add backup settings import and export UI"`.

### Task 5: Run full verification and inspect the final diff

**Files:**
- Modify: `README.md` if the final backup behavior needs user-facing documentation.

- [ ] Run `dotnet test GymTrackerMobile.sln`.
- [ ] Run `dotnet build GymTrackerMobile.sln`.
- [ ] Run `git diff master...HEAD --check` and inspect `git diff master...HEAD --stat`.
- [ ] Confirm every issue acceptance criterion is covered by code and tests.
- [ ] Commit any documentation-only update with `git add -A; git commit -m "docs: describe backup and restore"`.
