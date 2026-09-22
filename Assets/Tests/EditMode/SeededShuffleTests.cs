using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    public class SeededShuffleTests
    {
        [TestCase(3, 8, 12345, 32, new[] { 6, 0, 5, 7, 4, 3, 8, 2, 1 })]
        [TestCase(3, 4, 0, 32, new[] { 4, 1, 5, 7, 8, 6, 3, 2, 0 })]
        [TestCase(2, 3, 1, 12, new[] { 0, 3, 2, 1 })]
        [TestCase(4, 7, int.MinValue, 64, new[] { 3, 4, 7, 1, 8, 0, 2, 10, 12, 5, 6, 15, 9, 11, 14, 13 })]
        public void VersionOneHasStableReferenceLayouts(int size, int empty, int seed, int steps, int[] expected)
        {
            CollectionAssert.AreEqual(expected, Tiles(SeededShuffle.Create(size, empty, seed, steps)));
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        public void AllEmptyTileIdsAndManySeedsPreservePermutationAndAreUnsolved(int size)
        {
            for (var empty = 0; empty < size * size; empty++)
            for (var seed = -50; seed <= 50; seed++)
            {
                var board = SeededShuffle.Create(size, empty, seed, size * size * 4);
                CollectionAssert.AreEquivalent(Enumerable.Range(0, board.CellCount), Tiles(board));
                Assert.That(board.IsSolved, Is.False, $"size={size}, empty={empty}, seed={seed}");
                Assert.That(board[board.EmptyCell], Is.EqualTo(empty));
                CollectionAssert.AreEqual(Tiles(board), Tiles(SeededShuffle.Create(size, empty, seed, size * size * 4)));
            }
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveShuffleLengthIsRejected(int steps) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => SeededShuffle.Create(3, 8, 1, steps));

        [Test]
        public void EveryTwoByTwoShuffleIsReachableFromItsSolvedBoard()
        {
            for (var empty = 0; empty < 4; empty++)
            {
                var reachable = new HashSet<string>();
                var queue = new Queue<BoardState>();
                queue.Enqueue(new BoardState(2, empty));
                while (queue.Count > 0)
                {
                    var board = queue.Dequeue();
                    if (!reachable.Add(string.Join(",", Tiles(board)))) continue;
                    for (var cell = 0; cell < 4; cell++)
                    {
                        var next = new BoardState(2, empty, Tiles(board));
                        if (next.TryMove(cell)) queue.Enqueue(next);
                    }
                }
                Assert.That(reachable.Count, Is.EqualTo(12));
                for (var seed = 0; seed < 100; seed++)
                for (var steps = 1; steps <= 24; steps++)
                    Assert.That(reachable.Contains(string.Join(",", Tiles(SeededShuffle.Create(2, empty, seed, steps)))), Is.True);
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        public void ReplayReconstructsEachAcceptedMove(int size)
        {
            var board = SeededShuffle.Create(size, 0, -37, 40);
            var moves = new List<int>();
            var snapshots = new List<int[]>();
            var previousEmpty = -1;
            for (var i = 0; i < 100 && !board.IsSolved; i++)
            {
                var cell = Enumerable.Range(0, board.CellCount).First(c => c != previousEmpty && board.CanMove(c));
                previousEmpty = board.EmptyCell;
                board.TryMove(cell);
                moves.Add(cell);
                snapshots.Add(Tiles(board));
            }
            var data = new ReplayData(1, size, 0, -37, 40, moves);
            var replay = data.CreatePlaybackBoard();
            for (var i = 0; i < data.Moves.Count; i++)
            {
                Assert.That(replay.TryMove(data.Moves[i]), Is.True);
                CollectionAssert.AreEqual(snapshots[i], Tiles(replay));
            }
        }

        [Test]
        public void ReplayCopiesMovesAndDoesNotExposeAMutableArray()
        {
            var moves = new[] { 8 };
            var data = new ReplayData(1, 3, 8, 1, 1, moves);
            moves[0] = -1;
            Assert.That(data.Moves[0], Is.EqualTo(8));
            Assert.Throws<NotSupportedException>(() => ((IList<int>)data.Moves)[0] = -1);
        }

        [Test]
        public void ReplayRejectsUnknownVersionNullAndInvalidMoves()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ReplayData(2, 3, 8, 1, 1, Array.Empty<int>()));
            Assert.Throws<ArgumentNullException>(() => new ReplayData(1, 3, 8, 1, 1, null));
            Assert.Throws<ArgumentException>(() => new ReplayData(1, 3, 8, 1, 1, new[] { -1 }).CreatePlaybackBoard());
            Assert.Throws<ArgumentException>(() => new ReplayData(1, 3, 8, 1, 1, new[] { 0 }).CreatePlaybackBoard());
        }

        [Test]
        public void ReplayRejectsMovesAfterWinning()
        {
            Assert.Throws<ArgumentException>(() => new ReplayData(1, 3, 8, 1, 1, new[] { 8, 7 }).CreatePlaybackBoard());
        }

        private static int[] Tiles(BoardState board) => Enumerable.Range(0, board.CellCount).Select(i => board[i]).ToArray();
    }
}
