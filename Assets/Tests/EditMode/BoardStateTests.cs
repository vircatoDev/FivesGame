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
        public void NewBoard_IsSolved(int size)
        {
            var board = new BoardState(size);

            Assert.That(board.Size, Is.EqualTo(size));
            Assert.That(board.CellCount, Is.EqualTo(size * size));
            Assert.That(CopyTiles(board), Is.EqualTo(Enumerable.Range(0, size * size)));
            Assert.That(board.IsSolved, Is.True);
        }

        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void Constructor_RejectsUnsupportedSize(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardState(size));
        }

        [Test]
        public void Constructor_RejectsCellCountOverflow()
        {
            Assert.Throws<OverflowException>(() => new BoardState(46341));
        }

        [Test]
        public void Import_RejectsNullTiles()
        {
            Assert.Throws<ArgumentNullException>(() => new BoardState(2, null));
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
            Assert.Throws<ArgumentException>(() => new BoardState(2, tiles));
        }

        [Test]
        public void Import_CopiesTilesAndIndexesCells()
        {
            var tiles = new[] { 0, 3, 2, 1 };
            var board = new BoardState(2, tiles);
            tiles[0] = 3;

            Assert.That(CopyTiles(board), Is.EqualTo(new[] { 0, 3, 2, 1 }));
            Assert.That(board.IsSolved, Is.False);
            Assert.That(board.CellOf(1), Is.EqualTo(3));
            Assert.That(board.CellOf(4), Is.EqualTo(-1));

            board.TrySwap(new Swap(0, 1));
            Assert.That(tiles[1], Is.EqualTo(3), "Swapping must not mutate the caller's array.");
        }

        [Test]
        public void Swap_OrdersItsCells()
        {
            Assert.That(new Swap(5, 4), Is.EqualTo(new Swap(4, 5)));
            Assert.That(new Swap(5, 4).First, Is.EqualTo(4));
        }

        [TestCase(0, new[] { 1, 3 })]
        [TestCase(2, new[] { 1, 5 })]
        [TestCase(4, new[] { 1, 3, 5, 7 })]
        [TestCase(6, new[] { 3, 7 })]
        [TestCase(8, new[] { 5, 7 })]
        public void AreNeighbors_OnlyAcceptsOrthogonalNeighbors(int cell, int[] expectedCells)
        {
            var board = new BoardState(3);
            var neighbors = Enumerable.Range(-1, board.CellCount + 2).Where(other => board.AreNeighbors(cell, other));

            Assert.That(neighbors, Is.EqualTo(expectedCells));
            Assert.That(board.IsSolved, Is.True, "Checking neighbors must not change the board.");
        }

        [TestCase(0, 0)]
        [TestCase(0, 4)]
        [TestCase(0, 8)]
        [TestCase(2, 3)]
        [TestCase(5, 6)]
        [TestCase(-1, 0)]
        [TestCase(8, 9)]
        [TestCase(int.MinValue, int.MaxValue)]
        public void InvalidSwap_LeavesEveryCellUnchanged(int a, int b)
        {
            var board = new BoardState(3);

            Assert.That(board.TrySwap(new Swap(a, b)), Is.False);
            Assert.That(board.IsSolved, Is.True);
        }

        [Test]
        public void Swap_ExchangesTwoCellsAndIsItsOwnInverse()
        {
            var board = new BoardState(3);

            Assert.That(board.TrySwap(new Swap(4, 1)), Is.True);
            Assert.That(CopyTiles(board), Is.EqualTo(new[] { 0, 4, 2, 3, 1, 5, 6, 7, 8 }));
            Assert.That(board.CellOf(4), Is.EqualTo(1));
            Assert.That(board.TrySwap(new Swap(1, 4)), Is.True);
            Assert.That(board.IsSolved, Is.True);
        }

        [Test]
        public void SameInitialStateAndInputs_ProduceIdenticalResults()
        {
            var first = new BoardState(3, new[] { 8, 0, 1, 2, 3, 4, 5, 6, 7 });
            var second = new BoardState(3, CopyTiles(first));
            var swaps = new[] { new Swap(0, 1), new Swap(1, 4), new Swap(2, 3), new Swap(7, 8), new Swap(-1, 0) };

            foreach (var swap in swaps)
            {
                Assert.That(first.TrySwap(swap), Is.EqualTo(second.TrySwap(swap)));
                Assert.That(CopyTiles(first), Is.EqualTo(CopyTiles(second)));
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
        public void SwapSequences_PreservePermutationAndChangeOnlyTwoCells(int size, int seed)
        {
            var random = new Random(seed);
            var board = new BoardState(size);
            var solved = Enumerable.Range(0, board.CellCount).ToArray();

            for (var step = 0; step < 1000; step++)
            {
                var before = CopyTiles(board);
                var a = random.Next(-1, board.CellCount + 1);
                var b = random.Next(2) == 0 ? a + 1 : a + size;
                var legal = a >= 0 && a < board.CellCount && NeighborCells(a, size).Contains(b);

                Assert.That(board.TrySwap(new Swap(a, b)), Is.EqualTo(legal), $"seed={seed}, step={step}, swap={a}<->{b}");
                var after = CopyTiles(board);
                Assert.That(after, Is.EquivalentTo(solved));
                Assert.That(board.IsSolved, Is.EqualTo(after.SequenceEqual(solved)));
                Assert.That(Enumerable.Range(0, board.CellCount).All(c => board.CellOf(after[c]) == c));

                if (!legal)
                {
                    Assert.That(after, Is.EqualTo(before));
                    continue;
                }

                Assert.That(after[a], Is.EqualTo(before[b]));
                Assert.That(after[b], Is.EqualTo(before[a]));
                Assert.That(Enumerable.Range(0, board.CellCount).Where(i => before[i] != after[i]),
                    Is.EquivalentTo(new[] { a, b }));
            }
        }

        [Test]
        public void TwoByTwo_SwapsReachEveryArrangement()
        {
            var pending = new Queue<BoardState>();
            var visited = new HashSet<string>();
            var solved = new BoardState(2);
            pending.Enqueue(solved);
            visited.Add(string.Join(",", CopyTiles(solved)));

            while (pending.Count > 0)
            {
                var board = pending.Dequeue();
                for (var a = 0; a < board.CellCount; a++)
                {
                    foreach (var b in NeighborCells(a, board.Size))
                    {
                        var next = new BoardState(2, CopyTiles(board));
                        Assert.That(next.TrySwap(new Swap(a, b)), Is.True);
                        if (visited.Add(string.Join(",", CopyTiles(next))))
                            pending.Enqueue(next);
                    }
                }
            }

            Assert.That(visited.Count, Is.EqualTo(24), "Any arrangement of four tiles is solvable by swaps.");
        }

        private static int[] CopyTiles(BoardState board) =>
            Enumerable.Range(0, board.CellCount).Select(cell => board[cell]).ToArray();

        // An independent boundary-based oracle, rather than the production distance formula.
        private static IEnumerable<int> NeighborCells(int cell, int size)
        {
            if (cell % size > 0) yield return cell - 1;
            if (cell % size < size - 1) yield return cell + 1;
            if (cell >= size) yield return cell - size;
            if (cell < size * (size - 1)) yield return cell + size;
        }
    }
}
