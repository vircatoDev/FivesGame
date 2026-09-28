using System.Linq;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    /// <summary>Перемешивание, которым раздаётся новое поле 4×3.</summary>
    public sealed class SeededShuffleTests
    {
        /// <summary>Seed 12345 раздаёт ту же раскладку, что и раньше: алгоритм перемешивания не изменился.</summary>
        [Test]
        public void ReferenceSeed_GivesTheSameLayoutAsBefore()
        {
            // Если тест упал, изменился алгоритм перемешивания: тот же seed раздаёт другое поле.
            CollectionAssert.AreEqual(new[] { 5, 8, 4, 9, 6, 10, 0, 3, 2, 11, 7, 1 }, Tiles(SeededShuffle.Create(4, 3, 12345)));
        }

        /// <summary>
        /// Для каждого seed от -500 до 500 раскладка — перестановка, где ни одна плитка не стоит на своём месте, и
        /// тот же seed даёт ту же раскладку.
        /// </summary>
        [Test]
        public void EverySeed_DealsAPermutationWithNoTileInPlace_TheSameEachTime()
        {
            for (var seed = -500; seed <= 500; seed++)
            {
                var board = SeededShuffle.Create(4, 3, seed);
                var tiles = Tiles(board);
                CollectionAssert.AreEquivalent(Enumerable.Range(0, board.CellCount), tiles);
                Assert.That(Enumerable.Range(0, board.CellCount).All(cell => tiles[cell] != cell), $"seed={seed}");
                Assert.That(board.IsSolved, Is.False);
                CollectionAssert.AreEqual(tiles, Tiles(SeededShuffle.Create(4, 3, seed)));
            }
        }

        private static int[] Tiles(BoardState board) => Enumerable.Range(0, board.CellCount).Select(i => board[i]).ToArray();
    }
}
