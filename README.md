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

## Launch on Android

With an emulator running or a device connected, launch the app from Visual Studio or with the MAUI tooling:

```powershell
dotnet build src/GymTrackerMobile/GymTrackerMobile.csproj -f net10.0-android -t:Run
```

Startup is local-only and renders a minimal Gym Tracker page. No account, backend, network service, or persistence setup is required.
