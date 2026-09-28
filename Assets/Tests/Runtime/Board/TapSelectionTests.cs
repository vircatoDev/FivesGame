using NUnit.Framework;

namespace Fives.Runtime.Tests.Board
{
    /// <summary>Ход тапами: первый тап выбирает плитку, тап по соседней меняет их местами.</summary>
    public sealed class TapSelectionTests
    {
        private BoardFixture _board;

        [SetUp]
        public void SetUp() => _board = new BoardFixture().WithSeededBoard(42).WithGameplaySystems();

        [TearDown]
        public void TearDown() => _board.Dispose();

        /// <summary>Тап выбирает плитку, но не двигает её.</summary>
        [Test]
        public void Tap_SelectsATileWithoutMovingIt()
        {
            var corner = _board.Layout[0];
            _board.Tap(corner);

            Assert.That(_board.Selected, Is.EqualTo(corner));
            Assert.That(_board.Moves, Is.Empty);
        }

        /// <summary>Тап по плитке, которая не соседствует с выбранной, переносит выбор на неё.</summary>
        [Test]
        public void TapOnANonNeighbor_MovesTheSelection()
        {
            var far = _board.Layout[11];
            _board.Tap(_board.Layout[0]);
            _board.Tap(far);

            Assert.That(_board.Selected, Is.EqualTo(far));
            Assert.That(_board.Moves, Is.Empty);
        }

        /// <summary>Повторный тап по выбранной плитке снимает выбор.</summary>
        [Test]
        public void SecondTapOnTheSelectedTile_ClearsIt()
        {
            var tile = _board.Layout[11];
            _board.Tap(tile);
            _board.Tap(tile);

            Assert.That(_board.Selected, Is.EqualTo(-1));
        }

        /// <summary>
        /// Тап по соседу выбранной плитки меняет их местами, снимает выбор и запускает анимацию обеих.
        /// </summary>
        [Test]
        public void TapOnANeighbor_SwapsAndClearsTheSelection()
        {
            var corner = _board.Layout[0];
            var right = _board.Layout[1];
            _board.Tap(corner);
            _board.Tap(right);

            Assert.That(_board.Moves.Count, Is.EqualTo(1));
            Assert.That(_board.Layout[0], Is.EqualTo(right));
            Assert.That(_board.Layout[1], Is.EqualTo(corner));
            Assert.That(_board.Selected, Is.EqualTo(-1));
            Assert.That(_board.MovingTiles, Is.EqualTo(2));
        }
    }
}
