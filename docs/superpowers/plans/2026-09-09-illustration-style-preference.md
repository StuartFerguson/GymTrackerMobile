# Illustration Style Preference Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Let users choose and persist Female, Male, or Neutral illustration artwork across the implemented workout screens.

**Architecture:** Store one validated style value through the existing `ISettingsRepository`. A shared resolver maps logical artwork keys to local assets, and Start Workout/Weekly Plan consume that mapping. The selector is hosted on Start Workout and preference failures fall back to Neutral without disrupting the screen.

**Tech Stack:** .NET 10, .NET MAUI, C#, xUnit v3, EF Core SQLite, local PNG assets.

**Spec:** `docs/superpowers/specs/2026-09-09-illustration-style-preference-design.md`

## Global Constraints

- Use the existing `UserSetting` and `ISettingsRepository` persistence contracts.
- Default missing or invalid preferences to `Neutral`.
- Preserve offline operation and the existing MAUI target frameworks.
- Keep workout/activity behavior unchanged; this feature only changes presentation.
- Use local PNG assets with no text, watermark, or remote runtime dependency.

---

### Task 1: Preference and resolver contract

**Files:**
- Create: `src/GymTrackerMobile.UI/IllustrationStyle.cs`
- Create: `src/GymTrackerMobile.UI/IllustrationAssetResolver.cs`
- Test: `tests/GymTrackerMobile.UI.Tests/IllustrationAssetResolverTests.cs`

- [x] **Step 1: Write failing resolver tests** for all three styles and seven logical keys, asserting non-empty unique filenames and Neutral as the fallback.
- [x] **Step 2: Run** `dotnet test tests/GymTrackerMobile.UI.Tests/GymTrackerMobile.UI.Tests.csproj --no-restore --filter FullyQualifiedName~IllustrationAssetResolverTests` and confirm the missing types fail the test build.
- [x] **Step 3: Implement** `IllustrationStyle`, `IllustrationAssetKey`, and `IllustrationAssetResolver.Resolve(IllustrationStyle, IllustrationAssetKey)` with explicit mappings.
- [x] **Step 4: Re-run** the focused test and confirm it passes.

### Task 2: Persisted illustration preference

**Files:**
- Create: `src/GymTrackerMobile.UI/IllustrationPreferenceViewModel.cs`
- Modify: `src/GymTrackerMobile/MauiProgram.cs`
- Test: `tests/GymTrackerMobile.UI.Tests/IllustrationPreferenceViewModelTests.cs`

- [x] **Step 1: Write failing tests** for missing/invalid values defaulting to Neutral, successful save/load, and save failure retaining the in-memory selection.
- [x] **Step 2: Run** the focused preference tests and confirm failure before production implementation.
- [x] **Step 3: Implement** `IllustrationPreferenceViewModel` with `LoadAsync`, `SelectAsync`, `SelectedStyle`, `ErrorMessage`, and the existing settings repository.
- [x] **Step 4: Register** the view model in MAUI DI and rerun the focused tests.

### Task 3: Wire shared style into Start Workout and Weekly Plan

**Files:**
- Modify: `src/GymTrackerMobile.UI/StartWorkoutViewModel.cs`
- Modify: `src/GymTrackerMobile.UI/StartWorkoutPage.cs`
- Modify: `src/GymTrackerMobile.UI/WeeklyPlanViewModel.cs`
- Modify: `src/GymTrackerMobile.UI/WeeklyPlanPage.cs`
- Modify: `tests/GymTrackerMobile.UI.Tests/StartWorkoutViewModelTests.cs`
- Modify: `tests/GymTrackerMobile.UI.Tests/WeeklyPlanTests.cs`

- [x] **Step 1: Add failing tests** proving both screens resolve the same logical artwork key using the selected style and that switching style updates presentation state.
- [x] **Step 2: Run** the affected UI tests and confirm the new assertions fail.
- [x] **Step 3: Implement** shared preference loading, style selector commands, and resolver-based image source selection without changing workout commands.
- [x] **Step 4: Run** the affected UI tests and confirm they pass.

### Task 4: Add male and neutral assets

**Files:**
- Create: `src/GymTrackerMobile/Resources/Images/male_*.png`
- Create: `src/GymTrackerMobile/Resources/Images/neutral_*.png`

- [x] **Step 1: Generate** seven male assets and seven neutral/abstract assets as transparent local PNGs using the approved flat-vector illustration direction.
- [x] **Step 2: Inspect** every asset for transparency, correct subject, no text/watermark, and consistent square framing.
- [x] **Step 3: Copy** accepted assets into the MAUI image resource folder and verify each resolver filename exists.

### Task 5: Full verification

**Files:**
- Modify: `docs/superpowers/plans/2026-09-09-illustration-style-preference.md`

- [x] **Step 1: Run** `dotnet test GymTrackerMobile.sln --no-restore` and require zero failures.
- [ ] **Step 2: Run** `dotnet build GymTrackerMobile.sln --no-restore` and require zero errors for all MAUI targets. (Android remains blocked by a Visual Studio file lock; Windows target passed independently.)
- [x] **Step 3: Run** `git diff --check` and inspect `git status --short` for only intended changes.
