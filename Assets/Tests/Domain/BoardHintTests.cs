using System;
using System.Linq;
using NUnit.Framework;

namespace Fives.Domain.Tests
{
    /// <summary>Подсказка: какую плитку она выбирает и каким путём ведёт её на место.</summary>
    public sealed class BoardHintTests
    {
        //  4 x 3:  0  1  2  3
        //          4  5  6  7
        //          8  9 10 11
        private static BoardState Board(params (int cell, int tile)[] moved)
        {
            var tiles = Enumerable.Range(0, 12).ToArray();
            foreach (var (cell, tile) in moved)
                tiles[cell] = tile;
            return new BoardState(4, 3, tiles);
        }

        /// <summary>На собранном поле подсказывать нечего.</summary>
        [Test]
        public void SolvedBoard_HasNoHint()
        {
            Assert.That(BoardHint.ClosestMisplaced(new BoardState(4, 3)), Is.Empty);
        }

        /// <summary>Подсказка выбирает из плиток, которые ближе всех к своему месту, и возвращает их все.</summary>
        [Test]
        public void ClosestMisplaced_ReturnsEveryTileAtTheSmallestDistance()
        {
            // Плитки 0 и 1 в одной клетке от своего места, плитки 3 и 8 — в пяти.
            var board = Board((0, 1), (1, 0), (3, 8), (8, 3));

            Assert.That(BoardHint.ClosestMisplaced(board), Is.EquivalentTo(new[] { 0, 1 }));
        }

        /// <summary>
        /// Из двух кратчайших путей подсказка выбирает тот, что не сдвигает плитку, уже стоящую на своём месте.
        /// </summary>
        [Test]
        public void Path_GoesAroundAPlacedTile_WhenAnotherShortestPathExists()
        {
            // Плитка 5 стоит в клетке 0; в клетке 1 своя плитка, в клетке 4 чужая.
            var board = Board((0, 5), (4, 0), (5, 4));
            Assert.That(BoardHint.PathOf(board, 5), Is.EqualTo(new[] { 0, 4, 5 }));

            var mirrored = Board((0, 5), (1, 0), (5, 1));
            Assert.That(BoardHint.PathOf(mirrored, 5), Is.EqualTo(new[] { 0, 1, 5 }));
        }

        /// <summary>Когда пути равны, подсказка сначала ведёт плитку по горизонтали.</summary>
        [Test]
        public void Path_PrefersHorizontalSteps_WhenRoutesAreEqual()
        {
            // В обеих клетках между 0 и 5 стоят чужие плитки.
            var board = Board((0, 5), (1, 4), (4, 1), (5, 0));

            Assert.That(BoardHint.PathOf(board, 5), Is.EqualTo(new[] { 0, 1, 5 }));
        }

        /// <summary>
        /// Для 300 раскладок и каждой плитки путь подсказки кратчайший, приводит плитку на место, а остальные плитки
        /// сдвигает не дальше чем на клетку.
        /// </summary>
        [Test]
        public void FollowingAPath_PlacesTheTile_AndMovesOthersByOneCellAtMost()
        {
            const int columns = 4;
            for (var seed = 1; seed <= 300; seed++)
            {
                var start = SeededShuffle.Create(columns, 3, seed);
                foreach (var tile in Enumerable.Range(0, start.CellCount))
                {
                    var board = start.Copy();
                    var path = BoardHint.PathOf(board, tile);
                    var from = board.CellOf(tile);

                    Assert.That(path.First(), Is.EqualTo(from));
                    Assert.That(path.Last(), Is.EqualTo(tile));
                    Assert.That(path.Count - 1, Is.EqualTo(Math.Abs(from % columns - tile % columns) + Math.Abs(from / columns - tile / columns)),
                        "a shortest path");

                    for (var step = 1; step < path.Count; step++)
                        Assert.That(board.TrySwap(new Swap(path[step - 1], path[step])), Is.True, $"seed={seed}, tile={tile}, step={step}");

                    Assert.That(board.CellOf(tile), Is.EqualTo(tile));
                    for (var other = 0; other < board.CellCount; other++)
                    {
                        if (other == tile) continue;
                        var before = start.CellOf(other);
                        var after = board.CellOf(other);
                        Assert.That(Math.Abs(before % columns - after % columns) + Math.Abs(before / columns - after / columns),
                            Is.LessThanOrEqualTo(1), $"seed={seed}, tile={tile}, other={other}");
                    }
                }
            }
        }
    }
}
