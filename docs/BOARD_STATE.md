# Deterministic board inside ECS

## Board contract

The puzzle is a rectangular swap puzzle (the game plays 4 × 3): every cell holds a fragment, and a move exchanges two
orthogonally adjacent cells. `BoardState` contains a private flat tile array plus a
reverse index. Cells and tile IDs are zero-based, in row-major order (`cell = row * Columns + column`). Solved means
tile `N` occupies cell `N`.

```csharp
var board = new BoardState(columns: 3, rows: 3);
board.TrySwap(new Swap(4, 1)); // tiles in cells 1 and 4 exchange places
board.CellOf(tileId: 4);       // 1, in O(1)
```

Construction validates dimensions and an imported permutation, copying input data.
`Swap` stores its cells in ascending order, so `Swap(4, 1)` equals `Swap(1, 4)`.
Invalid swaps (same cell, off the board, diagonal or across a row edge) return false
without mutation. A swap is its own inverse. Because adjacent swaps generate every
permutation, any arrangement is solvable. The board permits swaps from the solved
state; ECS decides when player input is locked.

## Picture crop

Puzzle pictures may have any aspect ratio. `BoardInitSystem` takes the largest centred part of the
sprite with the board's aspect (`Columns / Rows`) and cuts it into square tiles, so nothing is
stretched. Menu cards and the gameplay sample use `Image.SetCover` with an `AspectRatioFitter` in
`EnvelopeParent` mode: the picture covers its masked window and shows its centre.

The layout depends only on the cell count, so the square-board reference vectors did not change
when rectangular boards were introduced.

## Seeded shuffle version 2

`SeededShuffle.Create(columns, rows, seed)` runs Sattolo's algorithm driven by:

- xorshift32 with shifts 13, 17, 5 and unsigned 32-bit state;
- signed seeds interpreted as their unsigned bit pattern;
- seed zero mapped to `0x6D2B79F5`;
- for `i` from `cellCount - 1` down to 1, swap position `i` with `random % i`.

Sattolo's algorithm produces a single cycle, so **no fragment starts in its own cell**
and the board is never solved at the start. This is reproducible shuffling, not
cryptographic randomness. The runtime picks a seed once per run and keeps it in
`BoardHistoryComponent`; the UI shows it. Domain code never uses Unity random state
or `System.Random`.

## Input and Undo

`TileUiProvider` sends `TileClickEvent` on a tap and `TileSwipeEvent` (column/row
step, rows grow downward) when a drag ends. `BoardInputSystem` turns them into swaps:

- tap a tile to select it (`TileSelectionComponent` on the board entity);
- tap a neighbor to swap with the selection, tap the selected tile again to clear it,
  or tap any other tile to move the selection;
- swipe from a tile toward a neighbor to swap them directly; a swipe off the edge is rejected.

One input is accepted per tick. Input during movement, exit or the result delay
is ignored. `TileHighlightSystem` raises the selected tile.

History records accepted swaps in `Moves`. A swap is its own inverse: Undo re-applies
the last move and forgets it. Undo does not refund energy or rewind time.

## Hints

A hint costs `GlobalConfig.HintPrice` stars (5). `BoardHintSystem` picks one of the
misplaced tiles closest to their cells (`BoardHint.ClosestMisplaced`, random among ties)
and marks it with `BoardHintComponent`. The hint is shown once and ends with the next
move (a swap of any tiles or Undo); while it is shown the button is disabled. It is not sold while
tiles move or after the win; without enough stars the header reports it and nothing is spent.

`BoardHint.PathOf` returns a shortest route of cells from the tile to its cell. Among
the shortest routes it disturbs the fewest correctly placed tiles, horizontal first on ties.
Following the route places the tile and moves every other tile by one cell at most.
This guides one tile, not the whole board: it is not an optimal solver, and in rare
layouts two hinted tiles can push each other out, so a solution is not promised.
`BoardHintViewSystem` draws the route when the hint is bought: a frame on the tile,
dots along the way, an arrow on the last step and a pulsing frame on the target.

## Display integration

`BoardProjectionSystem` is the only bridge from board arrangement to tile destinations.
It runs on initialization or `BoardChangedEvent`; the first layout appears in place and
a swap animates both tiles at once. `TileMoveSystem` only animates.
`WinCheckSystem` uses the board's solved state.

## Verification

- 75 NUnit domain tests pass (`dotnet test`, Release, .NET 10 runtime with roll-forward):
  fixed shuffle vectors, 5,005 seed/dimension combinations with no fragment in place,
  random swap sequences with permutation invariants, exhaustive 2x2 reachability
  (all 24 arrangements), invalid input and hint routes over 300 seeds per board size.
- 64 `Fives.Runtime.Tests` EditMode tests pass in Unity against the real ECS systems:
  tap selection, swipes, edge rejection, rapid input, Undo, hints (price, repeat press,
  not enough stars, movement lock, ending on any move or Undo), projection, completion ordering,
  result delay, manual exit and cleanup.
- Not verified on a device yet.

### Manual acceptance in Unity

1. Start a puzzle: every cell is filled and no fragment is in place.
2. Tap a tile: it is raised. Tap a neighbor: they swap. Tap a far tile: the selection moves.
3. Swipe tiles in all four directions, including toward an edge (rejected).
4. Undo all moves: the initial layout returns.
5. Press the hint: 5 stars are spent and a route from a tile to its cell appears; any move hides it.
6. Solve the puzzle: the tiles dissolve into the whole picture, then the result screen shows moves and time.
