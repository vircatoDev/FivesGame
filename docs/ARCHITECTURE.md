# Target architecture

## Direction

The puzzle will use a small layered architecture:

```text
Unity Presentation
        ↓ intents / view state
Application Use Cases
        ↓
Pure C# Domain
        ↓ ports
Infrastructure

Bootstrap composes the layers with VContainer scopes.
```

The board is small and transactional. The target gameplay core will therefore be a deterministic C# model rather than another ECS migration. This makes atomic moves, Undo, Replay, solvers, and save migrations easy to verify without a scene.

## Assembly boundaries

- `Fives.Domain`: board, sessions, economy rules, replay; no Unity references.
- `Fives.Application`: use cases and infrastructure ports.
- `Fives.Presentation`: views, animation, input, and navigation.
- `Fives.Infrastructure`: persistence, Addressables, telemetry, and platform adapters.
- `Fives.Bootstrap`: VContainer composition and configuration.

Editor and test assemblies remain separate. Dependencies point inward and do not form cycles.

## Board model

One authoritative `BoardState` owns a flat tile array, dimensions, empty cell, and move count. `TryMove` validates and commits one move atomically, returning an immutable result for presentation. Every operation preserves one empty cell, unique tile IDs, valid bounds, and one terminal result per session.

Shuffle starts from a solved board and applies legal moves using a supplied seed. Replay data stores the rules version, seed, board size, accepted moves, and final hash. The same replay must produce the same result on every platform.

## Application and lifecycle

UI calls typed operations such as `TryMove`, `PurchaseTheme`, `ClaimReward`, and `LoadProgressAsync`. A View cannot debit currency or grant progress. Purchase and reward operations use idempotency keys and typed failure results.

VContainer remains the composition root with App, Menu, and GameSession scopes. Disposing a session cancels its asynchronous work and animations. Navigation serializes transitions so stale callbacks cannot reopen screens or destroy a later board.

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
