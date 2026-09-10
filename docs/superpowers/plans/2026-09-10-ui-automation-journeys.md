# UI Automation Journeys Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add deterministic Android UI automation for the primary Gym Tracker journeys in issue #43 and run it in Android-capable CI environments.

**Architecture:** Add a separate Appium/xUnit test project that drives the packaged app through semantic controls and page objects. Add a debug-only launch bootstrap that resets and seeds local SQLite data for repeatable tests, plus stable `AutomationId` values on the controls under test. Keep the existing unit and persistence tests as the normal offline gate and add an independent Android emulator job for device journeys.

**Tech Stack:** .NET 10, .NET MAUI, xUnit, Appium .NET client, Android SDK/emulator, GitHub Actions.

**Spec:** `docs/superpowers/specs/2026-09-10-ui-automation-journeys-design.md`

## Global Constraints

- Android is the first and only device target for this milestone.
- Tests must run without internet access after dependencies are restored.
- Test data must be local and deterministic.
- The debug-only UI-test bootstrap must be unavailable in production builds.
- Existing domain, persistence, and UI unit-test projects must continue to run in the normal CI job.
- UI automation may be skipped on runners without an Android emulator, but the project must still build and be discoverable.

---

### Task 1: Establish the app testability contract

**Files:**
- Modify: `src/GymTrackerMobile.UI/StartWorkoutPage.cs`
- Modify: `src/GymTrackerMobile.UI/ActiveWorkoutPage.cs`
- Modify: `src/GymTrackerMobile.UI/LogActivityPage.cs`
- Modify: `src/GymTrackerMobile.UI/HistoryPage.cs`
- Modify: `src/GymTrackerMobile.UI/ExerciseProgressPage.cs`
- Modify: `src/GymTrackerMobile.UI/WorkoutSummaryPage.cs`
- Modify: `src/GymTrackerMobile.UI/ActivitySummaryPage.cs`
- Test: `tests/GymTrackerMobile.UI.Tests/UiAutomationIdentifierTests.cs`

**Interfaces:**
- Produces stable `AutomationId` values consumed by Appium page objects.
- Preserves existing visible labels and command behavior.

- [ ] **Step 1: Add a failing identifier contract test**

Create a test that instantiates each page with the existing test doubles and asserts that the required controls expose the IDs used by the automation project, including `start-template-push`, `start-workout`, `active-weight-1`, `active-repetitions-1`, `active-save-set-1`, `active-complete`, `recommendation-accept`, `recommendation-edit`, `recommendation-ignore`, `activity-type-walking`, `activity-save`, `history-item-0`, and `history-progress-0`.

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```powershell
dotnet test tests/GymTrackerMobile.UI.Tests/GymTrackerMobile.UI.Tests.csproj --filter FullyQualifiedName~UiAutomationIdentifierTests
```

Expected: FAIL because the pages do not yet expose the required semantic IDs.

- [ ] **Step 3: Add IDs without changing behavior**

Assign the IDs directly to the controls created by the page builders. For repeated exercise/set controls, compose IDs from the stable set number or history index. Add IDs to visible summary labels where the automation needs to assert values, such as `workout-summary-name`, `workout-summary-completed-sets`, `activity-summary-type`, and `progress-exercise-name`.

- [ ] **Step 4: Run the focused test and verify it passes**

Run the same focused command and expect PASS, then run the existing UI test project to ensure page construction changes did not break ViewModel tests.

- [ ] **Step 5: Commit**

```powershell
git add src/GymTrackerMobile.UI tests/GymTrackerMobile.UI.Tests/UiAutomationIdentifierTests.cs
git commit -m "test: add stable UI automation identifiers"
```

### Task 2: Add the deterministic debug UI-test bootstrap

**Files:**
- Create: `src/GymTrackerMobile.Persistence/UiTestDataSeeder.cs`
- Create: `src/GymTrackerMobile.UI/UiTestLaunchOptions.cs`
- Modify: `src/GymTrackerMobile.Persistence/ServiceCollectionExtensions.cs`
- Modify: `src/GymTrackerMobile/MauiProgram.cs`
- Modify: `src/GymTrackerMobile/App.xaml.cs`
- Test: `tests/GymTrackerMobile.Persistence.Tests/UiTestDataSeederTests.cs`

**Interfaces:**
- Produces `UiTestLaunchOptions.IsEnabled` from the Android launch intent extra `gymtracker.uiTestMode`.
- Produces `UiTestDataSeeder.SeedAsync(GymTrackerDbContext, CancellationToken)` for a clean fixed scenario.

- [ ] **Step 1: Write the failing persistence fixture test**

Create a SQLite fixture test that deletes existing rows, calls `UiTestDataSeeder.SeedAsync`, and asserts that the Push template exists, one completed workout exists, one completed strength set exists for recommendation history, and no active workout exists.

- [ ] **Step 2: Run the focused test and verify it fails**

Run:

```powershell
dotnet test tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj --filter FullyQualifiedName~UiTestDataSeederTests
```

