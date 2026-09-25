using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    public class SeededShuffleTests
    {
        // Layouts depend only on the cell count, so square boards keep their pre-4x3 references.
        [TestCase(3, 3, 12345, new[] { 1, 5, 6, 0, 3, 8, 7, 4, 2 })]
        [TestCase(3, 3, 0, new[] { 5, 2, 3, 6, 0, 8, 4, 1, 7 })]
        [TestCase(2, 2, 1, new[] { 2, 3, 1, 0 })]
        [TestCase(4, 4, int.MinValue, new[] { 12, 0, 11, 6, 3, 14, 9, 15, 10, 7, 2, 13, 4, 1, 8, 5 })]
        [TestCase(4, 3, 12345, new[] { 5, 8, 4, 9, 6, 10, 0, 3, 2, 11, 7, 1 })]
        public void VersionTwoHasStableReferenceLayouts(int columns, int rows, int seed, int[] expected)
        {
            CollectionAssert.AreEqual(expected, Tiles(SeededShuffle.Create(columns, rows, seed)));
        }

        [TestCase(2, 2)]
        [TestCase(3, 3)]
        [TestCase(4, 3)]
        [TestCase(3, 4)]
        [TestCase(6, 6)]
        public void ManySeedsGiveDeterministicPermutationsWithNoTileInPlace(int columns, int rows)
        {
            for (var seed = -500; seed <= 500; seed++)
            {
                var board = SeededShuffle.Create(columns, rows, seed);
                var tiles = Tiles(board);
                CollectionAssert.AreEquivalent(Enumerable.Range(0, board.CellCount), tiles);
                Assert.That(Enumerable.Range(0, board.CellCount).All(cell => tiles[cell] != cell), $"{columns}x{rows}, seed={seed}");
                Assert.That(board.IsSolved, Is.False);
                CollectionAssert.AreEqual(tiles, Tiles(SeededShuffle.Create(columns, rows, seed)));
            }
        }

        private static int[] Tiles(BoardState board) => Enumerable.Range(0, board.CellCount).Select(i => board[i]).ToArray();
    }
}
