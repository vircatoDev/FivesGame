using System.Linq;
using Fives.Domain;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;
using Scripts.Helpers;
using Scripts.Services;
using Scripts.Systems;

namespace Fives.Runtime.Tests
{
    public sealed class HintTests
    {
        //  4 x 3:  0  1  2  3      tiles 0 and 1 are swapped (one cell from home),
        //          4  5  6  7      tiles 3 and 8 are swapped (five cells from home).
        //          8  9 10 11
        private static readonly int[] Layout = { 1, 0, 2, 8, 4, 5, 6, 7, 3, 9, 10, 11 };

        private BoardFixture _board;
        private StarService _stars;

        private void Build(int stars)
        {
            _board = new BoardFixture(4, 3).WithBoard(new BoardState(4, 3, Layout));
            var config = _board.Objects.Config(stars);
            _stars = new StarService(new PlayerDataSaveHelper(new MemoryStorage(), config));
            _board.Systems = new EcsSystems(_board.World)
                .Add(new BoardInputSystem()).Add(new BoardHintSystem(_stars)).Add(new BoardProjectionSystem()).Add(new TileMoveSystem())
                .OneFrame<TileSwipeEvent>().OneFrame<BoardControlEvent>().OneFrame<BoardChangedEvent>()
                .Inject(_board.Session).Inject(_board.Time).Inject(config);
            _board.Systems.Init();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        private int HintTile => _board.Board.Has<BoardHintComponent>() ? _board.Board.Get<BoardHintComponent>().TileId : -1;
        private CurrencyChangedEvent[] CurrencyEvents => _board.World.All<CurrencyChangedEvent>();

        [Test]
        public void Hint_CostsItsPrice_AndGuidesAClosestTile()
        {
            Build(stars: 12);
            _board.Control(BoardControl.Hint);

            Assert.That(HintTile, Is.EqualTo(0).Or.EqualTo(1));
            Assert.That(_stars.GetBalance(), Is.EqualTo(7));
            Assert.That(CurrencyEvents.Single().Delta, Is.EqualTo(-5));
            Assert.That(_board.World.Count<SaveDataEvent>(), Is.EqualTo(1));
        }

        [Test]
        public void SecondPress_WhileAHintIsActive_IsFree()
        {
            Build(stars: 12);
            _board.Control(BoardControl.Hint);
            _board.Control(BoardControl.Hint);

            Assert.That(_stars.GetBalance(), Is.EqualTo(7));
            Assert.That(CurrencyEvents.Length, Is.EqualTo(1));
        }

        [Test]
        public void NotEnoughStars_GivesNoHint_AndReportsIt()
        {
            Build(stars: 3);
            _board.Control(BoardControl.Hint);

            Assert.That(HintTile, Is.EqualTo(-1));
            Assert.That(_stars.GetBalance(), Is.EqualTo(3));
            Assert.That(CurrencyEvents.Single().Insufficient, Is.True);
        }

        [Test]
        public void Hint_IsNotSold_WhileTilesMove()
        {
            Build(stars: 12);
            _board.Swipe(_board.Layout[4], 0, 1);
            _board.Control(BoardControl.Hint);

            Assert.That(_board.MovingTiles, Is.GreaterThan(0));
            Assert.That(HintTile, Is.EqualTo(-1));
            Assert.That(_stars.GetBalance(), Is.EqualTo(12));
        }

        [Test]
        public void Hint_EndsWithTheNextMove_EvenOfAnotherTile()
        {
            Build(stars: 12);
            _board.Control(BoardControl.Hint);
            Assert.That(HintTile, Is.Not.EqualTo(-1));

            _board.Swipe(_board.Layout[9], 1, 0); // tiles 9 and 10, away from the hinted pair
            Assert.That(HintTile, Is.EqualTo(-1));
        }

        [Test]
        public void Hint_EndsWithUndo()
        {
            Build(stars: 12);
            _board.Swipe(_board.Layout[9], 1, 0);
            _board.Settle();
            _board.Control(BoardControl.Hint);
            _board.Control(BoardControl.Undo);

            Assert.That(HintTile, Is.EqualTo(-1));
            Assert.That(_stars.GetBalance(), Is.EqualTo(7));
        }
    }
}
