# FivesGame

A casual sliding-puzzle game and a modernization case study for production-oriented Unity development.

![FivesGame menu](Content/FivesGame_Menu.gif)
![FivesGame gameplay](Content/FivesGame_GamePlay.gif)

## Current version

The imported baseline contains a complete player loop:

- sliding-puzzle gameplay;
- themes and puzzle selection;
- energy and star currencies;
- player progress and local save data;
- menus, settings, audio, transitions, and rewards.

It was originally created as a two-week test assignment and is now maintained with Unity 6.0 (6000.0.71f1), LeoECS, VContainer, UniTask, DOTween, uGUI, TextMesh Pro, and JSON persistence.

## Modernization direction

Development now focuses on a compact, verifiable architecture:

- deterministic pure-C# puzzle domain;
- atomic moves, Undo, Replay, and seeded daily challenges;
- versioned saves with migrations and corruption recovery;
- explicit async and resource ownership;
- Addressables, RU/EN localization, and modern input;
- EditMode/PlayMode tests, CI artifacts, and measured performance budgets.

See the [baseline report](docs/BASELINE.md), [audit summary](docs/AUDIT.md), [target architecture](docs/ARCHITECTURE.md), and [roadmap](docs/ROADMAP.md).

## Open and build

1. Install Unity **6000.0.71f1** with Android Build Support, SDK, NDK, and OpenJDK.
2. Add this directory as a project.
3. Let Unity Package Manager restore the locked dependencies.
4. Open `Assets/Scenes/MainGame.unity`.
5. Build an Android App Bundle from `FivesGame > Build > Android App Bundle`.

The project targets Android only. The automated entry point is `Fives.Editor.AndroidBuild.BuildRelease`; its default output is `Builds/Android/FivesGame.aab`.

## Git Flow

- `main` contains stable, demonstrable releases.
- `develop` integrates the next release.
- `feature/*` branches start from and merge into `develop`.
- `release/*` stabilizes a version before `main`.
- `hotfix/*` starts from `main` and merges back into both `main` and `develop`.

See [CONTRIBUTING.md](CONTRIBUTING.md) for commit and pull-request rules.

## Verification status

The eight imported correctness problems now pass isolated regression probes, and the pure domain regressions compile as Unity EditMode tests. Unity 6 package resolution is reproducible. Full Test Runner execution, Android IL2CPP build, and device profiling remain pending because the current Codex sandbox blocks Unity's IL post-processor socket. No performance values are claimed before profiling a documented build and device.

## Licensing

A project-wide license has not been selected yet. Third-party packages and imported media retain their own terms. See [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md); unresolved media provenance blocks a public store release.
