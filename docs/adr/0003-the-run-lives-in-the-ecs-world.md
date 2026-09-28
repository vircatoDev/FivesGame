# ADR 0003: The run lives in the ECS world

Date: 2026-09-26. Status: accepted. Supersedes [ADR 0002](0002-keep-run-state-in-gamesession.md).

## Context

ADR 0002 kept the run's state in `GameSession` (`IsRunning`, `IsCompleted`, `EndRun`) while the board entity held the
board itself. The same fact then lived in two places: gameplay systems repeated session checks next to their board
filters, and a start whose picture failed to load could leave `IsRunning` set and block every later start.

## Decision

The board entity is the run. `StartRunRequest` asks for it, `BoardSolvedComponent` marks it solved, and
`GameEndEvent` removes the board, its tiles and a request not yet played. `GameSession` keeps only what outlives a
board: the player's choice, the last result and the reward claim.

## Consequences

- One owner: gameplay systems filter boards (solved boards are excluded) instead of reading a session flag, and
  cleanup cannot leave a stale flag behind.
- `GameStartService` refuses a second start while another one is loading, prepared or requested, or a board exists;
  it reads the ECS filters for the last two. A failed load leaves nothing to reset.
- The result screen reads `GameSession.LastGameResult`, which stays after the board is destroyed.
- Revisit if runs need to be saved and resumed, or if several boards can exist at once.
