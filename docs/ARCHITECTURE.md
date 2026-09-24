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
  Undo re-applies the last swap and moves it to `Undone`; Redo moves it back; a new
  move clears `Undone`. `TileSelectionComponent` marks a tapped tile.
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
3. `BoardInputSystem`: accept at most one Undo/Redo, swipe or tap per tick, in that
   order; animation blocks all input.
4. `BoardProjectionSystem`: synchronize tile destinations with the board. The
   initial layout appears in place; later changes animate.
5. `TileHighlightSystem`: raise the selected tile.
6. `TileMoveSystem`: animate destinations without changing logical board state.
7. `BoardHudSystem`: push the move count and Undo/Redo availability when they change.

`WinCheckSystem` runs after that group. It reads the board,
waits for movement to finish, freezes input, then retains the existing two-second
result display. `BoardDestroySystem` handles exit and releases the board entity
and visuals. The one-frame events are cleared after consumers run.

## Boundaries

`Fives.Domain` has no Unity/ECS references. Current systems and presentation remain
in `Assembly-CSharp`; VContainer remains the composition root. More assemblies
will be added only when they enforce a useful dependency boundary. ECS is not
being replaced with an object-oriented application-service layer.

See [the board, shuffle and Undo/Redo contracts](BOARD_STATE.md) for invariants,
format details, verification results and remaining manual checks.

## Future work, not implemented here

- Daily challenge and best results.
- Versioned saves, stable content IDs and explicit migrations.
- Addressables with clear ownership, RU/EN localization and Input System actions.
- Unity PlayMode automation, Android build artifacts and device profiling.

These should use the same ECS command flow where they affect gameplay. Storage,
platform APIs and content loading can remain services behind the relevant systems.
