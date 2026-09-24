# Deterministic board inside ECS

## Board contract

The puzzle is a swap puzzle: every cell holds a fragment, and a move exchanges two
orthogonally adjacent cells. `BoardState` contains a private flat tile array plus a
reverse index. Cells and tile IDs are zero-based, in row-major order. Solved means
tile `N` occupies cell `N`.

```csharp
var board = new BoardState(size: 3);
board.TrySwap(new Swap(4, 1)); // tiles in cells 1 and 4 exchange places
board.CellOf(tileId: 4);       // 1, in O(1)
```

Construction validates dimensions and an imported permutation, copying input data.
`Swap` stores its cells in ascending order, so `Swap(4, 1)` equals `Swap(1, 4)`.
Invalid swaps (same cell, off the board, diagonal or across a row edge) return false
without mutation. A swap is its own inverse. Because adjacent swaps generate every
permutation, any arrangement is solvable. The board permits swaps from the solved
state; ECS decides when player input is locked.

## Seeded shuffle version 2

`SeededShuffle.Create(size, seed)` runs Sattolo's algorithm driven by:

- xorshift32 with shifts 13, 17, 5 and unsigned 32-bit state;
- signed seeds interpreted as their unsigned bit pattern;
- seed zero mapped to `0x6D2B79F5`;
- for `i` from `cellCount - 1` down to 1, swap position `i` with `random % i`.

Sattolo's algorithm produces a single cycle, so **no fragment starts in its own cell**
and the board is never solved at the start. This is reproducible shuffling, not
cryptographic randomness. The runtime picks a seed once per run and keeps it in
`BoardHistoryComponent`; the UI shows it. Domain code never uses Unity random state
or `System.Random`.

## Input, Undo and Redo

`TileUiProvider` sends `TileClickEvent` on a tap and `TileSwipeEvent` (column/row
step, rows grow downward) when a drag ends. `BoardInputSystem` turns them into swaps:

- tap a tile to select it (`TileSelectionComponent` on the board entity);
- tap a neighbor to swap with the selection, tap the selected tile again to clear it,
  or tap any other tile to move the selection;
- swipe from a tile toward a neighbor to swap them directly; a swipe off the edge is rejected.

One input is accepted per tick. Input during movement, exit or the result delay
is ignored. `TileHighlightSystem` raises the selected tile.

History records accepted swaps in `Moves`. A swap is its own inverse: Undo re-applies
the last move and pushes it onto `Undone`; Redo re-applies the last undone swap and
moves it back. A new move clears `Undone`, so abandoned moves cannot be redone.
Undo does not refund energy or rewind time.

## Display integration

`BoardProjectionSystem` is the only bridge from board arrangement to tile destinations.
It runs on initialization or `BoardChangedEvent`; the first layout appears in place and
a swap animates both tiles at once. `TileMoveSystem` only animates.
`WinCheckSystem` uses the board's solved state.

## Verification

- 59 NUnit domain tests pass (`dotnet test`, Release, .NET 10 runtime with roll-forward):
  fixed shuffle vectors, 4,004 seed/size combinations with no fragment in place,
  random swap sequences with permutation invariants, exhaustive 2x2 reachability
  (all 24 arrangements) and invalid input.
- 70 ECS probes pass against the real systems with engine substitutes: tap selection,
  swipes, edge rejection, rapid input, Undo/Redo, redo-stack clearing, completion
  ordering and cleanup.
- Not verified in Play Mode or on a device yet.

### Manual acceptance in Unity

1. Start a puzzle: every cell is filled and no fragment is in place.
2. Tap a tile: it is raised. Tap a neighbor: they swap. Tap a far tile: the selection moves.
3. Swipe tiles in all four directions, including toward an edge (rejected).
4. Undo all moves: the initial layout returns. Redo them, then make a new move: Redo is disabled.
5. Solve the puzzle: moves and time appear on the result screen.
