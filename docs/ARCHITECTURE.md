# Target architecture

## Current increment

`Fives.Domain.BoardState` and its invariant tests are implemented independently of Unity.
The current ECS gameplay has not yet been switched to that model. See
[the board contract and integration sequence](BOARD_STATE.md) and
[standalone test instructions](../tools/domain-tests/README.md).
The sections below describe the direction; they are not a list of completed features.

## Direction

The puzzle will use a small layered architecture:

```text
Unity Presentation
        ↓ intents / view state
Application Use Cases
        ↓
Pure C# Domain

Infrastructure implements ports defined by Application.
Bootstrap composes the concrete dependencies with VContainer.
```

The board is small and transactional. The target gameplay core will therefore be a deterministic C# model rather than another ECS migration. This makes atomic moves, Undo, Replay, solvers, and save migrations easy to verify without a scene.

## Assembly boundaries

- `Fives.Domain`: board and economy rules; no Unity references.
- `Fives.Application`: session use cases, accepted move history, replay, and infrastructure ports.
- `Fives.Presentation`: views, animation, input, and navigation.
- `Fives.Infrastructure`: persistence, Addressables, telemetry, and platform adapters.
- `Fives.Bootstrap`: VContainer composition and configuration.

Editor and test assemblies remain separate. Dependencies point inward and do not form cycles.
Only Domain is required for this increment. Further assembly splits need a concrete
dependency boundary; the five responsibilities do not mandate five new assemblies at once.

## Board model

One authoritative `BoardState` owns a private flat tile array, size, hidden tile ID, and
empty cell. `TryMove(cell)` validates and synchronously commits one legal swap, returning
whether it was accepted. Every move preserves one empty cell, unique tile IDs and bounds.
Move count, history, presentation results and terminal-session policy belong to Application.

Shuffle starts from a solved board and applies legal moves using a supplied seed. Replay data stores the rules version, seed, board size, accepted moves, and final hash. The same replay must produce the same result on every platform.

## Application and lifecycle

UI calls typed operations such as `TryMove`, `PurchaseTheme`, `ClaimReward`, and `LoadProgressAsync`. A View cannot debit currency or grant progress. Purchase and reward operations use idempotency keys and typed failure results.

VContainer remains the composition root. Add narrower scopes only when a concrete owned
resource needs them. Cancellation belongs to asynchronous operations that can outlive
their owner, not to synchronous board rules. Navigation must prevent stale callbacks from
reopening screens or destroying a later board.

## Data and content

Save data uses a versioned envelope, stable IDs, validation, sequential migrations, and backup recovery. Addressables replace string-based resource lookup; every handle has an owner and matching release. Tiles display regions of a shared texture instead of allocating copied textures for each cell.

Localization uses RU/EN String Tables. Display names never identify progress. Input System maps touch, pointer, keyboard, and gamepad to the same domain commands.

## Portfolio features

- Undo and best-result tracking.
- Deterministic replay and share code.
- Daily challenge from UTC date plus rules version.
- A* hints for 3×3 and a bounded strategy for larger boards.
- Typed achievements over session results.
- An Editor catalog validator for IDs, assets, and localization.

Cloud services remain adapters and require a real use case, offline fallback, and documented data policy. Valuable rewards or leaderboard scores require server validation rather than trusting the client.

## Quality evidence

EditMode tests protect board invariants, economy, replay, and save migrations. PlayMode tests cover navigation, rapid input, cancellation, suspend/resume, and Addressables ownership. CI produces test and build artifacts. Performance reports name the device, scenario, baseline, budget, and measured result.
