# BoardState: first deterministic-core increment

Status: domain model and tests implemented; gameplay integration, seeded shuffle,
Undo, and replay remain subsequent reviewable increments.

Branch: `feature/deterministic-board`, based on the reviewed fixes at `c7900a8`.
Those prerequisite changes are still on `feature/baseline-verification`, awaiting
integration into `develop`. Merge that prerequisite PR before targeting this feature
at `develop`; no merge or history rewrite is part of this local increment.

## Contract

`BoardState` owns a square grid as a flat, private array of tile IDs. Both cells
and IDs are zero-based, in row-major order. A solved board has tile `N` in cell `N`.
`EmptyTileId` identifies the hidden image fragment; `EmptyCell` is its current cell.
They are deliberately different concepts. The existing game can hide any fragment,
so the domain does not silently change the rules to always hide the final tile.

```csharp
var board = new BoardState(size: 3, emptyTileId: 4);
bool moved = board.TryMove(cell: 1); // tile 1 slides into cell 4
int tile = board[4];                // 1
int empty = board.EmptyCell;        // 1
bool solved = board.IsSolved;      // false
```

- The solved constructor and imported-state constructor require an explicit empty tile ID.
- Imported data must contain each ID from 0 to `size * size - 1` exactly once.
  Bad construction data throws an argument exception (or overflow for an overflowing size).
- Input arrays/lists are copied. The public indexer has no setter and exposes no mutable array.
- `CanMove` and `TryMove` take a **cell index**, not an ID. Invalid move requests return
  `false`, leave the board unchanged, and do not allocate a result or throw.
- Only orthogonal neighbors of the empty cell may move. A move exchanges exactly two
  entries and updates the empty cell within a synchronous operation.
- `IsSolved` scans the small grid; there is no cached solved flag to keep synchronized.
- The model is mutable and intended for a single owning application session, not
  concurrent access. Deterministic means equal initial states plus equal inputs yield
  equal results; it does not mean thread-safe or immutable.

## Why this boundary

The class owns only arrangement and legal moves. It has no Unity/ECS types, clocks,
random generator, tasks, events, rewards, persistence, move history, or animation state.
No interface is needed for these fixed puzzle rules. The class is sealed because
inheritance is not an extension point for maintaining its permutation invariant.

Validation is concentrated at construction/import. Legal moves preserve that invariant,
so they do not rescan the permutation on every call. The constructor validates structure,
**not solvability**: importing an arbitrary permutation does not prove it is reachable.
The next shuffle increment must create reachable states through legal moves.

An imported board is copied for ownership, not designed as a serialization format.
Move count, accepted history, Undo and terminal-session behavior belong in the application
session. A solved board deliberately allows moves; freezing input after victory is a
session policy, while shuffle needs to move away from the solved arrangement.

## Integration sequence

1. Review this core, its tests, and the standalone test/CI entry points.
2. Add a specified seeded shuffle and application-session operations for moves, Undo and replay.
   Replay will include the hidden tile ID and algorithm/version information, not only a seed.
3. Connect the current ECS presentation to that session in one coherent change. Remove
   duplicate adjacency, logical swaps and victory checks from presentation at that point.
4. Verify rapid input, animations, victory/rewards, restart, and Android behavior in Unity.

Until step 3, the shipped gameplay continues to use its current ECS rules. The new model
is not a second live authority and no claim is made that gameplay integration is complete.

## Local verification (2026-09-21)

- Standalone Release build: 56 NUnit tests passed, 0 failed, 0 skipped, on macOS ARM64
  with .NET SDK 8.0.425. Of these, 51 are board cases and 5 are existing domain tests.
- Both Domain and its test assembly compile using Unity 6000.0.71f1 Roslyn and the
  project's generated reference lists, including Unity's NUnit assembly.
- All 34 existing service/presenter/ECS regression probes passed.
- Locked NuGet restore passed against a local feed of the official packages.
  The SDK could not reach the NuGet index in this environment; packages were downloaded
  over HTTPS from NuGet using curl. No alternate feed or machine-specific path is
  stored in the project. The workflow uses the default NuGet source.
- The CLI EditMode run was blocked by the existing Editor process (PID 38148).
  Standalone test execution and Unity-compatible compilation are not an Editor test run.
- GitHub-hosted Linux/Windows runs, PlayMode integration, Android builds, and native
  profiling have not run for this increment.

Final review on 2026-09-22: `actionlint` 1.7.12 accepted the workflow; the feature
diff passes whitespace checks. Additional TMP/URP and graphics-settings changes
appeared in the working tree during this task. They were not edited as part of this
increment and must be reviewed separately before any commit includes them.
