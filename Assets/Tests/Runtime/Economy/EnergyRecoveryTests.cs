using System;
using Fives.Components;
using Fives.Services;
using Fives.Systems;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Economy
{
    /// <summary>Энергия, восстановленная, пока игра была закрыта.</summary>
    public sealed class EnergyRecoveryTests
    {
        private static readonly DateTime Now = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
        private MemoryStorage _storage;
        private GameStand _game;
        private EcsSystems _systems;

        [SetUp]
        public void SetUp()
        {
            _storage = new MemoryStorage();
            _game = new GameStand(storage: _storage);
        }

        [TearDown]
        public void TearDown()
        {
            _systems?.Destroy();
            _game.Dispose();
        }

        /// <summary>Энергия, восстановленная офлайн, видна в шапке с первого кадра и сохраняется один раз.</summary>
        [Test]
        public void OfflineRecovery_UpdatesTheHeaderAtStartup_AndSavesOnce()
        {
            // Игру закрыли без энергии 2,5 часа назад, единица возвращается за час:
            // вернулись две, полчаса идут в счёт третьей.
            _game.Clock.UtcNow = Now;
            _game.Save.GetPlayerData().Energy = new EnergyData { CurrentEnergy = 0, LastRecoveryTime = Now.AddHours(-2.5) };
            _systems = new EcsSystems(_game.World)
                .Add(new EnergyRecoverySystem(_game.Energy))
                .Add(new CurrencySyncSystem(_game.Stars, _game.Energy))
                .Add(new CommonUIHeaderPanelSystem(_game.Header, _game.Energy, _game.Stars))
                .Add(new StorageSystem(_game.Energy)).OneFrame<SaveDataEvent>().OneFrame<CurrencyChangedEvent>()
                .Inject(_game.Save).Inject(new FakeFrameTime());
            _systems.Init();

            Assert.That(_game.Header.Energy, Is.EqualTo("2"), "the header shows the recovered balance from the first frame");

            _systems.Tick(2);
            Assert.That(_storage.Saves, Is.EqualTo(1));
            Assert.That(_storage.Data.Energy.CurrentEnergy, Is.EqualTo(2));
            Assert.That(_storage.Data.Energy.LastRecoveryTime, Is.EqualTo(Now.AddMinutes(-30)));
        }
    }
}
