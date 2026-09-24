# ADR 0001: Keep GameStartup as the single ECS composition point

Date: 2026-09-24. Status: accepted.

## Context

The code review suggested replacing `GameStartup` (a scene MonoBehaviour) with a VContainer
entry point (`IStartable`/`ITickable`) and splitting system registration into features
(`BoardFeature`, `UiFeature`, `EconomyFeature`).

Correctness here depends on system order. Every event is a `OneFrame` removed after all
systems, so a consumer must run after the systems that send its events within a frame
(for example, `SoundSystem` after `UISystem`, `GamePlayManagementSystem` before the
`gamePlay` group).

## Decision

Keep `GameStartup` as the one place that lists systems, their order and the one-frame events.
Systems keep receiving shared services through `EcsSystems.Inject` and single-use dependencies
through constructors.

## Consequences

- The whole frame is readable on one screen; the ordering rule stays next to the list it governs.
- Adding a system means editing this one class.
- Revisit when the game has more than about five independent features, or when a feature
  must be switched on and off as a unit.
