using System;
using System.Collections.Generic;
using System.Linq;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
using Scripts.Configs;
using Scripts.Helpers;
using Scripts.Models;
using Scripts.Services;
using Scripts.Systems;
using Scripts.UI.Presenters;
using UnityEngine;

namespace Fives.Runtime.Tests
{
    internal sealed class EnergyEventCounter : IEcsRunSystem
    {
        private readonly EcsFilter<CurrencyChangedEvent> _events = null;
        public int Count;

        public void Run()
        {
            foreach (var i in _events)
                if (_events.Get1(i).Currency == Currency.Energy)
                    Count++;
        }
    }

    internal sealed class SaveEveryFrame : IEcsRunSystem
    {
        private readonly EcsWorld _world = null;
        public void Run() => _world.Send<SaveDataEvent>();
    }

    public sealed class EconomyTests
    {
        private TestObjects _objects;
        private EcsWorld _world;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _world = new EcsWorld();
        }

        [TearDown]
        public void TearDown()
        {
            _world.Destroy();
            _objects.Dispose();
        }

        [Test]
        public void ThemePurchase_DebitsOnce()
        {
            var config = _objects.Config();
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            var stars = new StarService(save);
            var session = new GameSession(new GameBalance(config));
            var energy = new EnergyService(new GameBalance(config), save, new FakeClock { UtcNow = DateTime.UtcNow });
            var progress = new PlayerProgressService(save);
            var select = new SelectMenuPresenter(config, new GameStartService(session, energy, _world, new FakeSpriteLoader(_objects)),
                new ThemeShop(stars, progress, _world, new GameBalance(config)), progress, session, new FakeHeaderPanelView(), _world, new FakeTexts(),
                new FakeSpriteLoader(_objects), new FakeSpriteLoader(_objects).Previews(config), FakeThemeDownloads.Ready());
            select.Initialize(new FakeSelectMenuView());

            select.OnThemeBuy("cities");
            select.OnThemeBuy("cities");

            Assert.That(stars.GetBalance(), Is.EqualTo(140));
        }

        [Test]
        public void ThemeShop_WithoutEnoughStars_KeepsTheThemeLocked_AndSignalsTheHeader()
        {
            var config = _objects.Config(stars: 10);
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            var stars = new StarService(save);
            var progress = new PlayerProgressService(save);

            var result = new ThemeShop(stars, progress, _world, new GameBalance(config)).TryUnlock(config.Themes[0]);

            Assert.That(result, Is.EqualTo(PurchaseResult.NotEnoughStars));
            Assert.That(progress.IsUnlocked(config.Themes[0]), Is.False);
            Assert.That(stars.GetBalance(), Is.EqualTo(10));
            Assert.That(_world.Count<CurrencyChangedEvent>(), Is.EqualTo(1));
            Assert.That(_world.Count<SaveDataEvent>(), Is.Zero);
        }

        [Test]
        public void NegativeSpend_IsRejected()
        {
            var stars = new StarService(new PlayerDataSaveHelper(new MemoryStorage(), _objects.Config(), new GameBalance(_objects.Config())));

            Assert.That(stars.Spend(-10), Is.False);
            Assert.That(stars.GetBalance(), Is.EqualTo(200));
        }

        [Test]
        public void RewardClaim_IsIdempotent()
        {
            var config = _objects.Config();
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            var stars = new StarService(save);
            var theme = config.Themes[1];
            theme.Puzzles = _objects.Puzzles("dogs", "A");
            var session = new GameSession(new GameBalance(config));
            session.SetSelectedTheme(theme);
            session.SetSelectedImage(theme.Puzzles[0], null);
            session.BeginRun();
            var result = new GameResultPresenter(session, stars, new PlayerProgressService(save), _world, new FakeTexts(), new FakeRewardedAds());
            result.Initialize(new FakeGameResultView());

            result.GetReward();
            result.GetDoubleReward();

            Assert.That(stars.GetBalance(), Is.EqualTo(210));
        }

        [Test]
        public void ResultScreen_ShowsSolvedPuzzles_NotThePuzzlePosition()
        {
            var config = _objects.Config();
            var theme = config.Themes[1];
            theme.Puzzles = _objects.Puzzles("dogs", "A", "B", "C");
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            var session = new GameSession(new GameBalance(config));
            session.SetSelectedTheme(theme);
            session.SetSelectedImage(theme.Puzzles[2], null);
            session.BeginRun();
            var progress = new PlayerProgressService(save);
            progress.MarkCompleted(theme.Puzzles[2]); // PuzzleCompletionSystem, in the frame the board was solved
            var view = new FakeGameResultView();

            new GameResultPresenter(session, new StarService(save), progress, _world, new FakeTexts(), new FakeRewardedAds()).Initialize(view);

            Assert.That(view.Progress, Is.EqualTo("1/3"));
        }

