using System.Linq;
using Fives.Components;
using Fives.Domain;
using Fives.Systems;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Board
{
    /// <summary>Подсказка: цена в звёздах, когда она продаётся и когда гаснет.</summary>
    public sealed class HintTests
    {
        //  0  1  2  3      плитки 0 и 1 переставлены (в одной клетке от своего места),
        //  4  5  6  7      плитки 3 и 8 переставлены (в пяти клетках от своего места).
        //  8  9 10 11
        private static readonly int[] Layout = { 1, 0, 2, 8, 4, 5, 6, 7, 3, 9, 10, 11 };
        private const int Stars = 12;

        private BoardFixture _board;

        private void Build(int stars = Stars)
        {
            _board = new BoardFixture(stars).WithBoard(new BoardState(4, 3, Layout));
            var game = _board.Game;
            _board.Systems = new EcsSystems(_board.World)
                .Add(new BoardInputSystem()).Add(new BoardHintSystem(game.Stars)).Add(new BoardProjectionSystem()).Add(new TileMoveSystem())
                .Add(new CurrencySyncSystem(game.Stars, game.Energy))
                .OneFrame<TileSwipeEvent>().OneFrame<BoardControlEvent>().OneFrame<BoardChangedEvent>()
                .Inject(_board.Session).Inject(_board.Time).Inject(game.Balance);
            _board.Systems.Init();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        private int HintTile => _board.Board.Has<BoardHintComponent>() ? _board.Board.Get<BoardHintComponent>().TileId : -1;
        private int Balance => _board.Game.Stars.GetBalance();
        private CurrencyChangedEvent[] CurrencyEvents => _board.World.All<CurrencyChangedEvent>();

        /// <summary>
        /// Подсказка списывает 5 звёзд и показывает одну из ближайших к своему месту плиток;
        /// шапка и сохранение узнают о списании.
        /// </summary>
        [Test]
        public void Hint_CostsItsPrice_AndGuidesAClosestTile()
        {
            Build();
            _board.Control(BoardControl.Hint);

            Assert.That(HintTile, Is.EqualTo(0).Or.EqualTo(1));
            Assert.That(Balance, Is.EqualTo(Stars - TestConfig.HintPrice));
            Assert.That(CurrencyEvents.Single().Delta, Is.EqualTo(-TestConfig.HintPrice));
            Assert.That(_board.World.Count<SaveDataEvent>(), Is.EqualTo(1));
        }

        /// <summary>Пока подсказка горит, повторное нажатие бесплатное.</summary>
        [Test]
        public void SecondPress_WhileAHintIsActive_IsFree()
        {
            Build();
            _board.Control(BoardControl.Hint);
            _board.Control(BoardControl.Hint);

            Assert.That(Balance, Is.EqualTo(Stars - TestConfig.HintPrice));
            Assert.That(CurrencyEvents.Length, Is.EqualTo(1));
        }

        /// <summary>
        /// Если звёзд меньше цены, подсказки нет, звёзды не списываются, а шапка показывает нехватку.
        /// </summary>
        [Test]
        public void NotEnoughStars_GivesNoHint_AndReportsIt()
        {
            Build(stars: TestConfig.HintPrice - 2);
            _board.Control(BoardControl.Hint);

            Assert.That(HintTile, Is.EqualTo(-1));
            Assert.That(Balance, Is.EqualTo(TestConfig.HintPrice - 2));
            Assert.That(CurrencyEvents.Single().Insufficient, Is.True);
        }

        /// <summary>Пока плитки едут, подсказка не продаётся.</summary>
        [Test]
        public void Hint_IsNotSold_WhileTilesMove()
        {
            Build();
            _board.Swipe(_board.Layout[4], 0, 1);
            _board.Control(BoardControl.Hint);

            Assert.That(_board.MovingTiles, Is.GreaterThan(0));
            Assert.That(HintTile, Is.EqualTo(-1));
            Assert.That(Balance, Is.EqualTo(Stars));
        }

        /// <summary>Подсказка гаснет после любого хода, даже другой плиткой.</summary>
        [Test]
        public void Hint_EndsWithTheNextMove_EvenOfAnotherTile()
        {
            Build();
            _board.Control(BoardControl.Hint);
            Assert.That(HintTile, Is.Not.EqualTo(-1));

            _board.Swipe(_board.Layout[9], 1, 0); // плитки 9 и 10, далеко от подсказанной пары
            Assert.That(HintTile, Is.EqualTo(-1));
        }

        /// <summary>Undo гасит подсказку; звёзды за неё не возвращаются.</summary>
        [Test]
        public void Hint_EndsWithUndo()
        {
            Build();
            _board.Swipe(_board.Layout[9], 1, 0);
            _board.Settle();
            _board.Control(BoardControl.Hint);
            _board.Control(BoardControl.Undo);

            Assert.That(HintTile, Is.EqualTo(-1));
            Assert.That(Balance, Is.EqualTo(Stars - TestConfig.HintPrice));
        }
    }
}
