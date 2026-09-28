using System.Linq;
using Fives.Components;
using Fives.Domain;
using Fives.Systems;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Board
{
    /// <summary>Как плитки следуют за полем: на новом поле встают сразу, после хода едут только сдвинутые.</summary>
    public sealed class BoardProjectionTests
    {
        private BoardFixture _board;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardFixture().WithSeededBoard(42);
            _board.Systems = new EcsSystems(_board.World).Add(new BoardProjectionSystem())
                .OneFrame<BoardChangedEvent>().OneFrame<BoardInitializedEvent>().Inject(_board.Session);
            _board.Systems.Init();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        /// <summary>На новом поле каждая плитка сразу встаёт в свою клетку, без анимации.</summary>
        [Test]
        public void InitialProjection_PlacesEveryTileInstantly()
        {
            _board.World.NewEntity().Get<BoardInitializedEvent>();
            _board.Systems.Run();

            for (var cell = 0; cell < _board.Layout.CellCount; cell++)
            {
                var tile = _board.Tiles[_board.Layout[cell]];
                Assert.That(tile.Get<TileComponent>().Cell, Is.EqualTo(cell));
                Assert.That(tile.Get<MoveComponent>().InstaMove, Is.True);
            }
        }

        /// <summary>После обмена анимируются только две сдвинутые плитки, остальные стоят.</summary>
        [Test]
        public void BoardChange_AnimatesOnlyTheMovedTiles()
        {
            _board.Layout.TrySwap(new Swap(0, 1));
            _board.World.NewEntity().Get<BoardChangedEvent>();
            _board.Systems.Run();

            Assert.That(_board.Tiles[_board.Layout[0]].Get<MoveComponent>().InstaMove, Is.False);
            Assert.That(_board.Tiles[_board.Layout[1]].Has<MoveComponent>(), Is.True);
            var untouched = Enumerable.Range(2, _board.Layout.CellCount - 2);
            Assert.That(untouched.All(cell => !_board.Tiles[_board.Layout[cell]].Has<MoveComponent>()), Is.True);
        }
    }
}
