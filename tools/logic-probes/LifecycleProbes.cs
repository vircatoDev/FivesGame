using System;
using System.Collections.Generic;
using System.Reflection;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.Systems;
using Scripts.UI;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
using UnityEngine;

internal static partial class CorrectnessProbes
{
    private static void RunLifecycleProbes()
    {
        CheckRepeatedStart();
        CheckMovementAndWin();
        CheckOfflineRecovery();
        CheckSaveRecovery();
        CheckSoundVolume();
    }

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

    // Binds a stub view without running OnActivateView.
    private static void SetView(BasePresenter presenter, BaseView view) =>
        presenter.GetType().BaseType.GetProperty("View", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(presenter, view);

    private static void CheckRepeatedStart()
    {
        var config = CreateConfig();
        config.DefaultUnlockedThemes = new[] { "Cities" };
        var puzzle = new PuzzleData { Name = "One" };
        config.Themes[0].Puzzles = new[] { puzzle };
        var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
        var energy = new EnergyService(config, save, new FakeClock { UtcNow = DateTime.UtcNow });
        var session = new GameSession(CreateConfig());
        var world = new EcsWorld();
        var start = new GameStartService(session, energy, world);
        var progress = new PlayerProgressService(save);
        var view = new SelectMenuView();
        var select = new SelectMenuPresenter(config, start, new StarService(save), progress, world);
        SetView(select, view);
        SetField(select, "_selectedTheme", "Cities");
        var main = new MainMenuPresenter(config, progress, start, world);
        SetView(main, new MainMenuView());

        select.OnStartGame("One");
        select.OnStartGame("One");
        main.OnStartGame();
        Check("start is accepted once across both menus", energy.GetBalance() == 4 && session.IsRunning,
            $"energy={energy.GetBalance()}");
        Check("start waits for hide animation", world.GetFilter(typeof(EcsFilter<ChangeStateEvent>)).GetEntitiesCount() == 0,
            "no navigation before animation completes");
        view.HideCompletion.SetResult(true);
        Check("start navigates once", world.GetFilter(typeof(EcsFilter<ChangeStateEvent>)).GetEntitiesCount() == 1,
            "one request after animation");
        session.EndRun();
        Check("next run can start", start.TryStart(config.Themes[0], puzzle) && energy.GetBalance() == 3,
            $"energy={energy.GetBalance()}");
        session.EndRun();
        energy.Spend(energy.GetBalance());
        Check("no energy leaves session idle", !start.TryStart(config.Themes[0], puzzle) && !session.IsRunning,
            "no run created");
        world.Destroy();
    }

    private static void CheckMovementAndWin()
    {
        Time.deltaTime = 0.1f;
        Time.unscaledDeltaTime = 0.1f;
        var world = new EcsWorld();
        var state = world.NewEntity();
        state.Replace(new GameStateComponent { CurrentState = GameStateType.Playing });
        var session = new GameSession(CreateConfig());
        session.SetGameMode(new GameSettings { BoardSize = 3, TileSize = 1 });
        session.BeginRun();
        // One swap from solved: tile 7 sits in cell 8.
        var boardEntity = CreateBoard(world, new BoardState(3, new[] { 0, 1, 2, 3, 4, 5, 6, 8, 7 }));
        var board = boardEntity.Get<BoardComponent>().State;
        var solvingTile = 7;
        var destination = solvingTile;
        var tiles = CreateTiles(world, board);
        var systems = CreateBoardSystems(world, session);
        systems.Init();
        world.NewEntity().Replace(new TileSwipeEvent { Id = solvingTile, Dx = -1 });
        systems.Run();
        Check("movement retains input lock across frames", tiles[solvingTile].Has<MoveComponent>() && !session.IsCompleted,
            "last move is still animating");
        world.NewEntity().Replace(new TileSwipeEvent { Id = solvingTile, Dx = -1 });
        systems.Run();
        Check("reverse swipe during movement is ignored", tiles[solvingTile].Get<TileComponent>().Cell == destination,
            "logical tile stays in destination");
        Time.realtimeSinceStartup = 83f;
        for (int i = 0; i < 3; i++) systems.Run();
        Check("win starts after final movement completes", !tiles[solvingTile].Has<MoveComponent>() && session.IsCompleted,
            "animation completed and game locked");
        Check("win records moves and time for the result screen", session.LastGameResult.TurnCount == 1
            && session.LastGameResult.GameTime == TimeSpan.FromSeconds(83),
            $"moves={session.LastGameResult.TurnCount}, time={session.LastGameResult.GameTime}");
        Time.realtimeSinceStartup = 0f;
        world.NewEntity().Replace(new TileSwipeEvent { Id = solvingTile, Dx = -1 });
        systems.Run();
        Check("solved board rejects further moves", tiles[solvingTile].Get<TileComponent>().Cell == destination,
            "board remains solved during result delay");
        Check("result delay keeps solved board visible", world.GetFilter(typeof(EcsFilter<GameEndEvent>)).GetEntitiesCount() == 0,
            "no early cleanup");
        for (int i = 0; i < 30; i++) systems.Run();
        Check("win emits result and cleanup once", world.GetFilter(typeof(EcsFilter<GameEndEvent>)).GetEntitiesCount() == 1
            && world.GetFilter(typeof(EcsFilter<ChangeStateEvent>)).GetEntitiesCount() == 1,
            "one event of each type even before state consumer runs");
        state.Get<GameStateComponent>().CurrentState = GameStateType.MainMenu;
        systems.Run();
        session.EndRun(); session.BeginRun();
        Check("next run resets completion", !session.IsCompleted && session.IsRunning, "fresh session state");
        foreach (var i in (EcsFilter<GameEndEvent>)world.GetFilter(typeof(EcsFilter<GameEndEvent>)))
            ((EcsFilter<GameEndEvent>)world.GetFilter(typeof(EcsFilter<GameEndEvent>))).GetEntity(i).Destroy();
        foreach (var i in (EcsFilter<ChangeStateEvent>)world.GetFilter(typeof(EcsFilter<ChangeStateEvent>)))
            ((EcsFilter<ChangeStateEvent>)world.GetFilter(typeof(EcsFilter<ChangeStateEvent>))).GetEntity(i).Destroy();
        state.Get<GameStateComponent>().CurrentState = GameStateType.Playing;
        for (int i = 0; i < 18; i++) systems.Run();
        world.NewEntity().Get<GameEndEvent>();
        for (int i = 0; i < 5; i++) systems.Run();
        Check("manual exit wins over result delay", world.GetFilter(typeof(EcsFilter<ChangeStateEvent>)).GetEntitiesCount() == 0,
            "no stale Finished request after exit");
        systems.Destroy(); world.Destroy();
        Time.deltaTime = 1f;
    }

    private static void CheckOfflineRecovery()
    {
        var config = CreateConfig();
        var store = new MemoryStorage();
        var save = new PlayerDataSaveHelper(store, config);
        var now = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
        save.GetPlayerData().Energy = new EnergyData { CurrentEnergy = 0, LastRecoveryTime = now.AddHours(-2.5) };
        var energy = new EnergyService(config, save, new FakeClock { UtcNow = now });
        var header = new HeaderPanelView();
        var world = new EcsWorld();
        var systems = new EcsSystems(world)
            .Add(new EnergyRecoverySystem(energy))
            .Add(new CommonUIHeaderPanelSystem(header, energy, new StarService(save)))
            .Add(new StorageSystem()).OneFrame<SaveDataEvent>().OneFrame<CurrencyChangedEvent>().Inject(save);
        systems.Init();
        Check("header uses recovered balance during initialization", header.EnergyText == "2" && energy.GetBalance() == 2,
            $"header={header.EnergyText}");
        systems.Run(); systems.Run();
        Check("offline recovery saves once and preserves remainder", store.Saves == 1 && store.Data.Energy.CurrentEnergy == 2
            && store.Data.Energy.LastRecoveryTime == now.AddMinutes(-30), $"writes={store.Saves}");
        systems.Destroy(); world.Destroy();
    }

    private static void CheckSaveRecovery()
    {
        PlayerPrefs.DeleteAll();
        var storage = new StorageService();
        foreach (var invalid in new[] { "{", "null", "[]", "{\"Stars\":\"invalid\"}" })
        {
            PlayerPrefs.SetString("GameSaveData", invalid);
            var save = new PlayerDataSaveHelper(storage, CreateConfig());
            Check("invalid save falls back and preserves raw data", save.GetPlayerData().Stars == 200
                && PlayerPrefs.GetString("GameSaveData.corrupt") == invalid, invalid);
        }
        PlayerPrefs.SetString("GameSaveData", "{\"Stars\":41,\"PlayerProgress\":{\"CompletedPuzzles\":[\"One\"]}}");
        var partial = new PlayerDataSaveHelper(storage, CreateConfig()).GetPlayerData();
        Check("partial save keeps progress and restores missing data", partial.Stars == 41
            && partial.PlayerProgress.CompletedPuzzles.Contains("One") && partial.PlayerProgress.UnlockedThemes.Contains("Dogs")
            && partial.Energy.CurrentEnergy == 5 && partial.SoundSettings != null, "existing data retained");
        storage.Save("GameSaveData", partial);
        var restored = storage.Load<GameSaveData>("GameSaveData");
        Check("repaired save roundtrips", restored.Stars == 41 && restored.PlayerProgress.CompletedPuzzles.Contains("One"),
            "JSON roundtrip with real Newtonsoft.Json");
        Check("successful save retains corrupt backup", PlayerPrefs.HasKey("GameSaveData.corrupt"), "backup still available");
        PlayerPrefs.SetString("GameSaveData", "{\"Energy\":{},\"PlayerProgress\":{\"UnlockedThemes\":[\"RemovedTheme\",null]}}");
        var config = CreateConfig();
        var recoveredSave = new PlayerDataSaveHelper(storage, config);
        var world = new EcsWorld();
        var energy = new EnergyService(config, recoveredSave, new FakeClock { UtcNow = DateTime.UtcNow });
        var menu = new MainMenuPresenter(config, new PlayerProgressService(recoveredSave),
            new GameStartService(new GameSession(CreateConfig()), energy, world), world);
        menu.Initialize(new MainMenuView());
        Check("partial save and removed theme still allow menu startup",
            recoveredSave.GetPlayerData().Energy.LastRecoveryTime != default
            && recoveredSave.GetPlayerData().PlayerProgress.CompletedPuzzles != null,
            "known default selected; unknown theme ID retained for future content");
        world.Destroy();
    }

    private static void CheckSoundVolume()
    {
        var config = CreateConfig();
        config.AudioClipsCollection = new List<GameSoundCollection> { new GameSoundCollection { Key = "tap", AudioClip = new AudioClip() } };
        var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
        save.GetPlayerData().SoundSettings.SoundEffectsVolume = 0.25f;
        var sound = new SoundService(config, save);
        sound.PlaySoundEffect("tap", 0.8f);
        Check("SFX combines saved and per-effect volume", Mathf.Approximately(AudioSource.LastVolume, 0.2f),
            $"volume={AudioSource.LastVolume}");
        sound.SetSoundEffectVolume(0);
        sound.PlaySoundEffect("tap");
        Check("SFX mute reaches playback", AudioSource.LastVolume == 0, "volume=0");
    }
}