        [Test]
        public void ResultScreen_OnlyShows_TheCoreRecordsCompletion()
        {
            var config = _objects.Config();
            var theme = config.Themes[1];
            theme.Puzzles = _objects.Puzzles("dogs", "A");
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            var session = new GameSession(new GameBalance(config));
            session.SetSelectedTheme(theme);
            session.SetSelectedImage(theme.Puzzles[0], null);
            session.BeginRun();
            var progress = new PlayerProgressService(save);

            new GameResultPresenter(session, new StarService(save), progress, _world, new FakeTexts(), new FakeRewardedAds())
                .Initialize(new FakeGameResultView());

            Assert.That(progress.IsCompleted(theme.Puzzles[0]), Is.False);
            Assert.That(_world.Count<SaveDataEvent>(), Is.Zero);
        }
    }

    public sealed class CurrencySyncTests
    {
        private TestObjects _objects;
        private EcsWorld _world;
        private StarService _stars;
        private EnergyService _energy;
        private EcsSystems _systems;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _world = new EcsWorld();
            var config = _objects.Config();
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            _stars = new StarService(save);
            _energy = new EnergyService(new GameBalance(config), save, new FakeClock { UtcNow = DateTime.UtcNow });
            _systems = new EcsSystems(_world).Add(new CurrencySyncSystem(_stars, _energy)); // events are kept to be counted
            _systems.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _systems.Destroy();
            _world.Destroy();
            _objects.Dispose();
        }

        private CurrencyChangedEvent[] Events => _world.All<CurrencyChangedEvent>();

        [Test]
        public void ChangesMadeOutsideTheWorld_ReachTheHeader_AndOneSave()
        {
            _stars.Add(10);   // a menu reward
            _energy.Spend(1); // a run start
            _systems.Run();

            Assert.That(Events.Select(e => (e.Currency, e.Balance, e.Delta)),
                Is.EqualTo(new[] { (Currency.Stars, 210, 10), (Currency.Energy, 4, -1) }));
            Assert.That(_world.Count<SaveDataEvent>(), Is.EqualTo(1));
        }

        [Test]
        public void SeveralChangesInAFrame_ArriveAsTheirTotal()
        {
            _stars.Spend(5);
            _stars.Add(10);
            _systems.Run();

            Assert.That(Events.Single().Delta, Is.EqualTo(5));
            Assert.That(Events.Single().Balance, Is.EqualTo(205));
        }

        [Test]
        public void UnchangedBalances_SendNothing()
        {
            _energy.Spend(99); // refused
            _systems.Run();
            _systems.Run();

            Assert.That(Events, Is.Empty);
            Assert.That(_world.Count<SaveDataEvent>(), Is.Zero);
        }
    }

    public sealed class EnergyRecoveryTests
    {
        private static readonly DateTime Now = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
        private TestObjects _objects;
        private EcsWorld _world;
        private EcsSystems _systems;

        [SetUp]
        public void SetUp()
        {
            _objects = new TestObjects();
            _world = new EcsWorld();
        }

        [TearDown]
        public void TearDown()
        {
            _systems?.Destroy();
            _world.Destroy();
            _objects.Dispose();
        }

        [Test]
        public void Recovery_EmitsOneUiUpdate_AndKeepsTheRemainder()
        {
            var clock = new FakeClock { UtcNow = Now };
            var energy = new EnergyService(new GameBalance(_objects.Config()), new PlayerDataSaveHelper(new MemoryStorage(), _objects.Config(), new GameBalance(_objects.Config())), clock);
            energy.SetDataFromSave(new EnergyData { CurrentEnergy = 0, LastRecoveryTime = Now.AddHours(-2.5) });
            var counter = new EnergyEventCounter();
            var stars = new StarService(new PlayerDataSaveHelper(new MemoryStorage(), _objects.Config(), new GameBalance(_objects.Config())));
            _systems = new EcsSystems(_world)
                .Add(new EnergyRecoverySystem(energy)).Add(new CurrencySyncSystem(stars, energy)).Add(counter)
                .Inject(new FakeFrameTime());
            _systems.Init();
            _systems.Run();

            Assert.That(energy.GetBalance(), Is.EqualTo(2));
            Assert.That(counter.Count, Is.EqualTo(1));
            Assert.That(clock.UtcNow - energy.GetLastRecoveryTime(), Is.EqualTo(TimeSpan.FromMinutes(30)));
        }

        [Test]
        public void OfflineRecovery_UpdatesTheHeaderAtStartup_AndSavesOnce()
        {
            var config = _objects.Config();
            var store = new MemoryStorage();
            var save = new PlayerDataSaveHelper(store, config, new GameBalance(config));
            save.GetPlayerData().Energy = new EnergyData { CurrentEnergy = 0, LastRecoveryTime = Now.AddHours(-2.5) };
            var energy = new EnergyService(new GameBalance(config), save, new FakeClock { UtcNow = Now });
            var header = new FakeHeaderPanelView();
            var stars = new StarService(save);
            _systems = new EcsSystems(_world)
                .Add(new EnergyRecoverySystem(energy))
                .Add(new CurrencySyncSystem(stars, energy))
                .Add(new CommonUIHeaderPanelSystem(header, energy, stars))
                .Add(new StorageSystem(energy)).OneFrame<SaveDataEvent>().OneFrame<CurrencyChangedEvent>()
                .Inject(save).Inject(new FakeFrameTime());
            _systems.Init();

            Assert.That(header.Energy, Is.EqualTo("2"), "the header shows the recovered balance from the first frame");

            _systems.Tick(2);
            Assert.That(store.Saves, Is.EqualTo(1));
            Assert.That(store.Data.Energy.CurrentEnergy, Is.EqualTo(2));
            Assert.That(store.Data.Energy.LastRecoveryTime, Is.EqualTo(Now.AddMinutes(-30)));
        }
    }

    public sealed class SoundServiceTests
    {
        private TestObjects _objects;

        [SetUp]
        public void SetUp() => _objects = new TestObjects();

        [TearDown]
        public void TearDown() => _objects.Dispose();

        [Test]
        public void EffectVolume_CombinesTheSavedSettingWithTheEffectVolume_AndMuteSilences()
        {
            var config = _objects.Config();
            config.AudioClipsCollection = new List<GameSoundCollection>();
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config, new GameBalance(config));
            save.GetPlayerData().SoundSettings.SoundEffectsVolume = 0.25f;
            var sound = new SoundService(config, save);

            Assert.That(sound.EffectVolume(0.8f), Is.EqualTo(0.2f).Within(1e-5f));

            sound.SetSoundEffectVolume(0);
            Assert.That(sound.EffectVolume(1f), Is.Zero);
        }
    }
}
