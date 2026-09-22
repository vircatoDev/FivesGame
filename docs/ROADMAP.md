# Modernization roadmap

## 0. Unity 6 Android baseline

- Use Unity 6000.0.71f1 with Android Build Support and restore locked packages.
- Exercise menu → puzzle → win → reward → restart.
- Produce an IL2CPP/ARM64 Android App Bundle and capture its Build Report.
- Verify asset redistribution rights and add LICENSE/THIRD_PARTY_NOTICES.
- Pin Git dependencies explicitly in the manifest.

Exit: a clean clone opens in Unity 6000.0.71f1 and produces an Android App Bundle, or every remaining external blocker is documented with a reproducible command.

## 1. Correctness

- Add regressions for purchase, reward, recovery, event ordering, and rapid tile input.
- Make purchases and rewards atomic and idempotent.
- Introduce a clock abstraction and correct recovery remainder handling.
- Remove unowned delayed work and serialize navigation transitions.
- Handle corrupt saves and make board generation and win checks honor the configured size.

Exit: the eight reproduced defects have passing Unity tests.

## 2. Deterministic board in ECS

- Add assembly boundaries and pure `BoardState`.
- Support tested 3×3 and 4×4 rules without hardcoded width.
- Store history and playback on the board entity; systems handle Undo, replay and completion.
- Run property-style tests over many seeds and move sequences.
- Keep ECS as the game architecture; document the boundary between pure board rules and systems.

Implemented: seeded shuffle, Undo/current-attempt replay, ECS display integration, prefab controls and automated logic checks. Review fixes and the Riverside Village visual redesign are included in the develop integration. Final validation is compilation-only at the owner’s request; Unity interaction and Android acceptance remain pending.
Exit: reproducible replay and verified Unity/Android interaction. A separate application-session layer is not planned.

## 3. Persistence and content

- Add versioned saves, migrations, stable IDs, backup, and recovery.
- Move screens and themes to Addressables with ownership tests.
- Remove copied runtime textures and profile repeated sessions.
- Remove unused samples and packages.
- Add a UI Toolkit catalog validator.

Exit: content is data-driven and repeated loading does not grow live resources.

## 4. Portfolio gameplay

- Persistent/post-result replay viewer and share code (current-attempt playback is implemented).
- Daily challenge and streak.
- A* hints for 3×3 and bounded hints for 4×4.
- Move/time challenge rules and achievements.
- Typed local telemetry with a debug overlay.

Exit: every feature has tests, a documented trade-off, and a short demonstration.

## 5. Delivery quality

- RU/EN Localization.
- Touch, pointer, keyboard, and gamepad Input System actions.
- Focus navigation, safe area, high contrast, and reduced motion.
- Independent AudioMixer controls for music and SFX.
- Suspend/resume and active-session recovery.
- GitHub Actions/GameCI, Android artifacts, profiler report, and release video.

Exit: the full flow is reproducible in CI and available as a playable build.
