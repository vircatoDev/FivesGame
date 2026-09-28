using System.Linq;
using Fives.Components;
using Fives.Models;
using Fives.Systems;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Economy
{
    /// <summary>Изменения звёзд и энергии доходят до шапки и сохранения раз в кадр, кто бы их ни сделал.</summary>
    public sealed class CurrencySyncTests
    {
        private GameStand _game;
        private EcsSystems _systems;

        [SetUp]
        public void SetUp()
        {
            _game = new GameStand();
            // События не удаляются, чтобы их можно было посчитать.
            _systems = new EcsSystems(_game.World).Add(new CurrencySyncSystem(_game.Stars, _game.Energy));
            _systems.Init();
        }

        [TearDown]
        public void TearDown()
        {
            _systems.Destroy();
            _game.Dispose();
        }

        private CurrencyChangedEvent[] Events => _game.World.All<CurrencyChangedEvent>();

        /// <summary>
        /// Награда в меню и списание энергии вне ECS:
        /// шапка получает оба изменения, сохранение одно.
        /// </summary>
        [Test]
        public void ChangesMadeOutsideTheWorld_ReachTheHeader_AndOneSave()
        {
            _game.Stars.Add(10);   // награда в меню
            _game.Energy.Spend(1); // старт партии
            _systems.Run();

            Assert.That(Events.Select(e => (e.Currency, e.Balance, e.Delta)), Is.EqualTo(new[]
            {
                (Currency.Stars, TestConfig.Stars + 10, 10),
                (Currency.Energy, TestConfig.Energy - 1, -1)
            }));
            Assert.That(_game.World.Count<SaveDataEvent>(), Is.EqualTo(1));
        }

        /// <summary>Несколько изменений за кадр приходят одним событием с итоговой разницей.</summary>
        [Test]
        public void SeveralChangesInAFrame_ArriveAsTheirTotal()
        {
            _game.Stars.Spend(5);
            _game.Stars.Add(10);
            _systems.Run();

            Assert.That(Events.Single().Delta, Is.EqualTo(5));
            Assert.That(Events.Single().Balance, Is.EqualTo(TestConfig.Stars + 5));
        }

        /// <summary>Отклонённая трата ничего не меняет: ни событий, ни сохранения.</summary>
        [Test]
        public void UnchangedBalances_SendNothing()
        {
            _game.Energy.Spend(99); // отклонено: энергии меньше
            _systems.Run();
            _systems.Run();

            Assert.That(Events, Is.Empty);
            Assert.That(_game.World.Count<SaveDataEvent>(), Is.Zero);
        }
    }
}
