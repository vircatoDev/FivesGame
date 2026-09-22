using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fives.Domain;
using Leopotam.Ecs;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.Services.Interfaces;
using Scripts.Systems;
using Scripts.UI.Presenters;
using Scripts.UI.Views;
using UnityEngine;

internal sealed class MemoryStorage : IStorageService
{
    public int Saves;
    public GameSaveData Data;
    public void Save<T>(string key, T data) { Data = (GameSaveData)(object)data; Saves++; }
    public T Load<T>(string key, T fallback = default) => Data == null ? fallback : (T)(object)Data;
}

internal sealed class FakeClock : IClock
{
    public DateTime UtcNow { get; set; }
}

internal sealed class EnergyObserver : IEcsRunSystem
{
    private readonly EcsFilter<UpdateControlPanelEnergyEvent> _events = null;
    public int Count;
    public void Run() => Count += _events.GetEntitiesCount();
}

internal sealed class EmitSave : IEcsRunSystem
{
    private readonly EcsWorld _world = null;
    public IStorable Target;
    public void Run() => _world.NewEntity().Replace(new SaveDataEvent { StorableObject = Target });
}

internal static partial class CorrectnessProbes
{
    private static int _passed;

    private static GlobalConfig CreateConfig(int stars = 200) => new GlobalConfig
    {
        InitialStars = stars,
        InitialEnergy = 5,
        MaxEnergy = 10,
        EnergyRecoveryIntervalHours = 1,
        DefaultUnlockedThemes = new[] { "Dogs" },
        Themes = new List<ThemeConfig>
        {
            new ThemeConfig { ThemeName = "Cities", UnlockCost = 60, Puzzles = Array.Empty<PuzzleData>() },
            new ThemeConfig { ThemeName = "Dogs", Puzzles = Array.Empty<PuzzleData>() }
        }
    };

    private static void Check(string name, bool condition, string detail)
    {
        if (!condition) throw new Exception($"FAILED | {name} | {detail}");
        _passed++;
        Console.WriteLine($"PASS | {name} | {detail}");
    }

    private static void Main()
    {
        var clock = new FakeClock { UtcNow = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc) };
        var store = new MemoryStorage();
        var config = CreateConfig();
        var save = new PlayerDataSaveHelper(store, config);
        var stars = new StarService(save);
        var energy = new EnergyService(config, save, clock);
        var progress = new PlayerProgressService(save);
        var world = new EcsWorld();
        var commands = new ECSCommandService(world);

        var select = new SelectMenuPresenter(config, new GameStartService(new GameSession(), energy, commands), stars, progress, commands);
        typeof(SelectMenuPresenter).GetField("_view", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(select, new SelectMenuView());
        select.OnThemeBuy("Cities");
        Check("theme purchase debits once", stars.GetBalance() == 140, $"balance={stars.GetBalance()}");

        var beforeNegativeSpend = stars.GetBalance();
        Check("negative spend rejected", !stars.Spend(-10) && stars.GetBalance() == beforeNegativeSpend, $"balance={stars.GetBalance()}");

        var session = new GameSession();
        session.BeginRun();
        var resultPresenter = new GameResultPresenter(session, stars, progress, commands);
        var beforeReward = stars.GetBalance();
        resultPresenter.GetReward(false);
        resultPresenter.GetReward(true);
        Check("reward claim idempotent", stars.GetBalance() - beforeReward == 10, $"awarded={stars.GetBalance() - beforeReward}");

        energy.SetDataFromSave(new EnergyData { CurrentEnergy = 0, LastRecoveryTime = clock.UtcNow.AddHours(-2.5) });
        var observer = new EnergyObserver();
        var recoverySystems = new EcsSystems(world).Add(new EnergyRecoverySystem(energy)).Add(observer);
        recoverySystems.Init();
        recoverySystems.Run();
        Check("recovery emits UI update", energy.GetBalance() == 2 && observer.Count == 1, $"balance={energy.GetBalance()}, events={observer.Count}");
        Check("recovery keeps remainder", clock.UtcNow - energy.GetLastRecoveryTime() == TimeSpan.FromMinutes(30), $"remainder={clock.UtcNow - energy.GetLastRecoveryTime()}");
        recoverySystems.Destroy();
        world.Destroy();

        var saveStore = new MemoryStorage();
        var saveHelper = new PlayerDataSaveHelper(saveStore, CreateConfig());
        var saveWorld = new EcsWorld();
        var saveStars = new StarService(saveHelper);
        var saveSystems = new EcsSystems(saveWorld)
            .Add(new StorageSystem())
            .Add(new EmitSave { Target = saveStars })
            .Inject(saveHelper);
        saveSystems.Init();
        saveSystems.Run();
        saveSystems.Run();
        Check("late save survives until next tick", saveStore.Saves == 1, $"writes={saveStore.Saves}");
        saveSystems.Destroy();
        saveWorld.Destroy();

        CheckRapidClicks();
        var generated = SeededShuffle.Create(6, 35, 123, 144);
        Check("configured board size is honored", generated.CellCount == 36
            && Enumerable.Range(0, 36).Select(i => generated[i]).Distinct().Count() == 36,
            $"cells={generated.CellCount}, empty={generated.EmptyTileId}");
        RunBoardProbes();

        RunLifecycleProbes();
        Console.WriteLine($"SUMMARY | {_passed} correctness probes passed");
    }
}
