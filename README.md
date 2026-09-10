# GymTrackerMobile

Android-first .NET MAUI foundation for the Gym Tracker application.

## Prerequisites

- .NET SDK `10.0.400` (pinned in `global.json`)
- Android and MAUI workloads installed
- An Android emulator or device for launching the app

Restore the workloads once on a new machine:

```powershell
dotnet workload restore GymTrackerMobile.sln
```

## Restore, build, and test

```powershell
dotnet restore GymTrackerMobile.sln
dotnet build GymTrackerMobile.sln
dotnet test GymTrackerMobile.sln
```

The app targets `net10.0-android` plus the MAUI desktop targets available on the host. The UI, domain, and persistence layers target `net10.0`, and the domain and persistence layers have separate test projects.

## Continuous integration

GitHub Actions runs the same workload restore, solution build, and test commands for pushes and pull requests targeting `master`. The workflow check is named `CI / Restore, build, and test`; require this check in branch protection before merging changes.

## Launch on Android

With an emulator running or a device connected, launch the app from Visual Studio or with the MAUI tooling:

```powershell
dotnet build src/GymTrackerMobile/GymTrackerMobile.csproj -f net10.0-android -t:Run
```

Startup shows a branded Gym Tracker screen while the local EF Core SQLite database is migrated and seeded, then transitions to the app shell. Initialization failures remain recoverable through an in-app retry action. The database is stored as `gym-tracker.db` in `FileSystem.AppDataDirectory`; no account, backend, or network service is required.

In `DEBUG` builds, open **Backup & Settings → Developer tools** and choose **Reset local app data** to clear active workouts, workout history, activities, settings, and backup metadata. The action requires confirmation and restores the built-in templates and exercises. It is intentionally excluded from production builds.

Persistence tests run offline against isolated temporary SQLite files:

```powershell
dotnet test tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj
```
