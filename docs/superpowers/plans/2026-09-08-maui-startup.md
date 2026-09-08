# .NET MAUI Startup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans (recommended) or superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the reproducible Android-first .NET MAUI solution and minimal local startup described in the design spec.

**Architecture:** The MAUI executable is the composition root and references UI, Domain, and Persistence. Domain and Persistence remain framework-independent `net10.0` libraries with separate test projects; UI owns the initial page presentation.

**Tech Stack:** .NET SDK 10.0.400, .NET MAUI, Android target `net10.0-android`, C#, XAML, xUnit.

**Spec:** `docs/superpowers/specs/2026-09-08-maui-startup-design.md`

## Global Constraints

- Target the MAUI app with `net10.0-android`.
- Target reusable libraries and tests with `net10.0`.
- Keep startup local with no backend, account, or network service.
- Keep UI, Domain, and Persistence in separate projects/folders.
- Pin the SDK and explicitly define package versions.
- Document clean-checkout restore, build, test, and Android launch commands.

---

### Task 1: Scaffold the solution and project boundaries

**Files:**
- Create: `GymTrackerMobile.sln`
- Create: `global.json`
- Create: `src/GymTrackerMobile/GymTrackerMobile.csproj`
- Create: `src/GymTrackerMobile.UI/GymTrackerMobile.UI.csproj`
- Create: `src/GymTrackerMobile.Domain/GymTrackerMobile.Domain.csproj`
- Create: `src/GymTrackerMobile.Persistence/GymTrackerMobile.Persistence.csproj`
- Create: `tests/GymTrackerMobile.Domain.Tests/GymTrackerMobile.Domain.Tests.csproj`
- Create: `tests/GymTrackerMobile.Persistence.Tests/GymTrackerMobile.Persistence.Tests.csproj`

- [ ] Create the solution and projects with the exact target frameworks above.
- [ ] Add project references so the application references UI, Domain, and Persistence; UI and Persistence reference Domain; tests reference their production layers.
- [ ] Set nullable and implicit usings consistently and pin SDK version `10.0.400`.
- [ ] Run `dotnet restore GymTrackerMobile.sln` and confirm restore succeeds.

### Task 2: Add the minimal MAUI startup page

**Files:**
- Create: `src/GymTrackerMobile/App.xaml`
- Create: `src/GymTrackerMobile/App.xaml.cs`
- Create: `src/GymTrackerMobile/AppShell.xaml`
- Create: `src/GymTrackerMobile/AppShell.xaml.cs`
- Create: `src/GymTrackerMobile/MauiProgram.cs`
- Create: `src/GymTrackerMobile/MainPage.xaml`
- Create: `src/GymTrackerMobile/MainPage.xaml.cs`
- Create: `src/GymTrackerMobile/Platforms/Android/AndroidManifest.xml`

- [ ] Define the MAUI application resources, shell, and startup composition.
- [ ] Register `MainPage` with MAUI dependency injection and make it the shell content.
- [ ] Render a local title and readiness message without invoking network or persistence code.
- [ ] Build the app project for `net10.0-android` and fix only startup/compiler issues.

### Task 3: Add testable layer placeholders and behavior tests

**Files:**
- Create: `src/GymTrackerMobile.Domain/AssemblyInfo.cs`
- Create: `src/GymTrackerMobile.Persistence/AssemblyInfo.cs`
- Create: `tests/GymTrackerMobile.Domain.Tests/DomainAssemblyTests.cs`
- Create: `tests/GymTrackerMobile.Persistence.Tests/PersistenceAssemblyTests.cs`

- [ ] Write failing tests that load each layer assembly and verify its expected root namespace.
- [ ] Run each test project and confirm the tests fail for the missing assemblies/types rather than test setup errors.
- [ ] Add the minimal production assembly markers/types required for the tests to pass.
- [ ] Run both test projects and confirm all tests pass.

### Task 4: Document reproducible commands and verify the full solution

**Files:**
- Modify: `README.md`

- [ ] Document prerequisites, `dotnet workload restore`, `dotnet restore`, `dotnet build`, `dotnet test`, and Android launch commands.
- [ ] Run `dotnet restore GymTrackerMobile.sln`.
- [ ] Run `dotnet build GymTrackerMobile.sln`.
- [ ] Run `dotnet test GymTrackerMobile.sln`.
- [ ] Inspect `git diff --check` and `git status --short` before reporting completion.
