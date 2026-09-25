using System.Collections.Generic;
using Fives.Domain;
using Leopotam.Ecs;
using NUnit.Framework;
using Scripts.Components;

namespace Fives.Runtime.Tests
{
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

        [Test]
        public void EmptyHistory_IgnoresUndoAndRedo()
        {
            _board.Control(BoardControl.Undo);
            _board.Control(BoardControl.Redo);

            Assert.That(_board.Moves, Is.Empty);
            Assert.That(_board.Undone, Is.Empty);
            Assert.That(_board.Snapshot(), Is.EqualTo(_initial));
        }

        [Test]
        public void AcceptedMoves_PopulateHistory()
        {
            MakeThreeMoves();
            Assert.That(_board.Moves.Count, Is.EqualTo(3));
        }

        [Test]
        public void Undo_AnimatesAndMovesTheSwapToTheRedoStack()
        {
            MakeThreeMoves();
            _board.Control(BoardControl.Undo);

            Assert.That(_board.Moves.Count, Is.EqualTo(2));
            Assert.That(_board.Undone.Count, Is.EqualTo(1));
            Assert.That(_board.Snapshot(), Is.EqualTo(_afterMoves[1]));
            Assert.That(_board.MovingTiles, Is.EqualTo(2), "both swapped tiles animate");
        }

        [Test]
        public void Undo_DuringAnimation_IsIgnored()
        {
            MakeThreeMoves();
            _board.Control(BoardControl.Undo);
            _board.Control(BoardControl.Undo);

            Assert.That(_board.Moves.Count, Is.EqualTo(2));
        }

        [Test]
        public void Redo_ReappliesTheLastUndoneSwap()
        {
            MakeThreeMoves();
            _board.Control(BoardControl.Undo); _board.Settle();
            _board.Control(BoardControl.Undo); _board.Settle();
            _board.Control(BoardControl.Redo);

            Assert.That(_board.Moves.Count, Is.EqualTo(2));
            Assert.That(_board.Undone.Count, Is.EqualTo(1));
            Assert.That(_board.Snapshot(), Is.EqualTo(_afterMoves[1]));
            Assert.That(_board.MovingTiles, Is.EqualTo(2));
        }

        [Test]
        public void Redo_WalksForwardToTheLatestMove_ThenDoesNothing()
        {
            MakeThreeMoves();
            _board.Control(BoardControl.Undo); _board.Settle();
            _board.Control(BoardControl.Undo); _board.Settle();
            _board.Control(BoardControl.Redo); _board.Settle();
            _board.Control(BoardControl.Redo); _board.Settle();

            Assert.That(_board.Moves.Count, Is.EqualTo(3));
            Assert.That(_board.Undone, Is.Empty);
            Assert.That(_board.Snapshot(), Is.EqualTo(_afterMoves[2]));

            _board.Control(BoardControl.Redo); _board.Settle();
            Assert.That(_board.Moves.Count, Is.EqualTo(3));
            Assert.That(_board.Snapshot(), Is.EqualTo(_afterMoves[2]));
        }

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
            Assert.That(_board.Undone.Count, Is.EqualTo(3));
        }

        [Test]
        public void NewMoveAfterUndo_ClearsTheRedoStack()
        {
            MakeThreeMoves();
            while (_board.Moves.Count > 0)
            {
                _board.Control(BoardControl.Undo);
                _board.Settle();
            }
            _board.Swipe(_board.Layout[8], 0, -1);
            _board.Settle();

            Assert.That(_board.Moves, Is.EqualTo(new[] { new Swap(5, 8) }));
            Assert.That(_board.Undone, Is.Empty);
        }

        [Test]
        public void InvalidTileId_LeavesBoardAndHistoryIntact()
        {
            _board.Tap(-1);
            _board.Swipe(-1, 1, 0);

            Assert.That(_board.Moves, Is.Empty);
            Assert.That(_board.Selected, Is.EqualTo(-1));
            Assert.That(_board.Snapshot(), Is.EqualTo(_initial));
        }

        [Test]
        public void RectangularBoard_SwipesStayInsideTheirRow()
        {
            using var wide = new BoardFixture(4, 3).WithSeededBoard(42).WithGameplaySystems();

            wide.Swipe(wide.Layout[3], 1, 0);  // right from the end of the first row: not the next row's start
            wide.Swipe(wide.Layout[11], 0, 1); // down from the bottom row
            Assert.That(wide.Moves, Is.Empty);

            wide.Swipe(wide.Layout[3], 0, 1);
            wide.Settle();
            Assert.That(wide.Moves, Is.EqualTo(new[] { new Swap(3, 7) }));
        }

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

    public sealed class TapSelectionTests
    {
        private BoardFixture _board;

        [SetUp]
        public void SetUp() => _board = new BoardFixture().WithSeededBoard(42).WithGameplaySystems();

        [TearDown]
        public void TearDown() => _board.Dispose();

        [Test]
        public void Tap_SelectsATileWithoutMovingIt()
        {
            var corner = _board.Layout[0];
            _board.Tap(corner);

            Assert.That(_board.Selected, Is.EqualTo(corner));
            Assert.That(_board.Moves, Is.Empty);
        }

        [Test]
        public void TapOnANonNeighbor_MovesTheSelection()
        {
            var far = _board.Layout[8];
            _board.Tap(_board.Layout[0]);
            _board.Tap(far);

            Assert.That(_board.Selected, Is.EqualTo(far));
            Assert.That(_board.Moves, Is.Empty);
        }

        [Test]
        public void SecondTapOnTheSelectedTile_ClearsIt()
        {
            var far = _board.Layout[8];
            _board.Tap(far);
            _board.Tap(far);

            Assert.That(_board.Selected, Is.EqualTo(-1));
        }

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

        [Test]
        public void SwipeOffTheBoardEdge_IsRejected()
        {
            _board.Swipe(_board.Layout[2], 1, 0);
            _board.Swipe(_board.Layout[6], 0, 1);

            Assert.That(_board.Moves, Is.Empty);
        }

        [Test]
        public void SwipeDown_SwapsWithTheTileBelow()
        {
            _board.Swipe(_board.Layout[4], 0, 1);

            Assert.That(_board.Moves, Is.EqualTo(new[] { new Swap(4, 7) }));
        }
    }
}
