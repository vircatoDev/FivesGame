# Deterministic board inside ECS

Branch: `feature/deterministic-board`. The prerequisite fixes at `c7900a8` are
still on `feature/baseline-verification`; integrate that prerequisite before
merging this feature into `develop`. No merge or history rewrite is part of this change.

## Board contract

`BoardState` contains a private flat tile array. Cells and tile IDs are zero-based,
in row-major order. Solved means tile `N` occupies cell `N`. `EmptyTileId` identifies
the hidden fragment; `EmptyCell` is its current location. Any fragment can be hidden.

```csharp
var board = new BoardState(size: 3, emptyTileId: 4);
board.TryMove(cell: 1); // tile 1 enters cell 4; empty cell becomes 1
```

Construction validates dimensions, hidden ID and an imported permutation, copying
input data. Invalid move requests return false without mutation. Orthogonal legal
moves preserve the permutation. Imported permutations are not checked for solvability.
The board deliberately permits moves from the solved state; ECS decides when player
input is locked. This is a synchronous model owned by one board entity, not a game
session or a concurrency abstraction.

## Seeded shuffle version 1

`SeededShuffle.Create(size, emptyTileId, seed, steps)` starts from solved and makes
legal moves, so every generated board is reachable. The recipe is:

- xorshift32 with shifts 13, 17, 5 and unsigned 32-bit state;
- signed seeds are interpreted as their unsigned bit pattern;
- seed zero maps to `0x6D2B79F5`;
- neighbors are considered left, right, up, down; the immediately preceding empty
  cell is excluded; selection is `random % candidateCount`;
- `steps` must be positive. If the walk returns to solved, one additional legal
  move uses the first remaining candidate.

This is reproducible shuffling, not cryptographic randomness or a uniform sample
of every reachable permutation. Shuffle length is not a guarantee of puzzle difficulty.
The runtime picks a seed once per run, uses `seed % cellCount` as the hidden ID and
`cellCount * 4` shuffle steps, and retains that recipe in `BoardHistoryComponent`.
The UI shows the seed. Domain code never uses Unity random state or `System.Random`.

## Moves, Undo and replay

`BoardInputSystem` maps a tapped tile ID to its source cell, calls the board rule,
and records only accepted cells. One command is accepted per tick. Input during
movement, exit or the result delay cannot enqueue stale moves.

Undo moves the empty cell back to its preceding location, then removes the final
history entry. The initial empty cell is kept for undoing the first move. A new
move after Undo therefore starts a new path; abandoned moves are not replayed.
Undo is available during an unfinished attempt after its last animation finishes.
It does not refund energy or rewind wall-clock time.

`ReplayData` stores format version, size, hidden tile ID, seed, shuffle length and
accepted cells. It copies the history and validates the entire sequence, rejecting
illegal moves and moves after a solved board. Version 1 fixes both shuffle and
move semantics; changing either requires version handling, not silently changing
old replay results. This is an in-memory contract, not yet a save/share file format.

Replay starts from that recipe on a separate board in `BoardReplayComponent`.
`BoardReplaySystem` advances one move after the previous animation. The live board
and history remain unchanged. Stop interrupts even an in-progress animation;
projection snaps back to the live attempt. Finishing playback returns automatically.
Replay does not trigger completion, energy spending or reward flows.

This increment exposes playback of the **current unfinished attempt**. A persistent
archive, post-result viewer, import/export buttons and a share code are future work.
The board entity, history and replay are released on exit.

## Display integration

`BoardProjectionSystem` is the only bridge from board arrangement to tile destinations.
`TileMoveSystem` only animates those destinations. `WinCheckSystem` uses the board's
solved state rather than reconstructing it from floating-point visual coordinates.
The old `PuzzleGenerator`, `ShuffleSystem`, `TileClickSystem`, empty-tile marker and
unused `isEmpty` flag have been removed.

The existing uGUI screen creates a small touch toolbar below its preview panel:
**Отмена** and **Повтор / Стоп**, plus move count and seed. Buttons send ECS events;
they cannot directly mutate the board. The layout uses the existing landscape
Canvas and font; portrait/safe-area redesign is outside this increment.

## Verification

- **74 NUnit tests pass** in Release on macOS ARM64 with .NET SDK 8.0.425.
  These include the previous 56 cases, four fixed shuffle vectors, 6,565
  combinations of seed/size/hidden ID, exhaustive 2x2 reachability, replay sequence
  reconstruction, copying and invalid input. Sizes 2, 3, 4 and 6 are covered.
- **50 service/ECS probes pass** against the actual game systems and LeoECS source
  with minimal engine substitutes. They cover rapid taps, Undo, branching history,
  playback/interrupt, completion ordering, cleanup/restart and previous economy,
  storage and lifecycle regressions. These do not simulate Unity rendering.
- Domain, domain tests and the full runtime assembly compile with Unity
  **6000.0.71f1** Roslyn and the project's actual assembly references. Existing
  unrelated unawaited-call/unused-field warnings remain.
- The existing Linux/Windows GitHub Actions workflow discovers the new domain tests
  through the same source glob. It is unchanged. No remote run for this uncommitted
  increment is claimed; ECS probes are still local, not part of that workflow.
- Unity CLI EditMode execution is blocked because the project is already open in
  Editor (PID 38148). Pipeline is not installed in that Editor. Native UI automation
  is unavailable because Computer Use permission is not granted. No PlayMode,
  visual-layout, Android build or device-performance success is claimed.

### Manual acceptance in Unity

1. Open `Assets/Scenes/MainGame.unity` with 6000.0.71f1 and enter Play Mode.
2. Start a puzzle. Check the hidden tile, touch toolbar, count and seed.
3. Make several moves; tap quickly during animation. Only accepted moves count.
4. Undo all moves: the initial shuffled layout must return. Make a different move.
5. Play replay, stop during a move, then let it finish. Both paths must restore
   the live board and history without energy or rewards changing.
6. Solve a puzzle: the final animation completes before the result delay; further
   taps/Undo are blocked. Claim the reward and start another puzzle.
7. Exit during movement/playback, then start again. Check for stale tiles or history.
8. Repeat on an Android device, checking button size, margins, frame time and logs.

TMP/URP assets and graphics settings already had unrelated changes in the working
tree. They are not part of this feature's review patch.
