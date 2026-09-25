# ECS gameplay architecture

## Decision

Gameplay remains based on **LeoECS components and systems**. A separate application
session or a five-layer assembly split is not the target of this increment.

`BoardState` is a small pure C# model for a tile permutation and legal swaps. It is
owned by an ECS board entity. It does not manage a game session, input, history,
rewards, animations, or navigation. Pure rules remain independently testable while
systems control the game loop.

```text
Touch UI → TileClickEvent / BoardControlEvent
                       ↓
              BoardInputSystem
                       ↓
    Board entity: BoardComponent + BoardHistoryComponent
                       ↓
    BoardProjectionSystem → MoveComponent → TileMoveSystem → uGUI

Victory: BoardComponent + completed animations → WinCheckSystem
Exit: GameEndEvent → BoardDestroySystem
```

## Data ownership

- `BoardComponent.State` is the authoritative live board.
- `BoardHistoryComponent` contains the seed, the accepted swaps and the start time.
  Undo re-applies the last swap and forgets it. `TileSelectionComponent` marks a tapped
  tile; `BoardHintComponent` marks the tile whose route a bought hint shows until the next move.
- `TileComponent.Cell` is a display destination; `GameSettings.CellToAnchored`
  converts it to a UI position. It cannot determine a legal move or victory.
  `MoveComponent` tracks animation progress only.
- The existing `GameSession` retains selected content, run lifecycle, and rewards.
  It has no board, move or Undo operations.

One board entity owns attempt data. Destroying it also discards its history and
selection. No cancellation tokens, delayed cleanup tasks, new locks, or run
identifiers were added.

## Systems and ordering

The `gamePlay` group runs:

1. `BoardSetupSystem`: create one seeded board entity for an active run.
2. `BoardInitSystem`: create the existing board/tile visuals.
3. `BoardInputSystem`: accept at most one Undo, swipe or tap per tick, in that
   order; animation blocks all input.
   `BoardHintSystem`: sell a hint for stars and drop it on the next move.
   `BoardHintViewSystem`: show or hide the hint route.
4. `BoardProjectionSystem`: synchronize tile destinations with the board. The
   initial layout appears in place; later changes animate.
5. `TileHighlightSystem`: raise the selected tile.
6. `TileMoveSystem`: animate destinations without changing logical board state.
7. `BoardHudSystem`: push the move count and Undo/Hint availability when they change.

`WinCheckSystem` runs after that group. It reads the board,
waits for movement to finish, freezes input, sends `BoardSolvedEvent` (`BoardRevealSystem`
fades the tiles into the whole picture), then retains the existing two-second result display. `BoardDestroySystem` handles exit and releases the board entity
and visuals. The one-frame events are cleared after consumers run.

## Boundaries

Assemblies, each referencing only what it declares:

- `Fives.Domain` — board rules, shuffle, economy and save migration; no Unity or ECS references.
- `Fives.Runtime` (`Assets/Scripts`) — ECS systems, services, presenters and views; references
  Domain, LeoECS, VContainer, UniTask, DOTween.Modules, uGUI, TextMeshPro and Simple Scroll-Snap.
  VContainer remains the composition root.
- `DOTween.Modules` — DOTween's UI/audio/physics extension sources, moved out of
  `Assembly-CSharp-firstpass` so an asmdef can reference them.
- `Fives.Domain.Tests`, `Fives.Runtime.Tests` and `Fives.UI.Editor.Tests` — EditMode tests of the domain,
  of ECS systems, services and presenters (with fake views and a fake `IFrameTime`), and of the
  gameplay prefab. Runtime internals are visible to `Fives.Runtime.Tests` only.
- Test boundaries in Runtime: `IFrameTime` for frame timing, view contracts in `ViewContracts.cs`
  (`IMainMenuView`, `ISelectMenuView`, `IGamePlayView`, `IGameResultView`, `IHeaderPanelView`) and a key
  prefix in `StorageService`, so tests never touch the player's save.

More assemblies will be added only when they enforce a useful dependency boundary. ECS is not
being replaced with an object-oriented application-service layer.

See [the board, shuffle, Undo and hint contracts](BOARD_STATE.md) for invariants,
format details, verification results and remaining manual checks.

## Future work, not implemented here

- Daily challenge and best results.
- Versioned saves, stable content IDs and explicit migrations.
- Addressables with clear ownership, RU/EN localization and Input System actions.
- Unity PlayMode automation, Android build artifacts and device profiling.

These should use the same ECS command flow where they affect gameplay. Storage,
platform APIs and content loading can remain services behind the relevant systems.

## Screen sizes

The game is landscape and designed at 1920x1080. The canvas scaler uses `Expand`, so the
canvas is never smaller than the design: wide phones (19.5:9, 21:9) get extra width, 4:3
tablets extra height, and nothing is clipped. Screen content is anchored to the centre, so
it stays one group with the background visible around it. The gameplay screen is the exception:
the board is centred and its side panels are pinned to the safe-area edges, so wide phones move
them apart instead of leaving empty meadow between them. Backgrounds cover the screen with
an `AspectRatioFitter` in `EnvelopeParent` mode. The header sits in `SafeAreaFitter`, which
keeps its buttons away from notches and the Dynamic Island. `AdaptiveLayoutTests` check the
scaler, the backgrounds and the safe area.

## Decisions

- [ADR 0001: Keep GameStartup as the single ECS composition point](adr/0001-keep-gamestartup-composition.md)
- [ADR 0002: Keep run state in GameSession](adr/0002-keep-run-state-in-gamesession.md)
