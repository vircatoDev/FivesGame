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

        private static int[] Tiles(BoardState board) => Enumerable.Range(0, board.CellCount).Select(i => board[i]).ToArray();
    }
}
