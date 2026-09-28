# Architecture

Two scenes and two VContainer scopes. The **Boot** scene prepares the app: balance, save, texts and content. The
**MainGame** scene runs the game in LeoECS: the board, input, animations and the result. Pure rules live in
`Fives.Domain` without Unity references. Screens are MVP over view interfaces. A presenter calls services directly
and reaches the ECS world only through one-frame events.

## Scenes and scopes

`AppLifetimeScope` (Boot scene, `DontDestroyOnLoad`) holds the services that live for the whole app: the save,
the balance, language and texts, sound, currencies, progress, theme content, ads and the loading screen. Its entry
point `BootFlow` runs the loading steps in order and fills the progress bar:

1. `RemoteBalance`: Remote Config with a 3 s timeout; offline, the cached or built-in values apply.
2. `PlayerSave.Load`: right after the balance, because a new save takes its starting stars and energy from
   it. Reading the save earlier throws, so a service created too early fails at once.
3. `LocalizedTexts`: the saved language and its tables. WebGL cannot load them synchronously later.
4. `ThemePreviews`: the small theme pictures, held for the session.
5. Theme bundles: every theme downloads here, so the menus open any theme at once. A failed download waits for
   **Retry** on the loading screen ([content](CONTENT.md)).
6. The MainGame scene. The loading screen stays until `GameStartup` has started the game.

`GameLifetimeScope` (MainGame scene) is a child scope: the ECS world, `GameSession`, `GameStartService`,
`ThemeShop`, the screen navigator, the state machine, the header and the presenters. `GameStartup` is the one place
that lists the systems, their order and the one-frame events ([ADR 0001](adr/0001-keep-gamestartup-composition.md)).

## Gameplay in ECS

`BoardState` is a small pure C# model for a tile permutation and legal swaps. It is owned by an ECS board entity and
does not manage input, history, rewards, animations or navigation. Pure rules stay independently testable while
systems run the game loop.

```text
Touch UI → TileClickEvent / TileSwipeEvent / BoardControlEvent
                       ↓
              BoardInputSystem
                       ↓
    Board entity: BoardComponent + BoardHistoryComponent
                       ↓
    BoardProjectionSystem → MoveComponent → TileMoveSystem → uGUI

Victory: BoardComponent + finished animations → WinCheckSystem → BoardSolvedComponent
Exit: GameEndEvent → BoardDestroySystem
```

### Data ownership

- The board entity is the run ([ADR 0003](adr/0003-the-run-lives-in-the-ecs-world.md)). `StartRunRequest` asks for
  it, and `GameEndEvent` removes it with its history, selection and tiles.
- `BoardComponent.State` is the authoritative live board.
- `BoardHistoryComponent` holds the seed, the accepted swaps and the start time. Undo re-applies the last swap and
  forgets it. `TileSelectionComponent` marks a tapped tile. `BoardHintComponent` marks the tile whose route a bought
  hint shows until the next move.
- `TileComponent.Cell` is a display destination. `GameSettings.CellToAnchored` converts it to a UI position. It
  cannot decide a legal move or a victory. `MoveComponent` tracks animation progress only.
- `GameSession` keeps what outlives a board: the player's choice, the last result and the reward claim.

### Systems and ordering

`GamePlayManagementSystem` runs first and enables the `gamePlay` group only in the Playing state. The group runs:

1. `BoardSetupSystem`: turns a start request into one seeded board entity.
2. `BoardInitSystem`: creates the board and tile visuals.
3. `BoardInputSystem`: accepts at most one Undo, swipe or tap per tick, in that order. Animation blocks all input.
4. `BoardHintSystem`: sells a hint for stars and drops it on the next move. `BoardHintViewSystem` shows or hides the
   route.
5. `BoardProjectionSystem`: synchronizes tile destinations with the board. The initial layout appears in place, and
   later changes animate.
6. `TileHighlightSystem`: raises the selected tile.
7. `TileMoveSystem`: animates destinations without changing the board.
8. `WinCheckSystem`: once the last move has finished, marks the board solved and records the result. Two seconds
   later it ends the run and opens the result screen.
9. `PuzzleCompletionSystem`: records and saves the solved puzzle in the frame it is solved.
10. `BoardHudSystem`: pushes the move count and Undo/Hint availability when they change.