Expected: FAIL because the seeder does not exist.

- [ ] **Step 3: Implement the fixed seed scenario**

Implement the seeder as an idempotent database operation: clear mutable workout/activity/recommendation rows, call the normal catalogue/template seed path, insert one completed Push session with a known completed set, and leave active-workout and activity tables empty. Use fixed GUIDs and UTC timestamps so assertions do not depend on the current clock.

- [ ] **Step 4: Wire the opt-in debug launch path**

Read the Android intent extra only in `#if DEBUG` code. After normal database initialization, invoke the seeder when the flag is true. Do not add a production command-line or network backdoor. Keep the normal app startup unchanged when the flag is absent.

- [ ] **Step 5: Run persistence and UI tests**

Run the focused seeder test, the complete persistence test project, and the complete UI test project. Expect all to pass.

- [ ] **Step 6: Commit**

```powershell
git add src/GymTrackerMobile.Persistence src/GymTrackerMobile.UI/UiTestLaunchOptions.cs src/GymTrackerMobile/MauiProgram.cs src/GymTrackerMobile/App.xaml.cs tests/GymTrackerMobile.Persistence.Tests/UiTestDataSeederTests.cs
git commit -m "test: add deterministic UI test data bootstrap"
```

### Task 3: Create the Appium test harness and page objects

**Files:**
- Create: `tests/GymTrackerMobile.UI.AutomationTests/GymTrackerMobile.UI.AutomationTests.csproj`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Infrastructure/AndroidDriverFixture.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Infrastructure/UiTestSettings.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Pages/AppPage.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Pages/StartWorkoutPage.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Pages/ActiveWorkoutPage.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Pages/HistoryPage.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Pages/ActivityPage.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Pages/ProgressPage.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Pages/SummaryPage.cs`
- Modify: `GymTrackerMobile.sln`

**Interfaces:**
- `AndroidDriverFixture` owns one Appium session per test and quits it in `DisposeAsync`.
- Page objects locate controls by `AppiumBy.AccessibilityId` and wait for visibility/enabled state using explicit polling.
- `UiTestSettings` reads `APPIUM_SERVER_URL`, `ANDROID_DEVICE_NAME`, and `GYMTRACKER_APK_PATH` from environment variables with local defaults.

- [ ] **Step 1: Add the project and package references**

Target `net10.0`, mark the project non-packable, add `Microsoft.NET.Test.Sdk`, xUnit packages, `Appium.WebDriver` version `5.0.0`, and reference no production project. Add the project to the solution.

- [ ] **Step 2: Add a harness smoke test**

Create `AppLaunchTests.App_launches_to_the_dashboard` that is skipped when `APPIUM_SERVER_URL` or `GYMTRACKER_APK_PATH` is absent, otherwise launches the app with the UI-test intent flag and asserts the dashboard/start-workout control is visible.

- [ ] **Step 3: Implement explicit synchronization and failure artifacts**

Use `WebDriverWait` polling for semantic IDs and page text. In fixture cleanup, capture `PageSource` and the Appium log to the test results directory when a test fails, then always quit the driver.

- [ ] **Step 4: Build without an emulator**

Run:

```powershell
dotnet test tests/GymTrackerMobile.UI.AutomationTests/GymTrackerMobile.UI.AutomationTests.csproj --no-restore
```

Expected: the project builds and the device-dependent smoke test is skipped when no endpoint/apk is configured.

- [ ] **Step 5: Commit**

```powershell
git add GymTrackerMobile.sln tests/GymTrackerMobile.UI.AutomationTests
git commit -m "test: add Android Appium UI test harness"
```

### Task 4: Implement the workout and activity journeys

**Files:**
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Journeys/WorkoutJourneyTests.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Journeys/ActivityJourneyTests.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/StartWorkoutPage.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/ActiveWorkoutPage.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/ActivityPage.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/HistoryPage.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/SummaryPage.cs`

**Interfaces:**
- Page methods model user-visible actions only: `ChoosePushAsync`, `EnterSetAsync`, `CompleteAsync`, `ChooseActivityAsync`, `SaveAsync`, `OpenHistoryAsync`.
- Tests assert visible names/details and do not inspect repositories or database files.

- [ ] **Step 1: Add the failing workout journey**

Write a test that launches a fresh seeded app, selects Push, enters `40.0` kg and `8` repetitions for set 1, saves the set, completes the workout, and asserts `Push` and a completed-set summary are visible.

- [ ] **Step 2: Add the failing activity journey**

Write a test that selects Walking, enters `25` minutes, `2.5` kilometres, saves, opens History, and asserts `Walking`, `25m`, and `2.5 km` are visible. Add Running and Swimming cases for their type-specific fields.

- [ ] **Step 3: Implement page-object actions and assertions**

Implement only the controls needed by the tests, using accessibility IDs and explicit waits. Reset the app through the fixture before each test so the journeys are independent.

- [ ] **Step 4: Run the journeys against a local emulator**

