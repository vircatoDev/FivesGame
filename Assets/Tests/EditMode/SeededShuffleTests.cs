using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    public class SeededShuffleTests
    {
        [TestCase(3, 12345, new[] { 1, 5, 6, 0, 3, 8, 7, 4, 2 })]
        [TestCase(3, 0, new[] { 5, 2, 3, 6, 0, 8, 4, 1, 7 })]
        [TestCase(2, 1, new[] { 2, 3, 1, 0 })]
        [TestCase(4, int.MinValue, new[] { 12, 0, 11, 6, 3, 14, 9, 15, 10, 7, 2, 13, 4, 1, 8, 5 })]
        public void VersionTwoHasStableReferenceLayouts(int size, int seed, int[] expected)
        {
            CollectionAssert.AreEqual(expected, Tiles(SeededShuffle.Create(size, seed)));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        public void ManySeedsGiveDeterministicPermutationsWithNoTileInPlace(int size)
        {
            for (var seed = -500; seed <= 500; seed++)
            {
                var board = SeededShuffle.Create(size, seed);
                var tiles = Tiles(board);
                CollectionAssert.AreEquivalent(Enumerable.Range(0, board.CellCount), tiles);
                Assert.That(Enumerable.Range(0, board.CellCount).All(cell => tiles[cell] != cell), $"size={size}, seed={seed}");
                Assert.That(board.IsSolved, Is.False);
                CollectionAssert.AreEqual(tiles, Tiles(SeededShuffle.Create(size, seed)));
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        public void ReplayReconstructsEachAcceptedSwap(int size)
        {
            var board = SeededShuffle.Create(size, -37);
            var random = new Random(size);
            var moves = new List<Swap>();
            var snapshots = new List<int[]>();
            while (moves.Count < 100 && !board.IsSolved)
            {
                var cell = random.Next(board.CellCount);
                var swap = cell % size < size - 1 ? new Swap(cell, cell + 1) : new Swap(cell, cell - 1);
                Assert.That(board.TrySwap(swap), Is.True);
                moves.Add(swap);
                snapshots.Add(Tiles(board));
            }

            var data = new ReplayData(ReplayData.CurrentVersion, size, -37, moves);
            var replay = data.CreatePlaybackBoard();
            for (var i = 0; i < data.Moves.Count; i++)
            {
                Assert.That(replay.TrySwap(data.Moves[i]), Is.True);
                CollectionAssert.AreEqual(snapshots[i], Tiles(replay));
            }
        }

        [TestCase(2, 1)]
        [TestCase(3, 12345)]
        [TestCase(4, -7)]
        public void ReplayAcceptsASolvingPathAndRejectsMovesAfterWinning(int size, int seed)
        {
            var path = SolvingSwaps(SeededShuffle.Create(size, seed));
            var replay = new ReplayData(ReplayData.CurrentVersion, size, seed, path).CreatePlaybackBoard();
            foreach (var swap in path)
                replay.TrySwap(swap);
            Assert.That(replay.IsSolved, Is.True);

            var extra = path.Append(new Swap(0, 1)).ToList();
            Assert.Throws<ArgumentException>(() => new ReplayData(ReplayData.CurrentVersion, size, seed, extra).CreatePlaybackBoard());
        }

        [Test]
        public void ReplayCopiesMovesAndDoesNotExposeAMutableArray()
        {
            var moves = new[] { new Swap(0, 1) };
            var data = new ReplayData(ReplayData.CurrentVersion, 3, 1, moves);
            moves[0] = new Swap(7, 8);
            Assert.That(data.Moves[0], Is.EqualTo(new Swap(0, 1)));
            Assert.Throws<NotSupportedException>(() => ((IList<Swap>)data.Moves)[0] = new Swap(7, 8));
        }

        [Test]
        public void ReplayRejectsUnknownVersionNullAndInvalidSwaps()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReplayData(1, 3, 1, Array.Empty<Swap>()));
            Assert.Throws<ArgumentNullException>(() => new ReplayData(ReplayData.CurrentVersion, 3, 1, null));
            Assert.Throws<ArgumentException>(() =>
                new ReplayData(ReplayData.CurrentVersion, 3, 1, new[] { new Swap(-1, 0) }).CreatePlaybackBoard());
            Assert.Throws<ArgumentException>(() =>
                new ReplayData(ReplayData.CurrentVersion, 3, 1, new[] { new Swap(0, 4) }).CreatePlaybackBoard());
        }

        // Places tiles in row-major order: along the tile's row to the target column, then up.
        // Every cell on that path is still unplaced, so earlier tiles stay put.
        private static List<Swap> SolvingSwaps(BoardState start)
        {
            var board = start.Copy();
            var size = board.Size;
            var path = new List<Swap>();
            for (var target = 0; target < board.CellCount; target++)
            {
                var cell = board.CellOf(target);
                while (cell % size != target % size)
                    cell = Step(cell, cell % size > target % size ? cell - 1 : cell + 1);
                while (cell != target)
                    cell = Step(cell, cell - size);
            }
            return path;

            int Step(int from, int to)
            {
                var swap = new Swap(from, to);
                Assert.That(board.TrySwap(swap), Is.True);
                path.Add(swap);
                return to;
            }
        }

        private static int[] Tiles(BoardState board) => Enumerable.Range(0, board.CellCount).Select(i => board[i]).ToArray();
    }
}
