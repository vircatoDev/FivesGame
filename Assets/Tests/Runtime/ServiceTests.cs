using System;
using System.Collections.Generic;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
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
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
            var stars = new StarService(save);
            var session = new GameSession(config);
            var energy = new EnergyService(config, save, new FakeClock { UtcNow = DateTime.UtcNow });
            var progress = new PlayerProgressService(save);
            var select = new SelectMenuPresenter(config, new GameStartService(session, energy, _world),
                new ThemeShop(stars, progress, _world), progress, session, new FakeHeaderPanelView(), _world, new FakeTexts());
            select.Initialize(new FakeSelectMenuView());

            select.OnThemeBuy("cities");
            select.OnThemeBuy("cities");

            Assert.That(stars.GetBalance(), Is.EqualTo(140));
        }

        [Test]
        public void ThemeShop_WithoutEnoughStars_KeepsTheThemeLocked_AndSignalsTheHeader()
        {
            var config = _objects.Config(stars: 10);
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
            var stars = new StarService(save);
            var progress = new PlayerProgressService(save);

            var result = new ThemeShop(stars, progress, _world).TryUnlock(config.Themes[0]);

            Assert.That(result, Is.EqualTo(PurchaseResult.NotEnoughStars));
            Assert.That(progress.IsUnlocked(config.Themes[0]), Is.False);
            Assert.That(stars.GetBalance(), Is.EqualTo(10));
            Assert.That(_world.Count<CurrencyChangedEvent>(), Is.EqualTo(1));
            Assert.That(_world.Count<SaveDataEvent>(), Is.Zero);
        }

        [Test]
        public void NegativeSpend_IsRejected()
        {
            var stars = new StarService(new PlayerDataSaveHelper(new MemoryStorage(), _objects.Config()));

            Assert.That(stars.Spend(-10), Is.False);
            Assert.That(stars.GetBalance(), Is.EqualTo(200));
        }

        [Test]
        public void RewardClaim_IsIdempotent()
        {
            var config = _objects.Config();
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
            var stars = new StarService(save);
            var session = new GameSession(config);
            session.BeginRun();
            var result = new GameResultPresenter(session, stars, new PlayerProgressService(save), _world, new FakeTexts());

            result.GetReward(false);
            result.GetReward(true);

            Assert.That(stars.GetBalance(), Is.EqualTo(210));
        }

        [Test]
        public void ResultScreen_ShowsSolvedPuzzles_NotThePuzzlePosition()
        {
            var config = _objects.Config();
            var theme = config.Themes[1];
            theme.Puzzles = _objects.Puzzles("dogs", "A", "B", "C");
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
            var session = new GameSession(config);
            session.SetSelectedTheme(theme);
            session.SetSelectedImage(theme.Puzzles[2]);
            session.BeginRun();
            var view = new FakeGameResultView();

            new GameResultPresenter(session, new StarService(save), new PlayerProgressService(save), _world, new FakeTexts()).Initialize(view);

            Assert.That(view.Progress, Is.EqualTo("1/3"));
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
            var energy = new EnergyService(_objects.Config(), new PlayerDataSaveHelper(new MemoryStorage(), _objects.Config()), clock);
            energy.SetDataFromSave(new EnergyData { CurrentEnergy = 0, LastRecoveryTime = Now.AddHours(-2.5) });
            var counter = new EnergyEventCounter();
            _systems = new EcsSystems(_world).Add(new EnergyRecoverySystem(energy)).Add(counter).Inject(new FakeFrameTime());
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
            var save = new PlayerDataSaveHelper(store, config);
            save.GetPlayerData().Energy = new EnergyData { CurrentEnergy = 0, LastRecoveryTime = Now.AddHours(-2.5) };
            var energy = new EnergyService(config, save, new FakeClock { UtcNow = Now });
            var header = new FakeHeaderPanelView();
            _systems = new EcsSystems(_world)
                .Add(new EnergyRecoverySystem(energy))
                .Add(new CommonUIHeaderPanelSystem(header, energy, new StarService(save)))
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
            var save = new PlayerDataSaveHelper(new MemoryStorage(), config);
            save.GetPlayerData().SoundSettings.SoundEffectsVolume = 0.25f;
            var sound = new SoundService(config, save);

            Assert.That(sound.EffectVolume(0.8f), Is.EqualTo(0.2f).Within(1e-5f));

            sound.SetSoundEffectVolume(0);
            Assert.That(sound.EffectVolume(1f), Is.Zero);
        }
    }
}
