using System;
using System.Collections.Generic;
using System.Linq;
using Fives.Domain;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    public sealed class BoardStateTests
    {
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        public void NewBoard_IsSolvedWithAnyTileHidden(int size)
        {
            for (var emptyTile = 0; emptyTile < size * size; emptyTile++)
            {
                var board = new BoardState(size, emptyTile);

                Assert.That(board.Size, Is.EqualTo(size));
                Assert.That(board.CellCount, Is.EqualTo(size * size));
                Assert.That(board.EmptyTileId, Is.EqualTo(emptyTile));
                Assert.That(board.EmptyCell, Is.EqualTo(emptyTile));
                Assert.That(CopyTiles(board), Is.EqualTo(Enumerable.Range(0, size * size)));
                Assert.That(board.IsSolved, Is.True);
            }
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void Constructor_RejectsUnsupportedSize(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardState(size, 0));
        }

        [Test]
        public void Constructor_RejectsCellCountOverflow()
        {
            Assert.Throws<OverflowException>(() => new BoardState(46341, 0));
        }

        [TestCase(-1)]
        [TestCase(9)]
        public void Constructor_RejectsInvalidEmptyTile(int emptyTile)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardState(3, emptyTile));
        }

        [Test]
        public void Import_RejectsNullTiles()
        {
            Assert.Throws<ArgumentNullException>(() => new BoardState(2, 3, null));
        }

        [TestCase(new int[] { })]
        [TestCase(new[] { 0, 1, 2 })]
        [TestCase(new[] { 0, 1, 2, 3, 4 })]
        [TestCase(new[] { 0, 1, 1, 3 })]
        [TestCase(new[] { 0, 1, 2, 2 })]
        [TestCase(new[] { 0, 1, 2, -1 })]
        [TestCase(new[] { 0, 1, 2, 4 })]
        public void Import_RejectsInvalidPermutation(int[] tiles)
        {
            Assert.Throws<ArgumentException>(() => new BoardState(2, 3, tiles));
        }

        [Test]
        public void Import_CopiesTilesAndLocatesEmptyCell()
        {
            var tiles = new[] { 0, 3, 2, 1 };
            var board = new BoardState(2, 3, tiles);
            tiles[0] = 3;

            Assert.That(CopyTiles(board), Is.EqualTo(new[] { 0, 3, 2, 1 }));
            Assert.That(board.EmptyCell, Is.EqualTo(1));
            Assert.That(board.IsSolved, Is.False);

            board.TryMove(0);
            Assert.That(tiles[1], Is.EqualTo(3), "Moving must not mutate the caller's array.");
        }

        [Test]
        public void Import_DoesNotPretendToValidateSolvability()
        {
            var board = new BoardState(2, 3, new[] { 1, 0, 2, 3 });

            Assert.That(board.IsSolved, Is.False);
            Assert.That(board.CanMove(2), Is.True);
        }

        [TestCase(0, new[] { 1, 3 })]
        [TestCase(2, new[] { 1, 5 })]
        [TestCase(4, new[] { 1, 3, 5, 7 })]
        [TestCase(6, new[] { 3, 7 })]
        [TestCase(8, new[] { 5, 7 })]
        public void CanMove_OnlyAcceptsOrthogonalNeighbors(int emptyTile, int[] expectedCells)
        {
            var board = new BoardState(3, emptyTile);
            var movable = Enumerable.Range(0, board.CellCount).Where(board.CanMove);

            Assert.That(movable, Is.EqualTo(expectedCells));
            Assert.That(board.IsSolved, Is.True, "Checking moves must not change the board.");
        }

        [TestCase(-1)]
        [TestCase(9)]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        [TestCase(0)]
        [TestCase(4)]
        [TestCase(8)]
        public void InvalidMove_LeavesEveryCellUnchanged(int cell)
        {
            var board = new BoardState(3, 8);
            var before = CopyTiles(board);

            Assert.That(board.CanMove(cell), Is.False);
            Assert.That(board.TryMove(cell), Is.False);
            Assert.That(CopyTiles(board), Is.EqualTo(before));
            Assert.That(board.EmptyCell, Is.EqualTo(8));
            Assert.That(board.IsSolved, Is.True);
        }

        [TestCase(2, 3)]
        [TestCase(3, 2)]
        [TestCase(5, 6)]
        [TestCase(6, 5)]
        public void Move_DoesNotWrapAcrossRows(int emptyTile, int cell)
        {
            var board = new BoardState(3, emptyTile);

            Assert.That(board.TryMove(cell), Is.False);
            Assert.That(board.IsSolved, Is.True);
        }

        [Test]
        public void Move_UsesCellIndexRatherThanTileId()
        {
            var board = new BoardState(2, 3, new[] { 2, 0, 3, 1 });

            Assert.That(board.TryMove(0), Is.True);
            Assert.That(CopyTiles(board), Is.EqualTo(new[] { 3, 0, 2, 1 }));
            Assert.That(board.EmptyCell, Is.Zero);
        }

        [Test]
        public void ReverseMove_RestoresSolvedBoardWithNonLastEmptyTile()
        {
            var board = new BoardState(3, 4);

            Assert.That(board.TryMove(1), Is.True);
            Assert.That(board.IsSolved, Is.False);
            Assert.That(board.TryMove(4), Is.True);
            Assert.That(board.IsSolved, Is.True);
            Assert.That(board.EmptyCell, Is.EqualTo(4));
        }

        [Test]
        public void SameInitialStateAndInputs_ProduceIdenticalResults()
        {
            var first = new BoardState(3, 4);
            var second = new BoardState(3, 4, CopyTiles(first));
            var cells = new[] { 1, 0, 3, 6, 7, 8, 5, 2, 1, -1, 9, 4 };

            foreach (var cell in cells)
            {
                Assert.That(first.TryMove(cell), Is.EqualTo(second.TryMove(cell)));
                Assert.That(CopyTiles(first), Is.EqualTo(CopyTiles(second)));
                Assert.That(first.EmptyCell, Is.EqualTo(second.EmptyCell));
                Assert.That(first.IsSolved, Is.EqualTo(second.IsSolved));
            }
        }

        [TestCase(2, 17)]
        [TestCase(2, 913)]
        [TestCase(3, 17)]
        [TestCase(3, 913)]
        [TestCase(4, 17)]
        [TestCase(4, 913)]
        [TestCase(6, 17)]
        [TestCase(6, 913)]
        public void MoveSequences_PreservePermutationAndChangeOnlyTwoCells(int size, int seed)
        {
            var random = new Random(seed);
            var board = new BoardState(size, random.Next(size * size));
            var solved = Enumerable.Range(0, board.CellCount).ToArray();

            for (var step = 0; step < 1000; step++)
            {
                var before = CopyTiles(board);
                var empty = board.EmptyCell;
                var cell = random.Next(-1, board.CellCount + 1);
                var legal = NeighborCells(empty, size).Contains(cell);

                Assert.That(board.TryMove(cell), Is.EqualTo(legal), $"seed={seed}, step={step}, cell={cell}");
                var after = CopyTiles(board);
                Assert.That(after, Is.EquivalentTo(solved));
                Assert.That(after[board.EmptyCell], Is.EqualTo(board.EmptyTileId));
                Assert.That(board.IsSolved, Is.EqualTo(after.SequenceEqual(solved)));

                if (!legal)
                {
                    Assert.That(after, Is.EqualTo(before));
                    Assert.That(board.EmptyCell, Is.EqualTo(empty));
                    continue;
                }

                Assert.That(board.EmptyCell, Is.EqualTo(cell));
                Assert.That(after[empty], Is.EqualTo(before[cell]));
                Assert.That(Enumerable.Range(0, board.CellCount).Where(i => before[i] != after[i]),
                    Is.EquivalentTo(new[] { cell, empty }));
                Assert.That(board.TryMove(empty), Is.True);
                Assert.That(CopyTiles(board), Is.EqualTo(before), "A reverse move must restore the exact state.");
                Assert.That(board.TryMove(cell), Is.True);
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TwoByTwo_ExhaustiveTraversalFindsTwelveReachableStates(int emptyTile)
        {
            var pending = new Queue<BoardState>();
            var visited = new HashSet<string>();
            var solved = new BoardState(2, emptyTile);
            pending.Enqueue(solved);
            visited.Add(string.Join(",", CopyTiles(solved)));

            while (pending.Count > 0)
            {
                var board = pending.Dequeue();
                var neighbors = NeighborCells(board.EmptyCell, board.Size).ToArray();
                for (var cell = 0; cell < board.CellCount; cell++)
                {
                    var next = new BoardState(2, emptyTile, CopyTiles(board));
                    Assert.That(next.TryMove(cell), Is.EqualTo(neighbors.Contains(cell)));
                    var key = string.Join(",", CopyTiles(next));
                    if (visited.Add(key))
                        pending.Enqueue(next);
                }
            }

            Assert.That(visited.Count, Is.EqualTo(12));
        }

        private static int[] CopyTiles(BoardState board) =>
            Enumerable.Range(0, board.CellCount).Select(cell => board[cell]).ToArray();

        // An independent boundary-based oracle, rather than the production distance formula.
        private static IEnumerable<int> NeighborCells(int empty, int size)
        {
            if (empty % size > 0) yield return empty - 1;
            if (empty % size < size - 1) yield return empty + 1;
            if (empty >= size) yield return empty - size;
            if (empty < size * (size - 1)) yield return empty + size;
        }
    }
}
