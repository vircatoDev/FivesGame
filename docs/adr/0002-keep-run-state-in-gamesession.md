# ADR 0002: Keep run state in GameSession

Date: 2026-09-24. Status: accepted.

## Context

`GameSession` holds both the player's selection (theme, puzzle, mode) and the state of the
current run (`IsRunning`, `IsCompleted`, result, reward claim). The review suggested moving run
state into a `RunComponent` on the board entity, so ECS owns it and cleanup removes it.

Run state (`IsRunning`, `IsCompleted`) is read by services and presenters
(`GameStartService`, `PlayerProgressService`, `MainMenuPresenter`, `SelectMenuPresenter`) and
five board systems (`BoardSetup`, `BoardInput`, `BoardProjection`, `BoardHud`, `WinCheck`). `GameStartService.TryStart` starts the run before the board exists, and
the result screen reads the result after the board entity is destroyed.

## Decision

Keep run state in `GameSession`.

## Consequences

- The start flow (energy check, navigation, menu hide animation) keeps its current frame order.
- Reset is explicit (`BeginRun`/`EndRun`) and covered by EditMode tests: the next run resets
  completion; an ended run cannot recreate a board.
- Revisit if runs need to be saved and resumed, or if several boards can exist at once
  (for example, a daily challenge played alongside a theme puzzle).
