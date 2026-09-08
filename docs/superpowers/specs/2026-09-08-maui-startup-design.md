# .NET MAUI Startup Design

## Goal

Create a reproducible Android-first .NET MAUI solution that launches a minimal local page and establishes separate UI, domain, persistence, and test boundaries for future Gym Tracker work.

## Architecture

`GymTrackerMobile` is the executable MAUI application and composition root. `GymTrackerMobile.UI` owns reusable presentation types, `GymTrackerMobile.Domain` owns framework-independent business types, and `GymTrackerMobile.Persistence` owns the future storage boundary. The application references the layers in one direction and performs no network, account, or backend work during startup.

The first page is intentionally minimal and local. It proves that MAUI startup, dependency injection, XAML/resource compilation, and Android targeting are wired correctly without introducing domain or storage behavior prematurely.

## Project and framework boundaries

- `src/GymTrackerMobile/GymTrackerMobile.csproj`: MAUI executable targeting `net10.0-android`.
- `src/GymTrackerMobile.UI/GymTrackerMobile.UI.csproj`: class library targeting `net10.0`.
- `src/GymTrackerMobile.Domain/GymTrackerMobile.Domain.csproj`: class library targeting `net10.0`.
- `src/GymTrackerMobile.Persistence/GymTrackerMobile.Persistence.csproj`: class library targeting `net10.0`.
- `tests/GymTrackerMobile.Domain.Tests/GymTrackerMobile.Domain.Tests.csproj`: unit tests targeting `net10.0`.
- `tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj`: unit tests targeting `net10.0`.

The solution references the application and all required testable layers. Tests reference the production layer they exercise. The application references UI, Domain, and Persistence; UI references Domain; Persistence references Domain.

## Versioning and reproducibility

The repository pins the .NET SDK with `global.json`. Project files explicitly set target frameworks and package versions. Build artifacts remain ignored by the existing Visual Studio `.gitignore`. `README.md` documents the exact restore/build/test commands and the Android launch prerequisite.

## Startup behavior

`MauiProgram.CreateMauiApp()` configures the MAUI app and registers the initial page. `App` exposes the application shell, and the shell presents a page with a title and a short local status message. No external service is contacted and no account or persistence setup is required.

## Verification

- Restore from the solution file on a clean checkout.
- Build the solution with the Android workload available.
- Run the domain and persistence test projects.
- Confirm the app project compiles with the Android target and that the startup page is represented in the app source.