Set `APPIUM_SERVER_URL`, `ANDROID_DEVICE_NAME`, and `GYMTRACKER_APK_PATH`, then run:

```powershell
dotnet test tests/GymTrackerMobile.UI.AutomationTests --filter FullyQualifiedName~WorkoutJourneyTests
dotnet test tests/GymTrackerMobile.UI.AutomationTests --filter FullyQualifiedName~ActivityJourneyTests
```

Expected: all workout and activity assertions pass from a clean app-data state.

- [ ] **Step 5: Commit**

```powershell
git add tests/GymTrackerMobile.UI.AutomationTests
git commit -m "test: cover workout and activity UI journeys"
```

### Task 5: Implement history, recommendation, and validation journeys

**Files:**
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Journeys/HistoryJourneyTests.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Journeys/RecommendationJourneyTests.cs`
- Create: `tests/GymTrackerMobile.UI.AutomationTests/Journeys/ValidationJourneyTests.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/ActiveWorkoutPage.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/HistoryPage.cs`
- Modify: `tests/GymTrackerMobile.UI.AutomationTests/Pages/ProgressPage.cs`

**Interfaces:**
- Recommendation page methods expose `AcceptAsync`, `EditAsync(double)`, and `IgnoreAsync`, with outcome assertions based on visible state.
- Validation tests use the same page instance after an invalid submission and assert that valid input values remain visible.

- [ ] **Step 1: Add the failing history/progress journey**

Launch seeded data, open History, select the seeded Push workout, open its first exercise progress view, and assert the exercise name and stored completed-set history are visible.

- [ ] **Step 2: Add the failing recommendation journeys**

Use three fresh launches. In the first, accept the recommendation and assert the accepted state. In the second, edit the proposed weight to `42.5` kg and assert the edited state and value. In the third, ignore it and assert the ignored state.

- [ ] **Step 3: Add the failing validation journey**

Submit a workout set with negative weight and missing repetitions, assert the validation message, then assert the entered weight remains visible. Submit an activity with an invalid duration, assert the activity validation message, then correct and save it.

- [ ] **Step 4: Implement page methods and run focused tests**

Use the stable IDs from Task 1, wait for outcome labels/messages, and run the three journey classes against the local emulator. Capture screenshots/page source on failure.

- [ ] **Step 5: Commit**

```powershell
git add tests/GymTrackerMobile.UI.AutomationTests
git commit -m "test: cover history recommendations and validation journeys"
```

### Task 6: Integrate the automation project into CI

**Files:**
- Modify: `.github/workflows/ci.yml`
- Create: `.github/workflows/android-ui-tests.yml`
- Modify: `README.md`

**Interfaces:**
- The existing `CI / Restore, build, and test` job remains the offline unit/persistence/UI gate.
- `Android UI / Primary journeys` builds the debug APK, starts Android API 35 x86_64, launches Appium, and runs the automation project.

- [ ] **Step 1: Add the project build to the normal CI solution**

Ensure solution restore/build includes the automation project without attempting device execution. Keep the existing three explicit test commands unchanged.

- [ ] **Step 2: Add the Android UI workflow**

Create a workflow that uses an Android-capable Ubuntu runner, installs .NET 10 and the MAUI workload, restores dependencies, starts the API 35 x86_64 emulator, builds the debug APK, starts Appium, exports the endpoint/apk/device variables, runs the automation project, and uploads test results plus page-source/Appium artifacts on failure.

- [ ] **Step 3: Document local execution**

Add the exact emulator/Appium prerequisites and PowerShell commands to README, including the environment variables and the fact that tests are skipped when no device endpoint is configured.

- [ ] **Step 4: Validate CI configuration statically**

Run `dotnet build GymTrackerMobile.sln --configuration Release`, `dotnet test GymTrackerMobile.sln --configuration Release`, and inspect the workflow YAML for matching project paths, APK output path, and environment variable names.

- [ ] **Step 5: Commit**

```powershell
git add .github/workflows README.md GymTrackerMobile.sln
git commit -m "ci: run Android UI journey tests"
```

### Task 7: Final verification

**Files:**
- Verify: all files from Tasks 1-6

- [ ] **Step 1: Run formatting and diff checks**

Run `git diff --check` and inspect `git diff master...HEAD` for accidental production behavior, unstable selectors, or network-dependent test setup.

- [ ] **Step 2: Run all offline tests**

Run:

```powershell
dotnet test GymTrackerMobile.sln --configuration Release
```

Expected: domain, persistence, and UI tests pass; automation tests build and skip only when no Appium endpoint is configured.

- [ ] **Step 3: Run the complete Android suite**

On an API 35 emulator with Appium configured, run:

```powershell
dotnet test tests/GymTrackerMobile.UI.AutomationTests --configuration Release
```

Expected: all primary journey tests pass repeatedly from clean app data.

- [ ] **Step 4: Commit any final test-only corrections**

```powershell
git add -A
git commit -m "test: verify primary UI journeys"
```