After the group:
- `BoardRevealSystem` fades the tiles into the whole picture.
- `BoardDestroySystem` ends the run.
- `GameStateSystem` applies the frame's last state request through the state machine and the screen navigator.
- `EnergyRecoverySystem`, `SoundSystem`.
- `CurrencySyncSystem` runs after every spender, before the header and the save.
- `CommonUIHeaderPanelSystem`.
- `StorageSystem` writes at most one save per frame.

Every event is a `OneFrame` removed after all systems. A consumer therefore runs after the systems that send its
events within a frame.

See [the board, shuffle, Undo and hint contracts](BOARD_STATE.md) for invariants and format details.

## Screens, services and the ECS world

- Screens are MVP. A presenter sees its view only through an interface (`ViewContracts.cs`), so the runtime tests run
  presenters with fake views. `ScreenNavigator` opens a screen's prefab and resolves the presenter by the type its
  view declares.
- Presenters and services call services directly, for example `ThemeShop` → `StarService`.
- They change the game only through one-frame requests and events in the ECS world: `EcsWorldEvents.Send`,
  `ChangeState` and `PlaySound`, and the `StartRunRequest` of `GameStartService`. Systems handle these in the next
  `Run()`. The world is the game's event bus, so no screen or service touches a system or a board component. The only
  reads are `GameStartService`'s filters: a second start is refused while a start is requested or a board exists.

## Content and texts

- Theme pictures are Addressables bundles on GitHub Pages. Only the loading screen downloads them.
  `ISpriteLoader` loads a picture from the downloaded bundle into memory for an owner and releases everything the
  owner loaded:
  - the menu previews, held for the session;
  - the open theme's puzzle cards, until the theme list comes back;
  - the current run's picture, until the next run.

  See [content](CONTENT.md).
- Texts come from Unity Localization. Static labels and the main menu's theme names are `LocalizeStringEvent`
  components that follow a language change by themselves. Texts built in code go through `ITexts` when a screen
  opens. See [localization](LOCALIZATION.md).

## Assemblies

Each assembly references only what it declares:

- `Fives.Domain`: board rules, shuffle, hints, economy and save migration. No Unity or ECS references. Its tests
  also run without Unity in CI.
- `Fives.Runtime` (`Assets/Scripts`): ECS systems, services, presenters and views. It references Domain, LeoECS,
  VContainer, UniTask, DOTween.Modules, uGUI, TextMesh Pro, Simple Scroll-Snap, Localization, Addressables, Unity
  Services (Remote Config) and LevelPlay. Its internals are visible to `Fives.Runtime.Tests` only.
- `Fives.Editor`: player and content builds, the content menu, the Excel text import, Play Mode from the Boot scene
  and the child-directed Android manifest.
- `DOTween.Modules`: DOTween's UI, audio and physics extensions, moved out of `Assembly-CSharp-firstpass` so an asmdef
  can reference them.
- Tests: `Fives.Domain.Tests`, `Fives.Runtime.Tests` (ECS systems, services and presenters on fakes),
  `Fives.UI.Editor.Tests` (prefabs, translations, layout) and `Fives.Tests.Support`. See [tests](TESTS.md).

Test seams in Runtime:
- `IFrameTime` for frame timing and `IClock` for energy;
- the view contracts;
- `IThemeDownloads`, `ISpriteLoader` and `ILoadingScreen` for content;
- a key prefix in `StorageService`, so tests never touch the player's save.

## Screen sizes

The game is landscape and designed at 1920x1080. The canvas scaler uses `Expand`, so the canvas is never smaller
than the design: wide phones (19.5:9, 21:9) get extra width, 4:3 tablets extra height, and nothing is clipped. Screen
content is anchored to the centre, so it stays one group with the background visible around it.

The gameplay screen is the exception: the board is centred and its side panels are pinned to the safe-area edges. On
wide phones they move apart instead of leaving empty meadow between them.

Backgrounds cover the screen with an `AspectRatioFitter` in `EnvelopeParent` mode. The header sits in
`SafeAreaFitter`, which keeps its buttons away from notches and the Dynamic Island. `AdaptiveLayoutTests` check the
scaler, the backgrounds and the safe area.

## Not implemented yet

- Daily challenge, best results and share codes.
- Input System actions.
- Unity tests in CI (they need a Unity license) and Android device profiling.

## Decisions

- [ADR 0001: Keep GameStartup as the single ECS composition point](adr/0001-keep-gamestartup-composition.md)
- [ADR 0002: Keep run state in GameSession](adr/0002-keep-run-state-in-gamesession.md) (superseded by ADR 0003)
- [ADR 0003: The run lives in the ECS world](adr/0003-the-run-lives-in-the-ecs-world.md)
