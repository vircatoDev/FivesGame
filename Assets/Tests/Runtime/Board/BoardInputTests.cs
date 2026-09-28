using System.Collections.Generic;
using Fives.Components;
using Fives.Domain;
using Leopotam.Ecs;
using NUnit.Framework;

namespace Fives.Runtime.Tests.Board
{
    /// <summary>Свайпы, Undo и порядок ввода внутри одного кадра.</summary>
    public sealed class BoardInputTests
    {
        private BoardFixture _board;
        private int[] _initial;
        private List<int[]> _afterMoves;

        [SetUp]
        public void SetUp()
        {
            _board = new BoardFixture().WithSeededBoard(42).WithGameplaySystems();
            _initial = _board.Snapshot();
            _afterMoves = new List<int[]>();
        }

        [TearDown]
        public void TearDown() => _board.Dispose();

        private void MakeThreeMoves()
        {
            foreach (var (cell, dx, dy) in new[] { (0, 1, 0), (4, 1, 0), (6, 0, -1) })
            {
                _board.Swipe(_board.Layout[cell], dx, dy);
                _board.Settle();
                _afterMoves.Add(_board.Snapshot());
            }
        }

        /// <summary>Два свайпа в одном кадре: засчитывается только первый.</summary>
        [Test]
        public void RapidSwipes_AcceptExactlyOneSwap()
        {
            var first = _board.Layout[0];
            _board.World.NewEntity().Replace(new TileSwipeEvent { Id = _board.Layout[0], Dx = 1 });
            _board.World.NewEntity().Replace(new TileSwipeEvent { Id = _board.Layout[4], Dx = 1 });
            _board.Systems.Run();

            Assert.That(_board.Moves.Count, Is.EqualTo(1));
            Assert.That(_board.Layout[1], Is.EqualTo(first));
        }

        /// <summary>Свайп вниз меняет плитку с той, что под ней: клетка 4 и клетка 8.</summary>
        [Test]
        public void SwipeDown_SwapsWithTheTileBelow()
        {
            _board.Swipe(_board.Layout[4], 0, 1);

            Assert.That(_board.Moves, Is.EqualTo(new[] { new Swap(4, 8) }));
        }

        /// <summary>
        /// Свайпы за край поля во все четыре стороны отклоняются;
        /// свайп вправо с конца ряда не переходит на следующий ряд.
        /// </summary>
        [Test]
        public void SwipesOffTheBoard_AreRejected_AndRowsDoNotWrap()
        {
            _board.Swipe(_board.Layout[3], 1, 0);  // вправо с конца первого ряда: не на начало следующего
            _board.Swipe(_board.Layout[8], -1, 0); // влево с начала последнего ряда
            _board.Swipe(_board.Layout[1], 0, -1); // вверх с верхнего ряда
            _board.Swipe(_board.Layout[11], 0, 1); // вниз с нижнего ряда

            Assert.That(_board.Moves, Is.Empty);
        }

        /// <summary>Тап и свайп по несуществующей плитке не меняют ни поле, ни историю ходов, ни выбор.</summary>
        [Test]
        public void InvalidTileId_LeavesBoardAndHistoryIntact()
        {
            _board.Tap(-1);
            _board.Swipe(-1, 1, 0);

            Assert.That(_board.Moves, Is.Empty);
            Assert.That(_board.Selected, Is.EqualTo(-1));
            Assert.That(_board.Snapshot(), Is.EqualTo(_initial));
        }

        /// <summary>Undo без сделанных ходов ничего не делает.</summary>
        [Test]
        public void EmptyHistory_IgnoresUndo()
        {
            _board.Control(BoardControl.Undo);

            Assert.That(_board.Moves, Is.Empty);
            Assert.That(_board.Snapshot(), Is.EqualTo(_initial));
        }

        /// <summary>
        /// Undo возвращает поле к состоянию до последнего хода, убирает ход из истории и анимирует обе плитки.
        /// </summary>
        [Test]
        public void Undo_AnimatesAndForgetsTheLastSwap()
        {
            MakeThreeMoves();
            _board.Control(BoardControl.Undo);

            Assert.That(_board.Moves.Count, Is.EqualTo(2));
            Assert.That(_board.Snapshot(), Is.EqualTo(_afterMoves[1]));
            Assert.That(_board.MovingTiles, Is.EqualTo(2), "both swapped tiles animate");
        }

        /// <summary>Второе Undo, пока плитки ещё едут, не срабатывает.</summary>
        [Test]
        public void Undo_DuringAnimation_IsIgnored()
        {
            MakeThreeMoves();
            _board.Control(BoardControl.Undo);
            _board.Control(BoardControl.Undo);

            Assert.That(_board.Moves.Count, Is.EqualTo(2));
        }

        /// <summary>Если отменить все ходы, поле возвращается к исходной раскладке.</summary>
        [Test]
        public void UndoAll_ReturnsToTheShuffledLayout()
        {
            MakeThreeMoves();
            while (_board.Moves.Count > 0)
            {
                _board.Control(BoardControl.Undo);
                _board.Settle();
            }

            Assert.That(_board.Snapshot(), Is.EqualTo(_initial));
        }

        /// <summary>Нажатие подсказки не считается ходом и не двигает плитки.</summary>
        [Test]
        public void Hint_IsNotABoardMove()
        {
            MakeThreeMoves();
            _board.Control(BoardControl.Hint);

            Assert.That(_board.Moves.Count, Is.EqualTo(3));
            Assert.That(_board.Snapshot(), Is.EqualTo(_afterMoves[2]));
        }

        /// <summary>В кадре выхода из партии Undo не выполняется.</summary>
        [Test]
        public void Exit_TakesPrecedenceOverBoardCommands()
        {
            _board.Swipe(_board.Layout[0], 1, 0);
            _board.Settle();
            var beforeExit = _board.Snapshot();
            _board.World.NewEntity().Get<GameEndEvent>();
            _board.Control(BoardControl.Undo);

            Assert.That(_board.Moves.Count, Is.EqualTo(1));
            Assert.That(_board.Snapshot(), Is.EqualTo(beforeExit));
        }
    }
}
