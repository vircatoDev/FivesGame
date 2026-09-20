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

It was originally created as a two-week test assignment with Unity 2021.3.25f1, LeoECS, VContainer, UniTask, DOTween, uGUI, TextMesh Pro, and JSON persistence.

## Modernization direction

Development now focuses on a compact, verifiable architecture:

- deterministic pure-C# puzzle domain;
- atomic moves, Undo, Replay, and seeded daily challenges;
- versioned saves with migrations and corruption recovery;
- explicit async and resource ownership;
- Addressables, RU/EN localization, and modern input;
- EditMode/PlayMode tests, CI artifacts, and measured performance budgets.

See the [audit summary](docs/AUDIT.md), [target architecture](docs/ARCHITECTURE.md), and [roadmap](docs/ROADMAP.md).

## Open the baseline

1. Install Unity **2021.3.25f1** through Unity Hub.
2. Add this directory as a project.
3. Let Unity Package Manager restore the locked dependencies.
4. Open `Assets/Scenes/MainGame.unity`.

Migration to Unity 6 will happen in a dedicated feature branch after the baseline compiles and its critical behavior is protected by tests.

## Git Flow

- `main` contains stable, demonstrable releases.
- `develop` integrates the next release.
- `feature/*` branches start from and merge into `develop`.
- `release/*` stabilizes a version before `main`.
- `hotfix/*` starts from `main` and merges back into both `main` and `develop`.

See [CONTRIBUTING.md](CONTRIBUTING.md) for commit and pull-request rules.

## Verification status

Static analysis and isolated C# probes reproduced eight correctness problems in the imported baseline. Full Editor import, PlayMode, IL2CPP build, and device profiling are pending until Unity 2021.3.25f1 is installed. No performance values are claimed before profiling a documented build and device.

## Licensing

A project-wide license has not been selected yet. Third-party packages and imported media retain their own terms. Before the first public release, the repository will include a reviewed root license and third-party notices.
