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

Replay: optional BoardReplayComponent → BoardReplaySystem → same projection
Victory: BoardComponent + completed animations → WinCheckSystem
Exit: GameEndEvent → BoardDestroySystem
```

## Data ownership

- `BoardComponent.State` is the authoritative live board.
- `BoardHistoryComponent` contains the seed, shuffle length, initial empty cell,
  and the accepted source-cell history. Undo removes the final history entry.
- `BoardReplayComponent` is temporary playback data on the same entity. Its board
  is separate from the live attempt. Removing this component ends playback.
- `TileComponent.Cell` is a display destination; `GameSettings.CellToAnchored`
  converts it to a UI position. It cannot determine a legal move or victory.
  `MoveComponent` tracks animation progress only.
- The existing `GameSession` retains selected content, run lifecycle, and rewards.
  It has no new board, move, Undo, or replay operations.

One board entity owns attempt data. Destroying it also discards its history and
playback state. No cancellation tokens, delayed cleanup tasks, new locks, or run
identifiers were added.

## Systems and ordering

The `gamePlay` group runs:

1. `BoardSetupSystem`: create one seeded board entity for an active run.
2. `BoardInitSystem`: create the existing board/tile visuals.
3. `BoardInputSystem`: accept at most one control or tile tap per tick. Controls
   take precedence over taps; animation blocks edits. Stop can interrupt replay.
4. `BoardReplaySystem`: advance playback only after the preceding animation ends.
5. `BoardProjectionSystem`: synchronize tile destinations with the live/playback
   board. Initial layout and replay start/stop snap instead of animating all tiles.
6. `TileMoveSystem`: animate destinations without changing logical board state.

`WinCheckSystem` runs after that group. It reads the live board, ignores replay,
waits for movement to finish, freezes input, then retains the existing two-second
result display. `BoardDestroySystem` handles exit and releases the board entity
and visuals. The one-frame events are cleared after consumers run.

## Boundaries

`Fives.Domain` has no Unity/ECS references. Current systems and presentation remain
in `Assembly-CSharp`; VContainer remains the composition root. More assemblies
will be added only when they enforce a useful dependency boundary. ECS is not
being replaced with an object-oriented application-service layer.

See [the board, shuffle and replay contracts](BOARD_STATE.md) for invariants,
format details, verification results and remaining manual checks.

## Future work, not implemented here

- Replay persistence and share/import UI; daily challenge and best results.
- Versioned saves, stable content IDs and explicit migrations.
- Addressables with clear ownership, RU/EN localization and Input System actions.
- Unity PlayMode automation, Android build artifacts and device profiling.

These should use the same ECS command flow where they affect gameplay. Storage,
platform APIs and content loading can remain services behind the relevant systems.
