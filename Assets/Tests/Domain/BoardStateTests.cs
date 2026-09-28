using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    /// <summary>
    /// Правила поля на игровом поле 4×3:
    ///   0  1  2  3
    ///   4  5  6  7
    ///   8  9 10 11
    /// </summary>
    public sealed class BoardStateTests
    {
        private const int Columns = 4;
        private const int Rows = 3;

        /// <summary>Новое поле 4×3 собрано: в клетке N стоит плитка N.</summary>
        [Test]
        public void NewBoard_IsSolved()
        {
            var board = new BoardState(Columns, Rows);

            Assert.That(board.CellCount, Is.EqualTo(12));
            Assert.That(CopyTiles(board), Is.EqualTo(Enumerable.Range(0, 12)));
            Assert.That(board.IsSolved, Is.True);
        }

        /// <summary>Поле, где по одной из сторон меньше двух клеток, не создаётся.</summary>
        [TestCase(1, 3)]  // один столбец
        [TestCase(3, 1)]  // одна строка
        public void Constructor_RejectsUnsupportedDimensions(int columns, int rows)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BoardState(columns, rows));
        }

        /// <summary>Поле из готовой раскладки принимает только перестановку всех своих плиток.</summary>
        [TestCase(new[] { 0, 1, 2 })]     // плиток меньше, чем клеток
        [TestCase(new[] { 0, 1, 1, 3 })]  // плитка 1 дважды
        [TestCase(new[] { 0, 1, 2, 4 })]  // плитки 4 на поле 2×2 нет
        public void Import_RejectsInvalidPermutation(int[] tiles)
        {
            Assert.Throws<ArgumentException>(() => new BoardState(2, 2, tiles));
        }

        /// <summary>
        /// Поле копирует раскладку:
        /// правка исходного массива не меняет поле, ход по полю не меняет массив.
        /// CellOf находит клетку плитки.
        /// </summary>
        [Test]
        public void Import_CopiesTilesAndIndexesCells()
        {
            var tiles = new[] { 0, 3, 2, 1 };
            var board = new BoardState(2, 2, tiles);
            tiles[0] = 3;

            Assert.That(CopyTiles(board), Is.EqualTo(new[] { 0, 3, 2, 1 }));
            Assert.That(board.IsSolved, Is.False);
            Assert.That(board.CellOf(1), Is.EqualTo(3));
            Assert.That(board.CellOf(4), Is.EqualTo(-1));

            board.TrySwap(new Swap(0, 1));
            Assert.That(tiles[1], Is.EqualTo(3), "Swapping must not mutate the caller's array.");
        }

        /// <summary>Обмен 5↔4 и обмен 4↔5 — один и тот же ход: клетки хранятся по возрастанию.</summary>
        [Test]
        public void Swap_OrdersItsCells()
        {
            Assert.That(new Swap(5, 4), Is.EqualTo(new Swap(4, 5)));
            Assert.That(new Swap(5, 4).First, Is.EqualTo(4));
        }

        /// <summary>
        /// Соседи клетки — только сверху, снизу, слева и справа;
        /// последняя клетка ряда не соседствует с первой клеткой следующего.
        /// </summary>
        [TestCase(3, new[] { 2, 7 })]         // правый верхний угол
        [TestCase(4, new[] { 0, 5, 8 })]      // левый край
        [TestCase(6, new[] { 2, 5, 7, 10 })]  // середина: четыре соседа
        [TestCase(11, new[] { 7, 10 })]       // правый нижний угол
        public void AreNeighbors_AcceptsOnlyOrthogonalCells_AndRowsDoNotWrap(int cell, int[] expectedCells)
        {
            var board = new BoardState(Columns, Rows);
            var neighbors = Enumerable.Range(-1, board.CellCount + 2).Where(other => board.AreNeighbors(cell, other));

            Assert.That(neighbors, Is.EqualTo(expectedCells));
            Assert.That(board.AreNeighbors(3, 4), Is.False, "The last cell of a row is not next to the first cell of the next one.");
        }

        /// <summary>Недопустимый обмен отклоняется и ничего на поле не меняет.</summary>
        [TestCase(0, 0)]    // клетка сама с собой
        [TestCase(0, 5)]    // по диагонали
        [TestCase(3, 4)]    // конец ряда и начало следующего
        [TestCase(-1, 0)]   // клетка за полем
        [TestCase(11, 12)]  // клетка за полем
        public void InvalidSwap_LeavesEveryCellUnchanged(int a, int b)
        {
            var board = new BoardState(Columns, Rows);

            Assert.That(board.TrySwap(new Swap(a, b)), Is.False);
            Assert.That(board.IsSolved, Is.True);
        }

        /// <summary>
        /// Обмен соседей меняет их плитки местами, повторный такой же обмен возвращает поле как было.
        /// </summary>
        [Test]
        public void Swap_ExchangesTwoCellsAndIsItsOwnInverse()
        {
            var board = new BoardState(Columns, Rows);

            Assert.That(board.TrySwap(new Swap(5, 1)), Is.True);
            Assert.That(CopyTiles(board), Is.EqualTo(new[] { 0, 5, 2, 3, 4, 1, 6, 7, 8, 9, 10, 11 }));
            Assert.That(board.CellOf(5), Is.EqualTo(1));
            Assert.That(board.TrySwap(new Swap(1, 5)), Is.True);
            Assert.That(board.IsSolved, Is.True);
        }

        /// <summary>
        /// 1000 случайных попыток обмена:
        /// допустимые меняют ровно две клетки, недопустимые ничего не меняют, поле всегда остаётся перестановкой.
        /// </summary>
        [Test]
        public void RandomSwaps_KeepAValidBoard_AndChangeOnlyTheTwoCells()
        {
            const int seed = 17;
            var random = new Random(seed);
            var board = new BoardState(Columns, Rows);
            var solved = Enumerable.Range(0, board.CellCount).ToArray();

            for (var step = 0; step < 1000; step++)
            {
                var before = CopyTiles(board);
                var a = random.Next(-1, board.CellCount + 1);
                var b = random.Next(2) == 0 ? a + 1 : a + Columns;
                var legal = a >= 0 && a < board.CellCount && NeighborCells(a).Contains(b);

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

        private static int[] CopyTiles(BoardState board) =>
            Enumerable.Range(0, board.CellCount).Select(cell => board[cell]).ToArray();

        // Соседи считаются заново по границам поля, а не той же формулой, что в коде игры.
        private static IEnumerable<int> NeighborCells(int cell)
        {
            if (cell % Columns > 0) yield return cell - 1;
            if (cell % Columns < Columns - 1) yield return cell + 1;
            if (cell >= Columns) yield return cell - Columns;
            if (cell < Columns * (Rows - 1)) yield return cell + Columns;
        }
    }
}
