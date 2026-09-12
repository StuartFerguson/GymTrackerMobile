# UI Automation Coverage for Primary User Journeys

## Goal

Add deterministic Android UI automation for the primary Gym Tracker user journeys described in issue #43, while preserving the existing fast unit and persistence test layers.

## Scope and constraints

- Android is the first and only device target for this milestone because the application is Android-first and the acceptance criteria explicitly allow execution only where an Android environment is available.
- Tests must run without internet access after dependencies are restored.
- Test data must be local and deterministic; tests must not depend on wall-clock ordering, a remote service, or a pre-existing emulator state.
- The existing `net10.0` unit-test projects remain unchanged in purpose and continue to run in the normal CI job.
- The UI automation suite is allowed to be skipped when the runner does not provide an Android emulator, but the project and test definitions must still build and be discoverable.

## Approach

Create a separate `tests/GymTrackerMobile.UI.AutomationTests` project using Appium's .NET client and xUnit. The project owns the device-session fixture, page-object helpers, seeded-data reset protocol, and journey tests. It should not reach into application repositories or view models: the test talks to the packaged app through visible controls and verifies visible outcomes.

The app will expose a debug-only UI-test bootstrap path. On launch, when the app is built with the UI-test symbol, it will reset the local database and insert a fixed scenario containing completed history and a recommendation-capable workout. The bootstrap is opt-in and unavailable in production builds. The automation fixture will clear app data, launch the app with the bootstrap flag, wait for the startup screen to complete, and use stable semantic identifiers to interact with pages.

## Accessibility contract

Controls needed by automation receive stable `AutomationId` values rather than tests matching presentation text. IDs will cover:

- dashboard navigation and start-workout template selection;
- active-workout exercise selection, weight/repetition inputs, set status, recommendation accept/edit/ignore, and completion;
- activity type, activity fields, save, and validation message;
- history rows and exercise-progress navigation;
- workout and activity summary values.

The IDs are implementation details of the app's testability contract and should remain stable across copy and illustration changes.

## Seeded scenario

The UI-test bootstrap creates:

- the normal Push template and catalogue data through the existing seed path;
- one completed Push workout with a stable completed-set record so History and Exercise Progress have an entry before the test starts;
- one prior completed strength set sufficient for the Push workout to display a recommendation;
- no active workout or unsaved activity.

Each test starts from a clean app-data state and uses a unique test session in a fresh app process. Tests must not rely on another test's writes. The workout journey itself creates its own active session through the UI.

## Journey coverage

1. Start Push, record valid weight and repetitions for the visible set, complete the workout, and verify the workout summary shows the Push name and completed-set count.
2. Open Log Activity, save a Walking record using deterministic values, open History, and verify the activity type and entered details are visible. The same automation helper may parameterize Running and Swimming coverage where their field rules differ.
3. Open the seeded workout from History, open the exercise progress view, and verify the exercise title and stored history are shown.
4. For separate fresh sessions, accept, edit, and ignore the visible recommendation, verifying the corresponding outcome and edited/accepted value before continuing.
5. Submit invalid workout input and invalid activity input, verify the validation message, then correct and save the same form. The previously entered valid fields must still be present after the validation attempt.

## Error handling and synchronization

Page objects wait for visible semantic controls and explicit state text rather than fixed sleeps. A failed bootstrap, app launch, or missing control produces a test failure with the current page source captured as an artifact. Test cleanup must quit the driver even when a journey fails.

## CI

Keep the existing Linux build/test job as the required offline-safe gate. Add a separate Android UI job that:

- restores the MAUI Android workload and NuGet dependencies;
- starts a pinned Android API 35 x86_64 emulator image;
- builds and installs the debug testable app;
- runs the automation project with the emulator endpoint;
- uploads Appium logs and page-source artifacts on failure.

The job is conditional on the repository's Android-runner capability. When the runner cannot provide an emulator, the normal CI job remains green and the UI automation project remains available for device-capable runners.

## Verification

- The automation project builds without an emulator.
- The existing domain, persistence, and UI unit-test projects continue to pass.
- On an Android emulator, all listed journeys pass from a clean app-data state and can be run repeatedly without ordering dependence or internet access.
